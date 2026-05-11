using HospitalApp.Models;

namespace HospitalApp.Services
{
   
    public class HospitalService
    {
        // ── Private Collections ─────────────────────────────────────────────────
        private readonly List<Doctor>      _doctors      = new();
        private readonly List<Patient>     _patients     = new();
        private readonly List<Nurse>       _nurses       = new();
        private readonly List<Appointment> _appointments = new();
        private readonly List<Department>  _departments  = new();

        public string HospitalName { get; }
        public DateTime SystemStarted { get; } = DateTime.Now;

        // ── Constructor ─────────────────────────────────────────────────────────
        public HospitalService(string hospitalName)
        {
            if (string.IsNullOrWhiteSpace(hospitalName))
                throw new ArgumentException("Hospital name cannot be empty.");
            HospitalName = hospitalName;
        }

        // ════════════════════════════════════════════════════════════════════════
        // DOCTOR OPERATIONS
        // ════════════════════════════════════════════════════════════════════════

        public Doctor AddDoctor(string fullName, int age, string phone,
                                string specialization, decimal salary,
                                int yearsOfService = 0, string email = "")
        {
            var doctor = new Doctor(fullName, age, phone, specialization,
                                    salary, yearsOfService, email);
            _doctors.Add(doctor);
            return doctor;
        }

        public Doctor GetDoctor(int id) =>
            _doctors.FirstOrDefault(d => d.Id == id)
            ?? throw new KeyNotFoundException($"Doctor #{id} not found.");

        public Doctor? FindDoctor(string name) =>
            _doctors.FirstOrDefault(d =>
                d.FullName.Contains(name, StringComparison.OrdinalIgnoreCase));

        public bool RemoveDoctor(int id)
        {
            var doc = _doctors.FirstOrDefault(d => d.Id == id);
            if (doc is null) return false;

            // Remove from department
            var dept = _departments.FirstOrDefault(d =>
                d.Doctors.Any(dr => dr.Id == id));
            dept?.RemoveDoctor(id);

            // Cancel future appointments
            _appointments
                .Where(a => a.Doctor.Id == id &&
                            a.Status is AppointmentStatus.Pending or AppointmentStatus.Confirmed)
                .ToList()
                .ForEach(a => a.Cancel("Doctor removed from system."));

            _doctors.Remove(doc);
            return true;
        }

        public IReadOnlyList<Doctor>      GetAllDoctors()      => _doctors.AsReadOnly();
        public IEnumerable<Doctor>        GetDoctorsBySpec(string spec) =>
            _doctors.Where(d => d.Specialization.Contains(spec, StringComparison.OrdinalIgnoreCase));

        // ════════════════════════════════════════════════════════════════════════
        // PATIENT OPERATIONS
        // ════════════════════════════════════════════════════════════════════════

        public Patient AddPatient(string fullName, int age, string phone,
                                  string bloodType, string email = "")
        {
            var patient = new Patient(fullName, age, phone, bloodType, email);
            _patients.Add(patient);
            return patient;
        }

