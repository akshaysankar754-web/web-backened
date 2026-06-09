
using InternshipPortalApi.Data;
using InternshipPortalApi.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using System.Text;

var builder = WebApplication.CreateBuilder(args);


// ================= CONTROLLERS =================

builder.Services.AddControllers();


// ================= DATABASE =================

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnection"
    );

    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    );
});


// ================= JWT SERVICE =================

builder.Services.AddScoped<JwtService>();

builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddHttpClient();

// ================= CORS =================

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAngular",
        policy =>
        {
            policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});


// ================= AUTHENTICATION =================

builder.Services

.AddAuthentication(
JwtBearerDefaults.AuthenticationScheme
)

.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
    new TokenValidationParameters
    {
        ValidateIssuer = true,

        ValidateAudience = true,

        ValidateLifetime = true,

        ValidateIssuerSigningKey = true,

        ValidIssuer =
        builder.Configuration["Jwt:Issuer"],

        ValidAudience =
        builder.Configuration["Jwt:Audience"],

        IssuerSigningKey =
        new SymmetricSecurityKey(

            Encoding.UTF8.GetBytes(
                builder.Configuration["Jwt:Key"]
            )
        )
    };
});


// ================= AUTHORIZATION =================

builder.Services.AddAuthorization();


// ================= SWAGGER =================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "Internship API",
            Version = "v1"
        });

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",

            Type = SecuritySchemeType.ApiKey,

            Scheme = "Bearer",

            BearerFormat = "JWT",

            In = ParameterLocation.Header,

            Description =
            "Enter: Bearer your_token"
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                    new OpenApiReference
                    {
                        Type =
                        ReferenceType.SecurityScheme,

                        Id = "Bearer"
                    }
                },

                Array.Empty<string>()
            }
        });
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}


// ================= MIDDLEWARE =================

if(app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AllowAngular");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

