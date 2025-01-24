using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using workshop.wwwapi.Endpoints;
using workshop.wwwapi.Models.Responses;
using workshop.wwwapi.Models;
using workshop.wwwapi.Repository;
using workshop.wwwapi.Data;
using Microsoft.EntityFrameworkCore;

namespace workshop.tests
{

    public class PatientTests
    {

        private Mock<IPatientRepository> _mockRepo;

        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<IPatientRepository>();
        }

        [Test]
        public async Task GetPatients_ReturnsListOfPatients()
        {
            // Arrange: Seed test data
            var patients = new List<Patient>
                {
                    new Patient { Id = 1, FullName = "John Doe", Appointments = new List<Appointment>() },
                    new Patient { Id = 2, FullName = "Jane Smith", Appointments = new List<Appointment>() }
                };

            _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(patients);

            // Act: Call the endpoint method
            var result = await PatientEndpoints.GetPatients(_mockRepo.Object) as Ok<List<PatientDTO>>;

            // Assert: Verify the result
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
            Assert.AreEqual(2, result.Value.Count);
        }

        [Test]
        public async Task CreatePatient_ValidPatient_ReturnsCreatedResult()
        {
            // Arrange: New patient data
            var newPatient = new Patient { Id = 3, FullName = "Alice Johnson" };
            _mockRepo.Setup(repo => repo.AddAsync(It.IsAny<Patient>())).ReturnsAsync(newPatient);

            // Act
            var result = await PatientEndpoints.CreatePatient(_mockRepo.Object, newPatient) as Created<Patient>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status201Created, result.StatusCode);
            Assert.AreEqual(newPatient.FullName, result.Value.FullName);
        }
    }
}
