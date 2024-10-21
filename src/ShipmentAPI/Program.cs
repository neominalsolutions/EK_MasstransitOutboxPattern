using MassTransit;
using Microsoft.EntityFrameworkCore;
using ShipmentAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<SampleDbContext>(opt =>
{
  opt.UseSqlServer(builder.Configuration.GetConnectionString("SqlConn"));
});

// Masstransit OutBox Settings

builder.Services.AddMassTransit(opt =>
{
  opt.UsingRabbitMq((context, config) =>
  {
    config.Host(builder.Configuration.GetConnectionString("RabbitConn"));
    config.UseMessageRetry(c => c.Interval(3, TimeSpan.FromSeconds(1)));
    config.ConfigureEndpoints(context);
  });

  opt.AddEntityFrameworkOutbox<SampleDbContext>(o =>
  {
    o.DisableInboxCleanupService();
    o.UseSqlServer().UseBusOutbox();
  });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
