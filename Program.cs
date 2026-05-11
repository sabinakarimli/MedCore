using HospitalApp.Data;
using HospitalApp.Helpers;
using HospitalApp.Models;
using HospitalApp.Services;

// ── Bootstrap ────────────────────────────────────────────────────────────────
var hospital = new HospitalService("Baku Clinical Hospital");

ConsoleUI.Banner();
ConsoleUI.Spinner("Loading hospital data...", 1200);
SeedData.Populate(hospital);
ConsoleUI.WriteSuccess("System ready. Demo data loaded.");
ConsoleUI.PressAnyKey();

// ── Main Loop ────────────────────────────────────────────────────────────────
while (true)
{
    ConsoleUI.Banner();
    ConsoleUI.Menu("MAIN MENU",
        ("1", "Doctor Management"),
        ("2", "Patient Management"),
        ("3", "Nurse Management"),
        ("4", "Appointment Management"),
        ("5", "Department Management"),
        ("6", "Medical Records"),
        ("7", "Statistics & Reports"),
        ("0", "Exit System"));

    switch (Console.ReadLine()?.Trim())
    {
        case "1": DoctorMenu();      break;
        case "2": PatientMenu();     break;
        case "3": NurseMenu();       break;
        case "4": AppointmentMenu(); break;
        case "5": DepartmentMenu();  break;
        case "6": RecordsMenu();     break;
        case "7": StatsMenu();       break;
        case "0":
            ConsoleUI.Notify("Thank you for using MedCore HMS. Goodbye!", ConsoleColor.Cyan);
            return;
        default:
            ConsoleUI.WriteError("Invalid option. Please try again.");
            ConsoleUI.PressAnyKey();
            break;
    }
}

// ════════════════════════════════════════════════════════════════════════════
// DOCTOR MENU
// ════════════════════════════════════════════════════════════════════════════
void DoctorMenu()
{
    while (true)
    {
        ConsoleUI.SectionHeader("Doctor Management", "Manage hospital physicians");
        ConsoleUI.Menu("OPTIONS",
            ("1", "Add New Doctor"),
            ("2", "List All Doctors"),
            ("3", "View Doctor Details"),
            ("4", "View Doctor Schedule"),
            ("5", "Search Doctor by Name"),
            ("6", "Remove Doctor"),
            ("B", "Back to Main Menu"));

        switch (Console.ReadLine()?.Trim().ToUpper())
        {
            case "1": AddDoctor();            break;
            case "2": ListDoctors();          break;
            case "3": ViewDoctorDetails();    break;
            case "4": ViewDoctorSchedule();   break;
            case "5": SearchDoctor();         break;
            case "6": RemoveDoctor();         break;
            case "B": return;
            default:
                ConsoleUI.WriteError("Invalid option.");
                ConsoleUI.PressAnyKey();
                break;
        }
    }
}

void AddDoctor()
{
    ConsoleUI.SubHeader("Add New Doctor");
    try
    {
        string name  = ConsoleUI.Prompt("Full Name");
        int    age   = ConsoleUI.PromptInt("Age", 18, 80);
        string phone = ConsoleUI.Prompt("Phone");
        string spec  = ConsoleUI.Prompt("Specialization");
        decimal sal  = ConsoleUI.PromptDecimal("Salary (USD)");
        int    yrs   = ConsoleUI.PromptInt("Years of Service", 0, 60);
        string email = ConsoleUI.Prompt("Email (optional)");

        ConsoleUI.Spinner("Registering doctor...");
        var doc = hospital.AddDoctor(name, age, phone, spec, sal, yrs, email);
        ConsoleUI.WriteSuccess($"Doctor registered successfully. ID: #{doc.Id:D4}");
    }
    catch (Exception ex) { ConsoleUI.WriteError(ex.Message); }
    ConsoleUI.PressAnyKey();
}

void ListDoctors()
{
    ConsoleUI.SubHeader("All Registered Doctors");
    ConsoleUI.Table(hospital.GetAllDoctors());
    ConsoleUI.PressAnyKey();
}

