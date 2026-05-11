using HospitalApp.Models;

namespace HospitalApp.Data
{

    public static class SeedData
    {
        public static void Populate(Services.HospitalService hospital)
        {
            // ── Departments ─────────────────────────────────────────────────────
            var cardio  = hospital.AddDepartment("Cardiology",        6, "Floor 3");
            var neuro   = hospital.AddDepartment("Neurology",         4, "Floor 4");
            var surgery = hospital.AddDepartment("General Surgery",   8, "Floor 2");
            var pediatr = hospital.AddDepartment("Pediatrics",        5, "Floor 1");
            var er      = hospital.AddDepartment("Emergency Room",   10, "Ground");

            cardio.PhoneExt  = "3100";
            neuro.PhoneExt   = "4200";
            surgery.PhoneExt = "2300";
            er.PhoneExt      = "0911";

            // ── Doctors ─────────────────────────────────────────────────────────
            var d1 = hospital.AddDoctor("James Hartwell",   52, "555-0101", "Cardiology",       8500, 22, "j.hartwell@hospital.com");
            var d2 = hospital.AddDoctor("Emily Chen",       41, "555-0102", "Neurology",        7800, 14, "e.chen@hospital.com");
            var d3 = hospital.AddDoctor("Marcus Webb",      48, "555-0103", "General Surgery",  9200, 18, "m.webb@hospital.com");
            var d4 = hospital.AddDoctor("Sophia Adler",     36, "555-0104", "Pediatrics",       6900, 8,  "s.adler@hospital.com");
            var d5 = hospital.AddDoctor("Nathan Brooks",    44, "555-0105", "Emergency",        8100, 16, "n.brooks@hospital.com");
            var d6 = hospital.AddDoctor("Layla Osman",      39, "555-0106", "Cardiology",       7600, 11, "l.osman@hospital.com");

            d1.AddCertification("FACC"); d1.AddCertification("FESC");
            d2.AddCertification("FAAN"); d3.AddCertification("FACS");

            cardio.AddDoctor(d1);  cardio.AddDoctor(d6);  cardio.SetHeadDoctor(d1.Id);
            neuro.AddDoctor(d2);   neuro.SetHeadDoctor(d2.Id);
            surgery.AddDoctor(d3); surgery.SetHeadDoctor(d3.Id);
            pediatr.AddDoctor(d4); pediatr.SetHeadDoctor(d4.Id);
            er.AddDoctor(d5);      er.SetHeadDoctor(d5.Id);

            // ── Nurses ──────────────────────────────────────────────────────────
            var n1 = hospital.AddNurse("Rachel Kim",     29, "555-0201", "Cardiology Ward", ShiftType.Morning, "RN");
            var n2 = hospital.AddNurse("Daniel Torres",  34, "555-0202", "Neuro Ward",      ShiftType.Night,   "RN");
            var n3 = hospital.AddNurse("Grace Okafor",   27, "555-0203", "Surgery Ward",    ShiftType.Evening, "BSN");
            var n4 = hospital.AddNurse("Sam Patel",      31, "555-0204", "Pediatrics",      ShiftType.Morning, "RN");
            var n5 = hospital.AddNurse("Chloe Martin",   26, "555-0205", "ER",              ShiftType.Night,   "BSN");

            n1.IsHeadNurse = true;
            n1.AssignTask("Cardiac monitoring"); n1.AssignTask("Medication rounds");
            n5.AssignTask("Triage"); n5.AssignTask("IV placement");

            // ── Patients ────────────────────────────────────────────────────────
            var p1 = hospital.AddPatient("Oliver Grant",   67, "555-0301", "A+");
            var p2 = hospital.AddPatient("Mia Thompson",   45, "555-0302", "B-");
            var p3 = hospital.AddPatient("Ethan Rivera",   72, "555-0303", "O+");
            var p4 = hospital.AddPatient("Ava Johnson",     8, "555-0304", "AB+");
            var p5 = hospital.AddPatient("Liam Nguyen",    55, "555-0305", "A-");
            var p6 = hospital.AddPatient("Zoe Brennan",    33, "555-0306", "B+");

            p1.EmergencyContact = "Mary Grant (wife) - 555-9901";
            p3.EmergencyContact = "Carlos Rivera (son) - 555-9903";
            p1.InsuranceId = "INS-4421";
            p2.InsuranceId = "INS-8832";
            p2.AddAllergy("Penicillin"); p2.AddAllergy("Latex");
            p5.AddAllergy("Aspirin");

            p1.Admit("Cardio Ward 3A");
            p3.Admit("Neuro Ward 4B", isCritical: true);
            p5.Admit("Surgery Ward 2C");

            // ── Medical Records ─────────────────────────────────────────────────
            hospital.AddMedicalRecord(p1.Id, d1.Id, "Atrial Fibrillation",
                "Beta-blocker therapy + cardioversion",
                "Metoprolol 50mg, Warfarin 5mg",
                RecordType.Diagnosis, "Patient responding well.");

            hospital.AddMedicalRecord(p1.Id, d1.Id, "Follow-up: AF stable",
                "Continue current medication",
                "Metoprolol 50mg",
                RecordType.Consultation, "ECG normal.");

            hospital.AddMedicalRecord(p2.Id, d2.Id, "Migraine with Aura",
                "Triptan therapy, avoid triggers",
                "Sumatriptan 100mg PRN",
                RecordType.Prescription);

            hospital.AddMedicalRecord(p3.Id, d2.Id, "Ischemic Stroke",
                "Thrombolytic therapy, ICU monitoring",
                "Alteplase IV, Aspirin 325mg",
                RecordType.Emergency, "CRITICAL - continuous monitoring.");

            hospital.AddMedicalRecord(p4.Id, d4.Id, "Acute Tonsillitis",
                "Antibiotics, rest, fluids",
                "Amoxicillin 250mg x10 days",
                RecordType.Diagnosis);

            // ── Appointments ────────────────────────────────────────────────────
            hospital.BookAppointment(d1.Id, p1.Id,
                DateTime.Today.AddDays(1).AddHours(9),  "Cardiology follow-up");
            hospital.BookAppointment(d2.Id, p2.Id,
                DateTime.Today.AddDays(1).AddHours(11), "Migraine review");
            hospital.BookAppointment(d3.Id, p5.Id,
                DateTime.Today.AddDays(2).AddHours(8),  "Pre-operative assessment");
            hospital.BookAppointment(d4.Id, p4.Id,
                DateTime.Today.AddDays(2).AddHours(14), "Tonsillitis check-up");
            hospital.BookAppointment(d6.Id, p6.Id,
                DateTime.Today.AddDays(3).AddHours(10), "Routine cardiac screening");

            // Confirm first two
            var appts = hospital.GetAllAppointments();
            appts[0].Confirm();
            appts[1].Confirm();
        }
    }
}
