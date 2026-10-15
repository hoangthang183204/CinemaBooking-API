using CinemaBooking.Application.Services;
using CinemaBooking.Application.Interfaces;
using CinemaBooking.Infrastructure.Data;
using CinemaBooking.Infrastructure.Repositories;
using CinemaBooking.Infrastructure.Services;
using CinemaBooking.Application.UseCases.BookTicket;
using Microsoft.EntityFrameworkCore;
using CinemaBooking.Application.Interfaces.Repositories;
using CinemaBooking.API.Background;
using CinemaBooking.API.Middleware;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var allowedOrigins = new[]
{
    builder.Configuration["Frontend:BaseUrl"] ?? "http://localhost:5173",
    "http://localhost:5173",
    "https://localhost:5173",
    "http://localhost:5065",
    "https://localhost:7106"
}.Select(origin => origin.TrimEnd('/')).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

bool IsAllowedBrowserOrigin(string? origin)
{
    if (string.IsNullOrWhiteSpace(origin)) return true;
    var normalized = origin.TrimEnd('/');
    if (allowedOrigins.Contains(normalized, StringComparer.OrdinalIgnoreCase)) return true;
    return Uri.TryCreate(normalized, UriKind.Absolute, out var uri)
        && (uri.Host is "localhost" or "127.0.0.1");
}

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(origin => IsAllowedBrowserOrigin(origin))
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<CinemaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<MovieService>();
builder.Services.AddScoped<ShowTimeService>();
builder.Services.AddScoped<SeatService>();
builder.Services.AddScoped<RoomService>();

builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IShowTimeRepository, ShowTimeRepository>();
builder.Services.AddScoped<ISeatRepository, SeatRepository>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();

builder.Services.AddScoped<TicketService>();
builder.Services.AddScoped<BookTicketUseCase>();
builder.Services.AddScoped<ISeatHoldService, SeatHoldService>();
builder.Services.AddScoped<ICreateBookingService, CreateBookingService>();
builder.Services.AddHostedService<SeatHoldExpiryService>();
builder.Services.AddScoped<IBookingQueryService, BookingQueryService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IHomeContentService, HomeContentService>();
builder.Services.AddScoped<ISiteContentService, SiteContentService>();


// DI cho AuthService
builder.Services.AddScoped<IAuthService, AuthService>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Missing Jwt:Key");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();

// Swagger với nút Authorize (để test JWT)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CinemaBooking API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập 'Bearer {token}' vào ô bên dưới",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors();
app.Use(async (context, next) =>
{
    var method = context.Request.Method;
    if (HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsPatch(method) || HttpMethods.IsDelete(method))
    {
        var origin = context.Request.Headers.Origin.ToString();
        if (!IsAllowedBrowserOrigin(origin))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }
    }

    await next();
});
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Dev dùng HTTP 5065 qua proxy Vite. Redirect sang HTTPS làm trình duyệt bỏ header Authorization và API trả 401.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
