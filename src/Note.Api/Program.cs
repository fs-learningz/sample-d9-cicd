using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Data.Sqlite;
using Note.Api.Handlers;
using Note.Application;
using Note.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

builder.Services.AddApiVersioning(static x =>
{
    x.DefaultApiVersion = new ApiVersion(1, 0);
    x.AssumeDefaultVersionWhenUnspecified = true;
    x.ApiVersionReader = new UrlSegmentApiVersionReader();
    x.ReportApiVersions = true;
});

builder.AddSqliteConnection("sqlite");

builder.Services
    .AddAuthentication(static x => x.DefaultScheme = "None")
    .AddScheme<AuthenticationSchemeOptions, HmacAuthenticationHandler>(HmacAuthenticationHandler.Scheme, null);
builder.Services.AddAuthorization();

builder.Services.AddNoteApplication();
builder.Services.AddNoteInfrastructure();

var app = builder.Build();

app.MapOpenApi();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();