using System;
using System.Collections.Generic;

namespace DataPersist.SavedViews.Domain
{
    public partial class ActivityLog
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public DateTime LogDate { get; set; }
        public string Activity { get; set; } = null!;
        public string? Result { get; set; }
        public string? IpAddress { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ChangedOn { get; set; }
        public int? ChangedBy { get; set; }

        public virtual User? User { get; set; }
    }
}
