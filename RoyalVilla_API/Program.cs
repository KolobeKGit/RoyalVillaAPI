using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla.DTO;
using RoyalVilla_API.Services;
using Scalar.AspNetCore;
using System.Text;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;

var builder = WebApplication.CreateBuilder(args);

//Authentication middleware
var key = Encoding.ASCII.GetBytes(builder.Configuration.GetSection("JwtSettings")["Secret"]);

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>
{
    option.RequireHttpsMetadata = false;
    option.SaveToken = true;
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});
// Add services to the container.
builder.Services.AddApiVersioning(option =>
{
    option.AssumeDefaultVersionWhenUnspecified = true;
    option.DefaultApiVersion = new ApiVersion(1, 0);
    option.ReportApiVersions = true;
}).AddApiExplorer(option =>
{
    option.GroupNameFormat = "'v'VVV";
    option.SubstituteApiVersionInUrl = true;
});

builder.Services.AddCors();
builder.Services.AddDbContext<ApplicationDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddControllers();


var buildProvider = builder.Services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

    foreach (var description in buildProvider.ApiVersionDescriptions)
    {
        var versionName = description.GroupName;
        var versionNumber = description.ApiVersion.ToString();
        var displayName = $"Demo API---{versionNumber}";

    builder.Services.AddOpenApi(versionName, options =>
    {
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Info = new OpenApiInfo
            {
                Title = "Demo Royal API",
                Version = versionName,
                Description = displayName,
                Contact = new OpenApiContact
                {
                    Name = "Kamohelo Kolobe",
                    Email = "kk@gmail.com"

                }
            };
            document.Components ??= new();
            document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
            {
                ["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Enter JWT Bearer token"
                }
            };
            document.Security =
            [
                new OpenApiSecurityRequirement{
                { new OpenApiSecuritySchemeReference("Bearer"), new List<string>() }
            }
            ];

            return Task.CompletedTask;
        });
    });
}



//Register 2 documents v1 v2
//builder.Services.AddOpenApi("v1");
//builder.Services.AddOpenApi("v2");

builder.Services.AddAutoMapper(o =>
{
    //Villa
    o.CreateMap<Villa, VillaCreateDTO>().ReverseMap();
    o.CreateMap<Villa, VillaUpdateDTO>().ReverseMap();
    o.CreateMap<Villa, VillaDTO>().ReverseMap();
    o.CreateMap<VillaUpdateDTO, VillaDTO>().ReverseMap();
    
    //User
    o.CreateMap<User, UserDTO>().ReverseMap();

    //Villa Amenities
    
    o.CreateMap<VillaAmenities, VillaAmenitiesCreateDTO>().ReverseMap();
    o.CreateMap<VillaAmenities, VillaAmenitiesUpdateDTO>().ReverseMap();
    o.CreateMap<VillaAmenities, VillaAmenitiesDTO>()
    .ForMember(dest => dest.VillaName, opt => opt.MapFrom(src => src.Villa != null ? src.Villa.Name : null));
    o.CreateMap<VillaAmenitiesDTO, VillaAmenities>().ReverseMap();

});

builder.Services.AddScoped<IAuthService, AuthService>();


var app = builder.Build();
await SeedDataAsync(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/openapi/{documentName}.json");
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.MapScalarApiReference(option=>
    {
        option.Title = "Demo - Royal VIlla API";

        var sortedVersion = provider.ApiVersionDescriptions.OrderBy(v => v.ApiVersion).ToList();

        foreach(var description in sortedVersion)
        {
            var versionName = description.GroupName;
            var versionNumber = description.ApiVersion.ToString();
            var displayName = $"Demo API---{versionNumber}";
            var isDefault = description.ApiVersion.Equals(new ApiVersion(2, 0));
            option.AddDocument(versionName, displayName, $"openapi/{versionName}.json", isDefault);
        }
    });
}
app.UseCors(o => o.AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("*"));
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


static async Task SeedDataAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await context.Database.MigrateAsync();
}