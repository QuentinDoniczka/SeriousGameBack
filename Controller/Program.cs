using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Model;
using Model.Data;
using System.Text;
using dotenv.net;
using Microsoft.OpenApi.Models;
using Service.UserService;
using Service.TokenService;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
var rootPath = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;
DotEnv.Load(new DotEnvOptions(envFilePaths: new[] { Path.Combine(rootPath, ".env") }));

// Configuration for Docker environment
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Docker")
{
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.ListenAnyIP(80);
    });
}

ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

await InitializeDatabase(app);

ConfigureMiddleware(app);

await app.RunAsync();

void ConfigureServices(IServiceCollection services, IConfiguration config)
{
    // Existing database and identity configuration...
    services.AddDbContext<AppDbContext>(options =>
    {
        var host = Environment.GetEnvironmentVariable("DB_HOST");
        var port = Environment.GetEnvironmentVariable("DB_PORT");
        var database = Environment.GetEnvironmentVariable("MYSQL_DATABASE");
        var password = Environment.GetEnvironmentVariable("MYSQL_ROOT_PASSWORD");
        var uid = Environment.GetEnvironmentVariable("MYSQL_USER");

        if (host == null || port == null || database == null || password == null || uid == null)
        {
            throw new InvalidOperationException("Database environment variables are missing.");
        }

        var connectionString = $"server={host};port={port};database={database};uid={uid};password={password}";
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    });

    // Identity configuration...
    services.AddIdentity<User, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

    // CORS Configuration from working example
    services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigin",
            build => build.WithOrigins("http://localhost:5173", "https://front-cantine.vercel.app")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithExposedHeaders("Content-Disposition")
                .AllowCredentials());
    });

    // Existing JWT configuration...
    var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? 
                 throw new InvalidOperationException("JWT Key not found in environment variables.");
    var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? 
                    throw new InvalidOperationException("JWT Issuer not found in environment variables.");
    var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? 
                      throw new InvalidOperationException("JWT Audience not found in environment variables.");
    var jwtExpiration = Environment.GetEnvironmentVariable("JWT_EXPIRATION_IN_MINUTES") ?? 
                        throw new InvalidOperationException("JWT Expiration not found in environment variables.");
    
    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });
    
    services.AddScoped<ITokenService>(provider => 
        new TokenService(
            provider.GetRequiredService<UserManager<User>>(),
            jwtKey,
            jwtIssuer,
            jwtAudience,
            jwtExpiration
        ));
    
    services.AddScoped<IUserService, UserService>();
    services.AddControllers();
    services.AddRouting(options => options.LowercaseUrls = true);
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo 
        { 
            Title = "Mobile Serious Game API", 
            Version = "v1" 
        });
    
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        c.EnableAnnotations();
    
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
    
        var modelXmlFile = "Model.xml";
        var modelXmlPath = Path.Combine(AppContext.BaseDirectory, modelXmlFile);
        if (File.Exists(modelXmlPath))
        {
            c.IncludeXmlComments(modelXmlPath);
        }
    });
}

async Task InitializeDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
}

void ConfigureMiddleware(WebApplication app)
{
    // Swagger configuration
    app.UseSwagger();
    app.UseSwaggerUI();

    // Configure the HTTP request pipeline
    app.UseRouting();

    // Use CORS before authentication
    app.UseCors("AllowSpecificOrigin");

    // Only use HTTPS redirection if not in Docker environment
    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Docker")
    {
        app.UseHttpsRedirection();
    }
    
    app.UseAuthentication();
    app.UseAuthorization();
    
    app.MapControllers();
}