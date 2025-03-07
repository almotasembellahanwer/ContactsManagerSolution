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
    public class PersonsGetterService : IPersonsGetterService
    {
        private readonly IPersonsRepository _personsRepository;
        private readonly ILogger<PersonsAdderService> _logger;
        private readonly IDiagnosticContext _diagnosticContext;
        public PersonsGetterService(IPersonsRepository personsRepository, ILogger<PersonsAdderService> logger, IDiagnosticContext diagnosticContext)
        {
            _personsRepository = personsRepository;
            _logger = logger;
            _diagnosticContext = diagnosticContext;
        }

        #region Get


        public async Task<List<PersonResponse>> GetAllPersons()
        {
            _logger.LogInformation("GetAllPersons of PersonsService");
            var persons = await _personsRepository.GetAllPersons();
            //SELECT * FROM Persons
            return persons
                .Select(temp => temp.ToPersonResponse()).ToList();
            //return _db.sp_GetAllPersons()
            //    .Select(p=>ConvertPersonToPersonResponse(p)).ToList();
        }

        public async Task<PersonResponse?> GetPersonByPersonID(Guid? personID)
        {
            // check if personId is null
            if (personID == null)
            {
                return null;
            }
            // get matching person by corresponding personId
            Person? person = await _personsRepository.GetPersonByPersonID(personID);
            if (person == null)
                return null;
            // Convert the Person object to PersonResponse object and return it
            return person.ToPersonResponse();
        }

        public async Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString)
        {
            _logger.LogInformation("GetFilteredPersons of PersonsService");
            List<Person> persons;
            using (Operation.Time("Time for Filtered persons from Database"))
            {


                persons = searchBy switch
                {
                    nameof(PersonResponse.PersonName) => await
                         _personsRepository.GetFilteredPersons(p => p.PersonName.Contains(searchString!)),

                    nameof(PersonResponse.Address) => await
                    _personsRepository.GetFilteredPersons(p => p.Address!.Contains(searchString!)),
                    nameof(PersonResponse.Email) => await _personsRepository.GetFilteredPersons(p => p.Email!.Contains(searchString!)),
                    nameof(PersonResponse.DateOfBirth) => await _personsRepository.GetFilteredPersons(p => p.DateOfBirth!.Value.ToString("dd MMMM yyyy").Contains(searchString!)),
                    nameof(PersonResponse.Gender) =>
                        await _personsRepository.GetFilteredPersons(p => p.Gender!.Contains(searchString!)),
                    nameof(PersonResponse.CountryID) =>
                    await _personsRepository.GetFilteredPersons(p => p.CountryID!.Value.ToString().Contains(searchString!)),

                    _ => await _personsRepository.GetAllPersons()
                };
            } // end of "using block" of serilog timings
            _diagnosticContext.Set("Persons",persons);

            return persons.Select(p=>p.ToPersonResponse()).ToList();
        }

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

  
        public Task<MemoryStream> GetPersonsCSV()
        {
            MemoryStream memoryStream = new MemoryStream();
            StreamWriter streamWriter = new StreamWriter(memoryStream);
            CsvConfiguration csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture);
            CsvWriter csvWriter = new CsvWriter(streamWriter, csvConfiguration);
            // PersonName, Email, Address, Age, Gender, DateOfBirth, CountryID, Country, ReceiveNewsLetters
            csvWriter.WriteField(nameof(PersonResponse.PersonName));
            csvWriter.WriteField(nameof(PersonResponse.Email));
            csvWriter.WriteField(nameof(PersonResponse.Address));
            csvWriter.WriteField(nameof(PersonResponse.Age));
            csvWriter.WriteField(nameof(PersonResponse.Gender));
            csvWriter.WriteField(nameof(PersonResponse.DateOfBirth));
            csvWriter.WriteField(nameof(PersonResponse.Country));
            csvWriter.WriteField(nameof(PersonResponse.ReceiveNewsLetters));
            csvWriter.NextRecord();
            List<PersonResponse> persons = GetAllPersons().Result;
            foreach (var person in persons)
            {
                csvWriter.WriteField(person.PersonName);
                csvWriter.WriteField(person.Email);
                csvWriter.WriteField(person.Address);
                csvWriter.WriteField(person.Age);
                csvWriter.WriteField(person.Gender);
                if(person.DateOfBirth.HasValue)
                    csvWriter.WriteField(person.DateOfBirth.Value.ToString("dd MMM yyyy"));
                
                csvWriter.WriteField(person.Country);
                csvWriter.WriteField(person.ReceiveNewsLetters);
                csvWriter.NextRecord();
                csvWriter.Flush();
            }
            memoryStream.Position = 0;
            return Task.FromResult(memoryStream);
        }

        public virtual async Task<MemoryStream> GetPersonsExcel()
        {
            MemoryStream memoryStream = new MemoryStream();
            using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets
                    .Add("PersonsSheet");
                worksheet.Cells["A1"].Value = nameof(PersonResponse.PersonName);
                worksheet.Cells["B1"].Value = nameof(PersonResponse.Email);
                worksheet.Cells["C1"].Value = nameof(PersonResponse.DateOfBirth);
                worksheet.Cells["D1"].Value = nameof(PersonResponse.Age);
                worksheet.Cells["E1"].Value = nameof(PersonResponse.Gender);
                worksheet.Cells["F1"].Value = nameof(PersonResponse.Country);
                worksheet.Cells["G1"].Value = nameof(PersonResponse.Address);
                worksheet.Cells["H1"].Value = nameof(PersonResponse.ReceiveNewsLetters);
                using(ExcelRange headerCells = worksheet.Cells["A1:H1"])
                {
                    headerCells.Style.Font.Bold = true;
                    headerCells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    headerCells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }
                List<Person> allPersons = await _personsRepository.GetAllPersons();
                List<PersonResponse> persons = allPersons
                    .Select(temp=>temp.ToPersonResponse()).ToList();
                int row = 2;
                foreach (var person in persons)
                {
                    
                    worksheet.Cells[row,1].Value = person.PersonName;
                    worksheet.Cells[row, 2].Value = person.Email;
                    if(person.DateOfBirth.HasValue)
                    {
                        worksheet.Cells[row, 3].Value = person.DateOfBirth.Value
                            .ToString("yyyy-MM-dd");
                    };
                    worksheet.Cells[row, 4].Value = person.Age;
                    worksheet.Cells[row, 5].Value = person.Gender;
                    worksheet.Cells[row, 6].Value = person.Country;
                    worksheet.Cells[row, 7].Value = person.Address;
                    worksheet.Cells[row, 8].Value = person.ReceiveNewsLetters;
                    row++;
                }
                worksheet.Cells[$"A1:H{row}"].AutoFitColumns();
                await excelPackage.SaveAsync();
            }
            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}
