using UserManagementService.BL.Services;
using UserManagementService.BL.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using UserManagementService.DL.Repositories;
using UserManagementService.DL.Repositories.Interfaces;
using UserManagementService.Middelware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "UserManagementService", Version = "v1" });

    // Add security definition for OAuth2
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Implicit = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"https://{builder.Configuration["Auth0:Domain"]}/authorize?audience=https://{builder.Configuration["Auth0:Domain"]}/api/v2/"),   

                Scopes = new Dictionary<string, string>
                {
                    { "openid", "OpenID Connect" },
                    { "profile", "User Profile" },
                    { "email", "Email" }
                }
            }
        }
    });

    // Enable OAuth2 security requirement for all endpoints
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
            },
            new[] { "openid", "profile", "email" } // Specify required scopes
        }
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add your services here
//builder.Services.AddScoped<IUserRepository, UserRepository>(); // Assuming you have your UserRepository implementation
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuth0Service, Auth0Service>();
builder.Services.AddScoped<ITokenFetcher, Auth0TokenFetcher>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.Authority = $"https://{builder.Configuration["Auth0:Domain"]}/";
    options.Audience = $"https://{builder.Configuration["Auth0:Domain"]}/api/v2/";
    // Add other JWT Bearer options as needed (e.g., token validation parameters)
});

// Add Memory Cache for token caching
builder.Services.AddMemoryCache();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>(); // Add the exception handler middleware

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    // Enable Swagger UI with authentication options
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1");

        // Configure OAuth2 options
        c.OAuthClientId(builder.Configuration["Auth0:ClientIdUI"]); 
        c.OAuthAppName("UserManagementService"); 
        c.OAuthUsePkce(); 
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();