using System;
using System.Collections.Generic;

#nullable disable

namespace RemoteControl.Models
{
    public partial class Check
    {
        public long Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime? SentDate { get; set; }
        public DateTime PlanDate { get; set; }
        public string Code { get; set; }
        public DateTime? TypedDate { get; set; }
        public string Ip { get; set; }
        public string Info { get; set; }
        public byte WrongTypedCount { get; set; }
        public DateTime WorkingTimeEndDate { get; set; }

        public virtual Employee Employee { get; set; }
    }
}
