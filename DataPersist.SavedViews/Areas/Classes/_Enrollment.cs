
namespace DataPersist.SavedViews.Areas.Classes;

public class _Enrollment
{
    public int Id { get; set; }
    public string? EnrollmentNumber { get; set; }
    public int? CourseId { get; set; }
    public int StudentId { get; set; }
    public string Student { get; set; } = null!;
    public string? EnrollDate { get; set; }
    public string? AmountPaid { get; set; }
    public string Status { get; set; } = null!;
    public string? Fee { get; set; }
    public int TotalQuizzes { get; set; }
}
