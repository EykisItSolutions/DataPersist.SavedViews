using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews.Areas.Courses;

// ** Action Model Design Pattern

public class Edit : BaseModel
{
    #region Data

    public int Id { get; set; }
    public string? CourseNumber { get; set; }

    [Required(ErrorMessage = "Title is required")]
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "Department is required")]
    public int DepartmentId { get; set; }

    public int? InstructorId { get; set; }

    [Required(ErrorMessage = "Fee is required")]
    public decimal Fee { get; set; }

    #endregion

    #region Handlers

    public override async Task<IActionResult> GetAsync()
    {
        var course = Id == 0 ?
                      new Course() :
                      await _db.Courses.AsNoTracking().SingleAsync(s => s.Id == Id);

        _mapper.Map(course, this);

        return View(this);
    }

    public override async Task<IActionResult> PostAsync()
    {
        if (!ModelState.IsValid) return View(this);

        if (Id == 0)
        {
            var course = _mapper.Map<Course>(this);

            // ** Unit of work Design Pattern

            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                _db.Courses.Add(course);
                await _db.SaveChangesAsync();

                course.CourseNumber = string.Format("{0:CRS-00000}", course.Id);
                _db.Courses.Update(course);
                await _db.SaveChangesAsync();

                await transaction.CommitAsync();
            }

            await SettleInsertAsync(course);
        }
        else
        {
            var original = await _db.Courses.AsNoTracking().SingleAsync(c => c.Id == Id);
            var course = await _db.Courses.SingleAsync(c => c.Id == Id);

            _mapper.Map(this, course);

            _db.Courses.Update(course);

            await _db.SaveChangesAsync();

            await SettleUpdateAsync(original, course);
        }

        return LocalRedirect(Referer);
    }

    #endregion

    #region Helpers

    private async Task SettleInsertAsync(Course course)
    {
        // ** Cache pattern

        _cache.Courses.Add(course.Id, course);

        // ** Search pattern

        _search.UpdateCourse(course);

        // ** Rollup pattern

        await _rollup.CourseAsync(course);
    }

    private async Task SettleUpdateAsync(Course original, Course course)
    {
        // ** Cache pattern

        _cache.Courses[course.Id] = course;

        // ** Search pattern

        _search.UpdateCourse(course);

        // ** Rollup pattern

        await _rollup.CourseAsync(course);

        // ** Log Data pattern

        if (original.Title != course.Title)
            _db.DataLogs.Add(new DataLog { What = "Course", Column = "Title", WhatId = course.Id, Name = course.Title, LogDate = DateTime.Now,
                       OldValue = original.Title.ToLog(), NewValue = course.Title.ToLog(), UserId = _currentUser.Id });
       
        if (original.Description != course.Description)
            _db.DataLogs.Add(new DataLog { What = "Course", Column = "Description",  WhatId = course.Id, Name = course.Title, LogDate = DateTime.Now,
                       OldValue = original.Description.ToLog(), NewValue = course.Description.ToLog(), UserId = _currentUser.Id });
        
        if (original.DepartmentId != course.DepartmentId)
            _db.DataLogs.Add(new DataLog { What = "Course", Column = "DepartmentId", WhatId = course.Id, Name = course.Title, LogDate = DateTime.Now,
                       OldValue = original.DepartmentId.ToLog(), NewValue = course.DepartmentId.ToLog(), UserId = _currentUser.Id });
        
        if (original.InstructorId != course.InstructorId)
            _db.DataLogs.Add(new DataLog { What = "Course", Column = "InstructorId", WhatId = course.Id, Name = course.Title, LogDate = DateTime.Now,
                       OldValue = original.InstructorId.ToLog(), NewValue = course.InstructorId.ToLog(), UserId = _currentUser.Id });
        
        if (original.NumDays != course.NumDays)
            _db.DataLogs.Add(new DataLog { What = "Course", Column = "NumDays", WhatId = course.Id, Name = course.Title, LogDate = DateTime.Now,
                       OldValue = original.NumDays.ToLog(), NewValue = course.NumDays.ToLog(), UserId = _currentUser.Id });
        
        if (original.Fee != course.Fee)
            _db.DataLogs.Add(new DataLog { What = "Course", Column = "Fee", WhatId = course.Id, Name = course.Title, LogDate = DateTime.Now,
                       OldValue = original.Fee.ToLog(), NewValue = course.Fee.ToLog(), UserId = _currentUser.Id });
    
        await _db.SaveChangesAsync();
    }

    #endregion

    #region Mapping

    private class MapperProfile : BaseProfile
    {
        public MapperProfile()
        {
            CreateMap<Course, Edit>();

            CreateMap<Edit, Course>()
              .ForMember(dest => dest.Department, opt => opt.MapFrom(src => _cache.Departments[(int)src.DepartmentId!].Name))
              .ForMember(dest => dest.Instructor, opt => opt.MapFrom(src => src.InstructorId == null || src.InstructorId == 0 ? null : _cache.Instructors[(int)src.InstructorId!].FullName))
              .ForMember(dest => dest.CourseNumber, opt => opt.Ignore());
        }
    }

    #endregion
}
