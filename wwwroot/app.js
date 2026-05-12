const state = {
  route: "overview",
  doctors: [],
  patients: [],
  nurses: [],
  appointments: [],
  departments: [],
  overview: null,
};

const content = document.querySelector("#content");
const toast = document.querySelector("#toast");
const drawer = document.querySelector("#drawer");
const drawerBody = document.querySelector("#drawerBody");
const drawerShade = document.querySelector("#drawerShade");
const loadingTemplate = document.querySelector("#loadingTemplate");

document.querySelector("#todayLabel").textContent = new Date().toLocaleDateString(undefined, {
  weekday: "long",
  month: "short",
  day: "numeric",
});

document.querySelectorAll(".nav-link, .brand").forEach((item) => {
  item.addEventListener("click", (event) => {
    event.preventDefault();
    routeTo(item.dataset.route || "overview");
  });
});

document.querySelector(".drawer-close").addEventListener("click", closeDrawer);
drawerShade.addEventListener("click", closeDrawer);
window.addEventListener("hashchange", () => routeTo(location.hash.replace("#", "") || "overview", false));

init();

async function init() {
  try {
    await refreshAll(false);
    routeTo(location.hash.replace("#", "") || "overview", false);
  } catch (error) {
    content.innerHTML = `<div class="empty">Could not load data: ${escapeHtml(error.message)}</div>`;
  }
}

async function refreshAll(showSpinner = true) {
  if (showSpinner) showLoading();
  const [overview, doctors, patients, nurses, appointments, departments] = await Promise.all([
    api("/api/overview"),
    api("/api/doctors"),
    api("/api/patients"),
    api("/api/nurses"),
    api("/api/appointments"),
    api("/api/departments"),
  ]);
  Object.assign(state, { overview, doctors, patients, nurses, appointments, departments });
  document.querySelector("#systemStarted").textContent = formatDateTime(overview.systemStarted);
}

function routeTo(route, push = true) {
  state.route = route;
  if (push) location.hash = route;
  document.querySelectorAll(".nav-link").forEach((link) => link.classList.toggle("active", link.dataset.route === route));
  closeDrawer();

  const views = {
    overview: renderOverview,
    doctors: renderDoctors,
    patients: renderPatients,
    nurses: renderNurses,
    appointments: renderAppointments,
    departments: renderDepartments,
    records: renderRecords,
    reports: renderReports,
  };
  (views[route] || renderOverview)();
}

function showLoading() {
  content.replaceChildren(loadingTemplate.content.cloneNode(true));
}

async function api(url, options = {}) {
  const response = await fetch(url, {
    headers: { "Content-Type": "application/json", ...(options.headers || {}) },
    ...options,
  });
  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: "Request failed." }));
    throw new Error(error.message || "Request failed.");
  }
  return response.status === 204 ? null : response.json();
}

async function submitJson(url, data, method = "POST") {
  try {
    await api(url, { method, body: JSON.stringify(data) });
    await refreshAll(false);
    routeTo(state.route, false);
    notify("Operation completed successfully.");
  } catch (error) {
    notify(error.message, true);
  }
}

function notify(message, isError = false) {
  const node = document.createElement("div");
  node.className = `toast-msg ${isError ? "error" : ""}`;
  node.textContent = message;
  toast.append(node);
  setTimeout(() => node.remove(), 3800);
}

