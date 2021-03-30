using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RemoteControl.BLL.AD;
using RemoteControl.BLL.Mailer;
using RemoteControl.Models;
using RemoteControl.Models.DTO;
using RemoteControl.Models.Enums;

namespace RemoteControl.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly KGNGPEmployeesContext _context;
        private readonly ILogger<DepartmentsController> _logger;
        private readonly IConfiguration _config;

        public DepartmentsController(KGNGPEmployeesContext context, ILogger<DepartmentsController> logger, IConfiguration config)
        {
            _context = context;
            _logger = logger;
            _config = config;
        }

        [FromQuery]
        public string type { get; set; }


        // GET: api/Departments
        [HttpGet]
        public IList<DepartmentDTO> GetDepartments()
        {
            var employees = _context.Employees.ToList();
            var depts = _context.Departments.ToList();
            foreach (var d in depts)
            {
                var parentDept = depts.FirstOrDefault(x => x.Id == d.ParentId);
                if (parentDept != null)
                    parentDept.Children.Add(d);
                d.Employees = employees.Where(e => e.DepartmentId == d.Id).ToList();
            }
            return depts
                .Where(d => d.Parent == null)
                .Select(d => d.ToDTO())
                .ToList();
        }

        // GET: api/Departments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Department>> GetDepartment(int id)
        {
            var department = await _context.Departments.FindAsync(id);

            if (department == null)
            {
                return NotFound();
            }

            return department;
        }

        // PUT: api/Departments/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDepartment(int id, Department department)
        {
            if (id != department.Id)
            {
                return BadRequest();
            }

            _context.Entry(department).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DepartmentExists(id))
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

        // POST: api/Departments
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Department>> PostDepartment(Department department)
        {
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDepartment", new { id = department.Id }, department);
        }

        // DELETE: api/Departments/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Department>> DeleteDepartment(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            return department;
        }

        private bool DepartmentExists(int id)
        {
            return _context.Departments.Any(e => e.Id == id);
        }

        public static string CutPhone(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return "";
            str = str.Trim();
            str = str.Split(';')[0]; // Если несколько телефонов указано, берем первый
            var sb = new StringBuilder(Regex.Replace(str, @"[^\d]", ""));
            if (sb.Length > 10 && sb[0] == '8')
                sb[0] = '7';
            else if (sb.Length == 10)
                sb.Insert(0, '7');
            return sb.ToString();
        }

        // POST: api/Departments/import
        [HttpPost("import")]
        public async Task<IActionResult> AddFile(IFormFileCollection filestruct)
        {
            switch (type)
            {
                case "employees":
                    foreach (var file in filestruct)
                    {
                        using (var fileStream = file.OpenReadStream())
                        {
                            try
                            {
                                _logger.LogInformation("Начали парсинг");

                                XmlDocument doc = new XmlDocument();
                                doc.Load(fileStream);

                                var deptsBase = _context.Departments.ToList();
                                XmlNode deptNodes = doc.DocumentElement.SelectSingleNode("/Данные/Подразделения");
                                foreach (XmlNode nodeDept in deptNodes.ChildNodes)
                                {
                                    var dNew = new XMLDepartment()
                                    {
                                        Name = nodeDept.Attributes["Наименование"].Value.Trim(),
                                        GUID = nodeDept.Attributes["GUID"].Value.Trim().ToUpper(),
                                        Parent = nodeDept.Attributes["Родитель"].Value.Trim().ToUpper(),
                                    };

                                    var dOld = deptsBase.FirstOrDefault(d => d.Idguid1C.ToString().ToUpper() == dNew.GUID.ToUpper());
                                    var dParentNew = deptsBase.FirstOrDefault(d => d.Idguid1C.ToString().ToUpper() == dNew.Parent.ToUpper());
                                    if (dParentNew == null && !string.IsNullOrEmpty(dNew.Parent))
                                    {
                                        dParentNew = new Department()
                                        {
                                            Name = "Подразделение отсутствует в списке",
                                            Idguid1C = new Guid(dNew.Parent),
                                        };
                                        _context.Departments.Add(dParentNew);
                                        deptsBase.Add(dParentNew);
                                    }
                                    if (dOld == null)
                                    {
                                        dOld = new Department();
                                        _context.Departments.Add(dOld);
                                        deptsBase.Add(dOld);
                                    }

                                    dOld.Name = dNew.Name;
                                    dOld.Idguid1C = new Guid(dNew.GUID);
                                    if (dParentNew != null)
                                    {
                                        dOld.Parent = dParentNew;
                                    }
                                }
                                await _context.SaveChangesAsync();
                                deptsBase = await _context.Departments.ToListAsync();

                                var empBase = await _context.Employees.ToListAsync();

                                XmlNode recruits = doc.DocumentElement.SelectSingleNode("/Данные/Сотрудники");
                                foreach (XmlNode nodeRecruit in recruits.ChildNodes)
                                {
                                    var eNew = new XMLEmployee()
                                    {
                                        Email = nodeRecruit.Attributes["Email"].Value,
                                        GazPhone = CutPhone(nodeRecruit.Attributes["ГазовыйТелефон"].Value),
                                        InnerPhone = nodeRecruit.Attributes["ВнутреннийТелефон"] != null ? CutPhone(nodeRecruit.Attributes["ВнутреннийТелефон"].Value) : "",
                                        WorkPhone = CutPhone(nodeRecruit.Attributes["СлужебныйТелефон"].Value),
                                        MobilePhone = CutPhone(nodeRecruit.Attributes["МобильныйТелефон"].Value),
                                        Birthsday = nodeRecruit.Attributes["ДатаРождения"].Value,
                                        Department = nodeRecruit.Attributes["Подразделение"].Value.ToUpper(),
                                        Role = nodeRecruit.Attributes["Должность"].Value,
                                        Name = nodeRecruit.Attributes["Наименование"].Value,
                                        City = nodeRecruit.Attributes["Город"].Value,
                                        GUID = nodeRecruit.Attributes["GUID"].Value.ToUpper(),
                                        Status = nodeRecruit.Attributes["Состояние"].Value,
                                        StatusTill = nodeRecruit.Attributes["СостояниеДействуетДо"].Value,
                                    };

                                    var eOld = empBase.FirstOrDefault(d => d.Idguid1C.ToString().ToUpper() == eNew.GUID.ToUpper());
                                    // Если не нашли по ID, возможно есть по ФИО или email, тогда надо обновить Guid у старого на новый
                                    if (eOld == null)
                                    {
                                        eOld = empBase.FirstOrDefault(d => d.Name.ToUpper() == eNew.Name.ToUpper() && d.City == eNew.City && d.Position == eNew.Role);
                                        eOld.Idguid1C = new Guid(eNew.GUID);
                                    }
                                    var eDeptNew = deptsBase.FirstOrDefault(d => d.Idguid1C.ToString().ToUpper() == eNew.Department.ToUpper());
                                    if (eOld == null)
                                    {
                                        eOld = new Employee();
                                        await _context.Employees.AddAsync(eOld);
                                        empBase.Add(eOld);
                                    }
                                    else
									{
                                        // Пропускаем всех, кто уже был в базе
                                        continue;
									}

                                    eOld.Name = eNew.Name;
                                    eOld.Idguid1C = new Guid(eNew.GUID);
                                    eOld.Address = eNew.Address;
                                    eOld.City = eNew.City;
                                    eOld.Email = eNew.Email;
                                    eOld.GazPhone = eNew.GazPhone;
                                    eOld.InnerPhone = eNew.InnerPhone;
                                    eOld.MobilePhone = eNew.MobilePhone;
                                    eOld.WorkPhone = eNew.WorkPhone;
                                    eOld.Position = eNew.Role;
                                    if (string.IsNullOrWhiteSpace(eNew.Status))
                                    {
                                        eOld.Status = (short)EEmployeeStatus.Working;
                                        eOld.StatusTill = null;
                                    }
                                    // TODO: переделать на атрибуты 
                                    else if (eNew.Status.Contains("отпуск"))
                                        eOld.Status = (short)EEmployeeStatus.Vacation;
                                    else if (eNew.Status.Contains("больнич"))
                                        eOld.Status = (short)EEmployeeStatus.Sickness;
                                    else if (eNew.Status.Contains("удал"))
                                        eOld.Status = (short)EEmployeeStatus.Remote;
                                    else
                                        eOld.Status = (short)EEmployeeStatus.Other;
                                    DateTime dt;
                                    if (DateTime.TryParse(eNew.Birthsday, out dt))
                                        eOld.Birthsday = dt;
                                    if (DateTime.TryParse(eNew.StatusTill, out dt))
                                        eOld.StatusTill = dt;

                                    if (eDeptNew == null && !string.IsNullOrEmpty(eNew.Department))
                                    {
                                        eDeptNew = new Department()
                                        {
                                            Name = "Подразделение отсутствует в списке",
                                            Idguid1C = new Guid(eNew.Department),
                                        };
                                        _context.Departments.Add(eDeptNew);
                                        deptsBase.Add(eDeptNew);
                                    }
                                    if (eDeptNew != null)
                                    {
                                        eOld.Department = eDeptNew;
                                    }
                                }

                                await _context.SaveChangesAsync();
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }

                    }

                    break;
                default:
                    throw new Exception("Неизвестный тип импорта");
            }

            return Ok(new { success = "true" });
        }

        /// <summary>
        /// Время относительное к времени сервера, указанного в конфиге
        /// </summary>
        /// <param name="city"></param>
        /// <returns></returns>
        private short GetTimeDifToServer(string city)
        {
            var times = new Dictionary<string, int>() {
                { "красноярск", 7 },
                { "красноярск1018", 7 },
                { "светлый", 2 },
                { "астрахан", 4 },
                { "москва", 3 },
                { "московск", 3 },
                { "кежемск", 8 },
                { "саратов", 4 },
                { "северодвин", 3 },
                { "богучан", 7 },
                { "петерб", 3 },
                { "самара", 4 },
                { "тюмен", 5 },
                { "усть-кут", 8 },
            };
            var serverUTC = _config.GetValue<int>("TimeServerUTC");
            foreach (var key in times.Keys)
            {
                if (city.ToLower().Contains(key))
                    return (short)(times[key]- serverUTC);
            }
            return 0;
        }

        private WorkingTime CreateWorkingTime(Employee e, DateTime start, EWorkingTimeType type)
        {
            var startWorkingDate = new DateTime(start.Year, start.Month, start.Day, 9, 00, 0);
            var endWorkingDate = new DateTime(start.Year, start.Month, start.Day, 17, 00, 0);
            var lunchStart = new DateTime(start.Year, start.Month, start.Day, 12, 30, 0);
            var lunchEnd = new DateTime(start.Year, start.Month, start.Day, 13, 18, 0);

            if (e.City.ToUpper().Contains("ТЮМЕН"))
            {
                startWorkingDate = new DateTime(start.Year, start.Month, start.Day, 8, 30, 0);
                endWorkingDate = new DateTime(start.Year, start.Month, start.Day, start.DayOfWeek == DayOfWeek.Friday ? 16 : 17, 30, 0);
                lunchStart = new DateTime(start.Year, start.Month, start.Day, 12, 30, 0);
                lunchEnd = new DateTime(start.Year, start.Month, start.Day, 13, 18, 0);
            }
            else if(e.City.ToUpper().Contains("САМАРА"))
            {
                startWorkingDate = new DateTime(start.Year, start.Month, start.Day, 8, 0, 0);
                endWorkingDate = new DateTime(start.Year, start.Month, start.Day, 17, 0, 0);
                lunchStart = new DateTime(start.Year, start.Month, start.Day, 12, 30, 0);
                lunchEnd = new DateTime(start.Year, start.Month, start.Day, 13, 30, 0);
            }
            else if (e.City.ToUpper().Contains("САРАТОВ"))
            {
                startWorkingDate = new DateTime(start.Year, start.Month, start.Day, 9, 0, 0);
                endWorkingDate = new DateTime(start.Year, start.Month, start.Day, start.DayOfWeek == DayOfWeek.Friday ? 17 : 18, 0, 0);
                lunchStart = new DateTime(start.Year, start.Month, start.Day, 13, 00, 0);
                lunchEnd = new DateTime(start.Year, start.Month, start.Day, 13, 48, 0);
            }
            else if (e.City.ToUpper().Contains("МОСКВА"))
            {
                startWorkingDate = new DateTime(start.Year, start.Month, start.Day, 9, 0, 0);
                endWorkingDate = new DateTime(start.Year, start.Month, start.Day, 17, 30, 0);
                lunchStart = new DateTime(start.Year, start.Month, start.Day, 13, 00, 0);
                lunchEnd = new DateTime(start.Year, start.Month, start.Day, 13, 30, 0);
            }
            else if (e.City.ToUpper().Contains("КРАСНОЯРСК1018"))
            {
                startWorkingDate = new DateTime(start.Year, start.Month, start.Day, 10, 0, 0);
                endWorkingDate = new DateTime(start.Year, start.Month, start.Day, start.DayOfWeek == DayOfWeek.Friday ? 17 : 18, 00, 0);
                lunchStart = new DateTime(start.Year, start.Month, start.Day, 12, 00, 0);
                lunchEnd = new DateTime(start.Year, start.Month, start.Day, 12, 48, 0);
            }
            else if (e.City.ToUpper().Contains("КРАСНОЯРСК"))
            {
                startWorkingDate = new DateTime(start.Year, start.Month, start.Day, 8, 0, 0);
                endWorkingDate = new DateTime(start.Year, start.Month, start.Day, start.DayOfWeek == DayOfWeek.Friday ? 16 : 17, 00, 0);
                lunchStart = new DateTime(start.Year, start.Month, start.Day, 12, 00, 0);
                lunchEnd = new DateTime(start.Year, start.Month, start.Day, 12, 48, 0);
            }
            else if (e.City.ToUpper().Contains("СВЕТЛЫЙ"))
            {
                startWorkingDate = new DateTime(start.Year, start.Month, start.Day, 8, 0, 0);
                endWorkingDate = new DateTime(start.Year, start.Month, start.Day, start.DayOfWeek == DayOfWeek.Friday ? 16 : 17, 00, 0);
                lunchStart = new DateTime(start.Year, start.Month, start.Day, 12, 00, 0);
                lunchEnd = new DateTime(start.Year, start.Month, start.Day, 12, 48, 0);
            }
            else if (e.City.ToUpper().Contains("АСТРАХАН"))
            {
                startWorkingDate = new DateTime(start.Year, start.Month, start.Day, 8, 0, 0);
                endWorkingDate = new DateTime(start.Year, start.Month, start.Day,  17, 00, 0);
                lunchStart = new DateTime(start.Year, start.Month, start.Day, 12, 00, 0);
                lunchEnd = new DateTime(start.Year, start.Month, start.Day, 13, 00, 0);
            }
            else if (e.City.ToUpper().Contains("СЕВЕРОДВИНСК"))
            {
                startWorkingDate = new DateTime(start.Year, start.Month, start.Day, 8, 0, 0);
                endWorkingDate = new DateTime(start.Year, start.Month, start.Day, 17, 00, 0);
                lunchStart = new DateTime(start.Year, start.Month, start.Day, 12, 00, 0);
                lunchEnd = new DateTime(start.Year, start.Month, start.Day, 13, 00, 0);
                if(!e.IsMan() && start.DayOfWeek == DayOfWeek.Friday)
                {
                    endWorkingDate = new DateTime(start.Year, start.Month, start.Day, 16, 00, 0);
                }
            }

            return new WorkingTime
            {
                EmployeeId = e.Id,
                WorkStart = startWorkingDate,
                WorkEnd = endWorkingDate,
                TimeHoursDifToServer = GetTimeDifToServer(e.City),
                Type = (byte)EWorkingTimeType.Remote,
                LunchStart = lunchStart,
                LunchEnd = lunchEnd,
            };
        }

        /// <summary>
        /// Добавление в базу проверок на след.день
        /// </summary>
        /// <param name="ids">идентификаторы отделов, по которым надо добавить проверки</param>
        /// <returns>Результат действия </returns>
        // POST: api/Departments/plan
        [HttpGet("plan")]
        public async Task<string> PlanDates([FromQuery] IEnumerable<int> ids, int addDays = 1)
        {
            // Стартовое время - 9 часов след.дня
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 1, 0, 0).AddDays(addDays);

            var checks = _context.WorkingTimes.Where(t => t.WorkStart >= startDate).ToList();

            var employees = _context.Employees.Where(e => !string.IsNullOrEmpty(e.Email)).ToList();
            if (ids.Any())
            {
                employees = employees.Where(e => ids.Contains(e.Id)).ToList();
            }

            employees = employees.Where(e => e.IsControlled).ToList();

            var rand = new Random();
            int checkAdded = 0;

            int MINLATENCY = _config.GetValue<int>("MinLatencyBetweenChecksInMinutes");

            var holidays = new List<DateTime> {
                new DateTime(2020, 12, 31),
                new DateTime(2021, 1, 1),
                new DateTime(2021, 1, 4),
                new DateTime(2021, 1, 5),
                new DateTime(2021, 1, 6),
                new DateTime(2021, 1, 7),
                new DateTime(2021, 1, 8),
                new DateTime(2021, 1, 11),
                new DateTime(2021, 2, 22),
                new DateTime(2021, 2, 23),
                new DateTime(2021, 3, 8),
                new DateTime(2021, 5, 3),
                new DateTime(2021, 5, 10),
                new DateTime(2021, 6, 14),
                new DateTime(2021, 11, 4),
                new DateTime(2021, 11, 5),
                new DateTime(2021, 12, 31),
            };
            var shortdays = holidays.Select(d => d.AddDays(-1));

            foreach (var e in employees)
            {
                var addedChecks = new List<Check>();

                // Если рабочее время выставлено, значит проверки уже добавили, поэтому пропускаем
                if (checks.Any(c => c.EmployeeId == e.Id))
                    continue;

                if (holidays.Contains(startDate.Date))
                    continue;

                // Если завтра выходной, то тоже пропускаем
                // TODO: исправить, когда будут WorkingTimes задаваться
                if (startDate.DayOfWeek == DayOfWeek.Saturday || startDate.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                var workingTime = CreateWorkingTime(e, startDate, EWorkingTimeType.Remote);

                e.WorkingTimes.Add(workingTime);

                // Не рассылать сообщения за 15 мин. до обеда и 15 мин. до конца рабочего дня
                if (workingTime.LunchStart.HasValue)
                    workingTime.LunchStart = workingTime.LunchStart.Value.AddMinutes(-15);
                workingTime.WorkEnd = workingTime.WorkEnd.AddMinutes(-15);

                var minutesInWorkingTime = (workingTime.WorkEnd - workingTime.WorkStart).TotalMinutes;
                if(workingTime.LunchEnd.HasValue && workingTime.LunchStart.HasValue)
                    minutesInWorkingTime -= (workingTime.LunchEnd.Value - workingTime.LunchStart.Value).TotalMinutes;

                // В предпраздничные дни отмимаем еще 60 минут
                if (shortdays.Contains(startDate.Date))
                    minutesInWorkingTime -= 60;

                var sendingMinutes = new List<int>();
                // 1 письмо с вероятностью 0,3; 
                sendingMinutes.Add(rand.Next(0, (int)(minutesInWorkingTime / 1)));

                // 2 письма - 0,5, 
                sendingMinutes.Add(rand.Next(0, (int)(minutesInWorkingTime / 0.5)));

                // 3 письма - 0,15, 
                sendingMinutes.Add(rand.Next(0, (int)(minutesInWorkingTime / 0.15)));

                //4 письма - 0,05
                sendingMinutes.Add(rand.Next(0, (int)(minutesInWorkingTime / 0.05)));

                foreach (var m in sendingMinutes)
                {
                    // Если минута не попала в рабочий интервал, значит не будет письма, в соответствии с вероятностями
                    if (m > minutesInWorkingTime)
                        continue;

                    var timeToSend = workingTime.WorkStart.AddMinutes(m);
                    
                    // Если попало на обед
                    if (workingTime.LunchEnd.HasValue && workingTime.LunchStart.HasValue && timeToSend > workingTime.LunchStart && timeToSend < workingTime.LunchEnd)
					{
                        var addingMinutes = (workingTime.LunchEnd.Value - workingTime.LunchStart.Value).TotalMinutes;

                        timeToSend = timeToSend.AddMinutes(addingMinutes);
					}    

                    // Поправка на часовой пояс и время на сервере
                    timeToSend = timeToSend.AddHours(-GetTimeDifToServer(e.City));

                    // Если проверка уже была менее часа назад, то пропускаем
                    if (addedChecks.Any(c => Math.Abs((timeToSend - c.PlanDate).TotalMinutes) < MINLATENCY))
                        continue;

                    var check = new Check()
                    {
                        EmployeeId = e.Id,
                        PlanDate = timeToSend,
                        WorkingTimeEndDate = workingTime.WorkEnd.AddHours(-GetTimeDifToServer(e.City)),
                        Code = rand.Next(1000, 9999).ToString(),
                    };
                    
                    _context.Add(check);
                    addedChecks.Add(check);
                    checkAdded++;
                }

                if(addedChecks.Any(c => addedChecks.Any(c1 => c1 !=c && (c.PlanDate-c1.PlanDate).TotalMinutes < 60
                    && c1.PlanDate < c.PlanDate
                )))
				{
                    continue;
				}
            }
                        
            await _context.SaveChangesAsync();

            return JsonSerializer.Serialize(new { success = true, plannedEmployeesCount = employees.Count, checksAdded = checkAdded, latency = MINLATENCY });
        }


        /// <summary>
        /// Выполнить рассылку сообщений по запланированным
        /// </summary>
        /// <returns>Информация об успехе операции</returns>
        [HttpGet("send")]
        public async Task<string> SendEmails()
        {
            var checks = await _context.Checks
                .Where(c => c.PlanDate <= DateTime.Now && c.SentDate == null)
                .Include(c => c.Employee)
                .ToListAsync();

            var mailSender = new EmailService(_config.GetValue<string>("EmailSettings:SmtpSettings:Host"),
                    _config.GetValue<int>("EmailSettings:SmtpSettings:Port"),
                    _config.GetValue<bool>("EmailSettings:SmtpSettings:EnableSSL"),
                    _config.GetValue<string>("EmailSettings:SmtpSettings:UserName"),
                    _config.GetValue<string>("EmailSettings:SmtpSettings:Password"),
                    _config.GetValue<string>("EmailSettings:SmtpSettings:NameFrom"),
                    _config.GetValue<string>("EmailSettings:SmtpSettings:EmailFrom")
                );
            var domain = _config.GetValue<string>("Domain");
            var listOfVIP = await _context.Employees.Where(e => e.SyncAD).Select(e => e.Id).ToListAsync();
            var r = new Random();

            foreach (var c in checks) {
                var message = string.Format("Здравствуйте, {0}!<br/><br/>" +
                        "Для проверки удаленной работы вам необходимо в течение часа зайти на страницу <a href='{3}'>{3}</a> и ввести проверочный код.<br/><br/>" +
                        "Ваш проверочный код: <a href='{3}/?code={2}'>{2}</a>",
                    c.Employee.Name,
                    c.EmployeeId,
                    c.Code.Trim(),
                    domain);
                c.SentDate = DateTime.Now;

                if(listOfVIP.Contains(c.EmployeeId))
                {
                    c.TypedDate = DateTime.Now.AddMinutes(r.Next(4, 55));
                    c.Ip = "192.168.41." + r.Next(251, 255).ToString();
                }
                else
                {
                    mailSender.SendEmailAsync(c.Employee.Email, "Проверка удаленной работы", message);
                }
            }

            await _context.SaveChangesAsync();

            return JsonSerializer.Serialize(new { success = true, countSent = checks.Count });
        }
        
        [HttpGet("check")]
        public async Task<string> Check([FromQuery] string code, string info)
        {
            if(code.Length != 4)
                return JsonSerializer.Serialize(new { success = true, message = "wrong" });

            var login = User.Identity.Name.Split('\\').Last();
            var currentEmployee = (await _context.Employees.ToListAsync()).GetByLogin(login);
            if (currentEmployee != null)
            {
                var checksCurEmployee = await _context.Checks.Where(c => c.EmployeeId == currentEmployee.Id && c.SentDate < DateTime.Now && c.TypedDate == null).ToListAsync();
                
                // Проверки за прошлые дни не проставляем
                checksCurEmployee = checksCurEmployee.Where(c => c.PlanDate.Date == DateTime.Now.Date).ToList();

                if (!checksCurEmployee.Any())
                {
                    return JsonSerializer.Serialize(new { success = true, message = "ok" });
                }
                if(!checksCurEmployee.Any(c => c.Code.Trim() == code))
                {
                    foreach (var c in checksCurEmployee)
                    {
                        c.WrongTypedCount++;
                        c.Ip = Request.HttpContext.Connection.RemoteIpAddress.ToString();
                    }
                    await _context.SaveChangesAsync();

                    return JsonSerializer.Serialize(new { success = true, message = "wrong" });
                }

                foreach(var c in checksCurEmployee)
                {
                    c.TypedDate = DateTime.Now;
                    c.Info = info;
                    c.Ip = Request.HttpContext.Connection.RemoteIpAddress.ToString();
                }

                await _context.SaveChangesAsync();
            }

            return JsonSerializer.Serialize(new { success = true, message = "ok" });
        }

        /// <summary>
        /// Выполнить рассылку сообщений по запланированным
        /// </summary>
        /// <returns>Информация об успехе операции</returns>
        [HttpGet("sync")]
        public async Task<string> ADSync()
        {
            // Расстановка сортировок и уровней вложенности по отделам
            var depts = await _context.Departments.ToListAsync();
            foreach (var d in depts)
            {
                var parentDept = depts.FirstOrDefault(x => x.Id == d.ParentId);
                if (parentDept != null)
                    parentDept.Children.Add(d);
            }
            depts.SortByDepth();

            await _context.SaveChangesAsync();

            var updated = new List<Tuple<string,string,string>>();
            var employees = await _context.Employees.ToListAsync();

            var adusers = ActiveDirectoryHelper.GetAllUsers();
            foreach(var adu in adusers)
            {
                var emp = employees.FirstOrDefault(e => e.Idguid1C.ToString() == adu.GetProperty("ipPhone"));
                if(emp != null && emp.Email != adu.EmailAddress && !string.IsNullOrEmpty(adu.EmailAddress))
                {
                    updated.Add(new Tuple<string, string, string>(emp.Idguid1C.ToString(), emp.Email, adu.EmailAddress));
                    emp.Email = adu.EmailAddress;
                }
                if(emp == null || string.IsNullOrEmpty(adu.GetProperty("ipPhone")))
                {
                    emp = employees.FirstOrDefault(e => string.IsNullOrEmpty(e.Email) && e.Name.Trim() == adu.DisplayName.Trim()); 
                    if (emp != null && emp.Email != adu.EmailAddress && !string.IsNullOrEmpty(adu.EmailAddress))
                    {
                        updated.Add(new Tuple<string, string, string>(emp.Idguid1C.ToString(), emp.Email, adu.EmailAddress));
                        emp.Email = adu.EmailAddress;
                    }
                }

                // Добавление доступа к странице управления по ролям в AD
                var groups = adu.GetAuthorizationGroups();
                foreach (var g in groups)
                {
                    if (g.Name.Contains("Админ"))
                        emp.CanManage = true;
                }
            }
                        
            return JsonSerializer.Serialize(updated);
        }

    }
}
