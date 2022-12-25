using cotaparlamentar.api.MysqlDataContext;
using cotaparlamentar.api.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"Hash de autorização",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
      {
        {
          new OpenApiSecurityScheme
          {
            Reference = new OpenApiReference
              {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
              },
              Scheme = "oauth2",
              Name = "Bearer",
              In = ParameterLocation.Header,

            },
            new List<string>()
          }
        });
});

builder.Services.AddSingleton(new TokenService(builder.Configuration.GetSection("KeyHash").Value));
builder.Services.AddScoped<DeputadoService>();
builder.Services.AddScoped<CotaParlamentarService>();
builder.Services.AddScoped<AssessorParlamentarService>();
builder.Services.AddDbContext<MysqlContext>(
        options => options.UseMySql(builder.Configuration.GetConnectionString("mySql"),
            new MySqlServerVersion(new Version(8, 0, 11))));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();