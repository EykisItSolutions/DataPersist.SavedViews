using System;
using System.Collections.Generic;

namespace DataPersist.SavedViews.Domain
{
    public partial class Course
    {
        public Course()
        {
            Classes = new HashSet<Class>();
        }

        public int Id { get; set; }
        public string? CourseNumber { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int DepartmentId { get; set; }
        public string? Department { get; set; }
        public int? InstructorId { get; set; }
        public string? Instructor { get; set; }
        public int NumDays { get; set; }
        public decimal Fee { get; set; }
        public int TotalClasses { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ChangedOn { get; set; }
        public int? ChangedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public int? DeletedBy { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Department DepartmentNavigation { get; set; } = null!;
        public virtual Instructor? InstructorNavigation { get; set; }
        public virtual ICollection<Class> Classes { get; set; }
    }
}
