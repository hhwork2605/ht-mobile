using System.Text;
using HtMobile.Api.Auth;
using HtMobile.Application;
using HtMobile.Infrastructure;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Clean Architecture — nối các lớp (Identity + DB + cache cấu hình trong AddInfrastructure).
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// JWT bearer cho Ht.Admin (Angular SPA).
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<HtMobile.Api.Users.AdminUserService>();
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

var adminOrigins = builder.Configuration.GetSection("Cors:AdminOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(o => o.AddPolicy("HtAdmin", p =>
    p.WithOrigins(adminOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Phục vụ ảnh upload (ProductImage…) từ {ContentRoot}/wwwroot — khớp đường dẫn LocalFileStorage.
// Dùng PhysicalFileProvider tường minh (wwwroot có thể chưa tồn tại lúc host khởi tạo → WebRootFileProvider null).
var webRoot = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
Directory.CreateDirectory(Path.Combine(webRoot, "uploads", "products"));

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(webRoot) });
app.UseCors("HtAdmin");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
