using EcommerceStore.Services.Interfaces;

namespace EcommerceStore.Services
{
    /// <summary>
    /// Stores admin-uploaded images in wwwroot/uploads/products.
    /// (wwwroot/images/products is reserved for the seeded/static images that ship with the app.)
    /// Swap this class for an Azure Blob / S3 implementation later without touching controllers.
    /// </summary>
    public sealed class LocalImageStorageService : IImageStorageService
    {
        private const long DefaultMaxBytes = 2 * 1024 * 1024;

        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public LocalImageStorageService(IWebHostEnvironment env, IConfiguration config)
        {
            _env = env;
            _config = config;
        }

        public async Task<string> SaveProductImageAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            var maxBytes = _config.GetValue<long?>("Images:MaxBytes") ?? DefaultMaxBytes;

            if (file == null || file.Length == 0)
                throw new BadHttpRequestException("No file was uploaded.");
            if (file.Length > maxBytes)
                throw new BadHttpRequestException($"Image is too large. Maximum size is {maxBytes / 1024 / 1024} MB.");

            // Don't trust the file name or Content-Type: sniff the real format from the first bytes.
            var header = new byte[12];
            int read;
            await using (var stream = file.OpenReadStream())
            {
                read = await stream.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, cancellationToken);
            }

            var extension = read == header.Length ? DetectExtension(header) : null;
            if (extension == null)
                throw new BadHttpRequestException("Unsupported image. Allowed formats: JPEG, PNG, WebP.");

            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var directory = Path.Combine(webRoot, "uploads", "products");
            Directory.CreateDirectory(directory);

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(directory, fileName);

            await using (var target = new FileStream(fullPath, FileMode.CreateNew))
            {
                await file.CopyToAsync(target, cancellationToken);
            }

            return $"/uploads/products/{fileName}";
        }

        private static string? DetectExtension(byte[] h)
        {
            if (h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF) return ".jpg";
            if (h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47) return ".png";
            if (h[0] == 'R' && h[1] == 'I' && h[2] == 'F' && h[3] == 'F' &&
                h[8] == 'W' && h[9] == 'E' && h[10] == 'B' && h[11] == 'P') return ".webp";
            return null; // SVG deliberately not allowed (script injection risk)
        }
    }
}
