using CsvHelper;
using CsvHelper.Configuration;
using Entities;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services.Helpers;
using System.Globalization;
using System.Reflection;
using OfficeOpenXml;
using RepositoryContracts;
using Microsoft.Extensions.Logging;
using Serilog;
using SerilogTimings;
using Exceptions;
namespace Services
{
    public class PersonsAdderService : IPersonsAdderService
    {
        private readonly IPersonsRepository _personsRepository;
        private readonly ILogger<PersonsAdderService> _logger;
        private readonly IDiagnosticContext _diagnosticContext;
        public PersonsAdderService(IPersonsRepository personsRepository, ILogger<PersonsAdderService> logger, IDiagnosticContext diagnosticContext)
        {
            _personsRepository = personsRepository;
            _logger = logger;
            _diagnosticContext = diagnosticContext;
        }

        #region Add
        public async Task<PersonResponse?> AddPerson(PersonAddRequest? personAddRequest)
        {
            // check if personAddRequest is null
            if (personAddRequest == null)
            {
                throw new ArgumentNullException();
            }
            // Validate all propertoies of the personAddRequest
            ValidationHelper.ModelValidate(personAddRequest);
            // Convert the personAddRequest from PersonAddRequest object to a Person object
            Person? person = personAddRequest.ToPerson();
            // Generate a new PersonId
            person.PersonID = Guid.NewGuid();
            // Add the person to the list of persons
            await _personsRepository.AddPerson(person);
            //_db.sp_InsertPerson(person);
            // return a PersonResponse object

            return person.ToPersonResponse();
        }

        #endregion

 
    }
}
