using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews.Areas.Admin;

public class Histories : PagedModel<History>
{
    #region Data

    public int Id { get; set; } // The Undo Id

    #endregion

    #region Handlers

    public override async Task<IActionResult> GetAsync()
    {
        var query = _db.Histories.Where(h => h.UndoDate == null)
                       .OrderByDescending(h => h.Id).AsNoTracking().AsQueryable();

        TotalRows = await query.CountAsync();
        var items = await query.Skip(Skip).Take(Take).ToListAsync();

        _mapper.Map(items, Items);

        return View(this);
    }

    public override async Task<IActionResult> PostAsync()
    {
        // Reverse history

        var history = await _db.Histories.SingleAsync(h => h.Id == Id);

        if (history.Operation == "UPDATE")
        {
            switch (history.What)
            {
                case "Student":

                    var student = JsonSerializer.Deserialize<Student>(history.Content!)!;
                    
                    _db.Students.Attach(student);
                    
                    _db.Entry<Student>(student).State = EntityState.Modified;
                    
                    await _db.SaveChangesAsync();

                    SettleReverseUpdate(student);

                    break;

                case "Class":

                    var clas = JsonSerializer.Deserialize<Class>(history.Content!)!;
                    
                    _db.Classes.Attach(clas);
                    
                    _db.Entry<Class>(clas).State = EntityState.Modified;
                    
                    await _db.SaveChangesAsync();

                    SettleReverseUpdate(clas);

                    break;
            }

            
        }
        else // if (history.Operation == "DELETE")
        {
            switch (history.What)
            {
                case "Student":

                    using (var transaction = _db.Database.BeginTransaction())
                    {
                        var student = JsonSerializer.Deserialize<Student>(history.Content!)!;
                        _db.Students.Add(student);
                        
                        await _db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Student ON;");
                        await _db.SaveChangesAsync();
                        await _db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Student OFF");
                        
                        transaction.Commit();
                        
                        SettleReverseDelete(student);
                    }

                    break;

                case "Class":

                    using (var transaction = _db.Database.BeginTransaction())
                    {
                        var clas = JsonSerializer.Deserialize<Class>(history.Content!)!;
                        _db.Classes.Add(clas);
                        
                        await _db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Class ON;");
                        await _db.SaveChangesAsync();
                        await _db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Class OFF");
                        
                        transaction.Commit();
                        
                        SettleReverseDelete(clas);
                    }


                    break;
            }
        }

        history.UndoDate = DateTime.Now;
        history.UndoBy = _currentUser.Id;

        _db.Histories.Update(history);
        await _db.SaveChangesAsync();

        Success = "Reversed"; 

        return View(this);
        
    }

    #endregion

    #region Helpers

    private void SettleReverseUpdate(Student student)
    {
        // ** Cache pattern

        _cache.Students[student.Id] = student;

        // ** Search pattern

        _search.UpdateStudent(student);
    }

    private void SettleReverseDelete(Student student)
    {
        // ** Cache pattern

        _cache.Students[student.Id] = student;

        // ** Search pattern

        _search.UpdateStudent(student);
    }

    private void SettleReverseUpdate(Class clas)
    {
        // ** Cache pattern

        _cache.Classes[clas.Id] = clas;

        // ** Search pattern

        _search.UpdateClass(clas);
    }

    private void SettleReverseDelete(Class clas)
    {
        // ** Cache pattern

        _cache.Classes[clas.Id] = clas;

        // ** Search pattern

        _search.UpdateClass(clas);
    }

    #endregion

    #region Mapping

    public class MapperProfile : BaseProfile
    {
        public MapperProfile()
        {
            CreateMap<Domain.History, History>()
              .ForMember(dest => dest.HistoryDate, opt => opt.MapFrom(src => src.HistoryDate.ToDate()))
              .ForMember(dest => dest.User, opt => opt.MapFrom(src => _cache.Users[(int)src.UserId!].FullName))
              .ForMember(dest => dest.Txn, opt => opt.MapFrom(src => src.Txn == null ? "None" : src.Txn.ToString()));
        }
    }

    #endregion
}

public class History
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string User { get; set; } = null!;
    public string HistoryDate { get; set; } = null!;
    public string? Txn { get; set; }
    public string What { get; set; } = null!;
    public int WhatId { get; set; }
    public string Name { get; set; } = null!;
    public string Operation { get; set; } = null!;
    public string? Content { get; set; }
}
