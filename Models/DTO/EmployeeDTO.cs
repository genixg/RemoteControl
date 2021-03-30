using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControl.Models.DTO
{
    public class EmployeeDTO
    {
        public int Id { get; set; }
        public Guid Idguid1C { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
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
        public IEnumerable<CheckDTO> Checks { get; set; }
        public IEnumerable<WorkingTimeDTO> WorkingTimes { get; set; }
        public bool CanManage { get; set; }
        public bool IsControlled { get; set; }
        public int ChecksTotal { get { return Checks.Count(); } }
        public int ChecksNotSent { get { return Checks.Count(c => !c.SentDate.HasValue); } }
        public int ChecksNotTyped { get { return Checks.Count(c => c.SentDate.HasValue && !c.TypedDate.HasValue); } }
        public int MaxTimePeriod { get { return !Checks.Any(c => c.SentDate.HasValue) ? 0 : (int)Checks
                .Where(c => c.SentDate.HasValue)
                .Max(x => x.TypedDate.HasValue ? 
                    (x.TypedDate - x.SentDate).Value.TotalMinutes :
                    (x.WorkingTimeEndDate - x.SentDate).Value.TotalMinutes); 
            }
        }

        public int SumLatency
        {
            get
            {
                return !Checks.Any(c => c.SentDate.HasValue) ? 0 : (int)Checks
                    .Where(c => c.SentDate.HasValue)
                    .Sum(x => x.TypedDate.HasValue ?
                        (x.TypedDate - x.SentDate).Value.TotalMinutes :
                        (x.WorkingTimeEndDate - x.SentDate).Value.TotalMinutes);
            }
        }

        private const int MinutesLimit = 60;

        private int? _LateDays;
        /// <summary>
        /// Число дней с опозданием более заданного
        /// </summary>
        public int LateDays
        {
            get
            {
                if (_LateDays.HasValue)
                    return _LateDays.Value;
                Dictionary<DateTime, int> latesByDay = new Dictionary<DateTime, int>();
                foreach (var c in Checks)
                {
                    var day = new DateTime(c.PlanDate.Year, c.PlanDate.Month, c.PlanDate.Day);
                    if (c.LateInMinutes > MinutesLimit)
                    {
                        if (latesByDay.ContainsKey(day))
                            latesByDay[day]++;
                        else
                            latesByDay.Add(day, 1);
                    }
                }
                return latesByDay.Count;
            }
            set
            {
                _LateDays = value;
            }
        }        

        public string ChecksHtml { get
            {
                StringBuilder sb = new StringBuilder();
                foreach(var c in Checks)
                {
                    sb.AppendFormat(@"<div class='e_check'>
                                <span>{0}</span>
                                <span>{1}</span>
                                <span class='e_check_{3}'>{2}</span>
                            </div>",
                        (c.SentDate.HasValue ? c.SentDate : c.PlanDate).Value.AddHours(TimeHoursDifToServer).ToString("dd.MM HH:mm"),
                        c.TypedDate.HasValue ? c.TypedDate.Value.AddHours(TimeHoursDifToServer).ToString("HH:mm") : "",
                        c.SentDate.HasValue ? ((c.TypedDate ?? c.WorkingTimeEndDate) - c.SentDate).Value.TotalMinutes.ToString("F0")+" мин" : "",
                        !c.SentDate.HasValue ? "notsent"
                            : !c.TypedDate.HasValue ? "nottyped"
                                : (c.TypedDate - c.SentDate).Value.TotalMinutes > MinutesLimit ? "toomuch"
                                    : "good");
                }
                return sb.ToString();
            }
        }
        public string ChecksText
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                var firstline = true;
                foreach (var c in Checks)
                {
                    // Не выводим предстоящие проверки
                    if (!c.SentDate.HasValue)
                        continue;
                    sb.AppendFormat(@"{3}Запрос {0}, ответ{1} ({2})",
                        (c.SentDate.HasValue ? c.SentDate : c.PlanDate).Value.AddHours(TimeHoursDifToServer).ToString("dd.MM HH:mm"),
                        c.TypedDate.HasValue ? c.TypedDate.Value.AddHours(TimeHoursDifToServer).ToString(" HH:mm") : "а нет",
                        c.SentDate.HasValue ? ((c.TypedDate ?? c.WorkingTimeEndDate) - c.SentDate).Value.TotalMinutes.ToString("F0") + " мин" : "",
                        firstline ? "" : Environment.NewLine);
                    firstline = false;
                }
                return sb.ToString();
            }
        }

        public string ChecksInfoHtml
		{
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (var c in Checks)
                {
                    sb.AppendFormat(@"<div class='e_check'>{0}</div>", c.Info);
                }
                return sb.ToString();
            }
        }
        public string ChecksInfoText
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                var firstLine = true;
                foreach (var c in Checks)
                {
                    sb.AppendFormat(@"{1}{0}", c.Info, firstLine ? "" : Environment.NewLine);
                    firstLine = false;
                }
                return sb.ToString();
            }
        }

        public short TimeHoursDifToServer { get { return WorkingTimes.Any() ? WorkingTimes.First().TimeHoursDifToServer : (short)0; } }
        public string WorkingTimeStart { get { return WorkingTimes.Any() ? WorkingTimes.First().WorkStart.ToString("HH:mm") : ""; } }
        public string WorkingTimeEnd { get { return WorkingTimes.Any() ? WorkingTimes.First().WorkEnd.ToString("HH:mm") : ""; } }
        public string WorkingTime { get { return WorkingTimes.Any() ?
                    string.Format("{0} - {1}",
                        WorkingTimeStart,
                        WorkingTimeEnd
                    ) : ""; } }
    }
}
