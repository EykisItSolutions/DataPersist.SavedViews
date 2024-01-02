using System;
using System.Collections.Generic;

namespace DataPersist.SavedViews.Domain
{
    public partial class User
    {
        public User()
        {
            ActivityLogs = new HashSet<ActivityLog>();
            DataLogs = new HashSet<DataLog>();
            Errors = new HashSet<Error>();
            Histories = new HashSet<History>();
            Preferences = new HashSet<Preference>();
            Vieweds = new HashSet<Viewed>();
        }

        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Image { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ChangedOn { get; set; }
        public int? ChangedBy { get; set; }

        public virtual ICollection<ActivityLog> ActivityLogs { get; set; }
        public virtual ICollection<DataLog> DataLogs { get; set; }
        public virtual ICollection<Error> Errors { get; set; }
        public virtual ICollection<History> Histories { get; set; }
        public virtual ICollection<Preference> Preferences { get; set; }
        public virtual ICollection<Viewed> Vieweds { get; set; }
    }
}