void ViewDoctorDetails()
{
    ConsoleUI.SubHeader("Doctor Details");
    try
    {
        int id = ConsoleUI.PromptInt("Doctor ID");
        var doc = hospital.GetDoctor(id);
        ConsoleUI.BlankLine();
        ConsoleUI.WriteLine(doc.GenerateReport(), ConsoleUI.Accent);
    }
    catch (Exception ex) { ConsoleUI.WriteError(ex.Message); }
    ConsoleUI.PressAnyKey();
}

void ViewDoctorSchedule()
{
    ConsoleUI.SubHeader("Doctor Schedule");
    try
    {
        int id  = ConsoleUI.PromptInt("Doctor ID");
        var doc = hospital.GetDoctor(id);
        ConsoleUI.SubHeader($"Schedule: Dr. {doc.FullName}");
        ConsoleUI.WriteLine(doc.GetScheduleSummary(), ConsoleUI.Accent);
    }
    catch (Exception ex) { ConsoleUI.WriteError(ex.Message); }
    ConsoleUI.PressAnyKey();
}

void SearchDoctor()
{
    ConsoleUI.SubHeader("Search Doctor");
    string name = ConsoleUI.Prompt("Enter name (partial)");
    var doc = hospital.FindDoctor(name);
    if (doc is null) ConsoleUI.WriteWarning("No doctor found.");
    else ConsoleUI.WriteLine(doc.GenerateReport(), ConsoleUI.Accent);
    ConsoleUI.PressAnyKey();
}

void RemoveDoctor()
{
    ConsoleUI.SubHeader("Remove Doctor");
    try
    {
        int id = ConsoleUI.PromptInt("Doctor ID to remove");
        var doc = hospital.GetDoctor(id);
        if (!ConsoleUI.Confirm($"Remove Dr. {doc.FullName}? This will cancel their appointments."))
        {
            ConsoleUI.WriteInfo("Operation cancelled."); ConsoleUI.PressAnyKey(); return;
        }
        ConsoleUI.Spinner("Removing...");
        bool ok = hospital.RemoveDoctor(id);
        if (ok) ConsoleUI.WriteSuccess("Doctor removed successfully.");
        else    ConsoleUI.WriteError("Could not remove doctor.");
    }
    catch (Exception ex) { ConsoleUI.WriteError(ex.Message); }
    ConsoleUI.PressAnyKey();
}

// ════════════════════════════════════════════════════════════════════════════
// PATIENT MENU
// ════════════════════════════════════════════════════════════════════════════
void PatientMenu()
{
    while (true)
    {
        ConsoleUI.SectionHeader("Patient Management", "Manage hospital patients");
        ConsoleUI.Menu("OPTIONS",
            ("1", "Register New Patient"),
            ("2", "List All Patients"),
            ("3", "View Patient Details"),
            ("4", "Admit Patient to Ward"),
            ("5", "Discharge Patient"),
            ("6", "View Critical Patients"),
            ("7", "Search Patient by Name"),
            ("B", "Back to Main Menu"));

        switch (Console.ReadLine()?.Trim().ToUpper())
        {
            case "1": AddPatient();          break;
            case "2": ListPatients();        break;
            case "3": ViewPatientDetails();  break;
            case "4": AdmitPatient();        break;
            case "5": DischargePatient();    break;
            case "6": ListCritical();        break;
            case "7": SearchPatient();       break;
            case "B": return;
            default:
                ConsoleUI.WriteError("Invalid option.");
                ConsoleUI.PressAnyKey();
                break;
        }
    }
}

void AddPatient()
{
    ConsoleUI.SubHeader("Register New Patient");
    try
    {
        string name     = ConsoleUI.Prompt("Full Name");
        int    age      = ConsoleUI.PromptInt("Age", 0, 120);
        string phone    = ConsoleUI.Prompt("Phone");
        string blood    = ConsoleUI.Prompt("Blood Type (e.g. A+, O-)");
        string email    = ConsoleUI.Prompt("Email (optional)");
        string emg      = ConsoleUI.Prompt("Emergency Contact");
        string insId    = ConsoleUI.Prompt("Insurance ID (optional)");
        string allergies = ConsoleUI.Prompt("Allergies (comma-separated, or leave blank)");

        ConsoleUI.Spinner("Registering patient...");
        var patient = hospital.AddPatient(name, age, phone, blood, email);
        patient.EmergencyContact = emg;
        if (!string.IsNullOrWhiteSpace(insId)) patient.InsuranceId = insId;
        if (!string.IsNullOrWhiteSpace(allergies))
            foreach (var a in allergies.Split(','))
                patient.AddAllergy(a);

        ConsoleUI.WriteSuccess($"Patient registered. ID: #{patient.Id:D4}");
    }
    catch (Exception ex) { ConsoleUI.WriteError(ex.Message); }
    ConsoleUI.PressAnyKey();
}

