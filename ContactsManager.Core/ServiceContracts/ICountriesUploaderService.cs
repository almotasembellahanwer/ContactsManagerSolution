using Microsoft.AspNetCore.Http;
using ServiceContracts.DTO;

namespace ServiceContracts
{
    /// <summary>
    /// Represents busseiness logic for manipulating County entity
    /// </summary>
    public interface ICountriesUploaderService
    {
        Task<int> UploadCountriesFromExcelFile(IFormFile formFiles);
    }
}
