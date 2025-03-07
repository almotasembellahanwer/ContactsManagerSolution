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
    public class PersonsSorterService : IPersonsSorterService
    {
        private readonly IPersonsRepository _personsRepository;
        private readonly ILogger<PersonsAdderService> _logger;
        private readonly IDiagnosticContext _diagnosticContext;
        public PersonsSorterService(IPersonsRepository personsRepository, ILogger<PersonsAdderService> logger, IDiagnosticContext diagnosticContext)
        {
            _personsRepository = personsRepository;
            _logger = logger;
            _diagnosticContext = diagnosticContext;
        }

  

        #region Get

        public Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, SortOrderOptions sortOrder)
        {
            //            if (string.IsNullOrEmpty(sortBy))
            //            {
            //                return allPersons;
            //            }
            //            List<PersonResponse> sortedPersons = (sortBy, sortOrder) switch
            //            {
            //                (nameof(PersonResponse.PersonName), SortOrderOptions.ASC)
            //                => allPersons.OrderBy(p => p.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),
            //                (nameof(PersonResponse.PersonName), SortOrderOptions.DESC)
            //=> allPersons.OrderByDescending(p => p.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),

            //                (nameof(PersonResponse.Address), SortOrderOptions.ASC)
            //                   => allPersons.OrderBy(p => p.Address, StringComparer.OrdinalIgnoreCase).ToList(),
            //                (nameof(PersonResponse.Address), SortOrderOptions.DESC)
            //=> allPersons.OrderByDescending(p => p.Address, StringComparer.OrdinalIgnoreCase).ToList(),
            //                (nameof(PersonResponse.Email), SortOrderOptions.ASC)
            //=> allPersons.OrderBy(p => p.Email, StringComparer.OrdinalIgnoreCase).ToList(),
            //                (nameof(PersonResponse.Email), SortOrderOptions.DESC)
            //=> allPersons.OrderByDescending(p => p.Email, StringComparer.OrdinalIgnoreCase).ToList(),
            //                (nameof(PersonResponse.DateOfBirth), SortOrderOptions.ASC)
            //=> allPersons.OrderBy(p => p.DateOfBirth).ToList(),
            //                (nameof(PersonResponse.DateOfBirth), SortOrderOptions.DESC)
            // => allPersons.OrderByDescending(p => p.DateOfBirth).ToList(),
            //                (nameof(PersonResponse.Country), SortOrderOptions.ASC)
            //=> allPersons.OrderBy(p => p.Country, StringComparer.OrdinalIgnoreCase).ToList(),
            //                (nameof(PersonResponse.Country), SortOrderOptions.DESC)
            //=> allPersons.OrderByDescending(p => p.Country, StringComparer.OrdinalIgnoreCase).ToList(),

            //                (nameof(PersonResponse.Age), SortOrderOptions.ASC)
            //=> allPersons.OrderBy(p => p.Age).ToList(),
            //                (nameof(PersonResponse.Age), SortOrderOptions.DESC)
            //=> allPersons.OrderByDescending(p => p.Age).ToList(),
            //                _ => allPersons
            //            };

            //return sortedPersons;

            _logger.LogInformation("GetSortedPersons of PersonsService");

            // another solution using reflection
            if (string.IsNullOrEmpty(sortBy))
            {
                return Task.FromResult(allPersons);
            }
            Type personResponseType = typeof(PersonResponse);
            PropertyInfo? sortByProperty = personResponseType.GetProperty(sortBy);
            if (sortByProperty == null) { return Task.FromResult(allPersons); }
            IOrderedEnumerable<PersonResponse> sortedList;

            if(sortOrder == SortOrderOptions.ASC)
            {
                sortedList = sortByProperty.GetType() == typeof(string) ? allPersons.OrderBy(person=>(string?)sortByProperty.GetValue(person),StringComparer.OrdinalIgnoreCase)
                    : allPersons.OrderBy(p => sortByProperty.GetValue(p)!.ToString());
            }
            else
            {
                sortedList = sortByProperty.GetType() == typeof(string) ? allPersons.OrderByDescending(person => (string?)sortByProperty.GetValue(person), StringComparer.OrdinalIgnoreCase)
    :           allPersons.OrderByDescending(person => sortByProperty.GetValue(person)!.ToString());
            }
            return Task.FromResult(sortedList.ToList());
        }


        #endregion
    }
}
