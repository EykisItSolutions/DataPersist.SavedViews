using System;
using System.Collections.Generic;

namespace DataPersist.SavedViews.Domain
{
    public partial class Setting
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Value { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime LastChangeDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ChangedOn { get; set; }
        public int? ChangedBy { get; set; }
    }
}
