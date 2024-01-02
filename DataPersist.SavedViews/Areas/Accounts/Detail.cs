
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews.Areas.Accounts;

public class Detail : BaseModel
{
    #region Data

    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Image { get; set; } = null!;

    #endregion

    #region Handlers

    public override async Task<IActionResult> GetAsync()
    {
        var user = await _db.Users.SingleAsync(c => c.Id == _currentUser.Id);
        
        _mapper.Map(user, this);

        return View(this);
    }

    #endregion

    #region Mapping

    // ** Data Mapper Design Pattern

    public class MapperProfile : BaseProfile
    {
        public MapperProfile()
        {
            CreateMap<User, Detail>();
        }
    }

    #endregion
}