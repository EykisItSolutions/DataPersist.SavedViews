using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews.Areas.Instructors;

// ** Action Model Design Pattern

public class Detail : BaseModel
{
    #region Data

    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string Alias { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;

    public string? HireDate { get; set; }
    public bool IsFulltime { get; set; }
    public string? Salary { get; set; }
    public int TotalCourses { get; set; }
    public string? FullName { get; set; }

    // Related list

    public string? Tab { get; set; } = "details";

    public List<_Course> Courses { get; set; } = new();

    #endregion

    #region Handlers

    public override async Task<IActionResult> GetAsync()
    {
        // Query with a filtered include

        var instructor = await _db.Instructors.AsNoTracking()
                                  .Include(i => i.Courses
                                                 .Where(c => c.InstructorId == Id)
                                                 .OrderByDescending(c => c.Id)
                                                 .Take(3))
                                  .SingleAsync(c => c.Id == Id);
       

        _mapper.Map(instructor, this);

        await _viewedService.SaveAsync(Id, "Instructor", instructor.FullName);

        return View(this);
    }

    #endregion

    #region Mapping

    // ** Data Mapper Design Pattern

    public class MapperProfile : BaseProfile
    {
        public MapperProfile()
        {
            CreateMap<Instructor, Detail>()
             .ForMember(dest => dest.HireDate, opt => opt.MapFrom(src => src.HireDate.ToDate()))
             .ForMember(dest => dest.Salary, opt => opt.MapFrom(src => src.Salary.ToCurrency()));

            CreateMap<Course, _Course>()
               .ForMember(dest => dest.Fee, opt => opt.MapFrom(src => src.Fee.ToCurrency()));

            //CreateMap<Instructor, Detail>();
            ////.ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth.ToDate()))
            ////.ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.CountryId != null ? _cache.Countries[(int)src.CountryId].Name : null));

            //CreateMap<Course, _Course>();
            //.ForMember(dest => dest.EnrollDate, opt => opt.MapFrom(src => src.EnrollDate.ToDate()))
            //.ForMember(dest => dest.Fee, opt => opt.MapFrom(src => src.Fee.ToCurrency()))
            //.ForMember(dest => dest.AmountPaid, opt => opt.MapFrom(src => src.AmountPaid.ToCurrency()));
            //CreateMap<Student, Edit>()
            //   .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate.ToDate()));

            //CreateMap<Edit, Student>()
            //   .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.CountryId > 0 ?
            //                                             _cache.Countries[src.CountryId].Name : null))
            //   .Ignore(dest => dest.CountryNavigation)
            //   .Ignore(dest => dest.Enrollments);

            //CreateMap<Enrollment, _Enrollment>()
            //   .ForMember(dest => dest.EnrollDate, opt => opt.MapFrom(src => src.EnrollDate.ToDate()))
            //   .ForMember(dest => dest.Fee, opt => opt.MapFrom(src => src.Fee.ToCurrency()));
            //   .ForMember(dest => dest.AmountPaid, opt => opt.MapFrom(src => src.AmountPaid.ToCurrency()));

        }
    }

    #endregion
}