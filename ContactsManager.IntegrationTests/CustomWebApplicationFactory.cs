using Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RepositoryContracts;
namespace CRUDTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.UseEnvironment("Test");
            builder.ConfigureServices(services =>
            {
                // Remove existing DbContextOptions<ApplicationDbContext>
                var dbContextOptionsDescriptor = services.SingleOrDefault(temp =>
                    temp.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (dbContextOptionsDescriptor != null)
                {
                    services.Remove(dbContextOptionsDescriptor);
                }

                // Register InMemory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase("DatabaseForTesting")
                    );

                // Register a Mocked Repository for tests
                var mockPersonsRepository = new Mock<IPersonsRepository>();
                mockPersonsRepository.Setup(temp => temp.GetAllPersons()).ReturnsAsync(new List<Person>());
                services.AddSingleton(mockPersonsRepository.Object);

                var mockCountriesRepository = new Mock<ICountriesRepository>();
                mockCountriesRepository.Setup(temp => temp.GetAllCountries()).ReturnsAsync(new List<Country>());
                services.AddSingleton(mockCountriesRepository.Object);
            }
                
            );
        }
    }
}
