using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Model;
using Model.Data;
using System.Text;
using dotenv.net;
using Service.UserService;
using Service.TokenService;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
var rootPath = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;
DotEnv.Load(new DotEnvOptions(envFilePaths: new[] { Path.Combine(rootPath, ".env") }));

ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

await InitializeDatabase(app);

ConfigureMiddleware(app);

await app.RunAsync();

void ConfigureServices(IServiceCollection services, IConfiguration config)
{
    // Database configuration
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

    // Identity configuration with password requirements
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

    // JWT configuration
    var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? 
                 throw new InvalidOperationException("JWT Key not found in environment variables.");
    var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? 
                    throw new InvalidOperationException("JWT Issuer not found in environment variables.");
    var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? 
                      throw new InvalidOperationException("JWT Audience not found in environment variables.");
    var jwtExpiration = Environment.GetEnvironmentVariable("JWT_EXPIRATION") ?? 
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

    // Register Services
    services.AddScoped<ITokenService>(provider => 
        new TokenService(
            provider.GetRequiredService<UserManager<User>>(),
            jwtKey,
            jwtIssuer,
            jwtAudience,
            jwtExpiration
        ));
    
    services.AddScoped<IUserService, UserService>();

    // Add other services
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
}

async Task InitializeDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    await context.Database.MigrateAsync();

    // Vous pouvez ajouter ici l'initialisation des rôles et des utilisateurs par défaut si nécessaire
}

void ConfigureMiddleware(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    
    app.UseAuthentication();
    app.UseAuthorization();
    
    app.MapControllers();
}