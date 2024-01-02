
namespace DataPersist.SavedViews.Areas.Enrollments;

public class _Quiz
{
    public int Id { get; set; }
    public int EnrollmentId { get; set; }
    public string QuizNumber { get; set; } = null!;
    public string QuizDate { get; set; } = null!;
    public string Grade { get; set; } = null!;
}
