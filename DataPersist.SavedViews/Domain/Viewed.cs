using System;
using System.Collections.Generic;

namespace DataPersist.SavedViews.Domain
{
    public partial class Viewed
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int WhatId { get; set; }
        public string WhatType { get; set; } = null!;
        public string WhatName { get; set; } = null!;
        public DateTime ViewDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ChangedOn { get; set; }
        public int? ChangedBy { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
