using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteControl.Models.DTO
{
    public class WorkingTimeDTO
    {
        public long Id { get; set; }
        public int EmployeeId { get; set; }
        public byte Type { get; set; }
        public DateTime WorkStart { get; set; }
        public DateTime WorkEnd { get; set; }
        public DateTime? LunchStart { get; set; }
        public DateTime? LunchEnd { get; set; }
        public short TimeHoursDifToServer { get; set; }
    }
}
