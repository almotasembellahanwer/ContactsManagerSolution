using Entities;
using OfficeOpenXml;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class PersonsGetterServiceWithFewExcelFields : IPersonsGetterService
    {
        private readonly IPersonsRepository _personsRepository;
        private readonly PersonsGetterService _personsGetterService;

        public PersonsGetterServiceWithFewExcelFields(IPersonsRepository personsRepository, PersonsGetterService personsGetterService)
        {
            _personsRepository = personsRepository;
            _personsGetterService = personsGetterService;
        }

        public async Task<List<PersonResponse>> GetAllPersons()
        {
            return await _personsGetterService.GetAllPersons();
        }

        public async Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString)
        {
            return await _personsGetterService.GetFilteredPersons(searchBy, searchString);
        }

        public async Task<PersonResponse?> GetPersonByPersonID(Guid? personId)
        {
            return await _personsGetterService.GetPersonByPersonID(personId);

        }

        public async Task<MemoryStream> GetPersonsCSV()
        {
            return await _personsGetterService.GetPersonsCSV();
        }

        public async Task<MemoryStream> GetPersonsExcel()
        {
            MemoryStream memoryStream = new MemoryStream();
            using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets
                    .Add("PersonsSheet");
                worksheet.Cells["A1"].Value = nameof(PersonResponse.PersonName);
                worksheet.Cells["B1"].Value = nameof(PersonResponse.Age);
                worksheet.Cells["C1"].Value = nameof(PersonResponse.Gender);
                using (ExcelRange headerCells = worksheet.Cells["A1:C1"])
                {
                    headerCells.Style.Font.Bold = true;
                    headerCells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    headerCells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }
                List<Person> allPersons = await _personsRepository.GetAllPersons();
                List<PersonResponse> persons = allPersons
                    .Select(temp => temp.ToPersonResponse()).ToList();
                int row = 2;
                foreach (var person in persons)
                {

                    worksheet.Cells[row, 1].Value = person.PersonName;
                    worksheet.Cells[row, 2].Value = person.Age;
                    worksheet.Cells[row, 3].Value = person.Gender;
                    row++;
                }
                worksheet.Cells[$"A1:C{row}"].AutoFitColumns();
                await excelPackage.SaveAsync();
            }
            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}