function renderOverview() {
  const counts = state.overview.counts;
  const next = state.overview.upcomingAppointments[0];
  content.innerHTML = `
    <div class="hero-strip">
      <section class="clinical-card">
        <span class="ribbon">Live hospital dashboard</span>
        <h2>${escapeHtml(state.overview.hospitalName)}</h2>
        <p>All console modules are now represented as a professional clinical workspace: staff, patients, appointments, records, departments, schedules and reports.</p>
      </section>
      <section class="command-card">
        <span class="badge blue">Next appointment</span>
        <strong>${next ? escapeHtml(next.time) : "Clear"}</strong>
        <p>${next ? `Dr. ${escapeHtml(next.doctorName)} with ${escapeHtml(next.patientName)} on ${escapeHtml(next.date)}.` : "No upcoming appointments are currently queued."}</p>
        <button class="btn secondary" data-route-button="appointments">Open scheduler</button>
      </section>
    </div>
    <div class="grid metrics">
      ${metric("Doctors", counts.doctors, "Active physicians")}
      ${metric("Patients", counts.patients, `${counts.inpatients} admitted`)}
      ${metric("Nurses", counts.nurses, "Shift coverage")}
      ${metric("Critical", counts.critical, "High priority")}
    </div>
    <div class="grid columns">
      <div class="panel">
        <h3>Department Capacity</h3>
        <div class="list">${state.departments.map(departmentListItem).join("")}</div>
      </div>
      <div class="panel">
        <h3>Upcoming Appointments</h3>
        <div class="list">${state.overview.upcomingAppointments.length ? state.overview.upcomingAppointments.map(appointmentListItem).join("") : empty("No upcoming appointments.")}</div>
      </div>
    </div>
    <div class="grid triple" style="margin-top:16px">
      ${spotlightCard("Doctors on schedule", state.doctors.filter((d) => d.appointments > 0).length, "Open doctor schedules from the Doctors module.")}
      ${spotlightCard("Medical records", allRecords().length, "Patient history, diagnosis, treatment and medication notes.")}
      ${spotlightCard("Departments", counts.departments, "Capacity limited assignment and occupancy tracking.")}
    </div>
  `;
  wireRouteButtons();
}

