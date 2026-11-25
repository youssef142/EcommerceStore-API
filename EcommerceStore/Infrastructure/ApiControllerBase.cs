using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceStore.Infrastructure
{
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        /// <summary>
        /// Public base URL used to turn stored relative image paths into absolute URLs.
        /// Set App:PublicBaseUrl in production if the API runs behind a proxy/CDN.
        /// </summary>
        protected string BaseUrl
        {
            get
            {
                var configured = HttpContext.RequestServices
                    .GetRequiredService<IConfiguration>()["App:PublicBaseUrl"];

                return !string.IsNullOrWhiteSpace(configured)
                    ? configured.TrimEnd('/')
                    : $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            }
        }
    }
}
