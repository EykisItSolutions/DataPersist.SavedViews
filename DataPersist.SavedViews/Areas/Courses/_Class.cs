
namespace DataPersist.SavedViews.Areas.Courses;

public record _Class
{
    public int Id { get; set; }
    public string? ClassNumber { get; set; }
    public string Location { get; set; } = null!;
    public string StartDate { get; set; } = null!;
    public string EndDate { get; set; } = null!;

    public int MaxEnrollments { get; set; }
    public int TotalEnrollments { get; set; }
}
