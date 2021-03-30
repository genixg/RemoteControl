using System;
using System.Collections.Generic;

#nullable disable

namespace RemoteControl.Models
{
    public partial class Department
    {
        public Department()
        {
            Employees = new HashSet<Employee>();
            Children = new HashSet<Department>();
        }

        public int Id { get; set; }
        public Guid Idguid1C { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public int Level { get; set; }
        public int SortOrder { get; set; }

        public virtual Department Parent { get; set; }
        public virtual ICollection<Employee> Employees { get; set; }
        public virtual ICollection<Department> Children { get; set; }

        public List<int> ChildDepartmentsIDs()
        {
            List<int> listIds = new List<int>();
            foreach (var cd in this.Children)
            {
                listIds.Add(cd.Id);
                listIds.AddRange(cd.ChildDepartmentsIDs());
            }
            return listIds;
        }
    }
}
