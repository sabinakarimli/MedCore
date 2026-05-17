using HospitalApp.Data;
using HospitalApp.Models;
using HospitalApp.Services;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MedCore Hospital API",
        Version = "v1",
        Description = "Hospital management REST API for the MedCore (HospitalApp) demo system."
    });
});
builder.Services.AddSingleton(_ =>
{
    var hospital = new HospitalService("Baku Clinical Hospital");
    SeedData.Populate(hospital);
    return hospital;
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "MedCore Hospital API v1");
    options.RoutePrefix = "swagger";
});

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/overview", (HospitalService hospital) =>
{
    var doctors = hospital.GetAllDoctors();
    var patients = hospital.GetAllPatients();
    var nurses = hospital.GetAllNurses();
    var departments = hospital.GetAllDepartments();
    var appointments = hospital.GetAllAppointments();

    return Results.Ok(new
    {
        hospital.HospitalName,
        systemStarted = hospital.SystemStarted,
        counts = new
        {
            doctors = doctors.Count,
            patients = patients.Count,
            nurses = nurses.Count,
            departments = departments.Count,
            appointments = appointments.Count,
            inpatients = hospital.GetInpatients().Count(),
            critical = hospital.GetCriticalPatients().Count(),
            upcoming = hospital.GetUpcoming().Count()
        },
        appointmentStatus = appointments
            .GroupBy(a => a.Status.ToString())
            .Select(g => new { status = g.Key, count = g.Count() })
            .OrderBy(x => x.status),
        departments = departments.Select(ToDepartmentDto),
        upcomingAppointments = hospital.GetUpcoming().Take(5).Select(ToAppointmentDto),
        criticalPatients = hospital.GetCriticalPatients().Select(ToPatientDto)
    });
});

app.MapGet("/api/doctors", (HospitalService hospital) =>
    Results.Ok(hospital.GetAllDoctors().Select(ToDoctorDto)));

app.MapPost("/api/doctors", (HospitalService hospital, CreateDoctorRequest request) =>
{
    try
    {
        var doctor = hospital.AddDoctor(
            request.FullName,
            request.Age,
            request.Phone,
            request.Specialization,
            request.Salary,
            request.YearsOfService,
            request.Email ?? string.Empty);

        return Results.Created($"/api/doctors/{doctor.Id}", ToDoctorDto(doctor));
    }
    catch (Exception ex)
    {
        return BadRequest(ex);
    }
});

app.MapDelete("/api/doctors/{id:int}", (HospitalService hospital, int id) =>
{
    try
    {
        return hospital.RemoveDoctor(id) ? Results.NoContent() : Results.NotFound(new { message = "Doctor not found." });
    }
    catch (Exception ex)
    {
        return BadRequest(ex);
    }
});

app.MapGet("/api/patients", (HospitalService hospital) =>
    Results.Ok(hospital.GetAllPatients().Select(ToPatientDto)));

app.MapPost("/api/patients", (HospitalService hospital, CreatePatientRequest request) =>
{
    try
    {
        var patient = hospital.AddPatient(request.FullName, request.Age, request.Phone, request.BloodType, request.Email ?? string.Empty);
        patient.EmergencyContact = string.IsNullOrWhiteSpace(request.EmergencyContact) ? "N/A" : request.EmergencyContact;
        patient.InsuranceId = string.IsNullOrWhiteSpace(request.InsuranceId) ? null : request.InsuranceId;

        foreach (var allergy in SplitCsv(request.Allergies))
        {
            patient.AddAllergy(allergy);
        }

        return Results.Created($"/api/patients/{patient.Id}", ToPatientDto(patient));
    }
    catch (Exception ex)
    {
        return BadRequest(ex);
    }
});

app.MapPost("/api/patients/{id:int}/admit", (HospitalService hospital, int id, AdmitPatientRequest request) =>
{
    try
    {
        var patient = hospital.GetPatient(id);
        patient.Admit(request.Ward, request.IsCritical);
        return Results.Ok(ToPatientDto(patient));
    }
    catch (Exception ex)
    {
        return BadRequest(ex);
    }
});