        public Patient GetPatient(int id) =>
            _patients.FirstOrDefault(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Patient #{id} not found.");

        public Patient? FindPatient(string name) =>
            _patients.FirstOrDefault(p =>
                p.FullName.Contains(name, StringComparison.OrdinalIgnoreCase));

        public bool DischargePatient(int id)
        {
            var patient = _patients.FirstOrDefault(p => p.Id == id);
            if (patient is null || !patient.IsInpatient) return false;
            patient.Discharge();
            return true;
        }

        public IReadOnlyList<Patient>   GetAllPatients()     => _patients.AsReadOnly();
        public IEnumerable<Patient>     GetInpatients()      =>
            _patients.Where(p => p.IsInpatient);
        public IEnumerable<Patient>     GetCriticalPatients() =>
            _patients.Where(p => p.IsCritical);

        // ════════════════════════════════════════════════════════════════════════
        // NURSE OPERATIONS
        // ════════════════════════════════════════════════════════════════════════

        public Nurse AddNurse(string fullName, int age, string phone,
                              string ward, ShiftType shift,
                              string qualification = "RN", string email = "")
        {
            var nurse = new Nurse(fullName, age, phone, ward, shift, qualification, email);
            _nurses.Add(nurse);
            return nurse;
        }

        public Nurse GetNurse(int id) =>
            _nurses.FirstOrDefault(n => n.Id == id)
            ?? throw new KeyNotFoundException($"Nurse #{id} not found.");

        public IReadOnlyList<Nurse>    GetAllNurses()        => _nurses.AsReadOnly();
        public IEnumerable<Nurse>      GetNursesByShift(ShiftType shift) =>
            _nurses.Where(n => n.Shift == shift);

        // ════════════════════════════════════════════════════════════════════════
        // APPOINTMENT OPERATIONS
        // ════════════════════════════════════════════════════════════════════════

        public Appointment BookAppointment(int doctorId, int patientId,
                                           DateTime dateTime, string reason)
        {
            var doctor  = GetDoctor(doctorId);
            var patient = GetPatient(patientId);

            // Business rule: patient cannot have two appointments same day
            bool clash = _appointments.Any(a =>
                a.Patient.Id == patientId &&
                a.DateTime.Date == dateTime.Date &&
                a.Status is not (AppointmentStatus.Cancelled or AppointmentStatus.NoShow));

            if (clash)
                throw new InvalidOperationException(
                    $"{patient.FullName} already has an appointment on {dateTime:dd/MM/yyyy}.");

            var appt = new Appointment(doctor, patient, dateTime, reason);
            _appointments.Add(appt);
            return appt;
        }

        public Appointment GetAppointment(int id) =>
            _appointments.FirstOrDefault(a => a.Id == id)
            ?? throw new KeyNotFoundException($"Appointment #{id} not found.");

        public IReadOnlyList<Appointment> GetAllAppointments()   => _appointments.AsReadOnly();
        public IEnumerable<Appointment>   GetTodayAppointments() =>
            _appointments.Where(a => a.DateTime.Date == DateTime.Today)
                         .OrderBy(a => a.DateTime);
        public IEnumerable<Appointment>   GetUpcoming()          =>
            _appointments.Where(a => a.DateTime >= DateTime.Now &&
                                     a.Status is AppointmentStatus.Pending
                                              or AppointmentStatus.Confirmed)
                         .OrderBy(a => a.DateTime);
        public IEnumerable<Appointment>   GetByDoctor(int docId) =>
            _appointments.Where(a => a.Doctor.Id == docId).OrderBy(a => a.DateTime);
        public IEnumerable<Appointment>   GetByPatient(int patId) =>
            _appointments.Where(a => a.Patient.Id == patId).OrderBy(a => a.DateTime);

        // ════════════════════════════════════════════════════════════════════════
        // MEDICAL RECORD OPERATIONS
        // ════════════════════════════════════════════════════════════════════════

        public MedicalRecord AddMedicalRecord(int patientId, int doctorId,
                                              string diagnosis, string treatment,
                                              string medication,
                                              RecordType type = RecordType.Diagnosis,
                                              string notes = "")
        {
            var patient = GetPatient(patientId);
            var doctor  = GetDoctor(doctorId);
            var record  = new MedicalRecord(diagnosis, treatment, medication,
                                            doctor, type, notes);
            patient.AddRecord(record);
            return record;
        }

        // ════════════════════════════════════════════════════════════════════════
        // DEPARTMENT OPERATIONS
        // ════════════════════════════════════════════════════════════════════════

        public Department AddDepartment(string name, int capacity, string floor = "N/A")
        {
            if (_departments.Any(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Department '{name}' already exists.");

            var dept = new Department(name, capacity, floor);
            _departments.Add(dept);
            return dept;
        }

        public Department GetDepartment(string name) =>
            _departments.FirstOrDefault(d =>
                d.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            ?? throw new KeyNotFoundException($"Department '{name}' not found.");

        public bool AssignDoctorToDepartment(int doctorId, string deptName)
        {
            var dept = GetDepartment(deptName);
            var doc  = GetDoctor(doctorId);
            return dept.AddDoctor(doc);
        }

        public IReadOnlyList<Department> GetAllDepartments() => _departments.AsReadOnly();

        // ════════════════════════════════════════════════════════════════════════
        // STATISTICS & REPORTS
        // ════════════════════════════════════════════════════════════════════════

        public string GenerateSystemReport()
        {
            int totalAppts     = _appointments.Count;
            int pendingAppts   = _appointments.Count(a => a.Status == AppointmentStatus.Pending);
            int confirmedAppts = _appointments.Count(a => a.Status == AppointmentStatus.Confirmed);
            int completedAppts = _appointments.Count(a => a.Status == AppointmentStatus.Completed);
            int cancelledAppts = _appointments.Count(a => a.Status == AppointmentStatus.Cancelled);
            int inpatients     = _patients.Count(p => p.IsInpatient);
            int critical       = _patients.Count(p => p.IsCritical);

            var busyDoctor = _appointments
                .GroupBy(a => a.Doctor.FullName)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            return $"""
             ╔══ SYSTEM STATISTICS ════════════════════════════════════════╗

               Hospital    : {HospitalName}
               System Up   : {SystemStarted:dd/MM/yyyy HH:mm}

               STAFF
               ─────────────────────────────────────────────
               Total Doctors    : {_doctors.Count}
               Total Nurses     : {_nurses.Count}
               Departments      : {_departments.Count}

               PATIENTS
               ─────────────────────────────────────────────
               Total Registered : {_patients.Count}
               Currently Admitted: {inpatients}
               Critical         : {critical}

               APPOINTMENTS
               ─────────────────────────────────────────────
               Total            : {totalAppts}
               Pending          : {pendingAppts}
               Confirmed        : {confirmedAppts}
               Completed        : {completedAppts}
               Cancelled        : {cancelledAppts}
               Busiest Doctor   : {busyDoctor?.Key ?? "N/A"} ({busyDoctor?.Count() ?? 0} appts)

             ╚══════════════════════════════════════════════════════════════╝
             """;
        }
    }
}
