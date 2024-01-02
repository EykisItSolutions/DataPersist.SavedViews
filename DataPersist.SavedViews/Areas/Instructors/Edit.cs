using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews.Areas.Instructors;

// ** Action Model Design Pattern

public class Edit : BaseModel
{
    #region Data

    public int Id { get; set; }

    [Required(ErrorMessage = "Alias is required")]
    public string Alias { get; set; } = null!;

    [Required(ErrorMessage = "First Name is required")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Last Name is required")]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Is Fulltime is required")]

    public bool IsFulltime { get; set; }
    public string? HireDate { get; set; }

    public decimal Salary { get; set; }
    public string? FullName { get; set; }

    #endregion

    #region Handlers

    public override async Task<IActionResult> GetAsync()
    {
        var instructor = Id == 0 ?
                     new Instructor() :
                     await _db.Instructors.AsNoTracking().SingleAsync(s => s.Id == Id);

        _mapper.Map(instructor, this);

        return View(this);
    }

    public override async Task<IActionResult> PostAsync()
    {
        if (!ModelState.IsValid) return View(this);

        if (Id == 0)
        {
            var instructor = _mapper.Map<Instructor>(this);

            _db.Instructors.Add(instructor);

            await _db.SaveChangesAsync();

            await SettleInsertAsync(instructor);
        }
        else
        {
            var original = await _db.Instructors.AsNoTracking().SingleAsync(c => c.Id == Id);
            var instructor = await _db.Instructors.SingleAsync(c => c.Id == Id);

            _mapper.Map(this, instructor);

            _db.Instructors.Update(instructor);

            await _db.SaveChangesAsync();

            await SettleUpdateAsync(original, instructor);
        }

        return LocalRedirect(Referer);
    }

    #endregion

    #region Helpers

    private async Task SettleInsertAsync(Instructor instructor)
    {
        // ** Cache pattern

        _cache.Instructors.Add(instructor.Id, instructor);

        // ** Search pattern

        _search.UpdateInstructor(instructor);

        // ** Rollup pattern

        await _rollup.InstructorAsync(instructor);
    }

    private async Task SettleUpdateAsync(Instructor original, Instructor instructor)
    {
        // ** Cache pattern

        _cache.Instructors[instructor.Id] = instructor;

        // ** Search pattern

        _search.UpdateInstructor(instructor);

        // ** Rollup pattern

        await _rollup.InstructorAsync(instructor);

        // ** Log Data pattern

        if (original.FirstName != instructor.FirstName)
            _db.DataLogs.Add(new DataLog { What = "Instructor", Column = "FirstName", WhatId = instructor.Id, Name = instructor.FullName, LogDate = DateTime.Now,
                       OldValue = original.FirstName.ToLog(), NewValue = instructor.FirstName.ToLog(), UserId = _currentUser.Id });
       
        if (original.LastName != instructor.LastName)
            _db.DataLogs.Add(new DataLog { What = "Instructor", Column = "LastName", WhatId = instructor.Id, Name = instructor.FullName, LogDate = DateTime.Now,
                       OldValue = original.LastName.ToLog(), NewValue = instructor.LastName.ToLog(), UserId = _currentUser.Id });
        
        if (original.Email != instructor.Email)
            _db.DataLogs.Add(new DataLog { What = "Instructor", Column = "Email", WhatId = instructor.Id, Name = instructor.FullName, LogDate = DateTime.Now,
                       OldValue = original.Email.ToLog(), NewValue = instructor.Email.ToLog(), UserId = _currentUser.Id });
        
        if (original.Alias != instructor.Alias)
            _db.DataLogs.Add(new DataLog { What = "Instructor", Column = "Alias", WhatId = instructor.Id, Name = instructor.FullName, LogDate = DateTime.Now,
                       OldValue = original.Alias.ToLog(), NewValue = instructor.Alias.ToLog(), UserId = _currentUser.Id });
         
        if (original.IsFulltime != instructor.IsFulltime)
            _db.DataLogs.Add(new DataLog { What = "Instructor", Column = "IsFulltime", WhatId = instructor.Id, Name = instructor.FullName, LogDate = DateTime.Now,
                       OldValue = original.IsFulltime.ToLog(), NewValue = instructor.IsFulltime.ToLog(), UserId = _currentUser.Id });
    
         if (original.Salary != instructor.Salary)
            _db.DataLogs.Add(new DataLog { What = "Instructor", Column = "Salary", WhatId = instructor.Id, Name = instructor.FullName, LogDate = DateTime.Now,
                       OldValue = original.Salary.ToLog(), NewValue = instructor.Salary.ToLog(), UserId = _currentUser.Id });
    
        await _db.SaveChangesAsync();
    }

    #endregion

    #region Mapping

    public class MapperProfile : BaseProfile
    {
        public MapperProfile()
        {
            CreateMap<Instructor, Edit>()
             .ForMember(dest => dest.HireDate, opt => opt.MapFrom(src => src.HireDate.ToDate()));

            CreateMap<Edit, Instructor>();
        }
    }

    #endregion
}