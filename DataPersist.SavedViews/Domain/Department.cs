using System;
using System.Collections.Generic;

namespace DataPersist.SavedViews.Domain
{
    public partial class Department
    {
        public Department()
        {
            Courses = new HashSet<Course>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ChangedOn { get; set; }
        public int? ChangedBy { get; set; }

        public virtual ICollection<Course> Courses { get; set; }
    }
}
