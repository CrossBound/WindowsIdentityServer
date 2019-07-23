using IdentityModel;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace LegacyBank.LEAF.Authentication.Windows.API.Controllers
{
    public class AccountController : Controller
    {
        private IIdentityServerInteractionService _interactionService;

        public AccountController(IIdentityServerInteractionService InteractionService)
        {
            if (InteractionService == null)
            {
                throw new ArgumentNullException(nameof(InteractionService)
                    , nameof(InteractionService) + " is required");
            }

            _interactionService = InteractionService;
        }

        [HttpGet]
        public async Task<IActionResult> Login([FromQuery]string ReturnURL)
        {
            if (_interactionService.IsValidReturnUrl(ReturnURL) == false)
            {
                // Security concern
                throw new ArgumentOutOfRangeException(nameof(ReturnURL)
                    , nameof(ReturnURL) + " is not a valid URL");
            }

            // attempt to login via Windows
            var loginResult = await HttpContext.AuthenticateAsync("Windows");
            if(loginResult.Succeeded)
            {
                var windowsIdentity = (WindowsIdentity)loginResult.Principal.Identity;
                var sid = windowsIdentity.User.Value;
                var username = windowsIdentity.Name;

                var newIdentity = new ClaimsIdentity("Windows");
                newIdentity.AddClaim(new Claim(JwtClaimTypes.Subject, sid));
                newIdentity.AddClaim(new Claim(JwtClaimTypes.Name, username));
                var principal = new ClaimsPrincipal(newIdentity);

                await HttpContext.SignInAsync(principal);
                
                return Redirect(ReturnURL);
            }
            else
            {
                return Challenge("Windows");
            }
        }
    }
}