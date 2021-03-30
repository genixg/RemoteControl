using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteControl.Models.DTO
{
    public static class ExtensionsDTO
    {
        private static int CountEmployees(Department dept)
        {
            var childCount = dept.Children.Select(CountEmployees).Sum();
            return dept.Employees.Count + childCount;
        }

        public static DepartmentDTO ToDTO(this Department dept)
        {

            return new DepartmentDTO() { 
                ID = dept.Id, 
                Name = dept.Name, 
                GUID = dept.Idguid1C.ToString(), 
                Children = dept.Children.Select(ToDTO).ToList(),
                ParentID = dept.ParentId ?? 0,
                countEmployees = CountEmployees(dept),
                Level = dept.Level,
                SortOrder = dept.SortOrder,
            };
        }

        public static EmployeeDTO ToDTO(this Employee e)
        {
            return new EmployeeDTO()
            {
                Id = e.Id,
                Name = e.Name,
                Address = e.Address,
                Birthsday = e.Birthsday,
                City = e.City,
                DepartmentId = e.DepartmentId,
                DepartmentName = e.Department != null ? e.Department.Name : "",
                Email = e.Email,
                Login = e.Login,
                GazPhone = e.GazPhone,
                Idguid1C = e.Idguid1C,
                InnerPhone = e.InnerPhone,
                MobilePhone = e.MobilePhone,
                Position = e.Position,
                Status = e.Status,
                StatusTill = e.StatusTill,
                WorkPhone = e.WorkPhone,
                CanManage = e.CanManage,
                IsControlled = e.IsControlled,

                Checks = e.Checks.Select(ToDTO),

                WorkingTimes = e.WorkingTimes.Select(ToDTO),
            };
        }
        
        public static bool IsMan(this Employee e)
        {
            var fio = e.Name.ToUpper().Split(" ");
            if (fio.Length == 3)
            {
                if (fio[2].EndsWith("ИЧ") || fio[2].EndsWith("ЛЫ"))
                    return true;
                if (fio[2].EndsWith("НА") || fio[2].EndsWith("ЗЫ"))
                    return false;
                if (fio[0].EndsWith("ОВ"))
                    return true;
            }
            return false;
        }

        public static CheckDTO ToDTO(this Check c)
        {
            return new CheckDTO()
            {
                Id = c.Id,
                Code = c.Code,
                EmployeeId = c.EmployeeId,
                Info = c.Info,
                Ip = c.Ip,
                PlanDate = c.PlanDate,
                SentDate = c.SentDate,
                TypedDate = c.TypedDate,
                WorkingTimeEndDate = c.WorkingTimeEndDate,
                WrongTypedCount = c.WrongTypedCount,
            };
        }

        public static WorkingTimeDTO ToDTO(this WorkingTime t)
        {
            return new WorkingTimeDTO()
            {
                Id = t.Id,
                EmployeeId = t.EmployeeId,
                LunchEnd = t.LunchEnd,
                LunchStart = t.LunchStart,
                TimeHoursDifToServer = t.TimeHoursDifToServer,
                Type = t.Type,
                WorkEnd = t.WorkEnd,
                WorkStart = t.WorkStart,
            };
        }


        private static void SortChildDepartments(this Department dept, int parentLevel, ref int parentSort)
        {
            foreach (var d in dept.Children)
            {
                d.SortOrder = parentSort++;
                d.Level = parentLevel + 1;
                SortChildDepartments(d, d.Level, ref parentSort);
            }
        }

        public static void SortByDepth(this List<Department> depts)
        {
            int counter = 0;
            foreach (var d in depts.Where(d => !d.ParentId.HasValue || d.ParentId == 0))
            {
                d.SortOrder = counter++;
                d.Level = 0;
                SortChildDepartments(d, 0, ref counter);
            }
            depts = depts.OrderBy(d => d.SortOrder).ToList();
        }

        public static Employee GetByLogin(this List<Employee> employees, string login)
		{
            return employees.FirstOrDefault(e => (e.Login != null && login != null && e.Login.ToLower() == login.ToLower()) 
                || (e.Email!=null && e.Email.ToLower().StartsWith(login + "@")));
        }
    }
}
