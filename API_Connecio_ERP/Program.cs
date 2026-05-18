using API_Connecio_ERP.Application.Endpoints;
using API_Connecio_ERP.Services;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5100");

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new Exception("Falta ConnectionStrings:DefaultConnection en appsettings.json");

var dbConn = new DatabaseConnection(connectionString);
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");

app.MapPresupuestoEndpoints(dbConn);

app.Run();
