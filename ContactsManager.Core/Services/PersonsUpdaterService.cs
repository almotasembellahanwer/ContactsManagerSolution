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
    public class PersonsUpdaterService : IPersonsUpdaterService
    {
        private readonly IPersonsRepository _personsRepository;
        private readonly ILogger<PersonsAdderService> _logger;
        private readonly IDiagnosticContext _diagnosticContext;
        public PersonsUpdaterService(IPersonsRepository personsRepository, ILogger<PersonsAdderService> logger, IDiagnosticContext diagnosticContext)
        {
            _personsRepository = personsRepository;
            _logger = logger;
            _diagnosticContext = diagnosticContext;
        }


        #region UpdatePerson
        public async Task<PersonResponse?> UpdatePerson(PersonUpdateRequest? personUpdateRequest)
        {
            if (personUpdateRequest == null)
                throw new ArgumentNullException(nameof(personUpdateRequest));

            //validation
            ValidationHelper.ModelValidate(personUpdateRequest);

            //get matching person object to update
            Person? matchingPerson = await _personsRepository.GetPersonByPersonID(personUpdateRequest.PersonID);
            if (matchingPerson == null)
            {
                throw new InvalidPersonIDException("Given person id doesn't exist");
            }

            //update all details
            matchingPerson.PersonName = personUpdateRequest.PersonName;
            matchingPerson.Email = personUpdateRequest.Email;
            matchingPerson.DateOfBirth = personUpdateRequest.DateOfBirth;
            matchingPerson.Gender = personUpdateRequest.Gender.ToString();
            matchingPerson.CountryID = personUpdateRequest.CountryID;
            matchingPerson.Address = personUpdateRequest.Address;
            matchingPerson.ReceiveNewsLetters = personUpdateRequest.ReceiveNewsLetters;

            await _personsRepository.UpdatePerson(matchingPerson); //UPDATE

            return matchingPerson.ToPersonResponse();
        }

        #endregion
    }
}
