using ServiceContracts.DTO;
using ServiceContracts.Enums;
namespace ServiceContracts
{
    public interface IPersonsUpdaterService
    {
  
        /// <summary>
        /// Return person the have been updated
        /// </summary>
        /// <param name="personToUpdate">person to update</param>
        /// <returns>Returns person that have been updated of type 'PersonResponse'</returns>

        Task<PersonResponse?> UpdatePerson(PersonUpdateRequest? personToUpdate);

    }
}