void ListPatients()
{
    ConsoleUI.SubHeader("All Registered Patients");
    ConsoleUI.Table(hospital.GetAllPatients());
    ConsoleUI.PressAnyKey();
}

void ViewPatientDetails()
{
    ConsoleUI.SubHeader("Patient Details");
    try
    {
        int id = ConsoleUI.PromptInt("Patient ID");
        ConsoleUI.BlankLine();
        ConsoleUI.WriteLine(hospital.GetPatient(id).GenerateReport(), ConsoleUI.Accent);
    }
    catch (Exception ex) { ConsoleUI.WriteError(ex.Message); }
    ConsoleUI.PressAnyKey();
}

void AdmitPatient()
{
    ConsoleUI.SubHeader("Admit Patient");
    try
    {
        int    id       = ConsoleUI.PromptInt("Patient ID");
        string ward     = ConsoleUI.Prompt("Ward Name");
        bool   critical = ConsoleUI.Confirm("Mark as critical?");
        hospital.GetPatient(id).Admit(ward, critical);
        ConsoleUI.WriteSuccess("Patient admitted successfully.");
    }
    catch (Exception ex) { ConsoleUI.WriteError(ex.Message); }
    ConsoleUI.PressAnyKey();
}

void DischargePatient()
{
    ConsoleUI.SubHeader("Discharge Patient");
    try
    {
        int id = ConsoleUI.PromptInt("Patient ID");
        ConsoleUI.Spinner("Processing discharge...");
        bool ok = hospital.DischargePatient(id);
        if (ok) ConsoleUI.WriteSuccess("Patient discharged.");
        else    ConsoleUI.WriteWarning("Patient is not currently admitted.");
    }
    catch (Exception ex) { ConsoleUI.WriteError(ex.Message); }
    ConsoleUI.PressAnyKey();
}

void ListCritical()
{
    ConsoleUI.SubHeader("Critical Patients");
    ConsoleUI.Table(hospital.GetCriticalPatients(), "No critical patients.");
    ConsoleUI.PressAnyKey();
}

void SearchPatient()
{
    ConsoleUI.SubHeader("Search Patient");
    string name = ConsoleUI.Prompt("Name (partial)");
    var p = hospital.FindPatient(name);
    if (p is null) ConsoleUI.WriteWarning("No patient found.");
    else ConsoleUI.WriteLine(p.GenerateReport(), ConsoleUI.Accent);
    ConsoleUI.PressAnyKey();
}

// ════════════════════════════════════════════════════════════════════════════
// NURSE MENU
// ════════════════════════════════════════════════════════════════════════════
void NurseMenu()
{
    while (true)
    {
        ConsoleUI.SectionHeader("Nurse Management");
        ConsoleUI.Menu("OPTIONS",
            ("1", "Add New Nurse"),
            ("2", "List All Nurses"),
            ("3", "View Nurse Details"),
            ("4", "List by Shift"),
            ("5", "Assign Task to Nurse"),
            ("B", "Back"));

        switch (Console.ReadLine()?.Trim().ToUpper())
        {
            case "1": AddNurse();           break;
            case "2": ListNurses();         break;
            case "3": ViewNurseDetails();   break;
            case "4": ListByShift();        break;
            case "5": AssignTask();         break;
            case "B": return;
            default:
                ConsoleUI.WriteError("Invalid option.");
                ConsoleUI.PressAnyKey();
                break;
        }
    }
}

