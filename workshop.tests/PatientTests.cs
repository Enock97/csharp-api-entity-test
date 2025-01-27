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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net;
using workshop.wwwapi.Repository.SpecificRepositories;

namespace workshop.tests
{

    public class PatientTests
    {

        private Mock<IPatientRepository> _mockRepo;

        private HttpClient _client;
        private WebApplicationFactory<Program> _factory;

        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<IPatientRepository>();
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddJsonFile("appsettings.json");
                    });

                    builder.ConfigureServices((context, services) =>
                    {
                        // Set the environment to Testing
                        context.HostingEnvironment.EnvironmentName = "Testing";

                        // Remove existing DbContext registration (if any)
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<DatabaseContext>));
                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }

                        // Add the InMemory database for testing
                        services.AddDbContext<DatabaseContext>(options =>
                            options.UseInMemoryDatabase("TestDb"));
                    });
                });

            _client = _factory.CreateClient();
        }




        [Test]
        public async Task PatientEndpointStatus()
        {
            // Arrange
            var patientExamples = new List<Patient>
            {
                new Patient { Id = 1, FullName = "John Doe" },
                new Patient { Id = 2, FullName = "Jane Smith" }
            };

            _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(patientExamples);

            // Act
            var response = await _client.GetAsync("/patients");
            var responseBody = await response.Content.ReadAsStringAsync();

            // Debugging logs
            Console.WriteLine($"Response Status: {response.StatusCode}");
            Console.WriteLine($"Response Body: {responseBody}");

            // Fail if response is not 200 OK
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,
                $"Expected status code 200 OK, but got {response.StatusCode}. Response body: {responseBody}");

            // Ensure the response is valid JSON before deserialization
            Assert.IsTrue(responseBody.StartsWith("{") || responseBody.StartsWith("["),
                $"Response is not valid JSON. Response body: {responseBody}");

            // Parse JSON safely
            try
            {
                var patients = JsonConvert.DeserializeObject<List<PatientDTO>>(responseBody);
                Assert.IsNotNull(patients, "Patients list should not be null.");
                Assert.IsTrue(patients.Count >= 0,
                    $"Unexpected patient count. Response Body: {responseBody}");
            }
            catch (JsonException ex)
            {
                Assert.Fail($"JSON deserialization failed: {ex.Message} \nResponse Body: {responseBody}");
            }
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

            // Debugging: Print the result
            Console.WriteLine($"Test Result Type: {result?.GetType()}");

            // Ensure response is JSON and contains expected data
            Assert.IsNotNull(result, "The result should not be null.");
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode, "Expected status code 200 OK.");
            Assert.IsNotNull(result.Value, "The response value should not be null.");
            Assert.AreEqual(2, result.Value.Count, "The number of returned patients does not match.");
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
