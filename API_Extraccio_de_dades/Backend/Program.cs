
﻿using Microsoft.Extensions.Configuration;
using Backend.Services;
using Backend.Application;
using Backend.ServiceOCR;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5000");

builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
DatabaseConnection dbConn = new DatabaseConnection(connectionString);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddHttpClient();
builder.Services.AddScoped<IOcrService, OcrService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

WebApplication webApp = builder.Build();

webApp.UseSwagger();
webApp.UseSwaggerUI();
webApp.UseCors("AllowAll");


webApp.MapProveidorEndpoints(dbConn);
webApp.MapOrdresEndpoints(dbConn);
webApp.MapOcrEndpoints();



webApp.Run();
