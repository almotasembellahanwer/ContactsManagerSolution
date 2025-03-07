using ServiceContracts;
using Services;
using Entities;
using RepositoryContracts;
using Repositories;
using Serilog;
using CRUDExample.Filters.ActionFilter;
using CRUDExample;
using CRUDExample.Middleware;
var builder = WebApplication.CreateBuilder(args);

//Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=PersonsDatabase;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False
builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    {
        loggerConfiguration.ReadFrom.Configuration(context.Configuration) // read configuration settings from built-in IConfiguration
        .ReadFrom.Services(services); // read out current app's services and make them avaliable on serilog
    });

builder.Services.ConfigureServices(builder.Configuration);
var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseExceptionHandlingMiddleware();
}

app.UseSerilogRequestLogging();
//app.Logger.LogDebug("debug-messege");
//app.Logger.LogInformation("information-messege");
//app.Logger.LogWarning("warning-messege");
//app.Logger.LogError("error-messege");
//app.Logger.LogCritical("critical-messege");

if (builder.Environment.IsEnvironment("Test") == false)
{
    Rotativa.AspNetCore.RotativaConfiguration.Setup("wwwroot", wkhtmltopdfRelativePath: "Rotativa");
}


app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.Run();

public partial class Program { } // Make the auto-generated program accessible programmatically