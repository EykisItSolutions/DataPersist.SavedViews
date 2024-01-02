using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews.Areas.Admin;

public class Settings : BaseModel
{
    #region Data

    public List<_Setting> _Settings { get; set; } = new();

    #endregion

    #region Handlers

    public override async Task<IActionResult> GetAsync()
    {
        var settings = await _db.Settings.OrderBy(s => s.Name).ToListAsync();

        _mapper.Map(settings, _Settings);

        return View(this);
    }

    public override async Task<IActionResult> PostAsync()
    {
        var dictionary = await _db.Settings.OrderBy(s => s.Name).ToDictionaryAsync(s => s.Id);

        foreach (var setting in _Settings)
        {
            var logActivity = false;

            if (setting.Id > 0) // existing setting
            {
                var original = dictionary[setting.Id];
                
                if (original.Value != setting.Value) 
                    logActivity = true;
                
                // ** Name Value pattern

                original.Name = setting.Name;
                original.Value = setting.Value;
                original.Description = setting.Description;
                original.LastChangeDate = DateTime.Now;

                _db.Settings.Update(original);
                await _db.SaveChangesAsync();

                // ** Log Activity pattern

                if (logActivity)
                   await _activityService.SaveAsync($"Changed Setting: {setting.Name} = {setting.Value}");
            }
            else // new setting
            {
                var newSetting = new Setting
                {
                    Name = setting.Name,
                    Value = setting.Value,
                    Description = setting.Description
                };

                _db.Settings.Add(newSetting);
                await _db.SaveChangesAsync();

                // ** Log Activity pattern

                await _activityService.SaveAsync($"Added Setting: {setting.Name} = {setting.Value}");
            }
        }

        Success = "Saved";

        return View(this);
    }
   
    #endregion

    #region Mapping

    private class MapperProfile : BaseProfile
    {
        public MapperProfile()
        {
            CreateMap<Domain.Setting, _Setting>()
               .ForMember(dest => dest.LastChangeDate, opt => opt.MapFrom(src => src.LastChangeDate.ToDate()));
            CreateMap<_Setting, Domain.Setting>();

        }
    }

    #endregion
}

public class _Setting
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Value { get; set; } = null!;
    public string? Description { get; set; }
    public string? LastChangeDate { get; set; }
}