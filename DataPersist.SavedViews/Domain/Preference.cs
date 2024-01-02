using System;
using System.Collections.Generic;

namespace DataPersist.SavedViews.Domain
{
    public partial class Preference
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? TimeZone { get; set; }
        public int? FontSize { get; set; }
        public string? PageLayout { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ChangedOn { get; set; }
        public int? ChangedBy { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
