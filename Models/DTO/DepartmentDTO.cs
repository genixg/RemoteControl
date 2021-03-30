using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteControl.Models.DTO
{
    public class DepartmentDTO
    {
        public DepartmentDTO()
        {
            Children = new List<DepartmentDTO>();
        }
        public DepartmentDTO(int aID, string aName, string aGUID)
        {
            ID = aID;
            Name = aName;
            GUID = aGUID;
            Children = new List<DepartmentDTO>();
        }

        public int ID { get; set; }

        public int ParentID { get; set; }

        public string Name { get; set; }

        public string GUID { get; set; }

        /// <summary>
        /// Не заполняется самостоятельно, нужно вычислять предварительно
        /// </summary>
        public int Level { get; set; }
        public int SortOrder { get; set; }

        public int countEmployees { get; set; }

        public List<DepartmentDTO> Children { get; set; }
    }    

}
