using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Mud_Q_Sass.Components;
using Mud_Q_Sass.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMudServices();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddBlazoredLocalStorage();

// Authorization
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("SuperAdmin", policy => policy.RequireRole("SuperAdmin"));
    options.AddPolicy("CompanyAdmin", policy => policy.RequireRole("CompanyAdmin"));
    options.AddPolicy("BranchAdmin", policy => policy.RequireRole("BranchAdmin"));
});

// AuthenticationStateProvider
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

// HttpClient للـ Auth (بدون Handler)
builder.Services.AddHttpClient("Auth", client =>
{
    client.BaseAddress = new Uri("https://localhost:7013/");
});

// AuthService
builder.Services.AddScoped<AuthService>();

// AuthHeaderHandler
builder.Services.AddScoped<AuthHeaderHandler>(sp =>
    new AuthHeaderHandler(
        sp.GetRequiredService<ILocalStorageService>(),
        sp.GetRequiredService<IJSRuntime>()
    )
);

// HttpClient للـ API (مع Handler)
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri("https://localhost:7013/");
})
.AddHttpMessageHandler<AuthHeaderHandler>();

// Services
builder.Services.AddScoped<CompanyService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapStaticAssets();

// ⬇️ هنا الـ RenderMode بيتحدد
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();