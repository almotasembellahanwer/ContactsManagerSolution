using Entities;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountriesAdderService : ICountriesAdderService
    {
        // private list of countries
        private readonly ICountriesRepository _countriesRepository;
        public CountriesAdderService(ICountriesRepository countriesRepository)
        {
            _countriesRepository = countriesRepository;
        }
        #region AddCountry
        public async Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest)
        {
            // check if countryAddRequest is null
            if (countryAddRequest == null)
            {
                throw new ArgumentNullException(nameof(countryAddRequest));
            }
            // check if country name is null or empty
            if (string.IsNullOrWhiteSpace(countryAddRequest.CountryName))
            {
                throw new ArgumentException("Country name is required", nameof(countryAddRequest.CountryName));
            }
            // check if country with the same name already exists
            if (await _countriesRepository.GetCountryByCountryName(countryAddRequest.CountryName) != null)
            {
                throw new ArgumentException("Country with the same name already exists");
            }
            //Convert object from 'CountryAddRequest' to 'Country'
            Country country = countryAddRequest.ToCountry();

            // generate country id
            country.CountryID = Guid.NewGuid();
            // add country to the list of countries
            await _countriesRepository.AddCountry(country);
            // conevert object from 'Country' to 'CountryResponse'
            return country.ToCountryResponse();
        }
        #endregion
    }
}
