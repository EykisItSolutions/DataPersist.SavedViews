using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews.Areas.Enrollments;

// ** Action Model Design Pattern

public class Edit : BaseModel
{
    #region Data

    public int Id { get; set; }
    public string? EnrollmentNumber { get; set; }

    [Required(ErrorMessage = "Student is required.")]
    public int StudentId { get; set; }

    [Required(ErrorMessage = "Class is required.")]
    public int ClassId { get; set; }

    [Required(ErrorMessage = "Enroll Date is required.")]
    public string? EnrollDate { get; set; }

    [Range(0, 50000, ErrorMessage = "Please enter a numeric value")]
    public decimal AmountPaid { get; set; }

    public string Status { get; set; } = null!;

    #endregion

    #region Handlers

    public override async Task<IActionResult> GetAsync()
    {
        var enrollment = Id == 0 ? new Enrollment { EnrollDate = DateTime.Now } :
                                   await _db.Enrollments.SingleAsync(c => c.Id == Id);

        _mapper.Map(enrollment, this);

        return View(this);
    }

    public override async Task<IActionResult> PostAsync()
    {
        if (!ModelState.IsValid) return View(this);

        if (Id == 0) // new enrollment
        {
            // student can enroll only once for a given class

            int enrollments = _db.Enrollments.Count(e => e.StudentId == StudentId &&
                                                         e.ClassId == ClassId);
            if (enrollments > 0)
            {
                ModelState.AddModelError("StudentId", "Student is already enrolled in this class");
                return View(this);
            }

            var enrollment = new Enrollment();

            _mapper.Map(this, enrollment);

            // ** Unit of work Design Pattern

            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                _db.Enrollments.Add(enrollment);
                await _db.SaveChangesAsync();

                enrollment.EnrollmentNumber = string.Format("{0:ENR-00000}", enrollment.Id);
                _db.Enrollments.Update(enrollment);
                
                await _db.SaveChangesAsync();

                await transaction.CommitAsync();
            }

            await SettleInsertAsync(enrollment);
        }
        else
        {
            var enrollment = await _db.Enrollments.SingleAsync(e => e.Id == Id);
            
            await _historyService.SaveAsync(Id, enrollment.GetType(), enrollment.EnrollmentNumber, "UPDATE");

            _mapper.Map(this, enrollment);

            _db.Enrollments.Update(enrollment);

            await _db.SaveChangesAsync();

            await SettleUpdateAsync(enrollment);
        }

        return LocalRedirect(Referer);
    }

    #endregion

    #region Helpers

    private async Task SettleInsertAsync(Enrollment enrollment)
    {
        // ** Search pattern

        _search.UpdateEnrollment(enrollment);

        // ** Rollup pattern

        await _rollup.EnrollmentAsync(enrollment);
    }

    private async Task SettleUpdateAsync(Enrollment enrollment)
    {
        // ** Search pattern

        _search.UpdateEnrollment(enrollment);

        // ** Rollup pattern

        await _rollup.EnrollmentAsync(enrollment);
    }

    #endregion

    #region Mapping

    private class MapperProfile : BaseProfile
    {
        public MapperProfile()
        {
            CreateMap<Enrollment, Edit>()
              .ForMember(dest => dest.EnrollDate, opt => opt.MapFrom(src => src.EnrollDate.ToDate()));
              
            CreateMap<Edit, Enrollment>()
              .ForMember(dest => dest.Student, opt => opt.MapFrom(src => _cache.Students[src.StudentId!].FullName))
              .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => _cache.Classes[src.ClassId!].CourseId))
              .ForMember(dest => dest.Course, opt => opt.MapFrom(src => _cache.Classes[src.ClassId!].Course))
              .ForMember(dest => dest.Fee, opt => opt.MapFrom(src => _cache.Courses[_cache.Classes[src.ClassId!].CourseId].Fee))
              .ForMember(dest => dest.EnrollDate, opt => opt.MapFrom(src => DateTime.Parse(src.EnrollDate!)))
              .ForMember(dest => dest.EnrollmentNumber, opt => opt.Ignore());
        }
    }

    #endregion
}