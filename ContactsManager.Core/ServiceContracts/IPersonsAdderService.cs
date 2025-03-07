using ServiceContracts.DTO;
using ServiceContracts.Enums;
namespace ServiceContracts
{
    public interface IPersonsAdderService
    {
        /// <summary>
        /// Adds a new person to the list of persons
        /// </summary>
        /// <param name="person">person to add</param>
        /// <returns>returns a PersonResponse object</returns>
        Task<PersonResponse?> AddPerson(PersonAddRequest? personAddRequest);

    }
}
