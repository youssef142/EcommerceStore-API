namespace EcommerceStore.Services.Interfaces
{
    public interface IImageStorageService
    {
        /// <summary>Validates and stores a product image; returns the relative public path (e.g. /uploads/products/abc.jpg).</summary>
        Task<string> SaveProductImageAsync(IFormFile file, CancellationToken cancellationToken = default);
    }
}
