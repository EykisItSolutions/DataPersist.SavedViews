using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;
using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews.Areas.Views;

public class Edit : BaseModel
{
    #region Data

    public int Id { get; set; }

    [Required(ErrorMessage = "View Name is required")]
    public string Name { get; set; } = null!;
    public string? What { get; set; }
    public string? FilterLogic { get; set; }
    public string? SortLogic { get; set; }
    public string? FilterClause { get; set; }
    public string? SortClause { get; set; }
    public string? Parms { get; set; }

    public List<Filter> Filters { get; set; } = new();
    public List<Sort> Sorts { get; set; } = new();

    public class Filter
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string? Column { get; set; }
        public string? Operator { get; set; }
        public string? Value { get; set; }
    }

    public class Sort
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string? Column { get; set; }
        public string? Direction { get; set; }
    }

    #endregion

    #region Handlers

    public override async Task<IActionResult> GetAsync()
    {
        if (Id == 0)    // Nwe View
        {
            // Ensure there are 5 and 2 of each.

            for (int i = 0; i < 5; i++) Filters.Add(new Filter { Number = i + 1 });
            for (int i = 0; i < 2; i++) Sorts.Add(new Sort { Number = i + 1 });

        }
        else // Existing View
        {
            var view = await _db.Views.SingleOrDefaultAsync(v => v.Id == Id);
            var viewFilters = _db.ViewFilters.Where(f => f.ViewId == Id).OrderBy(f => f.Number);
            var viewSorts = _db.ViewSorts.Where(f => f.ViewId == Id).OrderBy(f => f.Number);

            _mapper.Map(view, this);
            _mapper.Map(viewFilters, Filters);
            _mapper.Map(viewSorts, Sorts);
           
            // Ensure there are 5 and 2 of each.

            for (int i = viewFilters.Count(); i < 5; i++) Filters.Add(new Filter { Number = i + 1 });
            for (int i = viewSorts.Count(); i < 2; i++) Sorts.Add(new Sort { Number = i + 1 });
        }

        if (Referer.IndexOf("students") > -1) 
            What = "Student";
        else // Other modules with saved views go here
            What = "Student";

        return View(this);
    }

    public override async Task<IActionResult> PostAsync()
    {
        if (!ModelState.IsValid) return View(this);
          

        if (!IsLogicValid(FilterLogic))
        {
            Failure = "Invalid filter logic, please correct.";
        }
        else if (Id == 0) // new view
        {
            var view = new View();
            var viewFilters = new List<ViewFilter>();
            var viewSorts = new List<ViewSort>();
            
            _mapper.Map(this, view);

            foreach (var filter in Filters)
            {
                // ensure clause is complete

                if (!string.IsNullOrEmpty(filter.Column) &&
                    !string.IsNullOrEmpty(filter.Operator) &&
                    !string.IsNullOrEmpty(filter.Value))
                {
                    viewFilters.Add(new()
                    {
                        Column = filter.Column,
                        Number = filter.Number,
                        Operator = filter.Operator,
                        Value = filter.Value
                    });
                }
            }

            foreach (var sort in Sorts)
            {
                // ensure clause is complete

                if (!string.IsNullOrEmpty(sort.Column) &&
                    !string.IsNullOrEmpty(sort.Direction))
                {
                    viewSorts.Add(new()
                    {
                        Number = sort.Number,
                        Column = sort.Column,
                        Direction = sort.Direction
                    });
                }
            }

            BuildQuery(view, viewFilters, viewSorts);

            // test query

            if (ValidateQuery(view))
            {
                
                // ** Unit of Work pattern

                using var transaction = _db.Database.BeginTransaction();

                try
                {
                    _db.Views.Add(view);
                    _db.SaveChanges();

                    foreach (var viewFilter in viewFilters)
                    {
                        viewFilter.ViewId = view.Id;
                        _db.ViewFilters.Add(viewFilter);
                    }

                    foreach (var viewSort in viewSorts)
                    {
                        viewSort.ViewId = view.Id;
                        _db.ViewSorts.Add(viewSort);
                    }

                    _db.SaveChanges();

                    await _activityService.SaveAsync($"Saved View added: {view.Name}");

                    transaction.Commit();

                }
                catch // (Exception ex)
                {
                    transaction.Rollback();

                    Failure = "Saving view was unsuccessful";
                }

                return LocalRedirect(Referer + "?ViewId=" + view.Id);
            }
            else
            {
                Failure = "View appears invalid. Please verify all conditions.";
            }
        }

        else // existing view
        {
            var view = new View();
            var viewFilters = new List<ViewFilter>();
            var viewSorts = new List<ViewSort>();

            _mapper.Map(this, view);
            

            foreach (var filter in Filters)
            {
                // ensure clause is complete

                if (!string.IsNullOrEmpty(filter.Column) &&
                    !string.IsNullOrEmpty(filter.Operator) &&
                    !string.IsNullOrEmpty(filter.Value))
                {
                    viewFilters.Add(new()
                    {
                        Column = filter.Column,
                        Number = filter.Number,
                        Operator = filter.Operator,
                        Value = filter.Value
                    });
                }
            }

            foreach (var sort in Sorts)
            {
                // ensure clause is complete

                if (!string.IsNullOrEmpty(sort.Column) &&
                    !string.IsNullOrEmpty(sort.Direction))
                {
                    viewSorts.Add(new()
                    {
                        Number = sort.Number,
                        Column = sort.Column,
                        Direction = sort.Direction
                    });
                }
            }

            BuildQuery(view, viewFilters, viewSorts);

            // test query

            if (ValidateQuery(view))
            {
                var originalView = await _db.Views.SingleAsync(v => v.Id == Id);
                var originalViewFilters = await _db.ViewFilters.Where(v => v.ViewId == Id).ToListAsync();
                var originalViewSorts = await _db.ViewSorts.Where(v => v.ViewId == Id).ToListAsync();

                // ** Unit of Work pattern

                using var transaction = _db.Database.BeginTransaction();

                try
                {
                    // Remove existing view first

                    foreach (var originalViewSort in originalViewSorts)
                        _db.ViewSorts.Remove(originalViewSort);

                    foreach (var originalViewFilter in originalViewFilters)
                        _db.ViewFilters.Remove(originalViewFilter);

                    _db.Views.Remove(originalView);

                    _db.SaveChanges();

                    // Start adding the new view

                    _db.Views.Add(view);
                    _db.SaveChanges();

                    foreach (var viewFilter in viewFilters)
                    {
                        viewFilter.ViewId = view.Id;
                        _db.ViewFilters.Add(viewFilter);
                    }

                    foreach (var viewSort in viewSorts)
                    {
                        viewSort.ViewId = view.Id;
                        _db.ViewSorts.Add(viewSort);
                    }

                    _db.SaveChanges();

                    await _activityService.SaveAsync($"Saved View updated: {view.Name}");


                    transaction.Commit();

                }
                catch // (Exception ex)
                {
                    transaction.Rollback();

                    Failure = "Saving view was unsuccessful";
                }

                return LocalRedirect(Referer + "?ViewId=" + view.Id);
            }
            else
            {
                Failure = "View appears invalid. Please verify all conditions.";
            }
        }
    
        return LocalRedirect(Referer ?? "/students");
    }

    #endregion

    #region Helpers

    // validate logic expressions by only allowing AND, OR, numbers, spaces, and parentheses

    private bool IsLogicValid(string? logic)
    {
        if (string.IsNullOrEmpty(logic)) return true;

        return Regex.IsMatch(logic, @"^[0-9orandORAND\s\)\(]*$");
    }

    // assemble query from component parts

    private void BuildQuery(View view, List<ViewFilter> filters, List<ViewSort> sorts)
    {
        int count = 0;
        foreach (var filter in filters)
            BuildFilterClause(view, filter, count++);

        foreach (var sort in sorts)
            BuildSortClause(sort);

        // assemble the SQL parts 

        var sb = new StringBuilder();
        if (string.IsNullOrEmpty(view.FilterLogic))
        {
            foreach (var filter in filters)
                sb.Append(filter.Clause + " AND ");

            view.FilterClause = sb.ToString();
            if (view.FilterClause.Length > 5)
                view.FilterClause = view.FilterClause.Substring(0, view.FilterClause.Length - 5);
        }
        else
        {
            var clause = view.FilterLogic;
            foreach (var filter in filters)
                clause = clause.Replace(filter.Number.ToString(), filter.Clause);

            view.FilterClause = clause;
        }

        if (sorts.Count == 0)
            view.SortClause = null;
        else if (sorts.Count == 1)
            view.SortClause = sorts[0].Clause;
        else
            view.SortClause = sorts[0].Clause + ", " + sorts[1].Clause;
    }

    private void BuildFilterClause(View view, ViewFilter filter, int count)
    {
        string clause = filter.Column;
        string parm = "";
        string c = "{" + count + "}";

        switch (filter.Operator)
        {
            case "equals": clause += $" = {c}"; parm = filter.Value; break;
            case "not equals to": clause += $" <> {c}"; parm = filter.Value; break;
            case "starts with": clause += $" LIKE {c}"; parm = filter.Value + "%"; break;
            case "contains": clause += $" LIKE {c}"; parm = "%" + filter.Value + "%"; break;
            case "does not contain": clause += $" NOT LIKE {c}"; parm = "%" + filter.Value + "%"; break;
            case "less than": clause += $" < {c}"; parm = filter.Value; break;
            case "greater than": clause += $" > {c}"; parm = filter.Value; break;
            case "less or equal": clause += $" <= {c}"; parm = filter.Value; break;
            case "greater or equal": clause += $" >= {c}"; parm = filter.Value; break;
        }

        filter.Clause = clause;

        view.Parms += (!string.IsNullOrEmpty(view.Parms) ? "|" : "") + parm;
    }

    private void BuildSortClause(ViewSort sort)
    {
        string clause = sort.Column;
        clause += sort.Direction == "Ascending" ? " ASC " : " DESC ";
        sort.Clause = clause;
    }

    private bool ValidateQuery(View view)
    {
        try
        {
            var where = view.FilterClause;
            var orderBy = view.SortClause;

            var parms = view.Parms == null ? 
                             Array.Empty<object>() :
                             view.Parms.Split('|').Cast<object>().ToArray();

            if (view.What == "Student")
            {
                var sql = $"SELECT * FROM [Student] WHERE {where}"; // ORDER BY {orderBy}";
                var students = _db.Students.FromSqlRaw(sql, parms).AsNoTracking().ToList();
            }
            // else if ... // Other modules can follow

            return true;
        }
        catch (Exception ex)
        {
            string s = ex.ToString();
            return false;
        }
    }

    #endregion

    #region Mapping

    private class MapperProfile : BaseProfile
    {
        public MapperProfile()
        {
            CreateMap<View, Edit>();
            CreateMap<Edit, View>()
              .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<ViewFilter, Filter>();
            CreateMap<Filter, ViewFilter>();

            CreateMap<ViewSort, Sort>();
            CreateMap<Sort, ViewSort>();
        }
    }

    #endregion
}

