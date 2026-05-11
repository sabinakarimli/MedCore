using HospitalApp.Interfaces;

namespace HospitalApp.Models
{
   
    public class Doctor : Person, ISchedulable, IReportable
    {
        // ── Private State ───────────────────────────────────────────────────────
        private decimal _salary;
        private readonly List<DateTime> _schedule    = new();
        private readonly List<string>   _certifications = new();

        // ── Properties ──────────────────────────────────────────────────────────
        public string Specialization { get; }
        public string Department     { get; set; } = "Unassigned";
        public int    YearsOfService { get; }

        public decimal Salary
        {
            get => _salary;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Salary cannot be negative.");
                _salary = value;
            }
        }

        public IReadOnlyList<string>   Certifications => _certifications.AsReadOnly();
        public IReadOnlyList<DateTime> Schedule       => _schedule.AsReadOnly();

        // ── Abstract Overrides ──────────────────────────────────────────────────
        public override string Role     => "Doctor";
        public override string RoleIcon => "[DR]";

        // ── Constructor ─────────────────────────────────────────────────────────
        public Doctor(string fullName, int age, string phone,
                      string specialization, decimal salary,
                      int yearsOfService = 0, string email = "")
            : base(fullName, age, phone, email)
        {
            Specialization = specialization;
            Salary         = salary;
            YearsOfService = yearsOfService;
        }

        // ── ISchedulable Implementation ─────────────────────────────────────────
        public bool IsAvailable(DateTime dt) =>
            !_schedule.Any(s => s.Date == dt.Date && s.Hour == dt.Hour);

        public void AddToSchedule(DateTime dt)
        {
            if (!IsAvailable(dt))
                throw new InvalidOperationException(
                    $"Dr. {FullName} is already booked at {dt:dd/MM/yyyy HH:mm}.");
            _schedule.Add(dt);
        }

        public void RemoveFromSchedule(DateTime dt) =>
            _schedule.RemoveAll(s => s.Date == dt.Date && s.Hour == dt.Hour);

        public string GetScheduleSummary()
        {
            var upcoming = _schedule.Where(d => d >= DateTime.Now).OrderBy(d => d).ToList();
            if (!upcoming.Any()) return "  No upcoming appointments.";
            return string.Join("\n", upcoming.Select(d =>
                $"  -> {d:ddd dd MMM yyyy} at {d:HH:mm}"));
        }

        public IReadOnlyList<DateTime> GetUpcomingSlots(int days = 7)
        {
            var cutoff = DateTime.Now.AddDays(days);
            return _schedule.Where(d => d >= DateTime.Now && d <= cutoff)
                            .OrderBy(d => d).ToList().AsReadOnly();
        }

        // ── Domain Methods ──────────────────────────────────────────────────────
        public void AddCertification(string cert)
        {
            if (!string.IsNullOrWhiteSpace(cert))
                _certifications.Add(cert.Trim());
        }

        // ── IReportable ─────────────────────────────────────────────────────────
        public string GenerateReport() =>
            $"""
             ╔══ DOCTOR REPORT ══════════════════════════════╗
               ID            : #{Id:D4}
               Name          : {FullName}
               Age           : {Age}
               Specialization: {Specialization}
               Department    : {Department}
               Salary        : ${Salary:N2}
               Years Service : {YearsOfService}
               Phone         : {Phone}
               Email         : {(string.IsNullOrEmpty(Email) ? "N/A" : Email)}
               Appointments  : {_schedule.Count}
               Certifications: {(Certifications.Any() ? string.Join(", ", Certifications) : "None")}
             ╚═══════════════════════════════════════════════╝
             """;

        // ── Override GetDetails ─────────────────────────────────────────────────
        public override string GetDetails() =>
            $"Specialization: {Specialization} | Dept: {Department} | " +
            $"Salary: ${Salary:N0} | Appointments: {_schedule.Count}";

        public override string GetInfo() =>
            base.GetInfo() + $"  {Specialization,-18} | {Department}";
    }
}
