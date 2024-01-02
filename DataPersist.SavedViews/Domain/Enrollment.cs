using System;
using System.Collections.Generic;

namespace DataPersist.SavedViews.Domain
{
    public partial class Enrollment
    {
        public Enrollment()
        {
            Quizzes = new HashSet<Quiz>();
        }

        public int Id { get; set; }
        public string EnrollmentNumber { get; set; } = null!;
        public int StudentId { get; set; }
        public string? Student { get; set; }
        public int ClassId { get; set; }
        public string? Class { get; set; }
        public int? CourseId { get; set; }
        public string? Course { get; set; }
        public DateTime EnrollDate { get; set; }
        public decimal Fee { get; set; }
        public decimal AmountPaid { get; set; }
        public string Status { get; set; } = null!;
        public decimal? AvgGrade { get; set; }
        public int TotalQuizzes { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ChangedOn { get; set; }
        public int? ChangedBy { get; set; }

        public virtual Class ClassNavigation { get; set; } = null!;
        public virtual Student StudentNavigation { get; set; } = null!;
        public virtual ICollection<Quiz> Quizzes { get; set; }
    }
}