app.MapPost("/api/patients/{id:int}/discharge", (HospitalService hospital, int id) =>
{
    try
    {
        return hospital.DischargePatient(id)
            ? Results.Ok(ToPatientDto(hospital.GetPatient(id)))
            : Results.BadRequest(new { message = "Patient is not currently admitted." });
    }
    catch (Exception ex)
    {
        return BadRequest(ex);
    }
});

app.MapGet("/api/nurses", (HospitalService hospital) =>
    Results.Ok(hospital.GetAllNurses().Select(ToNurseDto)));

app.MapPost("/api/nurses", (HospitalService hospital, CreateNurseRequest request) =>
{
    try
    {
        var nurse = hospital.AddNurse(request.FullName, request.Age, request.Phone, request.Ward, request.Shift, request.Qualification, request.Email ?? string.Empty);
        return Results.Created($"/api/nurses/{nurse.Id}", ToNurseDto(nurse));
    }
    catch (Exception ex)
    {
        return BadRequest(ex);
    }
});

app.MapPost("/api/nurses/{id:int}/tasks", (HospitalService hospital, int id, AssignTaskRequest request) =>
{
    try
    {
        var nurse = hospital.GetNurse(id);
        nurse.AssignTask(request.Task);
        return Results.Ok(ToNurseDto(nurse));
    }
    catch (Exception ex)
    {
        return BadRequest(ex);
    }
});

app.MapGet("/api/appointments", (HospitalService hospital) =>
    Results.Ok(hospital.GetAllAppointments().OrderBy(a => a.DateTime).Select(ToAppointmentDto)));

app.MapPost("/api/appointments", (HospitalService hospital, CreateAppointmentRequest request) =>
{
    try
    {
        var appointment = hospital.BookAppointment(request.DoctorId, request.PatientId, request.DateTime, request.Reason);
        return Results.Created($"/api/appointments/{appointment.Id}", ToAppointmentDto(appointment));
    }
    catch (Exception ex)
    {
        return BadRequest(ex);
    }
});

app.MapPost("/api/appointments/{id:int}/status", (HospitalService hospital, int id, UpdateAppointmentStatusRequest request) =>
{
    try
    {
        var appointment = hospital.GetAppointment(id);
        var notes = request.Notes ?? string.Empty;

        switch (request.Status)
        {
            case AppointmentStatus.Confirmed:
                appointment.Confirm();
                break;
            case AppointmentStatus.InProgress:
                appointment.Start();
                break;
            case AppointmentStatus.Completed:
                appointment.Complete(notes);
                break;
            case AppointmentStatus.Cancelled:
                appointment.Cancel(notes);
                break;
            case AppointmentStatus.NoShow:
                appointment.MarkNoShow();
                break;
            default:
                return Results.BadRequest(new { message = "Unsupported status transition." });
        }

        return Results.Ok(ToAppointmentDto(appointment));
    }
    catch (Exception ex)
    {
        return BadRequest(ex);
    }
});

app.MapGet("/api/departments", (HospitalService hospital) =>
    Results.Ok(hospital.GetAllDepartments().Select(ToDepartmentDto)));

app.MapPost("/api/departments", (HospitalService hospital, CreateDepartmentRequest request) =>
{
    try
    {
        var department = hospital.AddDepartment(request.Name, request.Capacity, request.Floor);
        department.PhoneExt = string.IsNullOrWhiteSpace(request.PhoneExt) ? "N/A" : request.PhoneExt;
        return Results.Created($"/api/departments/{department.Name}", ToDepartmentDto(department));
    }
    catch (Exception ex)
    {
        return BadRequest(ex);
    }
});

app.MapPost("/api/departments/assign-doctor", (HospitalService hospital, AssignDoctorRequest request) =>
{
    try
    {
        return hospital.AssignDoctorToDepartment(request.DoctorId, request.DepartmentName)
            ? Results.Ok(new { message = "Doctor assigned successfully." })
            : Results.BadRequest(new { message = "Department is full or doctor is already assigned." });
    }
    catch (Exception ex)
    {
        return BadRequest(ex);
    }
});

app.MapPost("/api/records", (HospitalService hospital, CreateRecordRequest request) =>
{
    try
    {
        var record = hospital.AddMedicalRecord(
            request.PatientId,
            request.DoctorId,
            request.Diagnosis,
            request.Treatment,
            request.Medication,
            request.Type,
            request.Notes ?? string.Empty);

        return Results.Created($"/api/records/{record.Id}", ToRecordDto(record));
    }
    catch (Exception ex)
    {
        return BadRequest(ex);
    }
});

