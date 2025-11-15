using Blazored.LocalStorage;
using Mud.Shared.Common;
using Mud.Shared.DTOs;

namespace Mud_Q_Sass.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;

        public event Action? OnAuthStateChanged;

        public AuthService(IHttpClientFactory httpClientFactory, ILocalStorageService localStorage)
        {
            _http = httpClientFactory.CreateClient("Auth");
            _localStorage = localStorage;
        }

        public async Task<ApiResponse<AuthResponse>?> LoginAsync(LoginDto dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/login", dto);
                var content = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();

                if (content?.Success == true && content.Data != null)
                {
                    await SaveTokens(content.Data);
                    Console.WriteLine($"✅ Login successful. Token saved: {content.Data.AccessToken.Substring(0, 20)}...");
                }
                else
                {
                    Console.WriteLine($"❌ Login failed: {content?.Message}");
                }

                return content;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Login error: {ex.Message}");
                return null;
            }
        }

        public async Task<ApiResponse<AuthResponse>?> RegisterAsync(RegisterDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/auth/register", dto);
            var content = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();

            if (content?.Success == true && content.Data != null)
            {
                await SaveTokens(content.Data);
            }

            return content;
        }

        public async Task<ApiResponse<AuthResponse>?> RefreshTokenAsync()
        {
            var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");
            if (string.IsNullOrEmpty(refreshToken)) return null;

            var response = await _http.PostAsJsonAsync("api/auth/refresh", new { refreshToken });
            var content = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();

            if (content?.Success == true && content.Data != null)
            {
                await SaveTokens(content.Data);
            }

            return content;
        }

        public async Task LogoutAsync()
        {
            var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");

            if (!string.IsNullOrEmpty(refreshToken))
            {
                try
                {
                    await _http.PostAsJsonAsync("api/auth/logout", new { refreshToken });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Logout API call failed: {ex.Message}");
                }
            }

            await _localStorage.RemoveItemAsync("accessToken");
            await _localStorage.RemoveItemAsync("refreshToken");
            await _localStorage.RemoveItemAsync("userRole");
            await _localStorage.RemoveItemAsync("username");

            OnAuthStateChanged?.Invoke();
        }

        private async Task SaveTokens(AuthResponse auth)
        {
            await _localStorage.SetItemAsync("accessToken", auth.AccessToken);
            await _localStorage.SetItemAsync("refreshToken", auth.RefreshToken);
            await _localStorage.SetItemAsync("userRole", auth.Role);
            await _localStorage.SetItemAsync("username", auth.Username);

            OnAuthStateChanged?.Invoke();
        }

        public Task<string?> GetTokenAsync() =>
            _localStorage.GetItemAsync<string>("accessToken").AsTask();

        public Task<string?> GetRoleAsync() =>
            _localStorage.GetItemAsync<string>("userRole").AsTask();
    }
}