void AddNurse()
{
    ConsoleUI.SubHeader("Add New Nurse");
    try
    {
        string name  = ConsoleUI.Prompt("Full Name");
        int    age   = ConsoleUI.PromptInt("Age", 18, 70);
        string phone = ConsoleUI.Prompt("Phone");
        string ward  = ConsoleUI.Prompt("Ward Assignment");
        ConsoleUI.WriteLine("  Shift options: 1=Morning  2=Evening  3=Night");
        int shiftChoice = ConsoleUI.PromptInt("Select Shift", 1, 3);
        var shift = shiftChoice switch { 1 => ShiftType.Morning, 2 => ShiftType.Evening, _ => ShiftType.Night };
        string qual  = ConsoleUI.Prompt("Qualification (RN/BSN)");

        ConsoleUI.Spinner("Registering nurse...");
        var nurse = hospital.AddNurse(name, age, phone, ward, shift, qual);
        ConsoleUI.WriteSuccess($"Nurse registered. ID: #{nurse.Id:D4}");
    }
    catch (Exception ex) { ConsoleUI.WriteError(ex.Message); }
    ConsoleUI.PressAnyKey();
}

void ListNurses()
{
    ConsoleUI.SubHeader("All Nurses");
    ConsoleUI.Table(hospital.GetAllNurses());
    ConsoleUI.PressAnyKey();
}

void ViewNurseDetails()
{
    ConsoleUI.SubHeader("Nurse Details");
    try
    {
        int id = ConsoleUI.PromptInt("Nurse ID");
        ConsoleUI.WriteLine(hospital.GetNurse(id).GenerateReport(), ConsoleUI.Accent);
    }
    catch (Exception ex) { ConsoleUI.WriteError(ex.Message); }
    ConsoleUI.PressAnyKey();
}

void ListByShift()
{
    ConsoleUI.SubHeader("Nurses by Shift");
    ConsoleUI.WriteLine("  1=Morning  2=Evening  3=Night");
    int c = ConsoleUI.PromptInt("Select", 1, 3);
    var shift = c switch { 1 => ShiftType.Morning, 2 => ShiftType.Evening, _ => ShiftType.Night };
    ConsoleUI.Table(hospital.GetNursesByShift(shift), $"No nurses on {shift} shift.");
    ConsoleUI.PressAnyKey();
}

void AssignTask()
{
    ConsoleUI.SubHeader("Assign Task");
    try
    {
        int    id   = ConsoleUI.PromptInt("Nurse ID");
        string task = ConsoleUI.Prompt("Task description");
        hospital.GetNurse(id).AssignTask(task);
        ConsoleUI.WriteSuccess("Task assigned.");
    }
    catch (Exception ex) { ConsoleUI.WriteError(ex.Message); }
    ConsoleUI.PressAnyKey();
}

// ════════════════════════════════════════════════════════════════════════════
// APPOINTMENT MENU
// ════════════════════════════════════════════════════════════════════════════
void AppointmentMenu()
{
    while (true)
    {
        ConsoleUI.SectionHeader("Appointment Management");
        ConsoleUI.Menu("OPTIONS",
            ("1", "Book New Appointment"),
            ("2", "List All Appointments"),
            ("3", "Today's Appointments"),
            ("4", "Upcoming Appointments"),
            ("5", "View Appointment Details"),
            ("6", "Update Appointment Status"),
            ("7", "Appointments by Doctor"),
            ("8", "Appointments by Patient"),
            ("B", "Back"));

        switch (Console.ReadLine()?.Trim().ToUpper())
        {
            case "1": BookAppointment();       break;
            case "2": ListAppointments();      break;
            case "3": TodaysAppointments();    break;
            case "4": UpcomingAppointments();  break;
            case "5": ViewAppointment();       break;
            case "6": UpdateStatus();          break;
            case "7": AppointmentsByDoctor();  break;
            case "8": AppointmentsByPatient(); break;
            case "B": return;
            default:
                ConsoleUI.WriteError("Invalid option.");
                ConsoleUI.PressAnyKey();
                break;
        }
    }
}

