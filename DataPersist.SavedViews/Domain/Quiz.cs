using System;
using System.Collections.Generic;

namespace DataPersist.SavedViews.Domain
{
    public partial class Quiz
    {
        public int Id { get; set; }
        public string QuizNumber { get; set; } = null!;
        public int EnrollmentId { get; set; }
        public string? Enrollment { get; set; }
        public DateTime QuizDate { get; set; }
        public decimal Grade { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ChangedOn { get; set; }
        public int? ChangedBy { get; set; }

        public virtual Enrollment EnrollmentNavigation { get; set; } = null!;
    }
}
