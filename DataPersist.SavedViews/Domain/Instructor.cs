using System;
using System.Collections.Generic;

namespace DataPersist.SavedViews.Domain
{
    public partial class Instructor
    {
        public Instructor()
        {
            Courses = new HashSet<Course>();
        }

        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Alias { get; set; } = null!;
        public DateTime? HireDate { get; set; }
        public bool IsFulltime { get; set; }
        public decimal? Salary { get; set; }
        public int TotalCourses { get; set; }
        public string? ExternalId { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ChangedOn { get; set; }
        public int? ChangedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public int? DeletedBy { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<Course> Courses { get; set; }
    }
}
