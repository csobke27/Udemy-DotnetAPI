using System.Text;
using DotnetAPI.Data;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors((options) =>
{
    options.AddPolicy("DevCors", (corsBuilder) =>
    {
        corsBuilder.WithOrigins("http://localhost:3000", "http://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });

    options.AddPolicy("ProdCors", (corsBuilder) =>
    {
        corsBuilder.WithOrigins("https://myproductiondomain.com")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddScoped<IUserRepository, UserRepository>();

string? tokenKey = builder.Configuration.GetSection("AppSettings:TokenKey").Value;
if(string.IsNullOrEmpty(tokenKey))
{
    throw new Exception("Token key is not configured");
}
SymmetricSecurityKey tokenKeyObj = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

TokenValidationParameters tokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuerSigningKey = false,
    IssuerSigningKey = tokenKeyObj,
    ValidateIssuer = false,
    ValidateAudience = false//,
    // ClockSkew = TimeSpan.Zero
};

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
}).AddJwtBearer("Bearer", options =>
{
    options.TokenValidationParameters = tokenValidationParameters;
});

// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options =>
//     {
//         options.TokenValidationParameters = tokenValidationParameters;
//     });

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevCors");
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseCors("ProdCors");
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// app.MapGet("/weatherforecast", () =>
// {

// })
// .WithName("GetWeatherForecast");

app.Run();

