using System;
using System.Collections.Generic;

namespace DataPersist.SavedViews.Domain
{
    public partial class Student
    {
        public Student()
        {
            Enrollments = new HashSet<Enrollment>();
        }

        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Alias { get; set; } = null!;
        public string? City { get; set; }
        public int? CountryId { get; set; }
        public string? Country { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public int TotalEnrollments { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ChangedOn { get; set; }
        public int? ChangedBy { get; set; }

        public virtual Country? CountryNavigation { get; set; }
        public virtual ICollection<Enrollment> Enrollments { get; set; }
    }
}
