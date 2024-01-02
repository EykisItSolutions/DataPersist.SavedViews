namespace DataPersist.SavedViews.Areas.Students;

public class _Enrollment
{
    public int Id { get; set; }
    public string EnrollmentNumber { get; set; } = null!;
    public string EnrollDate { get; set; } = null!;
    public string Course { get; set; } = null!;
    public string Fee { get; set; } = null!;
    public string? AmountPaid { get; set; } = null!;
    public string? AvgGrade { get; set; }
    public int TotalQuizzes { get; set; }
}