// Services/CustomAuthStateProvider.cs
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Mud_Q_Sass.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly AuthService _authService;

        public CustomAuthStateProvider(ILocalStorageService localStorage, AuthService authService)
        {
            _localStorage = localStorage;
            _authService = authService;

            // اشترك في تغيير حالة الـ Auth
            _authService.OnAuthStateChanged += NotifyAuthStateChanged;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("accessToken");

                if (string.IsNullOrEmpty(token))
                {
                    return CreateAnonymousState();
                }

                var claims = ParseClaimsFromJwt(token);
                var identity = new ClaimsIdentity(claims, "jwt");
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAuthenticationStateAsync: {ex.Message}");
                return CreateAnonymousState();
            }
        }

        private void NotifyAuthStateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        private AuthenticationState CreateAnonymousState()
        {
            var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
            return new AuthenticationState(anonymous);
        }

        private List<Claim> ParseClaimsFromJwt(string jwt)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwt);

                var claims = new List<Claim>();

                foreach (var claim in token.Claims)
                {
                    // تأكد إن الـ Role Claims بتتضاف بشكل صحيح
                    if (claim.Type == "role" || claim.Type == ClaimTypes.Role)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, claim.Value));
                    }
                    else if (claim.Type == "unique_name" || claim.Type == ClaimTypes.Name)
                    {
                        claims.Add(new Claim(ClaimTypes.Name, claim.Value));
                    }
                    else
                    {
                        claims.Add(claim);
                    }
                }

                return claims;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing JWT: {ex.Message}");
                return new List<Claim>();
            }
        }
    }
}