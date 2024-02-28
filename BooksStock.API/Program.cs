using BooksStock.API.Models;
using BooksStock.API.Services;
using Serilog;
using Asp.Versioning;
using BooksStock.API.Services.ApiKey;
using Microsoft.OpenApi.Models;

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
        policy.WithOrigins("");
        policy.WithHeaders([ApiConstants.ApiVersionHeader + " : 1"]);
        policy.SetIsOriginAllowed(origin => false);
        policy.AllowAnyMethod();
        policy.DisallowCredentials();
        policy.SetPreflightMaxAge(TimeSpan.FromSeconds(30));
    });

    options.AddPolicy("MyPolicyForUsers", policy =>
    {
        policy.WithOrigins("");
        policy.WithHeaders([ApiConstants.ApiVersionHeader + " : 2"]);
        policy.SetIsOriginAllowed(isOriginAllowed => false);
        policy.WithMethods("HttpGet");
        policy.DisallowCredentials();
        policy.SetPreflightMaxAge(TimeSpan.FromSeconds(30));
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
});

//Add support to logging with SERILOG
builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseMiddleware<ApiMiddleware>();

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
