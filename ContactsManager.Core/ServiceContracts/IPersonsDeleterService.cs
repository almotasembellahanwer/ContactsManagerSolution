using ServiceContracts.DTO;
using ServiceContracts.Enums;
namespace ServiceContracts
{
    public interface IPersonsDeleterService
    {

        /// <summary>
        /// Delete person by given personiD
        /// </summary>
        /// <param name="personId">personId for object to delete</param>
        /// <returns>return true if object is found otherwise false</returns>

        Task<bool> DeletePerson(Guid? personId);
    }
}
