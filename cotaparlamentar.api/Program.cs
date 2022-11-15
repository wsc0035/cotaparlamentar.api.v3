using cotaparlamentar.api.MysqlDataContext;
using cotaparlamentar.api.Service;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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