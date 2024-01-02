using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;

namespace DataPersist.SavedViews.Areas.Admin;

public class RecycleBin : PagedModel<Bin>
{
    #region Static

    private static readonly string sqlCourse =
        @"SELECT Id AS WhatId, 'Course' AS What, Title AS Name, DeletedOn, DeletedBy 
            FROM Course 
           WHERE IsDeleted = 1";

    private static readonly string sqlInstructor =
        @"SELECT Id AS WhatId, 'Instructor' AS What, FirstName + ' ' + LastName AS Name, DeletedOn, DeletedBy 
            FROM Instructor 
           WHERE IsDeleted = 1";

    #endregion

    #region Data

    public int WhatId { get; set; }   // The Id of What to be undeleted
    public string? What { get; set; } // Entity that iss undeleted

    #endregion

    #region Handlers

    public override async Task<IActionResult> GetAsync()
    {
        var sql = What switch
        {
            "Course" => sqlCourse,
            "Instructor" => sqlInstructor,
            _ => @$"{sqlCourse} UNION {sqlInstructor}"
        };

        using var connection = new SqlConnection(_db.Database.GetConnectionString());

        var items = (await connection.QueryAsync<Bin>(sql)).ToList();

        _mapper.Map(items, Items);

        return View(this);
    }

    public override async Task<IActionResult> PostAsync()
    {
        if (What == "Course")
            await UndeleteCourseAsync();
        else if (What == "Instructor")
            await UndeleteInstructorAsync();

        Success = "Undeleted";

        return LocalRedirect(Referer); // ?? $"/admin/recyclebin?Filter={What}");
    }

    #endregion

    #region Helpers

    private async Task UndeleteCourseAsync()
    {
        var course = await _db.Courses.IgnoreQueryFilters()
                              .SingleAsync(c => c.Id == WhatId);

        course.IsDeleted = false;
        course.DeletedBy = null;

        _db.Courses.Update(course);

        await _db.SaveChangesAsync();
    }

    private async Task UndeleteInstructorAsync()
    {
        var instructor = await _db.Instructors.IgnoreQueryFilters()
                                  .SingleAsync(i => i.Id == WhatId);

        instructor.IsDeleted = false;
        instructor.DeletedBy = null;

        _db.Instructors.Update(instructor);

        await _db.SaveChangesAsync();
    }
   
    #endregion

    #region Mapping

    public class MapperProfile : BaseProfile
    {
        public MapperProfile()
        {
            CreateMap<Bin, Bin>()
              .ForMember(dest => dest.DeletedOnDate, opt => opt.MapFrom(src => src.DeletedOn.ToDate()))
              .ForMember(dest => dest.DeletedByName, opt => opt.MapFrom(src => (src.DeletedBy == null || src.DeletedBy < 0) ? "" : _cache.Users[(int)src.DeletedBy].FullName));
        }
    }

    #endregion
}

public class Bin
{
    public int WhatId { get; set; }
    public string What { get; set; } = null!;
    public string Name { get; set; } = null!;

    public DateTime? DeletedOn { get; set; }
    public int? DeletedBy { get; set; }

    public string? DeletedOnDate { get; set; }
    public string? DeletedByName { get; set; }
}