void BookAppointment()
{
    ConsoleUI.SubHeader("Book New Appointment");
    try
    {
        ConsoleUI.SubHeader("Available Doctors");
        ConsoleUI.Table(hospital.GetAllDoctors());

        int      docId  = ConsoleUI.PromptInt("Doctor ID");
        int      patId  = ConsoleUI.PromptInt("Patient ID");
        DateTime dt     = ConsoleUI.PromptDateTime("Appointment Date & Time");
        string   reason = ConsoleUI.Prompt("Reason for visit");

        ConsoleUI.Spinner("Checking availability...");
        var appt = hospital.BookAppointment(docId, patId, dt, reason);
        ConsoleUI.WriteSuccess($"Appointment booked. ID: #{appt.Id}");
    }
    catch (Exception ex) { ConsoleUI.WriteError(ex.Message); }
    ConsoleUI.PressAnyKey();
}

void ListAppointments()
{
    ConsoleUI.SubHeader("All Appointments");
    ConsoleUI.Table(hospital.GetAllAppointments());
    ConsoleUI.PressAnyKey();
}

void TodaysAppointments()
{
    ConsoleUI.SubHeader($"Today's Appointments — {DateTime.Today:dd MMMM yyyy}");
    ConsoleUI.Table(hospital.GetTodayAppointments(), "No appointments today.");
    ConsoleUI.PressAnyKey();
}

void UpcomingAppointments()
{
    ConsoleUI.SubHeader("Upcoming Appointments");
    ConsoleUI.Table(hospital.GetUpcoming(), "No upcoming appointments.");
    ConsoleUI.PressAnyKey();
}

void ViewAppointment()
{
    ConsoleUI.SubHeader("Appointment Details");
    try
    {
        int id = ConsoleUI.PromptInt("Appointment ID");
        ConsoleUI.WriteLine(hospital.GetAppointment(id).GenerateReport(), ConsoleUI.Accent);
    }
    catch (Exception ex) { ConsoleUI.WriteError(ex.Message); }
    ConsoleUI.PressAnyKey();
}

void UpdateStatus()
{
    ConsoleUI.SubHeader("Update Appointment Status");
    try
    {
        int  id   = ConsoleUI.PromptInt("Appointment ID");
        var  appt = hospital.GetAppointment(id);

        ConsoleUI.WriteLine($"  Current status: {appt.GetStatusIcon()} {appt.Status}", ConsoleUI.Warning);
        ConsoleUI.Menu("NEW STATUS",
            ("1", "Confirm"),
            ("2", "Start (In Progress)"),
            ("3", "Complete"),
            ("4", "Cancel"),
            ("5", "Mark No-Show"));

        switch (Console.ReadLine()?.Trim())
        {
            case "1": appt.Confirm();
                ConsoleUI.WriteSuccess("Appointment confirmed."); break;
            case "2": appt.Start();
                ConsoleUI.WriteSuccess("Appointment started."); break;
            case "3":
                string notes = ConsoleUI.Prompt("Completion notes (optional)");
                appt.Complete(notes);
                ConsoleUI.WriteSuccess("Appointment completed."); break;
            case "4":
                string reason = ConsoleUI.Prompt("Cancellation reason");
                appt.Cancel(reason);
                ConsoleUI.WriteSuccess("Appointment cancelled."); break;
            case "5": appt.MarkNoShow();
                ConsoleUI.WriteSuccess("Marked as no-show."); break;
            default:  ConsoleUI.WriteError("Invalid option."); break;
        }
    }
    catch (Exception ex) { ConsoleUI.WriteError(ex.Message); }
    ConsoleUI.PressAnyKey();
}

void AppointmentsByDoctor()
{
    ConsoleUI.SubHeader("Appointments by Doctor");
    try
    {
        int id = ConsoleUI.PromptInt("Doctor ID");
        ConsoleUI.Table(hospital.GetByDoctor(id), "No appointments for this doctor.");
    }
    catch (Exception ex) { ConsoleUI.WriteError(ex.Message); }
    ConsoleUI.PressAnyKey();
}

void AppointmentsByPatient()
{
    ConsoleUI.SubHeader("Appointments by Patient");
    try
    {
        int id = ConsoleUI.PromptInt("Patient ID");
        ConsoleUI.Table(hospital.GetByPatient(id), "No appointments for this patient.");
    }
    catch (Exception ex) { ConsoleUI.WriteError(ex.Message); }
    ConsoleUI.PressAnyKey();
}

