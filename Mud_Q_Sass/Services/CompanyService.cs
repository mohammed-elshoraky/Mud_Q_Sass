// Frontend: Services/CompanyService.cs
using Mud.Shared.Common;
using Mud.Shared.DTOs;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;

namespace Mud_Q_Sass.Services
{
    public class CompanyService
    {
        private readonly HttpClient _http;

        public CompanyService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("API");
        }

        public async Task<List<CompanyDto>> GetAllAsync()
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<List<CompanyDto>>>("api/company");
            return response?.Success == true ? response.Data ?? new() : new();
        }

        public async Task<CompanyDto?> CreateAsync(string name, IBrowserFile? logo)
        {
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(name), "Name");
            formData.Add(new StringContent("true"), "IsActive");

            if (logo != null)
            {
                var fileContent = new StreamContent(logo.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024));
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(logo.ContentType);
                formData.Add(fileContent, "Logo", logo.Name);
            }

            var response = await _http.PostAsync("api/company", formData);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<CompanyDto>>();
            return result?.Success == true ? result.Data : null;
        }

        public async Task<bool> UpdateAsync(int id, string name, bool isActive, IBrowserFile? logo)
        {
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(name), "Name");
            formData.Add(new StringContent(isActive.ToString().ToLower()), "IsActive");

            if (logo != null)
            {
                var fileContent = new StreamContent(logo.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024));
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(logo.ContentType);
                formData.Add(fileContent, "Logo", logo.Name);
            }

            var response = await _http.PutAsync($"api/company/{id}", formData);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            return result?.Success == true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/company/{id}");
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            return result?.Success == true;
        }
    }
}