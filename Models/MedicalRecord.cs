using HospitalApp.Interfaces;
namespace HospitalApp.Models
{
    public enum RecordType { Diagnosis, Surgery, LabResult, Prescription, Consultation, Emergency }

    public class MedicalRecord : IReportable
    {
        private static int _nextId = 9000;

        public int        Id          { get; }
        public RecordType Type        { get; }
        public string     Diagnosis   { get; }
        public string     Treatment   { get; }
        public string     Medication  { get; }
        public string     Notes       { get; }
        public Doctor     IssuedBy    { get; }
        public DateTime   CreatedAt   { get; }
        public bool       IsFollowUp  { get; init; }

        public MedicalRecord(string diagnosis, string treatment,
                             string medication, Doctor issuedBy,
                             RecordType type = RecordType.Diagnosis,
                             string notes = "")
        {
            if (string.IsNullOrWhiteSpace(diagnosis))
                throw new ArgumentException("Diagnosis cannot be empty.");

            ArgumentNullException.ThrowIfNull(issuedBy);

            Id         = _nextId++;
            Diagnosis  = diagnosis.Trim();
            Treatment  = treatment.Trim();
            Medication = medication.Trim();
            IssuedBy   = issuedBy;
            Type       = type;
            Notes      = notes.Trim();
            CreatedAt  = DateTime.Now;
        }

        public string GetTypeIcon() => Type switch
        {
            RecordType.Diagnosis     => "[DX]",
            RecordType.Surgery       => "[SX]",
            RecordType.LabResult     => "[LB]",
            RecordType.Prescription  => "[RX]",
            RecordType.Consultation  => "[CX]",
            RecordType.Emergency     => "[ER]",
            _                        => "[??]"
        };

        public string GenerateReport() =>
            $"""
             ╔══ MEDICAL RECORD ══════════════════════════════╗
               Record ID  : #{Id}
               Type       : {GetTypeIcon()} {Type}
               Date       : {CreatedAt:dd MMMM yyyy  HH:mm}
               Physician  : Dr. {IssuedBy.FullName}  ({IssuedBy.Specialization})
               Diagnosis  : {Diagnosis}
               Treatment  : {Treatment}
               Medication : {(string.IsNullOrEmpty(Medication) ? "None prescribed" : Medication)}
               Notes      : {(string.IsNullOrEmpty(Notes) ? "—" : Notes)}
               Follow-Up  : {(IsFollowUp ? "Required" : "Not required")}
             ╚════════════════════════════════════════════════╝
             """;

        public override string ToString() =>
            $"  {GetTypeIcon()} #{Id}  [{CreatedAt:dd/MM/yyyy}]  {Diagnosis,-30}  Dr.{IssuedBy.FullName}";
    }
}
