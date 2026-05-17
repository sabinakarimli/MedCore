using HospitalApp.Interfaces;
namespace HospitalApp.Models
{
    public enum AppointmentStatus { Pending, Confirmed, InProgress, Completed, Cancelled, NoShow }

    public class Appointment : IReportable
    {
        // ── Static ID ───────────────────────────────────────────────────────────
        private static int _nextId = 5000;

        // ── Properties ──────────────────────────────────────────────────────────
        public int                Id         { get; }
        public Doctor             Doctor     { get; }
        public Patient            Patient    { get; }
        public DateTime           DateTime   { get; }
        public string             Reason     { get; }
        public string             Notes      { get; set; } = string.Empty;
        public AppointmentStatus  Status     { get; private set; }
        public DateTime           CreatedAt  { get; }
        public DateTime?          UpdatedAt  { get; private set; }

        // ── Constructor ─────────────────────────────────────────────────────────
        public Appointment(Doctor doctor, Patient patient, DateTime dateTime, string reason)
        {
            ArgumentNullException.ThrowIfNull(doctor);
            ArgumentNullException.ThrowIfNull(patient);

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Appointment reason cannot be empty.");
            if (reason.Trim().Length < 3)
                throw new ArgumentException("Appointment reason must be at least 3 characters.");
            if (dateTime < DateTime.Now)
                throw new ArgumentException("Appointment cannot be scheduled in the past.");

            Id        = _nextId++;
            Doctor    = doctor;
            Patient   = patient;
            DateTime  = dateTime;
            Reason    = reason;
            Status    = AppointmentStatus.Pending;
            CreatedAt = System.DateTime.Now;

            doctor.AddToSchedule(dateTime);
        }

        // ── State Machine Methods ────────────────────────────────────────────────
        public void Confirm()
        {
            EnsureStatus(AppointmentStatus.Pending);
            ChangeStatus(AppointmentStatus.Confirmed);
        }

        public void Start()
        {
            EnsureStatus(AppointmentStatus.Confirmed);
            ChangeStatus(AppointmentStatus.InProgress);
        }

        public void Complete(string notes = "")
        {
            if (Status != AppointmentStatus.InProgress && Status != AppointmentStatus.Confirmed)
                throw new InvalidOperationException("Cannot complete an appointment that hasn't started.");
            Notes = notes;
            ChangeStatus(AppointmentStatus.Completed);
            Doctor.RemoveFromSchedule(DateTime);
        }

        public void Cancel(string reason = "")
        {
            if (Status == AppointmentStatus.Completed)
                throw new InvalidOperationException("Cannot cancel a completed appointment.");
            Notes = reason;
            ChangeStatus(AppointmentStatus.Cancelled);
            Doctor.RemoveFromSchedule(DateTime);
        }

        public void MarkNoShow()
        {
            EnsureStatus(AppointmentStatus.Confirmed);
            ChangeStatus(AppointmentStatus.NoShow);
            Doctor.RemoveFromSchedule(DateTime);
        }

        // ── Private Helpers ─────────────────────────────────────────────────────
        private void EnsureStatus(AppointmentStatus required)
        {
            if (Status != required)
                throw new InvalidOperationException(
                    $"Expected status '{required}', but current status is '{Status}'.");
        }

        private void ChangeStatus(AppointmentStatus newStatus)
        {
            Status    = newStatus;
            UpdatedAt = System.DateTime.Now;
        }

        public string GetStatusIcon() => Status switch
        {
            AppointmentStatus.Pending    => "[?]",
            AppointmentStatus.Confirmed  => "[✓]",
            AppointmentStatus.InProgress => "[~]",
            AppointmentStatus.Completed  => "[+]",
            AppointmentStatus.Cancelled  => "[X]",
            AppointmentStatus.NoShow     => "[!]",
            _                            => "[ ]"
        };

        // ── IReportable ─────────────────────────────────────────────────────────
        public string GenerateReport() =>
            $"""
             ╔══ APPOINTMENT REPORT ══════════════════════════╗
               ID       : #{Id}
               Status   : {GetStatusIcon()} {Status}
               Date     : {DateTime:dddd, dd MMMM yyyy}
               Time     : {DateTime:HH:mm}
               Doctor   : Dr. {Doctor.FullName}  ({Doctor.Specialization})
               Patient  : {Patient.FullName}
               Reason   : {Reason}
               Notes    : {(string.IsNullOrEmpty(Notes) ? "—" : Notes)}
               Created  : {CreatedAt:dd/MM/yyyy HH:mm}
             ╚════════════════════════════════════════════════╝
             """;

        public override string ToString() =>
            $"  {GetStatusIcon()} #{Id}  {DateTime:dd/MM/yyyy HH:mm}  " +
            $"Dr.{Doctor.FullName,-22} + {Patient.FullName,-22}  {Reason}";
    }
}
