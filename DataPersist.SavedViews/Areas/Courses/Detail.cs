using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews.Areas.Courses;

// ** Action Model Design Pattern

public class Detail : BaseModel
{
    #region Data

    public int Id { get; set; }
    public string? CourseNumber { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int DepartmentId { get; set; }
    public string? Department { get; set; }
    public int? InstructorId { get; set; }
    public string? Instructor { get; set; }
    public int NumDays { get; set; }
    public string Fee { get; set; } = null!;
    public int TotalClasses { get; set; }

    // Related List

    public string Tab { get; set; } = "details";

    public List<_Class> Classes { get; set; } = new();

    #endregion

    #region Handlers

    public override async Task<IActionResult> GetAsync()
    {
        // Query with a filtered include

        var course = await _db.Courses.AsNoTracking()
                              .Include(c => c.Classes
                                             .Where(c => c.CourseId == Id)
                                             .OrderByDescending(c => c.Id)
                                             .Take(3))
                              .SingleAsync(c => c.Id == Id);

        _mapper.Map(course, this);

        await _viewedService.SaveAsync(Id, "Course", course.Title);

        return View(this);
    }

    

    #endregion

    #region Mapping

    private class MapperProfile : BaseProfile
    {
        public MapperProfile()
        {
            CreateMap<Course, Detail>()
              .ForMember(dest => dest.Fee, opt => opt.MapFrom(src => src.Fee.ToCurrency()));

            CreateMap<Class, _Class>()
               .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.ToDate()))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.ToDate()));
        }
    }

    #endregion

}
