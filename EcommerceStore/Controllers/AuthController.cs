using EcommerceStore.Dtos;
using EcommerceStore.Infrastructure;
using EcommerceStore.Models;
using EcommerceStore.Services.Interfaces;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EcommerceStore.Controllers
{
    /// <summary>Replaces Areas/Identity/Pages (Login, Register, ExternalLogin). Issues JWTs.</summary>
    [Route("api/auth")]
    public class AuthController : ApiControllerBase
    {
        private const string CustomerRole = "Customer";
        private const string GoogleProvider = "Google";

        private readonly UserManager<ApplicationUser> _users;
        private readonly SignInManager<ApplicationUser> _signIn;
        private readonly TokenService _tokens;
        private readonly ICartService _cart;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<ApplicationUser> users,
            SignInManager<ApplicationUser> signIn,
            TokenService tokens,
            ICartService cart,
            IConfiguration config,
            ILogger<AuthController> logger)
        {
            _users = users;
            _signIn = signIn;
            _tokens = tokens;
            _cart = cart;
            _config = config;
            _logger = logger;
        }

        [HttpPost("register")]
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName
            };

            var result = await _users.CreateAsync(user, request.Password);
            if (!result.Succeeded) return IdentityProblem(result);

            await _users.AddToRoleAsync(user, CustomerRole);
            await TryMergeGuestCartAsync(user.Id, request.GuestCartId);

            _logger.LogInformation("User registered: {UserId}", user.Id);
            return await IssueTokenAsync(user);
        }

        [HttpPost("login")]
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            var user = await _users.FindByEmailAsync(request.Email);
            if (user is null) return InvalidCredentials();

            var result = await _signIn.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (result.IsLockedOut)
                return Problem(title: "Account temporarily locked. Please try again later.", statusCode: StatusCodes.Status423Locked);
            if (!result.Succeeded) return InvalidCredentials();

            await TryMergeGuestCartAsync(user.Id, request.GuestCartId);
            return await IssueTokenAsync(user);
        }

        /// <summary>
        /// Google sign-in for SPAs: React obtains a Google ID token (Google Identity Services)
        /// and posts it here. We verify it and issue our own JWT.
        /// </summary>
        [HttpPost("google")]
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<AuthResponse>> Google(GoogleLoginRequest request)
        {
            var clientId = _config["Authentication:Google:ClientId"];
            if (string.IsNullOrWhiteSpace(clientId))
                return Problem(title: "Google login is not configured.", statusCode: StatusCodes.Status501NotImplemented);

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(
                    request.IdToken,
                    new GoogleJsonWebSignature.ValidationSettings { Audience = new[] { clientId } });
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogWarning(ex, "Invalid Google ID token");
                return Problem(title: "Invalid Google token.", statusCode: StatusCodes.Status401Unauthorized);
            }

            if (!payload.EmailVerified)
                return Problem(title: "Your Google account email is not verified.", statusCode: StatusCodes.Status400BadRequest);

            var user = await _users.FindByLoginAsync(GoogleProvider, payload.Subject);
            if (user is null)
            {
                // Existing local account with the same (Google-verified) email -> link it; otherwise create one.
                user = await _users.FindByEmailAsync(payload.Email);
                if (user is null)
                {
                    user = new ApplicationUser
                    {
                        UserName = payload.Email,
                        Email = payload.Email,
                        EmailConfirmed = true,
                        FirstName = payload.GivenName,
                        LastName = payload.FamilyName
                    };

                    var created = await _users.CreateAsync(user);
                    if (!created.Succeeded) return IdentityProblem(created);
                    await _users.AddToRoleAsync(user, CustomerRole);
                }

                var linked = await _users.AddLoginAsync(user, new UserLoginInfo(GoogleProvider, payload.Subject, GoogleProvider));
                if (!linked.Succeeded) return IdentityProblem(linked);
            }

            if (await _users.IsLockedOutAsync(user))
                return Problem(title: "Account temporarily locked. Please try again later.", statusCode: StatusCodes.Status423Locked);

            await TryMergeGuestCartAsync(user.Id, request.GuestCartId);
            return await IssueTokenAsync(user);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> Me()
        {
            var user = await _users.FindByIdAsync(CurrentUserId ?? string.Empty);
            if (user is null) return Unauthorized();

            return ToUserDto(user, await _users.GetRolesAsync(user));
        }

        // ---------------- helpers ----------------

        private async Task<ActionResult<AuthResponse>> IssueTokenAsync(ApplicationUser user)
        {
            var (token, expiresAt) = await _tokens.CreateAccessTokenAsync(user);
            var roles = await _users.GetRolesAsync(user);
            return new AuthResponse(token, expiresAt, ToUserDto(user, roles));
        }

        private static UserDto ToUserDto(ApplicationUser user, IList<string> roles) =>
            new(user.Id, user.Email ?? string.Empty, user.FirstName, user.LastName, roles.ToList());

        private ActionResult InvalidCredentials() =>
            Problem(title: "Invalid email or password.", statusCode: StatusCodes.Status401Unauthorized);

        private ActionResult IdentityProblem(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }
            return ValidationProblem(ModelState);
        }

        /// <summary>A failed cart merge must never fail the login/registration itself.</summary>
        private async Task TryMergeGuestCartAsync(string userId, string? guestCartId)
        {
            try
            {
                await _cart.MergeGuestCartOnLoginAsync(userId, guestCartId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error merging guest cart for user {UserId}", userId);
            }
        }
    }
}
