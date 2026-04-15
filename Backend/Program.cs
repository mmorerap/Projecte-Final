
﻿using Microsoft.Extensions.Configuration;
using Backend.Services;
using Backend.Application.Endpoints;

// using dbdemo.Endpoints;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configuració
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
DatabaseConnection dbConn = new DatabaseConnection(connectionString);

WebApplication webApp = builder.Build();

//webApp.MapProductEndpoints(dbConn);

webApp.MapProveidorEndpoints(dbConn);
webApp.MapOrdresEndpoints(dbConn);



webApp.Run();




