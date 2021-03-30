using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RemoteControl.Models;
using RemoteControl.Models.DTO;

namespace RemoteControl.Pages
{

    public class IndexModel : PageModel
    {
        public string greeting;
        public bool needInfo;
        public bool notfound = false;
        public bool woman = false;
        private List<Employee> employees;
        private Employee currentEmployee { get; set; }

        private readonly ILogger<IndexModel> _logger;
        private readonly KGNGPEmployeesContext _context;
        private readonly IConfiguration _config;

        public IndexModel(ILogger<IndexModel> logger, KGNGPEmployeesContext context, IConfiguration config)
        {
            _logger = logger;
            _context = context;
            _config = config;
            greeting = "Введите проверочный код: ";
        }

        public void OnGet()
        {
            employees = _context.Employees.Where(e => !string.IsNullOrEmpty(e.Email)).ToList();
            var login = User.Identity.Name.Split('\\').Last();
            if (string.IsNullOrEmpty(login))
                return;
            currentEmployee = employees.GetByLogin(login);
            if(currentEmployee != null)
            {
                int MINLATENCY = _config.GetValue<int>("MinLatencyBetweenChecksInMinutes");
                var lateChecks = _context.Checks.Where(c => c.EmployeeId == currentEmployee.Id
                    && c.TypedDate == null
                    && c.SentDate.HasValue).ToList();
                lateChecks = lateChecks.Where(c => c.PlanDate.Date == DateTime.Now.Date && (DateTime.Now - c.SentDate).Value.TotalMinutes > MINLATENCY).ToList();
                needInfo = lateChecks.Any();

                var fio = currentEmployee.Name.Split(" ");
                if (fio.Length > 2)
                    greeting = string.Format("{0} {1}, введите {2} проверочный код: ", fio[1], fio[2],
                        needInfo ? "объяснительную о причинах задержки и": "");
                woman = !currentEmployee.IsMan();
            } else
			{
                notfound = true;
                greeting = $"Вас не удалось опознать по логину {login}. Пожалуйста, обратитесь к сотрудникам технической поддержки";
            }
        }
    }
}
