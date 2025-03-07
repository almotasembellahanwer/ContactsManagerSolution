using ServiceContracts.DTO;
using ServiceContracts.Enums;
namespace ServiceContracts
{
    public interface IPersonsGetterService
    {
 
        /// <summary>
        /// Gets All Persons
        /// </summary>
        /// <returns>Returns a list of objects of PersonResponse</returns>
        Task<List<PersonResponse>> GetAllPersons();
        Task<PersonResponse?> GetPersonByPersonID(Guid? personId);

        /// <summary>
        /// Returns all person objects that matches with the given search field and search string
        /// </summary>
        /// <param name="searchBy">Search field to search</param>
        /// <param name="searchString">Search string to search</param>
        /// <returns>Returns all matching persons based on the given search field and search string</returns>
        Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString);

        /// <summary>
        /// Returns the persons as CSV file
        /// </summary>
        /// <returns>Return the memoryStream with CSV data</returns>
        Task<MemoryStream> GetPersonsCSV();
        /// <summary>
        /// Returns the persons as Excel file
        /// </summary>
        /// <returns>Return the memoryStream with Excel data of Persons</returns>
        Task<MemoryStream> GetPersonsExcel();

    }
}