// ════════════════════════════════════════════════════════════════════════════
// DEPARTMENT MENU
// ════════════════════════════════════════════════════════════════════════════
void DepartmentMenu()
{
    while (true)
    {
        ConsoleUI.SectionHeader("Department Management");
        ConsoleUI.Menu("OPTIONS",
            ("1", "Add New Department"),
            ("2", "List All Departments"),
            ("3", "View Department Details"),
            ("4", "Assign Doctor to Department"),
            ("5", "Remove Doctor from Department"),
            ("B", "Back"));

        switch (Console.ReadLine()?.Trim().ToUpper())
        {
            case "1": AddDepartment();        break;
            case "2": ListDepartments();      break;
            case "3": ViewDepartment();       break;
            case "4": AssignDoctor();         break;
            case "5": RemoveDoctorFromDept(); break;
            case "B": return;
            default:
                ConsoleUI.WriteError("Invalid option.");
                ConsoleUI.PressAnyKey();
                break;
        }
    }
}

void AddDepartment()
{
    ConsoleUI.SubHeader("Add New Department");
    try
    {
        string name  = ConsoleUI.Prompt("Department Name");
        int cap      = ConsoleUI.PromptInt("Maximum Capacity", 1, 50);
        string floor = ConsoleUI.Prompt("Floor");
        ConsoleUI.Spinner("Creating department...");
        var dept = hospital.AddDepartment(name, cap, floor);
        dept.PhoneExt = ConsoleUI.Prompt("Phone Extension");
        ConsoleUI.WriteSuccess($"Department '{dept.Name}' created.");
    }
    catch (Exception ex) { ConsoleUI.WriteError(ex.Message); }
    ConsoleUI.PressAnyKey();
}

void ListDepartments()
{
    ConsoleUI.SubHeader("All Departments");
    ConsoleUI.Table(hospital.GetAllDepartments());
    ConsoleUI.PressAnyKey();
}

void ViewDepartment()
{
    ConsoleUI.SubHeader("Department Details");
    try
    {
        string name = ConsoleUI.Prompt("Department name (partial)");
        ConsoleUI.WriteLine(hospital.GetDepartment(name).GenerateReport(), ConsoleUI.Accent);
    }
    catch (Exception ex) { ConsoleUI.WriteError(ex.Message); }
    ConsoleUI.PressAnyKey();
}

void AssignDoctor()
{
    ConsoleUI.SubHeader("Assign Doctor to Department");
    try
    {
        int    id   = ConsoleUI.PromptInt("Doctor ID");
        string dept = ConsoleUI.Prompt("Department Name");
        ConsoleUI.Spinner("Assigning...");
        bool ok = hospital.AssignDoctorToDepartment(id, dept);
        if (ok) ConsoleUI.WriteSuccess("Doctor assigned successfully.");
        else    ConsoleUI.WriteWarning("Could not assign. Department may be full or doctor already assigned.");
    }
    catch (Exception ex) { ConsoleUI.WriteError(ex.Message); }
    ConsoleUI.PressAnyKey();
}

void RemoveDoctorFromDept()
{
    ConsoleUI.SubHeader("Remove Doctor from Department");
    try
    {
        string deptName = ConsoleUI.Prompt("Department Name");
        int    docId    = ConsoleUI.PromptInt("Doctor ID");
        var    dept     = hospital.GetDepartment(deptName);
        bool   ok       = dept.RemoveDoctor(docId);
        if (ok) ConsoleUI.WriteSuccess("Doctor removed from department.");
        else    ConsoleUI.WriteWarning("Doctor not found in this department.");
    }
    catch (Exception ex) { ConsoleUI.WriteError(ex.Message); }
    ConsoleUI.PressAnyKey();
}

