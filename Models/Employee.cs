using System;
using System.Collections.Generic;

#nullable disable

namespace RemoteControl.Models
{
    public partial class Employee
    {
        public Employee()
        {
            Checks = new HashSet<Check>();
            WorkingTimes = new HashSet<WorkingTime>();
        }

        public int Id { get; set; }
        public Guid Idguid1C { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public int DepartmentId { get; set; }
        public string Email { get; set; }
        public string Login { get; set; }
        public string WorkPhone { get; set; }
        public string InnerPhone { get; set; }
        public string MobilePhone { get; set; }
        public string GazPhone { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public DateTime? Birthsday { get; set; }
        public short? Status { get; set; }
        public DateTime? StatusTill { get; set; }
        public bool CanManage { get; set; }
        public bool IsControlled { get; set; }
        public bool SyncAD { get; set; }

        public virtual Department Department { get; set; }
        public virtual ICollection<Check> Checks { get; set; }
        public virtual ICollection<WorkingTime> WorkingTimes { get; set; }
    }
}
