using Microsoft.JSInterop;
using System.Net.Http.Headers;

namespace Mud_Q_Sass.Services
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly AuthService _authService;
        private readonly IJSRuntime _jsRuntime;

        public AuthHeaderHandler(AuthService authService, IJSRuntime jsRuntime)
        {
            _authService = authService;
            _jsRuntime = jsRuntime;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
     HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // تأكد من أننا في الـ Browser
            if (_jsRuntime is IJSInProcessRuntime)
            {
                var token = await _authService.GetTokenAsync();
                if (!string.IsNullOrEmpty(token))
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            // أثناء prerendering → لا نفعل شيء
            return await base.SendAsync(request, cancellationToken);
        }

    }
}
