using Entities;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountriesUploaderService : ICountriesUploaderService
    {
        // private list of countries
        private readonly ICountriesRepository _countriesRepository;
        public CountriesUploaderService(ICountriesRepository countriesRepository)
        {
            _countriesRepository = countriesRepository;
        }
        public async Task<int> UploadCountriesFromExcelFile(IFormFile formFiles)
        {
            MemoryStream memoryStream = new MemoryStream();
            await formFiles.CopyToAsync(memoryStream);
            int countriesInserted = 0;
            using(ExcelPackage excelPackage = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets["Countries"];
                int rowCount = worksheet.Dimension.Rows;
                for(int row = 2;row <= rowCount; row++)
                {
                    string? cellValue = worksheet.Cells[row, 1].Value.ToString();
                    if (!string.IsNullOrEmpty(cellValue))
                    {
                        string? countryName = cellValue;
                        Country country = new Country
                        {
                            CountryName = countryName
                        };
                        if (await _countriesRepository.GetCountryByCountryName(countryName) == null)
                        {
                            await _countriesRepository.AddCountry(country);
                        }
                    }
                    countriesInserted++;

                   
                }
            }
            return countriesInserted;
        }
    }
}
