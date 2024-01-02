using System;
using System.Collections.Generic;

namespace DataPersist.SavedViews.Domain
{
    public partial class Class
    {
        public Class()
        {
            Enrollments = new HashSet<Enrollment>();
        }

        public int Id { get; set; }
        public string ClassNumber { get; set; } = null!;
        public int CourseId { get; set; }
        public string? Course { get; set; }
        public string Location { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MaxEnrollments { get; set; }
        public int TotalEnrollments { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ChangedOn { get; set; }
        public int? ChangedBy { get; set; }

        public virtual Course CourseNavigation { get; set; } = null!;
        public virtual ICollection<Enrollment> Enrollments { get; set; }
    }
}
