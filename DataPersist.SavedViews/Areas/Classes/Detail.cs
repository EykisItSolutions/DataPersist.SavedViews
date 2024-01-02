using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews.Areas.Classes;

public class Detail : BaseModel
{
    #region Data

    public int Id { get; set; }
    public string ClassNumber { get; set; } = null!;
    public int CourseId { get; set; }
    public string Course { get; set; } = null!;
    public string Location { get; set; } = null!;
    public string StartDate { get; set; } = null!;
    public string EndDate { get; set; } = null!;
    public int MaxEnrollments { get; set; }
    public int TotalEnrollments { get; set; }

    // Related List

    public string Tab { get; set; } = "details";

    public List<_Enrollment> Enrollments { get; set; } = new();

    #endregion

    #region Handlers

    public override async Task<IActionResult> GetAsync()
    {
        // Query with a filtered include

        var clas = await _db.Classes.AsNoTracking()
                            .Include(c => c.Enrollments
                                           .Where(e => e.ClassId == Id)
                                           .OrderByDescending(c => c.Id)
                                           .Take(3))
                            .SingleAsync(c => c.Id == Id);

        _mapper.Map(clas, this);
        
        await _viewedService.SaveAsync(Id, "Class", clas.ClassNumber);

        return View(this);
    }

    #endregion

    #region Mapping

    private class MapperProfile : BaseProfile
    {
        public MapperProfile()
        {
            CreateMap<Class, Detail>()
              .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.ToDate()))
              .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.ToDate()));

            CreateMap<Enrollment, _Enrollment>()
              .ForMember(dest => dest.EnrollDate, opt => opt.MapFrom(src => src.EnrollDate.ToDate()))
              .ForMember(dest => dest.Fee, opt => opt.MapFrom(src => src.Fee.ToCurrency()))
              .ForMember(dest => dest.AmountPaid, opt => opt.MapFrom(src => src.AmountPaid.ToCurrency()));
        }
    }

    #endregion
}
