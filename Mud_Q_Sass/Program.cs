using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Mud_Q_Sass.Components;
using Mud_Q_Sass.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMudServices();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddBlazoredLocalStorage();

// ⬇️ استخدم AddAuthorizationCore بس (بدون AddAuthentication)
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("SuperAdmin", policy => policy.RequireRole("SuperAdmin"));
    options.AddPolicy("CompanyAdmin", policy => policy.RequireRole("CompanyAdmin"));
    options.AddPolicy("BranchAdmin", policy => policy.RequireRole("BranchAdmin"));
});

// ⬇️ سجل الـ AuthenticationStateProvider
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

// HttpClients
builder.Services.AddHttpClient("Auth", client =>
{
    client.BaseAddress = new Uri("https://localhost:7013/");
});

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AuthHeaderHandler>();

builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri("https://localhost:7013/");
})
.AddHttpMessageHandler<AuthHeaderHandler>();

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

// ⚠️ لا تستخدم UseAuthentication أو UseAuthorization هنا
// لأن الـ Authentication بيحصل في الـ API مش في Blazor

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();