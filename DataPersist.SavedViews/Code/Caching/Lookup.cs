using DataPersist.SavedViews.Domain;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DataPersist.SavedViews
{
    // Generates 'fresh' dropdown display lists
    // 'SelectLists' are stateful and cannot be shared among users

    #region Interface

    public interface ILookup
    {
        List<SelectListItem> CountryItems { get; }
        List<SelectListItem> InstructorItems { get; }
        List<SelectListItem> DepartmentItems { get; }
        List<SelectListItem> StudentItems { get; }
        List<SelectListItem> CourseItems { get; }
        List<SelectListItem> ClassItems { get; }

        List<SelectListItem> GenderItems { get; }
        List<SelectListItem> StatusItems { get; }
        List<SelectListItem> FulltimeItems { get; }

        // Saved View related

        List<SelectListItem> OperatorItems { get; }
        List<SelectListItem> DirectionItems { get; }
        List<SelectListItem> ColumnItems(string table);

        Dictionary<string, Dictionary<string, Column>> Schema { get; }
    }

    #endregion

    public class Lookup : ILookup
    {
        #region Dependency Injection

        private readonly UniversityContext _db;
        private readonly ICache _cache;
        //private readonly ICurrentTenant _currentTenant;
        //private readonly IStringLocalizer<SharedResources> _localizer;

        public Lookup(UniversityContext db, ICache cache)
        {
            _db = db;
            _cache = cache;
        }

        #endregion

        #region Items

        // Selection list for countries

        public List<SelectListItem> CountryItems
        {
            // ** Lookup pattern

            get
            {
                var list = new List<SelectListItem>() { new(value: "", text: "-- Select --", selected: true) };

                foreach (var country in _cache.Countries.Values)
                    list.Add(new(value: country.Id.ToString(), text: country.Name));

                return list;
            }
        }

        public List<SelectListItem> DepartmentItems
        {
            get
            {
                // ** Lookup pattern

                var list = new List<SelectListItem>() { new(value: "", text: "-- Select --", selected: true) };

                foreach (var department in _cache.Departments.Values)
                    list.Add(new(value: department.Id.ToString(), text: department.Name));

                return list;
            }
        }

        public List<SelectListItem> InstructorItems
        {
            get
            {
                var list = new List<SelectListItem>() { new(value: "", text: "-- Select --", selected: true) };

                foreach (var instructor in _cache.Instructors.Values)
                    list.Add(new(value: instructor.Id.ToString(), text: instructor.FirstName + " " + instructor.LastName));

                return list;
            }
        }

        public List<SelectListItem> CourseItems
        {
            get
            {
                var list = new List<SelectListItem>() { new(value: "", text: "-- Select --", selected: true) };

                foreach (var course in _cache.Courses.Values)
                    list.Add(new(value: course.Id.ToString(), text: course.Title));

                return list;
            }
        }

        public List<SelectListItem> ClassItems
        {
            get
            {
                var list = new List<SelectListItem>() { new(value: "", text: "-- Select --", selected: true) };

                foreach (var clas in _cache.Classes.Values)
                    list.Add(new(value: clas.Id.ToString(), text: clas.StartDate.ToDate() + ": " + clas.Course));

                return list;
            }
        }

        public List<SelectListItem> StudentItems
        {
            get
            {
                var list = new List<SelectListItem>() { new(value: "", text: "-- Select --", selected: true) };

                foreach (var student in _cache.Students.Values)
                    list.Add(new(value: student.Id.ToString(), text: student.FullName));

                return list;
            }
        }

        // ** Check Constraint pattern.
        // We could potentially retrieve contraint values from the database

        public List<SelectListItem> GenderItems =>
                new()
                {
                    new(value: "", text: "-- Select --", selected: true),
                    new(value: "Male", text: "Male"),
                    new(value: "Female", text: "Female")
                };

        // ** Check Constraint pattern
        // We could potentially retrieve contraint values from the database

        public List<SelectListItem> StatusItems =>
               new()
               {
                   new(value: "", text: "-- Select --", selected: true),
                   new(value: "Pending", text: "Pending"),
                   new(value: "Paid", text: "Paid"),
                   new(value: "Canceled", text: "Canceled")
               };

        public List<SelectListItem> FulltimeItems =>
            new()
            {
                new(value: "", text: "-- Select --", selected: true),
                new(value: "Yes", text:"Yes"),
                new(value: "No", text:"No")
            };

        public List<SelectListItem> OperatorItems =>
           new()
           {
               new(value: "", text: "-- None --", selected: true),
               new(value: "equals", text: "equals"),
               new(value: "not equals to", text: "not equals to"),
               new(value: "starts with", text: "starts with"),
               new(value: "contains", text: "contains"),
               new(value: "does not contain", text: "does not contain"),
               new(value: "less than", text: "less than"),
               new(value: "greater than", text: "greater than"),
               new(value: "less or equal", text: "less or equal"),
               new(value: "greater or equal", text: "greater or equal")
           };

        public List<SelectListItem> DirectionItems =>
           new()
           {
               new(value: "", text: "-- None --", selected: true),
               new(value: "Ascending", text: "Ascending"),
               new(value: "Descending", text: "Descending")
           };

        public List<SelectListItem> ColumnItems(string table)
        {
            // Create selectable column list

            var cols = _cache.Schema[table].Values;

            var list = new SelectList(cols, "Name", "Name").ToList();
            list.Insert(0, new(value: "", text: "-- None --", selected: true));

            // Remove 'system' columns (in general: pk. fk, and audit fields).

            return list.Where(i => i.Text != "Id" && 
                                   i.Text != "CountryId" && 
                                   i.Text != "CreatedOn" &&
                                   i.Text != "CreatedBy" && 
                                   i.Text != "ChangedOn" && 
                                   i.Text != "ChangedBy").ToList();
        }

        public Dictionary<string, Dictionary<string, Column>> Schema => _cache.Schema;

        #endregion
    }
}