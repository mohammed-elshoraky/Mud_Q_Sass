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

        // استخدم "Auth" client (مش "API")
        public AuthService(IHttpClientFactory httpClientFactory, ILocalStorageService localStorage)
        {
            _http = httpClientFactory.CreateClient("Auth"); // ⬅️ تغيير مهم
            _localStorage = localStorage;
        }

        // ---------------------------
        //         LOGIN
        // ---------------------------
        public async Task<ApiResponse<AuthResponse>?> LoginAsync(LoginDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", dto);
            var content = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();

            if (content?.Success == true && content.Data != null)
            {
                await SaveTokens(content.Data);
            }

            return content;
        }

        // ---------------------------
        //        REGISTER
        // ---------------------------
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

        // ---------------------------
        //      REFRESH TOKEN
        // ---------------------------
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

        // ---------------------------
        //          LOGOUT
        // ---------------------------
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

        // ---------------------------
        //        SAVE TOKENS
        // ---------------------------
        private async Task SaveTokens(AuthResponse auth)
        {
            await _localStorage.SetItemAsync("accessToken", auth.AccessToken);
            await _localStorage.SetItemAsync("refreshToken", auth.RefreshToken);
            await _localStorage.SetItemAsync("userRole", auth.Role);
            await _localStorage.SetItemAsync("username", auth.Username);

            OnAuthStateChanged?.Invoke();
        }

        // ---------------------------
        //     GETTERS (Async)
        // ---------------------------
        public Task<string?> GetTokenAsync() =>
            _localStorage.GetItemAsync<string>("accessToken").AsTask();

        public Task<string?> GetRoleAsync() =>
            _localStorage.GetItemAsync<string>("userRole").AsTask();
    }
}