function renderDoctors() {
  content.innerHTML = `
    ${viewHead("Doctor Management", "Register doctors, inspect details, review schedules, search by name and remove records just like the console app.")}
    ${doctorForm()}
    ${tableTools("Search doctor name, specialization or department")}
    ${table(["Doctor", "Specialization", "Department", "Salary", "Schedule", "Actions"], state.doctors.map((d) => [
      profileCell(d.fullName, `#${d.id} - ${d.phone}`, initials(d.fullName)),
      badge(d.specialization),
      escapeHtml(d.department),
      `$${Number(d.salary).toLocaleString()}`,
      `${d.appointments} booked slot${d.appointments === 1 ? "" : "s"}`,
      actionButtons("doctor", d.id, true),
    ]))}
  `;
  wireCommon();
  wireForm("#doctorForm", (data) => submitJson("/api/doctors", data));
}

function renderPatients() {
  content.innerHTML = `
    ${viewHead("Patient Management", "Register patients, admit, discharge, mark critical, search and view full patient records.")}
    ${patientForm()}
    ${quickActionForm("Admission Control", "patientActionForm", [
      field("patientId", "Patient", "select", patientOptions()),
      field("ward", "Ward", "text", "Cardio Ward 3A"),
      field("isCritical", "Critical", "select", `<option value="false">No</option><option value="true">Yes</option>`),
    ], "Admit Patient", "Discharge Patient")}
    ${tableTools("Search patient name, blood type, ward or status")}
    ${table(["Patient", "Blood", "Status", "Ward", "Records", "Latest", "Actions"], state.patients.map((p) => [
      profileCell(p.fullName, `#${p.id} - ${p.phone}`, initials(p.fullName)),
      badge(p.bloodType),
      statusBadge(p.status),
      escapeHtml(p.assignedWard || "N/A"),
      p.recordCount,
      escapeHtml(p.latestDiagnosis),
      actionButtons("patient", p.id),
    ]))}
  `;
  wireCommon();
  wireForm("#patientForm", (data) => submitJson("/api/patients", data));
  const form = document.querySelector("#patientActionForm");
  form.addEventListener("submit", (event) => {
    event.preventDefault();
    const data = formData(form);
    submitJson(`/api/patients/${data.patientId}/admit`, { ward: data.ward, isCritical: data.isCritical === "true" });
  });
  form.querySelector("[data-secondary]").addEventListener("click", () => {
    const data = formData(form);
    submitJson(`/api/patients/${data.patientId}/discharge`, {});
  });
}

function renderNurses() {
  content.innerHTML = `
    ${viewHead("Nurse Management", "Add nurses, filter shifts visually, inspect duties and assign tasks.")}
    ${nurseForm()}
    ${quickActionForm("Task Assignment", "taskForm", [
      field("nurseId", "Nurse", "select", nurseOptions()),
      field("task", "Task", "text", "Medication rounds"),
    ], "Assign Task")}
    <div class="grid triple" style="margin-bottom:16px">
      ${shiftCard("Morning")}${shiftCard("Evening")}${shiftCard("Night")}
    </div>
    ${tableTools("Search nurse name, ward, shift or task")}
    ${table(["Nurse", "Ward", "Shift", "Qualification", "Tasks", "Actions"], state.nurses.map((n) => [
      profileCell(n.fullName, `#${n.id} - ${n.phone}`, initials(n.fullName)),
      escapeHtml(n.ward),
      badge(`${n.shift} - ${n.shiftHours}`),
      escapeHtml(n.qualification),
      n.tasks.length ? n.tasks.map(escapeHtml).join(", ") : "None",
      actionButtons("nurse", n.id),
    ]))}
  `;
  wireCommon();
  wireForm("#nurseForm", (data) => submitJson("/api/nurses", data));
  wireForm("#taskForm", (data) => submitJson(`/api/nurses/${data.nurseId}/tasks`, { task: data.task }));
}

function renderAppointments() {
  content.innerHTML = `
    ${viewHead("Appointment Management", "Book appointments, view today/upcoming lists and control status transitions professionally.")}
    ${appointmentForm()}
    ${quickActionForm("Status Workflow", "statusForm", [
      field("appointmentId", "Appointment", "select", appointmentOptions()),
      field("status", "New Status", "select", `<option value="Confirmed">Confirm</option><option value="InProgress">Start</option><option value="Completed">Complete</option><option value="Cancelled">Cancel</option><option value="NoShow">No-show</option>`),
      field("notes", "Notes", "text", "Optional notes or reason"),
    ], "Update Status")}
    <div class="grid metrics" style="margin-bottom:16px">
      ${metric("Today", state.appointments.filter((a) => isToday(a.dateTime)).length, "Appointments")}
      ${metric("Upcoming", state.overview.counts.upcoming, "Pending or confirmed")}
      ${metric("Confirmed", state.appointments.filter((a) => a.status === "Confirmed").length, "Ready")}
      ${metric("Completed", state.appointments.filter((a) => a.status === "Completed").length, "Closed")}
    </div>
    ${tableTools("Search doctor, patient, reason, date or status")}
    ${table(["Appointment", "Doctor", "Patient", "Reason", "Status", "Actions"], state.appointments.map((a) => [
      `<strong>#${a.id}</strong><br><small>${escapeHtml(a.date)} at ${escapeHtml(a.time)}</small>`,
      profileCell(`Dr. ${a.doctorName}`, a.specialization, initials(a.doctorName)),
      escapeHtml(a.patientName),
      escapeHtml(a.reason),
      statusBadge(a.status),
      actionButtons("appointment", a.id),
    ]))}
  `;
  wireCommon();
  wireForm("#appointmentForm", (data) => submitJson("/api/appointments", data));
  wireForm("#statusForm", (data) => submitJson(`/api/appointments/${data.appointmentId}/status`, { status: data.status, notes: data.notes }));
}

function renderDepartments() {
  content.innerHTML = `
    ${viewHead("Department Management", "Create departments, assign doctors, track occupancy and see each clinical team.")}
    ${departmentForm()}
    ${quickActionForm("Doctor Assignment", "assignDoctorForm", [
      field("doctorId", "Doctor", "select", doctorOptions()),
      field("departmentName", "Department", "select", departmentOptions()),
    ], "Assign Doctor")}
    <div class="grid columns">
      ${state.departments.map(departmentPanel).join("")}
    </div>
  `;
  wireForm("#departmentForm", (data) => submitJson("/api/departments", data));
  wireForm("#assignDoctorForm", (data) => submitJson("/api/departments/assign-doctor", data));
}

function renderRecords() {
  const records = allRecords();
  content.innerHTML = `
    ${viewHead("Medical Records", "Add patient records and inspect diagnosis, treatment, medication and physician notes.")}
    ${recordForm()}
    ${tableTools("Search records by patient, diagnosis, treatment or doctor")}
    ${table(["Record", "Patient", "Type", "Diagnosis", "Doctor", "Treatment", "Actions"], records.map((r) => [
      `<strong>#${r.id}</strong><br><small>${formatDateTime(r.createdAt)}</small>`,
      escapeHtml(r.patientName),
      badge(r.type),
      escapeHtml(r.diagnosis),
      `Dr. ${escapeHtml(r.doctorName)}`,
      escapeHtml(r.treatment),
      `<button class="btn small secondary" data-view="record" data-id="${r.id}">View</button>`,
    ]))}
  `;
  wireCommon();
  wireForm("#recordForm", (data) => submitJson("/api/records", data));
}

function renderReports() {
  const statusRows = ["Pending", "Confirmed", "InProgress", "Completed", "Cancelled", "NoShow"].map((status) => ({
    status,
    count: state.appointments.filter((a) => a.status === status).length,
  }));
  content.innerHTML = `
    ${viewHead("Statistics & Reports", "Console reports are now visual: staff totals, occupancy, inpatients, critical list and appointment state machine.")}
    <div class="grid metrics">
      ${metric("Total Staff", state.doctors.length + state.nurses.length, "Doctors and nurses")}
      ${metric("Inpatients", state.patients.filter((p) => ["Inpatient", "Critical"].includes(p.status)).length, "Currently admitted")}
      ${metric("Records", allRecords().length, "Clinical history")}
      ${metric("Departments", state.departments.length, "Operational units")}
    </div>
    <div class="grid columns">
      <div class="panel">
        <h3>Appointment State Machine</h3>
        <div class="list">${statusRows.map((x) => `<div class="list-item"><strong>${x.status}</strong><span class="badge ${x.count ? "" : "warn"}">${x.count}</span></div>`).join("")}</div>
      </div>
      <div class="panel">
        <h3>Current Inpatients</h3>
        <div class="list">${state.patients.filter((p) => ["Inpatient", "Critical"].includes(p.status)).map((p) => `<div class="list-item"><div><strong>${escapeHtml(p.fullName)}</strong><small>${escapeHtml(p.assignedWard || "N/A")} - ${escapeHtml(p.latestDiagnosis)}</small></div>${statusBadge(p.status)}</div>`).join("") || empty("No current inpatients.")}</div>
      </div>
    </div>
    <div class="panel" style="margin-top:16px">
      <h3>Department Occupancy Report</h3>
      <div class="list">${state.departments.map(departmentListItem).join("")}</div>
    </div>
  `;
}

function doctorForm() {
  return formCard("Add New Doctor", "Register a physician with specialization, salary and service years.", "doctorForm", [
    field("fullName", "Full Name", "text", "Aylin Mammadova"), field("age", "Age", "number", "38"), field("phone", "Phone", "text", "555-0140"), field("specialization", "Specialization", "text", "Cardiology"), field("salary", "Salary", "number", "7600"), field("yearsOfService", "Years", "number", "10"), field("email", "Email", "email", "doctor@hospital.com"),
  ], "Register Doctor");
}

function patientForm() {
  return formCard("Register Patient", "Create patient profile with emergency, insurance and allergy data.", "patientForm", [
    field("fullName", "Full Name", "text", "Nigar Aliyeva"), field("age", "Age", "number", "31"), field("phone", "Phone", "text", "555-0310"), field("bloodType", "Blood", "text", "O+"), field("email", "Email", "email", "patient@mail.com"), field("emergencyContact", "Emergency", "text", "Family - 555-9900"), field("insuranceId", "Insurance", "text", "INS-1001"), field("allergies", "Allergies", "text", "Latex, Penicillin"),
  ], "Register Patient");
}

function nurseForm() {
  return formCard("Add New Nurse", "Assign ward, shift and qualification.", "nurseForm", [
    field("fullName", "Full Name", "text", "Sara Abbasova"), field("age", "Age", "number", "28"), field("phone", "Phone", "text", "555-0240"), field("ward", "Ward", "text", "Emergency Room"), field("shift", "Shift", "select", `<option>Morning</option><option>Evening</option><option>Night</option>`), field("qualification", "Qualification", "text", "RN"), field("email", "Email", "email", "nurse@hospital.com"),
  ], "Add Nurse");
}

function appointmentForm() {
  const min = new Date(Date.now() + 60 * 60 * 1000).toISOString().slice(0, 16);
  return formCard("Book Appointment", "Doctor availability and same-day patient rules are enforced by the backend.", "appointmentForm", [
    field("doctorId", "Doctor", "select", doctorOptions()), field("patientId", "Patient", "select", patientOptions()), `<label class="field"><span>Date and Time</span><input name="dateTime" type="datetime-local" value="${min}" required></label>`, field("reason", "Reason", "text", "Clinical consultation"),
  ], "Book Appointment");
}

function departmentForm() {
  return formCard("Create Department", "Capacity controls how many doctors can be assigned.", "departmentForm", [
    field("name", "Name", "text", "Radiology"), field("capacity", "Capacity", "number", "4"), field("floor", "Floor", "text", "Floor 5"), field("phoneExt", "Phone Ext", "text", "5100"),
  ], "Create Department");
}

function recordForm() {
  return formCard("Add Medical Record", "Save diagnosis, treatment, medication and clinical notes.", "recordForm", [
    field("patientId", "Patient", "select", patientOptions()), field("doctorId", "Doctor", "select", doctorOptions()), field("type", "Type", "select", `<option>Diagnosis</option><option>Surgery</option><option>LabResult</option><option>Prescription</option><option>Consultation</option><option>Emergency</option>`), field("diagnosis", "Diagnosis", "text", "Routine check-up"), field("treatment", "Treatment", "text", "Follow-up care"), field("medication", "Medication", "text", "None"), `<label class="field double"><span>Notes</span><textarea name="notes" placeholder="Additional clinical notes"></textarea></label>`,
  ], "Save Record");
}

function formCard(title, subtitle, id, fields, buttonLabel) {
  return `<form class="form-card" id="${id}"><div class="form-head"><div><h3>${title}</h3><p>${subtitle}</p></div></div><div class="form-grid">${fields.join("")}<div class="field"><span>&nbsp;</span><button class="btn" type="submit">${buttonLabel}</button></div></div></form>`;
}

function quickActionForm(title, id, fields, primary, secondary = "") {
  return `<form class="form-card" id="${id}"><div class="form-head"><div><h3>${title}</h3><p>Fast operational action panel.</p></div></div><div class="form-grid">${fields.join("")}<div class="field"><span>&nbsp;</span><button class="btn" type="submit">${primary}</button></div>${secondary ? `<div class="field"><span>&nbsp;</span><button class="btn ghost" type="button" data-secondary>${secondary}</button></div>` : ""}</div></form>`;
}

function field(name, label, type, value) {
  if (type === "select") return `<label class="field"><span>${label}</span><select name="${name}" required>${value}</select></label>`;
  return `<label class="field"><span>${label}</span><input name="${name}" type="${type}" placeholder="${escapeHtml(value)}" ${type !== "email" ? "required" : ""}></label>`;
}

function wireCommon() {
  wireSearch();
  wireViewButtons();
  wireRouteButtons();
  document.querySelectorAll("[data-delete-doctor]").forEach((btn) => btn.addEventListener("click", async () => {
    if (!confirm("Remove this doctor and cancel future appointments?")) return;
    try {
      await api(`/api/doctors/${btn.dataset.deleteDoctor}`, { method: "DELETE" });
      await refreshAll(false);
      renderDoctors();
      notify("Doctor removed.");
    } catch (error) { notify(error.message, true); }
  }));
}

function wireRouteButtons() {
  document.querySelectorAll("[data-route-button]").forEach((button) => button.addEventListener("click", () => routeTo(button.dataset.routeButton)));
}

function wireViewButtons() {
  document.querySelectorAll("[data-view]").forEach((button) => button.addEventListener("click", () => openDetails(button.dataset.view, button.dataset.id)));
}

function wireForm(selector, handler) {
  const form = document.querySelector(selector);
  if (!form) return;
  form.addEventListener("submit", (event) => {
    event.preventDefault();
    handler(formData(form));
  });
}

function formData(form) {
  const data = Object.fromEntries(new FormData(form).entries());
  Object.keys(data).forEach((key) => {
    if (["age", "salary", "yearsOfService", "capacity"].includes(key) || key.endsWith("Id")) data[key] = Number(data[key]);
  });
  return data;
}

function wireSearch() {
  const input = document.querySelector("#tableSearch");
  if (!input) return;
  input.addEventListener("input", () => {
    const query = input.value.toLowerCase();
    document.querySelectorAll("tbody tr").forEach((row) => row.style.display = row.textContent.toLowerCase().includes(query) ? "" : "none");
  });
}

function openDetails(type, id) {
  const item = findItem(type, id);
  if (!item) return notify("Record not found.", true);
  drawerBody.innerHTML = detailMarkup(type, item);
  drawer.classList.add("open");
  drawerShade.classList.add("open");
  drawer.setAttribute("aria-hidden", "false");
}

function closeDrawer() {
  drawer.classList.remove("open");
  drawerShade.classList.remove("open");
  drawer.setAttribute("aria-hidden", "true");
}

function findItem(type, id) {
  const key = Number(id);
  if (type === "doctor") return state.doctors.find((x) => x.id === key);
  if (type === "patient") return state.patients.find((x) => x.id === key);
  if (type === "nurse") return state.nurses.find((x) => x.id === key);
  if (type === "appointment") return state.appointments.find((x) => x.id === key);
  if (type === "record") return allRecords().find((x) => x.id === key);
  return null;
}

function detailMarkup(type, item) {
  const rows = {
    doctor: [["ID", `#${item.id}`], ["Name", item.fullName], ["Age", item.age], ["Specialization", item.specialization], ["Department", item.department], ["Salary", `$${Number(item.salary).toLocaleString()}`], ["Years Service", item.yearsOfService], ["Phone", item.phone], ["Email", item.email || "N/A"], ["Certifications", item.certifications?.join(", ") || "None"], ["Schedule", item.schedule?.length ? item.schedule.map(formatDateTime).join(" | ") : "No upcoming slots"]],
    patient: [["ID", `#${item.id}`], ["Name", item.fullName], ["Age", item.age], ["Blood Type", item.bloodType], ["Status", item.status], ["Ward", item.assignedWard || "N/A"], ["Phone", item.phone], ["Insurance", item.insuranceId || "N/A"], ["Emergency", item.emergencyContact], ["Allergies", item.allergies?.join(", ") || "None"], ["Records", item.recordCount], ["Latest Diagnosis", item.latestDiagnosis]],
    nurse: [["ID", `#${item.id}`], ["Name", item.fullName], ["Age", item.age], ["Ward", item.ward], ["Shift", `${item.shift} (${item.shiftHours})`], ["Qualification", item.qualification], ["Phone", item.phone], ["Tasks", item.tasks?.join(", ") || "None"]],
    appointment: [["ID", `#${item.id}`], ["Status", item.status], ["Date", item.date], ["Time", item.time], ["Doctor", `Dr. ${item.doctorName} (${item.specialization})`], ["Patient", item.patientName], ["Reason", item.reason], ["Notes", item.notes || "None"], ["Created", formatDateTime(item.createdAt)]],
    record: [["Record ID", `#${item.id}`], ["Patient", item.patientName], ["Type", item.type], ["Diagnosis", item.diagnosis], ["Treatment", item.treatment], ["Medication", item.medication || "None"], ["Physician", `Dr. ${item.doctorName} (${item.specialization})`], ["Notes", item.notes || "None"], ["Created", formatDateTime(item.createdAt)]],
  }[type];
  return `<div class="detail-title"><span class="badge blue">${escapeHtml(type.toUpperCase())} REPORT</span><h2>${escapeHtml(item.fullName || item.patientName || item.reason || item.diagnosis || "Details")}</h2><p>Detailed report converted from the console experience into a clean UI panel.</p></div><div class="detail-grid">${rows.map(([label, value]) => `<div class="detail-row"><span>${escapeHtml(label)}</span><strong>${escapeHtml(value)}</strong></div>`).join("")}</div>`;
}