app.MapFallbackToFile("index.html");

app.Run();

static IResult BadRequest(Exception ex) => Results.BadRequest(new { message = ex.Message });

static IEnumerable<string> SplitCsv(string? value) =>
    string.IsNullOrWhiteSpace(value)
        ? Enumerable.Empty<string>()
        : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

static object ToDoctorDto(Doctor doctor) => new
{
    doctor.Id,
    doctor.FullName,
    doctor.Age,
    doctor.Phone,
    doctor.Email,
    doctor.Specialization,
    doctor.Department,
    doctor.Salary,
    doctor.YearsOfService,
    certifications = doctor.Certifications,
    appointments = doctor.Schedule.Count,
    schedule = doctor.Schedule.OrderBy(x => x)
};

static object ToPatientDto(Patient patient) => new
{
    patient.Id,
    patient.FullName,
    patient.Age,
    patient.Phone,
    patient.Email,
    patient.BloodType,
    status = patient.Status.ToString(),
    patient.AssignedWard,
    patient.InsuranceId,
    patient.EmergencyContact,
    allergies = patient.Allergies,
    records = patient.Records.Select(ToRecordDto),
    recordCount = patient.Records.Count,
    latestDiagnosis = patient.GetLatestRecord()?.Diagnosis ?? "No records yet"
};

static object ToNurseDto(Nurse nurse) => new
{
    nurse.Id,
    nurse.FullName,
    nurse.Age,
    nurse.Phone,
    nurse.Email,
    nurse.Ward,
    shift = nurse.Shift.ToString(),
    nurse.Qualification,
    nurse.IsHeadNurse,
    shiftHours = nurse.GetShiftHours(),
    tasks = nurse.AssignedTasks
};

static object ToAppointmentDto(Appointment appointment) => new
{
    appointment.Id,
    dateTime = appointment.DateTime,
    date = appointment.DateTime.ToString("dd MMM yyyy"),
    time = appointment.DateTime.ToString("HH:mm"),
    doctorId = appointment.Doctor.Id,
    doctorName = appointment.Doctor.FullName,
    specialization = appointment.Doctor.Specialization,
    patientId = appointment.Patient.Id,
    patientName = appointment.Patient.FullName,
    appointment.Reason,
    appointment.Notes,
    status = appointment.Status.ToString(),
    createdAt = appointment.CreatedAt
};

static object ToDepartmentDto(Department department) => new
{
    department.Name,
    department.Capacity,
    department.Floor,
    department.PhoneExt,
    department.HeadDoctor,
    department.DoctorCount,
    department.IsFull,
    occupancyRate = Math.Round(department.OccupancyRate, 0),
    doctors = department.Doctors.Select(d => new { d.Id, d.FullName, d.Specialization })
};

static object ToRecordDto(MedicalRecord record) => new
{
    record.Id,
    type = record.Type.ToString(),
    record.Diagnosis,
    record.Treatment,
    record.Medication,
    record.Notes,
    doctorId = record.IssuedBy.Id,
    doctorName = record.IssuedBy.FullName,
    specialization = record.IssuedBy.Specialization,
    createdAt = record.CreatedAt
};

record CreateDoctorRequest(string FullName, int Age, string Phone, string Specialization, decimal Salary, int YearsOfService, string? Email);
record CreatePatientRequest(string FullName, int Age, string Phone, string BloodType, string? Email, string? EmergencyContact, string? InsuranceId, string? Allergies);
record AdmitPatientRequest(string Ward, bool IsCritical);
record CreateNurseRequest(string FullName, int Age, string Phone, string Ward, ShiftType Shift, string Qualification, string? Email);
record AssignTaskRequest(string Task);
record CreateAppointmentRequest(int DoctorId, int PatientId, DateTime DateTime, string Reason);
record UpdateAppointmentStatusRequest(AppointmentStatus Status, string? Notes);
record CreateDepartmentRequest(string Name, int Capacity, string Floor, string? PhoneExt);
record AssignDoctorRequest(int DoctorId, string DepartmentName);
record CreateRecordRequest(int PatientId, int DoctorId, string Diagnosis, string Treatment, string Medication, RecordType Type, string? Notes);


