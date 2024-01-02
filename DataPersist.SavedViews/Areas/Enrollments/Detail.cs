using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews.Areas.Enrollments;

// ** Action Model Design Pattern

public class Detail : BaseModel
{
    #region Data

    public int Id { get; set; }
    public string EnrollmentNumber { get; set; } = null!;
    public int StudentId { get; set; }
    public string Student { get; set; } = null!;
    public int ClassId { get; set; }
    public string Class { get; set; } = null!;
    public int CourseId { get; set; }
    public string Course { get; set; } = null!;

    public string EnrollDate { get; set; } = null!;
    public string AmountPaid { get; set; } = null!;
    public string Status { get; set; } = null!;
    public int TotalQuizzes { get; set; }
    public string AvgGrade { get; set; } = null!;
    public string Fee { get; set; } = null!;
    public int NumDays { get; set; }

    // Related list

    public string Tab { get; set; } = "details";

    public List<_Quiz> Quizzes { get; set; } = new();

    // Helper fields for adding quizzes

    [Required(ErrorMessage = "Quizdate is required")]
    public string? QuizDate { get; set; }
    [Required(ErrorMessage = "Grade is required")]
    public decimal Grade { get; set; }

    #endregion
   
    #region Handlers

    public override async Task<IActionResult> GetAsync()
    {
        // Query with a filtered include

        var enrollment = await _db.Enrollments.AsNoTracking()
                                  .Include(e => e.Quizzes         
                                                 .Where(e => e.EnrollmentId == Id)
                                                 .OrderByDescending(e => e.Id))
                                  .SingleAsync(s => s.Id == Id);

        _mapper.Map(enrollment, this);

        await _viewedService.SaveAsync(Id, "Enrollment", enrollment.EnrollmentNumber);

        return View(this);
    }

    #endregion

    #region Mapping

    private class MapperProfile : BaseProfile
    {
        public MapperProfile()
        {
            CreateMap<Enrollment, Detail>()
              .ForMember(dest => dest.Student, opt => opt.MapFrom(src => _cache.Students[src.StudentId].FullName))
              .ForMember(dest => dest.EnrollDate, opt => opt.MapFrom(src => src.EnrollDate.ToDate()))
              .ForMember(dest => dest.AvgGrade, opt => opt.MapFrom(src => src.AvgGrade == null ? "n/a" : src.AvgGrade.ToGrade()))
              .ForMember(dest => dest.Fee, opt => opt.MapFrom(src => src.Fee.ToCurrency()))
              .ForMember(dest => dest.AmountPaid, opt => opt.MapFrom(src => src.AmountPaid.ToCurrency()));

            CreateMap<Quiz, _Quiz>()
              .ForMember(dest => dest.QuizDate, opt => opt.MapFrom(src => src.QuizDate.ToDate()))
              .ForMember(dest => dest.Grade, opt => opt.MapFrom(src => src.Grade.ToGrade()));
        }
    }

    #endregion
}