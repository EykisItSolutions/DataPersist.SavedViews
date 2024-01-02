using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews.Areas.Classes;

// ** Action Model Design Pattern

public class Edit : BaseModel
{
    #region Data

    public int Id { get; set; }
    public string? ClassNumber { get; set; }

    [Required(ErrorMessage = "Course is required")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Location is required")]
    public string Location { get; set; } = null!;

    [Required(ErrorMessage = "Start Date is required")]
    public string StartDate { get; set; } = null!;

    [Required(ErrorMessage = "End Date is required")]
    public string EndDate { get; set; } = null!;

    public int MaxEnrollments { get; set; }

    #endregion

    #region Handlers

    public override async Task<IActionResult> GetAsync()
    {
        var clas = Id == 0 ?
                   new Class() :
                   await _db.Classes.AsNoTracking().SingleAsync(s => s.Id == Id);

        _mapper.Map(clas, this);

        return View(this);
    }

    public override async Task<IActionResult> PostAsync()
    {
        if (!ModelState.IsValid) return View(this);
        
        if (Id == 0)
        {
            var clas = _mapper.Map<Class>(this);

            // ** Unit of Work Design Pattern

            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                _db.Classes.Add(clas);
                await _db.SaveChangesAsync();

                clas.ClassNumber = string.Format("{0:CLS-00000}", clas.Id);
                _db.Classes.Update(clas);

                await _db.SaveChangesAsync();

                await transaction.CommitAsync();
            }

            await SettleInsertAsync(clas);
        }
        else
        {
            var clas = await _db.Classes.SingleAsync(c => c.Id == Id);

            // ** History pattern

            await _historyService.SaveAsync(Id, clas.GetType(), clas.ClassNumber, "UPDATE");

            _mapper.Map(this, clas);

            _db.Classes.Update(clas);

            await _db.SaveChangesAsync();

            await SettleUpdateAsync(clas);
        }

        return LocalRedirect(Referer);
    }

    #endregion

    #region Helpers

    private async Task SettleInsertAsync(Class clas)
    {
        // ** Cache pattern

        _cache.Classes.Add(clas.Id, clas);

        // ** Search pattern

        _search.UpdateClass(clas);

        // ** Rollup pattern

        await _rollup.ClassAsync(clas);
    }

    private async Task SettleUpdateAsync(Class clas)
    {
        // ** Cache pattern

        _cache.Classes[clas.Id] = clas;

        // ** Search pattern

        _search.UpdateClass(clas);

        // ** Rollup pattern

        await _rollup.ClassAsync(clas);
    }

    #endregion

    #region Mapping

    private class MapperProfile : BaseProfile
    {
        public MapperProfile()
        {
            CreateMap<Class, Edit>()
             .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.ToDate()))
             .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.ToDate()));

            CreateMap<Edit, Class>()
             .ForMember(dest => dest.Course, opt => opt.MapFrom(src => _cache.Courses[(int)src.CourseId!].Title))
             .ForMember(dest => dest.ClassNumber, opt => opt.Ignore());
        }
    }

    #endregion
}
