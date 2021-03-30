using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using RemoteControl.Models;

namespace RemoteControl.Pages
{
    public class ManageModel : PageModel
    {
        private List<Employee> employees;
        private Employee currentEmployee { get; set; }

        private readonly ILogger<ManageModel> _logger;
        private readonly KGNGPEmployeesContext _context;
        //private readonly UserManager<IdentityUser> _userManager;

        public ManageModel(ILogger<ManageModel> logger, KGNGPEmployeesContext context/*, UserManager<IdentityUser> userManager*/)
        {
            _logger = logger;
            _context = context;
            //_userManager = userManager;
        }

        public IActionResult OnGet()
        {
            employees = _context.Employees.Where(e => !string.IsNullOrEmpty(e.Email)).ToList();
            var login = User.Identity.Name.Split('\\').Last();
            currentEmployee = employees.FirstOrDefault(e => e.Email.StartsWith(login));

            if (currentEmployee == null || !currentEmployee.CanManage)
            {
                return NotFound();
                //var userAD = _userManager.FindByEmailAsync(currentEmployee.Email).Result;
                //// Get the roles for the user
                //var roles = _userManager.GetRolesAsync(userAD).Result;

            }

            return Page();
        }
    }
}
