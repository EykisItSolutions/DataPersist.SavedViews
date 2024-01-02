using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews.Areas.Students;

// ** Action Model Design Pattern

public class Detail : BaseModel
{
    #region Data

    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Alias { get; set; } = null!;

    public string? City { get; set; }
    public int? CountryId { get; set; }
    public string? Country { get; set; }
    public string? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public int TotalEnrollments { get; set; }

    // Related items

    public string Tab { get; set; } = "details";

    public List<_Enrollment> Enrollments { get; set; } = new();

    #endregion

    #region Handlers

    public override async Task<IActionResult> GetAsync()
    {
        // Query with a filtered include

        var student = await _db.Students.AsNoTracking()
                               .Include(e => e.Enrollments
                                              .Where(e => e.StudentId == Id)
                                              .OrderByDescending(e => e.Id)
                                              .Take(3))
                               .SingleAsync(s => s.Id == Id);

        _mapper.Map(student, this);

        await _viewedService.SaveAsync(Id, "Student", student.FullName);

        return View(this);
    }

    #endregion

    #region Helpers

    #endregion

    #region Mapping

    // ** Data Mapper Design Pattern

    public class MapperProfile : BaseProfile
    {
        public MapperProfile()
        {
            CreateMap<Student, Detail>()
              .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth.ToDate()));

            CreateMap<Enrollment, _Enrollment>()
              .ForMember(dest => dest.EnrollDate, opt => opt.MapFrom(src => src.EnrollDate.ToDate()))
              .ForMember(dest => dest.Fee, opt => opt.MapFrom(src => src.Fee.ToCurrency()))
              .ForMember(dest => dest.AmountPaid, opt => opt.MapFrom(src => src.AmountPaid.ToCurrency()));
        }
    }

    #endregion
}