function viewHead(title, subtitle) {
  return `<div class="view-head"><div><h2>${title}</h2><p>${subtitle}</p></div><div class="actions"><button class="btn secondary" data-route-button="overview">Overview</button><button class="btn ghost" onclick="refreshAll(false).then(() => routeTo(state.route, false))">Refresh</button></div></div>`;
}

function metric(label, value, note) { return `<div class="metric"><span>${label}</span><strong>${value}</strong><small>${note}</small></div>`; }
function spotlightCard(title, value, text) { return `<div class="panel"><span class="badge blue">${title}</span><h3 style="font-size:2.4rem;margin:12px 0 8px">${value}</h3><p style="color:var(--muted);line-height:1.6">${text}</p></div>`; }
function tableTools(placeholder) { return `<div class="table-tools"><input id="tableSearch" class="search" placeholder="${placeholder}"><button class="btn secondary" onclick="refreshAll(false).then(() => routeTo(state.route, false))">Reload Data</button></div>`; }
function table(headers, rows) { return rows.length ? `<div class="table-wrap"><table><thead><tr>${headers.map((h) => `<th>${h}</th>`).join("")}</tr></thead><tbody>${rows.map((r) => `<tr>${r.map((c) => `<td>${c}</td>`).join("")}</tr>`).join("")}</tbody></table></div>` : empty("No data available yet."); }
function profileCell(name, meta, letters) { return `<div class="profile-top"><span class="avatar">${escapeHtml(letters)}</span><span><strong>${escapeHtml(name)}</strong><small>${escapeHtml(meta)}</small></span></div>`; }
function badge(value) { return `<span class="badge">${escapeHtml(value)}</span>`; }
function statusBadge(value) { const danger = ["Critical", "Cancelled", "NoShow"].includes(value); const warn = ["Pending", "InProgress"].includes(value); return `<span class="badge ${danger ? "danger" : warn ? "warn" : ""}">${escapeHtml(value)}</span>`; }
function actionButtons(type, id, allowDelete = false) { return `<div class="actions"><button class="btn small secondary" data-view="${type}" data-id="${id}">View Report</button>${allowDelete ? `<button class="btn small danger" data-delete-doctor="${id}">Remove</button>` : ""}</div>`; }
function appointmentListItem(a) { return `<div class="list-item"><div><strong>${escapeHtml(a.date)} at ${escapeHtml(a.time)}</strong><small>Dr. ${escapeHtml(a.doctorName)} with ${escapeHtml(a.patientName)}</small></div>${statusBadge(a.status)}</div>`; }
function departmentListItem(d) { return `<div class="list-item"><div><strong>${escapeHtml(d.name)}</strong><small>${escapeHtml(d.floor)} - ${d.doctorCount}/${d.capacity} doctors - Head: ${escapeHtml(d.headDoctor)}</small></div><div style="width:170px"><div class="progress"><span style="width:${d.occupancyRate}%"></span></div><small>${d.occupancyRate}% full</small></div></div>`; }
function departmentPanel(d) { return `<section class="panel"><span class="badge blue">${escapeHtml(d.floor)}</span><h3 style="margin-top:12px">${escapeHtml(d.name)}</h3><p style="color:var(--muted)">Ext ${escapeHtml(d.phoneExt)} - Head doctor: ${escapeHtml(d.headDoctor)}</p><div class="progress"><span style="width:${d.occupancyRate}%"></span></div><p><strong>${d.doctorCount}/${d.capacity}</strong> capacity used</p><div class="list">${d.doctors.length ? d.doctors.map((doc) => `<div class="list-item"><div><strong>Dr. ${escapeHtml(doc.fullName)}</strong><small>${escapeHtml(doc.specialization)}</small></div><span class="badge">#${doc.id}</span></div>`).join("") : empty("No doctors assigned.")}</div></section>`; }
function shiftCard(shift) { const nurses = state.nurses.filter((n) => n.shift === shift); return `<div class="panel"><span class="badge blue">${shift} shift</span><h3 style="font-size:2.2rem;margin:10px 0">${nurses.length}</h3><p style="color:var(--muted)">${nurses.map((n) => n.fullName).join(", ") || "No nurses assigned."}</p></div>`; }

function doctorOptions() { return state.doctors.map((d) => `<option value="${d.id}">Dr. ${escapeHtml(d.fullName)} - ${escapeHtml(d.specialization)}</option>`).join(""); }
function patientOptions() { return state.patients.map((p) => `<option value="${p.id}">${escapeHtml(p.fullName)} - ${escapeHtml(p.status)}</option>`).join(""); }
function nurseOptions() { return state.nurses.map((n) => `<option value="${n.id}">${escapeHtml(n.fullName)} - ${escapeHtml(n.shift)}</option>`).join(""); }
function appointmentOptions() { return state.appointments.map((a) => `<option value="${a.id}">#${a.id} - ${escapeHtml(a.patientName)} / Dr. ${escapeHtml(a.doctorName)}</option>`).join(""); }
function departmentOptions() { return state.departments.map((d) => `<option value="${escapeHtml(d.name)}">${escapeHtml(d.name)}</option>`).join(""); }
function allRecords() { return state.patients.flatMap((p) => p.records.map((r) => ({ ...r, patientName: p.fullName }))); }
function initials(name) { return String(name).split(" ").filter(Boolean).slice(0, 2).map((x) => x[0]).join("").toUpperCase(); }
function isToday(value) { return new Date(value).toDateString() === new Date().toDateString(); }
function empty(message) { return `<div class="empty">${message}</div>`; }
function formatDateTime(value) { return new Date(value).toLocaleString(undefined, { month: "short", day: "numeric", hour: "2-digit", minute: "2-digit" }); }
function escapeHtml(value = "") { return String(value).replaceAll("&", "&amp;").replaceAll("<", "&lt;").replaceAll(">", "&gt;").replaceAll('"', "&quot;").replaceAll("'", "&#039;"); }
