using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RemoteControl.BLL;
using RemoteControl.Models;
using RemoteControl.Models.DTO;

namespace RemoteControl.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExportController : ControllerBase
    {
        private readonly KGNGPEmployeesContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ExportController(KGNGPEmployeesContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }
        
        [HttpGet]
        public async Task<FileResult> GetFile([FromQuery] string type, string reportDateStart, string reportDateEnd, int departmentID, int maxLatency, int maxDays)
        {
            DateTime dateStart = DateTime.Now, dateEnd = DateTime.Now;
            DateTime.TryParse(reportDateStart, out dateStart);
            DateTime.TryParse(reportDateEnd, out dateEnd);
            if(type == "daily")
            {
                dateEnd = dateStart;
            }

            var depts = await _context.Departments.ToListAsync();
            foreach (var d in depts)
            {
                var parentDept = depts.FirstOrDefault(x => x.Id == d.ParentId);
                if (parentDept != null)
                    parentDept.Children.Add(d);
            }
                       
            var dept = depts.FirstOrDefault(d => d.Id == departmentID);
            //if (dept == null)
            //{
            //    throw new Exception("Нет такого подразделения");
            //}
            var childIds = dept == null ? depts.Select(d => d.Id).ToList() : dept.ChildDepartmentsIDs();
            childIds.Add(departmentID);

            var employees = _context.Employees
                .Where(e => childIds.Contains(e.DepartmentId))
                .OrderBy(e => e.Name);

            var checks = await _context.Checks
                .Where(c => employees.Any(e => c.EmployeeId == e.Id) && c.PlanDate >= dateStart && c.PlanDate <= dateEnd.AddDays(1))
                .OrderBy(c => c.PlanDate)
                .ToListAsync();
            var workingTimes = await _context.WorkingTimes.Where(c => employees.Any(e => c.EmployeeId == e.Id) && c.WorkStart >= dateStart && c.WorkStart <= dateEnd.AddDays(1)).ToListAsync();

            foreach (var e in employees)
            {
                e.Checks = checks.Where(c => c.EmployeeId == e.Id).ToList();
                e.WorkingTimes = workingTimes.Where(t => t.EmployeeId == e.Id).ToList();
            }


            var employeesToExcel = await employees
                .Select(e => e.ToDTO())
                .ToListAsync();

            if(type == "daily")
            {
                // Мониторинг работы сотрудников в определенный день. В отчет попадают работники, которые в указанный день работали удаленно 
                // и ввели код с опозданием более указанного времени (или не ввели его вовсе). Перечень сотрудников выводится в порядке убывания 
                // времени опоздания, при этом вначале указываются работники, которые не ответили на сообщение. Отчет может выводиться для конкретного
                // подразделения или группироваться по подразделениям
                employeesToExcel = employeesToExcel
                    .Where(e => e.MaxTimePeriod > maxLatency)
                    // В сортировке сначала указывать тех, кто не ввел код совсем, потом по убыванию суммарного времени опоздания
                    .OrderByDescending(e => e.Checks.Any(c => !c.TypedDate.HasValue) ? int.MaxValue : e.SumLatency)
                    .ToList();
            }
            else
            {
                // Мониторинг работы сотрудников за период. В отчет попадают работники, которые опоздали с вводом ответа более указанного 
                // периода времени более чем заданное количество дней (по умолчанию - 1). Для работника указывается количество дней, в которых он 
                // не уложился в заданное время. Отчет может выводиться для конкретного подразделения или группироваться по подразделениям. 
                // В отчете работники сортируются в обратном порядке по количеству опаздываний за заданный период
                List<EmployeeDTO> result = new List<EmployeeDTO>();
                foreach(var e in employeesToExcel)
                {
                    Dictionary<DateTime, int> latesByDay = new Dictionary<DateTime, int>();
                    foreach (var c in e.Checks)
                    {
                        var day = new DateTime(c.PlanDate.Year, c.PlanDate.Month, c.PlanDate.Day);
                        if(c.LateInMinutes > maxLatency)
                        {
                            if (latesByDay.ContainsKey(day))
                                latesByDay[day]++;
                            else
                                latesByDay.Add(day, 1);
                        }
                    }
                    if (latesByDay.Count > maxDays)
                    {
                        e.LateDays = latesByDay.Count;
                        result.Add(e);
                    }
                }

                employeesToExcel = result
                    .OrderByDescending(e => e.LateDays)
                    .ToList();
            }

            var deptsDTO = depts.OrderBy(d => d.SortOrder).Select(d => d.ToDTO()).ToList();

            var contentPath = _hostingEnvironment.ContentRootPath + "MonitoringReport_" + DateTime.Now.Ticks + ".xlsx";

            var excelExport = new ExcelExport();
            var resultPath = excelExport.GetMonitoringReportXLSX(dateStart, dateEnd, contentPath, employeesToExcel, deptsDTO);

            var bytes = await System.IO.File.ReadAllBytesAsync(resultPath);

            const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            HttpContext.Response.ContentType = contentType;
            HttpContext.Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");

            var fileContentResult = new FileContentResult(bytes, contentType)
            {
                FileDownloadName = Path.GetFileName(resultPath)
            };

            return fileContentResult;
        }
    }
}
