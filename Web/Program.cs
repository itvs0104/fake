using Services;
using Web;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services.AddRazorComponents()
    .AddInteractiveServerComponents();

services.AddServices(configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
}

app.UseStatusCodePagesWithReExecute("/");
app.UseExceptionHandler("/error");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
