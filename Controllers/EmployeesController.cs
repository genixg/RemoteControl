using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RemoteControl.Models;
using RemoteControl.Models.DTO;

namespace RemoteControl.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly KGNGPEmployeesContext _context;
        private readonly IConfiguration _config;

        public EmployeesController(KGNGPEmployeesContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // GET: api/Employees
        [HttpGet]
        public IEnumerable<EmployeeDTO> GetEmployees(int departmentID, string dateStart, string dateEnd)
        {
            DateTime dateS = DateTime.Now, dateE = DateTime.Now;
            DateTime.TryParse(dateStart, out dateS);
            DateTime.TryParse(dateEnd, out dateE);
            var depts = _context.Departments.ToList();
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

            var checks = _context.Checks
                .Where(c => employees.Any(e => c.EmployeeId == e.Id) && c.PlanDate >= dateS && c.PlanDate <= dateE.AddDays(1))
                .OrderBy(c => c.PlanDate)
                .ToList();
            var workingTimes = _context.WorkingTimes.Where(c => employees.Any(e => c.EmployeeId == e.Id) && c.WorkStart >= dateS && c.WorkStart <= dateE.AddDays(1)).ToList();

            foreach (var e in employees)
            {
                e.Checks = checks.Where(c => c.EmployeeId == e.Id).ToList();
                e.WorkingTimes = workingTimes.Where(t => t.EmployeeId == e.Id).ToList();
            }

            return employees
                .Select(e => e.ToDTO())
                .ToList();
        }

        // GET: api/Employees/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            return employee;
        }

        // GET: api/Employees/updateIsControlled
        [HttpGet("updateIsControlled")]
        public async Task<IActionResult> UpdateIsControlled(int id, bool value)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound();
            }
            employee.IsControlled = value;
            await _context.SaveChangesAsync();
            return Ok();
        }

        // PUT: api/Employees/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployee(int id, Employee employee)
        {
            if (id != employee.Id)
            {
                return BadRequest();
            }

            _context.Entry(employee).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Employees
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEmployee", new { id = employee.Id }, employee);
        }

        // DELETE: api/Employees/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Employee>> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return employee;
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}
