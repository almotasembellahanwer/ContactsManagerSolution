using Entities;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountriesGetterService : ICountriesGetterService
    {
        // private list of countries
        private readonly ICountriesRepository _countriesRepository;
        public CountriesGetterService(ICountriesRepository countriesRepository)
        {
            _countriesRepository = countriesRepository;
        }
        #region GetAllCountries
        public async Task<List<CountryResponse>> GetAllCountries()
        {
            List<Country> countries = await _countriesRepository.GetAllCountries();
            return countries.Select(c => c.ToCountryResponse()).ToList();
        }

        public async Task<CountryResponse?> GetCountryByCountryID(Guid? countryID)
        {
            // check if countryId is  not null
            if (countryID == null)
            {
                return null;
            }
            // get maching country from the list of countries
            Country? country = await _countriesRepository.GetCountryByCountryID(countryID);
            if (country == null) {
                return null;

            }
            // convert object from 'Country' to 'CountryResponse'
            CountryResponse response = country.ToCountryResponse();
            //return countryResponse object
            return response;
        }


        #endregion
    }
}
