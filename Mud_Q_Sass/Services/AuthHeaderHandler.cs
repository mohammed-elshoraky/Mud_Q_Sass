using Blazored.LocalStorage;
using Microsoft.JSInterop;
using System.Net.Http.Headers;

namespace Mud_Q_Sass.Services
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;
        private readonly IJSRuntime _jsRuntime;

        public AuthHeaderHandler(ILocalStorageService localStorage, IJSRuntime jsRuntime)
        {
            _localStorage = localStorage;
            _jsRuntime = jsRuntime;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine($"🔧 AuthHeaderHandler - Processing request to: {request.RequestUri}");

                // ✅ تحقق إذا كنا في Browser (مش في Prerendering)
                if (_jsRuntime is IJSInProcessRuntime)
                {
                    var token = await _localStorage.GetItemAsync<string>("accessToken");

                    if (!string.IsNullOrEmpty(token))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        Console.WriteLine($"✅ Token added to request: Bearer {token.Substring(0, Math.Min(20, token.Length))}...");
                    }
                    else
                    {
                        Console.WriteLine("⚠️ No token found in localStorage!");
                    }
                }
                else
                {
                    Console.WriteLine("⚠️ Skipping token - in prerendering mode");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in AuthHeaderHandler: {ex.Message}");
            }

            var response = await base.SendAsync(request, cancellationToken);

            Console.WriteLine($"📡 Response status from API: {response.StatusCode}");

            return response;
        }
    }
}