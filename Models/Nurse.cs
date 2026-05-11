using HospitalApp.Interfaces;
namespace HospitalApp.Models
{
    public enum ShiftType { Morning, Evening, Night }

   
    public class Nurse : Person, IReportable
    {
        // ── Properties ──────────────────────────────────────────────────────────
        public string    Ward            { get; set; }
        public ShiftType Shift           { get; private set; }
        public string    Qualification   { get; }
        public bool      IsHeadNurse     { get; set; }

        private readonly List<string> _assignedTasks = new();
        public IReadOnlyList<string>  AssignedTasks => _assignedTasks.AsReadOnly();

        // ── Abstract Overrides ──────────────────────────────────────────────────
        public override string Role     => IsHeadNurse ? "Head Nurse" : "Nurse";
        public override string RoleIcon => "[RN]";

        // ── Constructor ─────────────────────────────────────────────────────────
        public Nurse(string fullName, int age, string phone,
                     string ward, ShiftType shift,
                     string qualification = "RN", string email = "")
            : base(fullName, age, phone, email)
        {
            Ward          = ward;
            Shift         = shift;
            Qualification = qualification;
        }

        // ── Domain Methods ──────────────────────────────────────────────────────
        public void ChangeShift(ShiftType newShift) => Shift = newShift;

        public void AssignTask(string task)
        {
            if (!string.IsNullOrWhiteSpace(task))
                _assignedTasks.Add(task.Trim());
        }

        public void ClearTasks() => _assignedTasks.Clear();

        public string GetShiftHours() => Shift switch
        {
            ShiftType.Morning => "07:00 - 15:00",
            ShiftType.Evening => "15:00 - 23:00",
            ShiftType.Night   => "23:00 - 07:00",
            _                 => "Unknown"
        };

        // ── IReportable ─────────────────────────────────────────────────────────
        public string GenerateReport() =>
            $"""
             ╔══ NURSE REPORT ════════════════════════════════╗
               ID            : #{Id:D4}
               Name          : {FullName}
               Role          : {Role}
               Ward          : {Ward}
               Shift         : {Shift}  ({GetShiftHours()})
               Qualification : {Qualification}
               Phone         : {Phone}
               Tasks         : {(AssignedTasks.Any() ? string.Join(", ", AssignedTasks) : "None")}
             ╚════════════════════════════════════════════════╝
             """;

        public override string GetDetails() =>
            $"Ward: {Ward} | Shift: {Shift} ({GetShiftHours()}) | {Qualification}";

        public override string GetInfo() =>
            base.GetInfo() + $"  Ward:{Ward,-10}  Shift:{Shift}";
    }
}
