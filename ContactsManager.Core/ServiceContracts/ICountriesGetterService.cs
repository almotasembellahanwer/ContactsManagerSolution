using Microsoft.AspNetCore.Http;
using ServiceContracts.DTO;

namespace ServiceContracts
{
    /// <summary>
    /// Represents busseiness logic for manipulating County entity
    /// </summary>
    public interface ICountriesGetterService
    {
        /// <summary>
        /// Gets all countries from the list of countries
        /// </summary>
        /// <returns>All countries from the list</returns>
        Task<List<CountryResponse>> GetAllCountries();
        /// <summary>
        /// Gets a country by country id
        /// </summary>
        /// <param name="countryID">contry id(guid) to search</param>
        /// <returns>matching country as countryResponse object</returns>
        Task<CountryResponse?> GetCountryByCountryID(Guid? countryID);
    }
}
