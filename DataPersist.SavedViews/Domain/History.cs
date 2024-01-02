using System;
using System.Collections.Generic;

namespace DataPersist.SavedViews.Domain
{
    public partial class History
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public DateTime HistoryDate { get; set; }
        public Guid? Txn { get; set; }
        public string What { get; set; } = null!;
        public int WhatId { get; set; }
        public string Name { get; set; } = null!;
        public string Operation { get; set; } = null!;
        public string? Content { get; set; }
        public DateTime? UndoDate { get; set; }
        public int? UndoBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ChangedOn { get; set; }
        public int? ChangedBy { get; set; }

        public virtual User? User { get; set; }
    }
}
