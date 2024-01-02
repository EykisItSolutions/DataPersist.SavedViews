using System;
using System.Collections.Generic;

namespace DataPersist.SavedViews.Domain
{
    public partial class Country
    {
        public Country()
        {
            Students = new HashSet<Student>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ChangedOn { get; set; }
        public int? ChangedBy { get; set; }

        public virtual ICollection<Student> Students { get; set; }
    }
}
