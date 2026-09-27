using Microsoft.EntityFrameworkCore;
using Npgsql;
using TaskTrack.Repo;

var builder = WebApplication.CreateBuilder(args);

// Connection string: appsettings.json by default, overridden by DATABASE_URL on Render.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrWhiteSpace(databaseUrl))
    connectionString = ToNpgsqlConnectionString(databaseUrl);

builder.Services.AddDbContext<TaskManagementDbContext>(options => options.UseNpgsql(connectionString));

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL");
if (!string.IsNullOrWhiteSpace(frontendUrl))
    allowedOrigins = [.. allowedOrigins, .. frontendUrl.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];

builder.Services.AddCors(options =>
    options.AddPolicy("Frontend", policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger is also kept on in Production so the Render URL can be verified.
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("Frontend");
app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();

// Render gives postgres://user:pass@host:port/db; Npgsql needs key=value form.
static string ToNpgsqlConnectionString(string url)
{
    if (!url.StartsWith("postgres://") && !url.StartsWith("postgresql://"))
        return url;

    var uri = new Uri(url);
    var userInfo = uri.UserInfo.Split(':', 2);
    return new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Database = uri.AbsolutePath.TrimStart('/'),
        Username = Uri.UnescapeDataString(userInfo[0]),
        Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : null,
        SslMode = SslMode.Require,
    }.ToString();
}