// ════════════════════════════════════════════════════════════════════════════
// MEDICAL RECORDS MENU
// ════════════════════════════════════════════════════════════════════════════
void RecordsMenu()
{
    while (true)
    {
        ConsoleUI.SectionHeader("Medical Records");
        ConsoleUI.Menu("OPTIONS",
            ("1", "Add Medical Record"),
            ("2", "View Patient Records"),
            ("3", "View Latest Record"),
            ("B", "Back"));

        switch (Console.ReadLine()?.Trim().ToUpper())
        {
            case "1": AddRecord();         break;
            case "2": ViewRecords();       break;
            case "3": ViewLatestRecord();  break;
            case "B": return;
            default:
                ConsoleUI.WriteError("Invalid option.");
                ConsoleUI.PressAnyKey();
                break;
        }
    }
}

void AddRecord()
{
    ConsoleUI.SubHeader("Add Medical Record");
    try
    {
        int patId  = ConsoleUI.PromptInt("Patient ID");
        int docId  = ConsoleUI.PromptInt("Doctor ID");
        ConsoleUI.WriteLine("  Types: 1=Diagnosis 2=Surgery 3=LabResult 4=Prescription 5=Consultation 6=Emergency");
        int typeChoice = ConsoleUI.PromptInt("Record Type", 1, 6);
        var type = typeChoice switch
        {
            1 => RecordType.Diagnosis,    2 => RecordType.Surgery,
            3 => RecordType.LabResult,    4 => RecordType.Prescription,
            5 => RecordType.Consultation, _ => RecordType.Emergency
        };
        string diagnosis  = ConsoleUI.Prompt("Diagnosis");
        string treatment  = ConsoleUI.Prompt("Treatment Plan");
        string medication = ConsoleUI.Prompt("Medication Prescribed");
        string notes      = ConsoleUI.Prompt("Additional Notes (optional)");

        ConsoleUI.Spinner("Saving record...");
        var record = hospital.AddMedicalRecord(patId, docId, diagnosis, treatment,
                                                medication, type, notes);
        ConsoleUI.WriteSuccess($"Medical record saved. ID: #{record.Id}");
    }
    catch (Exception ex) { ConsoleUI.WriteError(ex.Message); }
    ConsoleUI.PressAnyKey();
}

void ViewRecords()
{
    ConsoleUI.SubHeader("Patient Medical Records");
    try
    {
        int id = ConsoleUI.PromptInt("Patient ID");
        var patient = hospital.GetPatient(id);
        ConsoleUI.SubHeader($"Records for: {patient.FullName}");
        if (!patient.Records.Any()) { ConsoleUI.WriteWarning("No medical records found."); }
        else foreach (var r in patient.Records) ConsoleUI.WriteLine(r.ToString(), ConsoleUI.Normal);
    }
    catch (Exception ex) { ConsoleUI.WriteError(ex.Message); }
    ConsoleUI.PressAnyKey();
}

void ViewLatestRecord()
{
    ConsoleUI.SubHeader("Latest Medical Record");
    try
    {
        int id     = ConsoleUI.PromptInt("Patient ID");
        var latest = hospital.GetPatient(id).GetLatestRecord();
        if (latest is null) ConsoleUI.WriteWarning("No records found.");
        else ConsoleUI.WriteLine(latest.GenerateReport(), ConsoleUI.Accent);
    }
    catch (Exception ex) { ConsoleUI.WriteError(ex.Message); }
    ConsoleUI.PressAnyKey();
}

// ════════════════════════════════════════════════════════════════════════════
// STATISTICS MENU
// ════════════════════════════════════════════════════════════════════════════
void StatsMenu()
{
    ConsoleUI.SectionHeader("Statistics & Reports");
    ConsoleUI.Spinner("Generating report...");
    ConsoleUI.WriteLine(hospital.GenerateSystemReport(), ConsoleUI.Accent);

    ConsoleUI.SubHeader("Department Occupancy");
    foreach (var dept in hospital.GetAllDepartments())
    {
        ConsoleUI.Write($"  {dept.Name,-28}", ConsoleUI.Normal);
        ConsoleUI.WriteLine(dept.GetOccupancyBar(), ConsoleUI.Accent);
    }

    ConsoleUI.SubHeader("Current Inpatients");
    ConsoleUI.Table(hospital.GetInpatients(), "No current inpatients.");

    ConsoleUI.PressAnyKey();
}
