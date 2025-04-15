namespace StudentsAdmin.Services
{
    public interface IImageService
    {
        Task<string> SaveImageAsync(IFormFile file);
    }
}
