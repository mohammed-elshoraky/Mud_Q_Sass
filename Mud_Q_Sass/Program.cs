using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Mud_Q_Sass.Components;
using Mud_Q_Sass.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMudServices();

// ⬇️ عطّل Prerendering هنا
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBlazoredLocalStorage();

// Authorization (بدون Authentication!)
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("SuperAdmin", policy => policy.RequireRole("SuperAdmin"));
    options.AddPolicy("CompanyAdmin", policy => policy.RequireRole("CompanyAdmin"));
    options.AddPolicy("BranchAdmin", policy => policy.RequireRole("BranchAdmin"));
});

// AuthenticationStateProvider
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

// HttpClients
builder.Services.AddHttpClient("Auth", client =>
{
    client.BaseAddress = new Uri("https://localhost:7013/");
});

builder.Services.AddScoped<AuthService>();

builder.Services.AddScoped<AuthHeaderHandler>(sp =>
    new AuthHeaderHandler(
        sp.GetRequiredService<ILocalStorageService>(),
        sp.GetRequiredService<IJSRuntime>()
    )
);

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

// ⚠️ لا تستخدم UseAuthentication() أو UseAuthorization() هنا!

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
     .AllowAnonymous();


app.Run();