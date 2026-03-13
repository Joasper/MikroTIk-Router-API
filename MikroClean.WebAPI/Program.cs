using Microsoft.EntityFrameworkCore;
using MikroClean.InversionOfControl;


var builder = WebApplication.CreateBuilder(args);

// Add dependency injection for services and repositories
builder.Services.AddDependencies(builder.Configuration);

// Add DbContext 
builder.Services.AddMikroCleanContext(builder.Configuration);

// Add JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:4201")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Automatically apply pending migrations to the database on application startup
builder.Services.AutomaticMigrate();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
