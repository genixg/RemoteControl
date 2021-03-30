using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteControl.Models.DTO
{
    public class CheckDTO
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

        public int LateInMinutes { get
            {
                return (int)(!SentDate.HasValue ? 0
                    : TypedDate.HasValue ? (TypedDate - SentDate).Value.TotalMinutes
                        : (DateTime.Now - SentDate).Value.TotalMinutes);
            } }
    }
}
