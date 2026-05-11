using HospitalApp.Interfaces;
namespace HospitalApp.Models
{

    public class Department : IReportable
    {
        private readonly List<Doctor> _doctors = new();

        public string Name        { get; }
        public int    Capacity    { get; }
        public string HeadDoctor  { get; private set; } = "Not assigned";
        public string Floor       { get; set; } = "N/A";
        public string PhoneExt    { get; set; } = "N/A";

        public IReadOnlyList<Doctor> Doctors     => _doctors.AsReadOnly();
        public int   DoctorCount                 => _doctors.Count;
        public bool  IsFull                      => _doctors.Count >= Capacity;
        public float OccupancyRate               => Capacity == 0 ? 0 : (float)DoctorCount / Capacity * 100;

        public Department(string name, int capacity, string floor = "N/A")
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Department name cannot be empty.");
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive.");

            Name     = name.Trim();
            Capacity = capacity;
            Floor    = floor;
        }

        public bool AddDoctor(Doctor doctor)
        {
            ArgumentNullException.ThrowIfNull(doctor);
            if (IsFull) return false;
            if (_doctors.Any(d => d.Id == doctor.Id)) return false;

            _doctors.Add(doctor);
            doctor.Department = Name;
            return true;
        }

        public bool RemoveDoctor(int doctorId)
        {
            var doc = _doctors.FirstOrDefault(d => d.Id == doctorId);
            if (doc is null) return false;
            _doctors.Remove(doc);
            doc.Department = "Unassigned";
            return true;
        }

        public void SetHeadDoctor(int doctorId)
        {
            var doc = _doctors.FirstOrDefault(d => d.Id == doctorId)
                      ?? throw new KeyNotFoundException($"Doctor #{doctorId} not in this department.");
            HeadDoctor = doc.FullName;
        }

        public Doctor? FindDoctor(string name) =>
            _doctors.FirstOrDefault(d =>
                d.FullName.Contains(name, StringComparison.OrdinalIgnoreCase));

        public IEnumerable<Doctor> GetDoctorsBySpecialization(string spec) =>
            _doctors.Where(d =>
                d.Specialization.Contains(spec, StringComparison.OrdinalIgnoreCase));

        public string GetOccupancyBar()
        {
            int filled = (int)(OccupancyRate / 10);
            return "[" + new string('#', filled) + new string('-', 10 - filled) + $"] {OccupancyRate:F0}%";
        }

        public string GenerateReport()
        {
            var doctorList = _doctors.Any()
                ? string.Join("\n", _doctors.Select(d => $"    - Dr. {d.FullName} ({d.Specialization})"))
                : "    No doctors assigned.";

            return $"""
             ╔══ DEPARTMENT REPORT ═══════════════════════════╗
               Name        : {Name}
               Floor       : {Floor}
               Phone Ext   : {PhoneExt}
               Head Doctor : {HeadDoctor}
               Occupancy   : {DoctorCount}/{Capacity}  {GetOccupancyBar()}
               Doctors     :
             {doctorList}
             ╚════════════════════════════════════════════════╝
             """;
        }

        public override string ToString() =>
            $"  [DEPT] {Name,-28}  Floor:{Floor,-4}  Doctors:{DoctorCount}/{Capacity}  {GetOccupancyBar()}";
    }
}
