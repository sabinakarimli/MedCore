using HospitalApp.Interfaces;
namespace HospitalApp.Models
{
    public enum PatientStatus { Outpatient, Inpatient, Discharged, Critical }

    public class Patient : Person, IReportable
    {
        // ── Private State ───────────────────────────────────────────────────────
        private readonly List<MedicalRecord> _records  = new();
        private readonly List<string>        _allergies = new();

        // ── Properties ──────────────────────────────────────────────────────────
        public string        BloodType    { get; }
        public PatientStatus Status       { get; private set; }
        public string?       AssignedWard { get; private set; }
        public string?       InsuranceId  { get; set; }
        public string        EmergencyContact { get; set; } = "N/A";

        public IReadOnlyList<MedicalRecord> Records   => _records.AsReadOnly();
        public IReadOnlyList<string>        Allergies => _allergies.AsReadOnly();

        public bool IsInpatient => Status == PatientStatus.Inpatient;
        public bool IsCritical  => Status == PatientStatus.Critical;

        // ── Abstract Overrides ──────────────────────────────────────────────────
        public override string Role     => "Patient";
        public override string RoleIcon => "[PT]";

        // ── Constructor ─────────────────────────────────────────────────────────
        public Patient(string fullName, int age, string phone,
                       string bloodType, string email = "")
            : base(fullName, age, phone, email)
        {
            if (age < 0 || age > 100)
                throw new ArgumentOutOfRangeException(nameof(age), "Patient age must be between 0 and 100.");
            BloodType = bloodType;
            Status    = PatientStatus.Outpatient;
        }

        // ── Domain Methods ──────────────────────────────────────────────────────
        public void Admit(string ward, bool isCritical = false)
        {
            if (string.IsNullOrWhiteSpace(ward))
                throw new ArgumentException("Ward name cannot be empty.");
            AssignedWard = ward;
            Status       = isCritical ? PatientStatus.Critical : PatientStatus.Inpatient;
        }

        public void Discharge()
        {
            AssignedWard = null;
            Status       = PatientStatus.Discharged;
        }

        public void MarkCritical()
        {
            if (!IsInpatient)
                throw new InvalidOperationException("Patient must be admitted before marking critical.");
            Status = PatientStatus.Critical;
        }

        public void AddRecord(MedicalRecord record)
        {
            ArgumentNullException.ThrowIfNull(record);
            _records.Add(record);
        }

        public void AddAllergy(string allergy)
        {
            if (!string.IsNullOrWhiteSpace(allergy))
                _allergies.Add(allergy.Trim());
        }

        public MedicalRecord? GetLatestRecord() =>
            _records.OrderByDescending(r => r.CreatedAt).FirstOrDefault();

        // ── IReportable ─────────────────────────────────────────────────────────
        public string GenerateReport()
        {
            var latest = GetLatestRecord();
            return $"""
             ╔══ PATIENT REPORT ═════════════════════════════╗
               ID            : #{Id:D4}
               Name          : {FullName}
               Age           : {Age}
               Blood Type    : {BloodType}
               Status        : {Status}
               Ward          : {AssignedWard ?? "N/A"}
               Phone         : {Phone}
               Insurance     : {InsuranceId ?? "N/A"}
               Emergency     : {EmergencyContact}
               Allergies     : {(Allergies.Any() ? string.Join(", ", Allergies) : "None")}
               Records       : {_records.Count}
               Last Diagnosis: {latest?.Diagnosis ?? "No records yet"}
             ╚═══════════════════════════════════════════════╝
             """;
        }

        public override string GetDetails() =>
            $"Blood: {BloodType} | Status: {Status,-12} | " +
            $"Ward: {AssignedWard ?? "N/A"} | Records: {_records.Count}";

        public override string GetInfo() =>
            base.GetInfo() + $"  Blood:{BloodType,-4}  Status:{Status}";
    }
}
