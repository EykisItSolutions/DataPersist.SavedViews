namespace DataPersist.SavedViews.Areas.Instructors
{
    public class _Course
    {
        public int Id { get; set; }
        public string? CourseNumber { get; set; }
        public string Title { get; set; } = null!;
        public string? Department { get; set; }
        public string? Fee { get; set; }
        public int TotalClasses { get; set; }
    }
}