using Microsoft.AspNetCore.Mvc;
using workshop.wwwapi.Models;
using workshop.wwwapi.Repository;
using workshop.wwwapi.Repository.SpecificRepositories;

namespace workshop.wwwapi.Endpoints
{
    public static class PrescriptionEndpoints
    {
        public static void ConfigurePrescriptionEndpoints(this WebApplication app)
        {
            var prescriptions = app.MapGroup("/prescriptions");

            prescriptions.MapGet("/", GetPrescriptions);
            prescriptions.MapGet("/{id}", GetPrescriptionById);
            prescriptions.MapGet("/appointment/{patientId}/{doctorId}", GetPrescriptionsByAppointment);
            prescriptions.MapPost("/", CreatePrescription);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        public static async Task<IResult> GetPrescriptions(IPrescriptionRepository repository)
        {
            var prescriptions = await repository.GetAllAsync();
            return TypedResults.Ok(prescriptions);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        public static async Task<IResult> GetPrescriptionById(IPrescriptionRepository repository, int id)
        {
            var prescription = await repository.GetPrescriptionWithDetails(id);
            if (prescription == null) return TypedResults.NotFound();

            return TypedResults.Ok(prescription);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        public static async Task<IResult> GetPrescriptionsByAppointment(IPrescriptionRepository repository, int patientId, int doctorId)
        {
            var prescriptions = await repository.GetPrescriptionsByAppointmentId(patientId, doctorId);
            return TypedResults.Ok(prescriptions);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public static async Task<IResult> CreatePrescription(
            IPrescriptionRepository repository, IAppointmentRepository appointmentRepository, Prescription prescription)
        {
            var appointment = await appointmentRepository.GetAppointmentDetails(prescription.PatientId, prescription.DoctorId);
            if (appointment == null)
            {
                return TypedResults.BadRequest($"No appointment found for Patient {prescription.PatientId} and Doctor {prescription.DoctorId}");
            }

            var createdPrescription = await repository.AddAsync(prescription);
            return TypedResults.Created($"/prescriptions/{createdPrescription.Id}", createdPrescription);
        }
    }
}
