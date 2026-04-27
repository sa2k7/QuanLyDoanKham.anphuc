using Microsoft.AspNetCore.Http;

namespace QuanLyDoanKham.API.Services.Files
{
    public interface IFileService
    {
        /// <summary>
        /// Saves a file to the specified subfolder under wwwroot/uploads.
        /// </summary>
        /// <param name="file">The file to save.</param>
        /// <param name="subFolder">Subfolder name (e.g., 'contracts', 'avatars').</param>
        /// <returns>Relative path of the saved file or an error message starting with 'Error:'.</returns>
        Task<(string? Path, string? Error)> SaveFileAsync(IFormFile file, string subFolder);

        /// <summary>
        /// Deletes a file from the physical storage.
        /// </summary>
        /// <param name="relativePath">The relative path stored in the database.</param>
        void DeleteFile(string relativePath);
        
        /// <summary>
        /// Returns the list of allowed extensions.
        /// </summary>
        string[] GetAllowedExtensions();
    }
}
