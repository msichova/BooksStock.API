using BooksStock.API.Models;
using BooksStock.API.Services;
using Serilog;
using Asp.Versioning;
using BooksStock.API.Services.ApiKey;
using Microsoft.OpenApi.Models;
using BooksStock.API.Services.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<StockDatabseSettings>(builder.Configuration.GetSection("ConnectionToMongoDB"));
builder.Services.AddSingleton<StockServices>();

//Adding services for Api Middleware
builder.Services.AddTransient<IApiKeyValidator, ApiKeyValidator>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddPolicy("MyPolicyForAdmin", policy =>
    {
        policy.WithOrigins("https://localhost:7053", "http://localhost:5259", "http://localhost:5000", "https://localhost:5000")
        .WithHeaders([ApiConstants.ApiVersionHeader + " : 1"])
        .SetIsOriginAllowed(origin => true)
        .AllowAnyMethod()
        .DisallowCredentials()
        .SetPreflightMaxAge(TimeSpan.FromSeconds(30));
    });

    options.AddPolicy("MyPolicyForUsers", policy =>
    {
        policy.WithOrigins("https://localhost:7053", "http://localhost:5259", "http://localhost:5000", "https://localhost:5000")
        .WithHeaders([ApiConstants.ApiVersionHeader + " : 2"])
        .SetIsOriginAllowed(isOriginAllowed => true)
        .WithMethods("HttpGet")
        .DisallowCredentials()
        .SetPreflightMaxAge(TimeSpan.FromSeconds(30));
    });
});

builder.Services
    .AddApiVersioning(options =>
    {
        options.ReportApiVersions = true;
        options.AssumeDefaultVersionWhenUnspecified = false;
        options.ApiVersionReader = new HeaderApiVersionReader(ApiConstants.ApiVersionHeader);
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddControllers();

//For Entity Framework
builder.Services.AddDbContext<AuthenticationApiDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AzureSQLConnection"), builder =>
        builder.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null)));

//For Identity
builder.Services.AddIdentity<ApiUser, IdentityRole>()
    .AddEntityFrameworkStores<AuthenticationApiDbContext>()
    .AddDefaultTokenProviders();

//Adding Authentication
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    //Adding JWT Bearer
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
            ValidAudience = builder.Configuration["JWT:ValidAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]!))
        };

    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    //adding api key scheme
    options.AddSecurityDefinition(ApiConstants.ApiKeyName, new OpenApiSecurityScheme()
    {
        In = ParameterLocation.Header,
        Description = "Please enter API Key",
        Name = ApiConstants.ApiKeyHeader,
        Type = SecuritySchemeType.ApiKey
    });

    //adding api key into global security requirement
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = ApiConstants.ApiKeyName
                }
            },
            Array.Empty<string>()
        }
    });

    //adding JWT tokens scheme
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    //adding Bearer into global security requirement
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme()
            {
                Reference = new OpenApiReference()
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

//Add support to logging with SERILOG
builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHsts();

app.UseMiddleware<ApiMiddleware>();

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();
app.UseAuthorization();

app.MapControllers();

app.Run();
