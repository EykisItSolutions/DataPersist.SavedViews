using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews.Areas.Students;

// ** Action Model Design Pattern

public class Edit : BaseModel
{
    #region Data

    public int Id { get; set; }

    [Required(ErrorMessage = "Alias is required")]
    [StringLength(10, ErrorMessage = "Max length is 10 characters")]
    public string Alias { get; set; } = null!;

    [Required(ErrorMessage = "First Name is required")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Last Name is required")]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "City is required")]
    public string City { get; set; } = null!;

    [Required(ErrorMessage = "Country is required")]
    public int CountryId { get; set; }

    public string? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? FullName { get; set; }

    #endregion

    #region Handlers

    public override async Task<IActionResult> GetAsync()
    {
        var student = Id == 0 ?
                      new Student() :
                      await _db.Students.AsNoTracking().SingleAsync(s => s.Id == Id);

        _mapper.Map(student, this);

        return View(this);
    }

    public override async Task<IActionResult> PostAsync()
    {
        if (!ModelState.IsValid) return View(this);

        if (Id == 0)
        {
            var student = _mapper.Map<Student>(this);

            _db.Students.Add(student);

            await _db.SaveChangesAsync();

            SettleInsert(student);
        }
        else
        {
            var student = await _db.Students.SingleAsync(c => c.Id == Id);

            // ** History pattern

            await _historyService.SaveAsync(Id, student.GetType(), student.FullName, "UPDATE");

            _mapper.Map(this, student);

            _db.Students.Update(student);

            await _db.SaveChangesAsync();

            await SettleUpdateAsync(student);
        }

        return LocalRedirect(Referer);
    }

    #endregion

    #region Helpers

    private void SettleInsert(Student student)
    {
        // ** Cache pattern

        _cache.Students.Add(student.Id, student);

        // ** Search pattern

        _search.UpdateStudent(student);
    }

    private async Task SettleUpdateAsync(Student student)
    {
        // ** Cache pattern

        _cache.Students[student.Id] = student;

        // ** Search pattern

        _search.UpdateStudent(student);

        // ** Rollup pattern

        await _rollup.StudentAsync(student);
    }

    #endregion

    #region Mapping

    private class MapperProfile : BaseProfile
    {
        public MapperProfile()
        {
            CreateMap<Student, Edit>()
             .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth.ToDate()));

            CreateMap<Edit, Student>()
             .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.CountryId > 0 ?
                                                           _cache.Countries[src.CountryId].Name : null));
        }
    }

    #endregion
}
