using MassTransit;
using ShipmentTrackingAPI.Consumers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddMassTransit(opt =>
{
  opt.AddConsumer<ShipmentCreatedConsumer>();

  opt.UsingRabbitMq((context, config) =>
  {
    config.Host(builder.Configuration.GetConnectionString("RabbitConn"));
    config.UseMessageRetry(c => c.Interval(3, TimeSpan.FromSeconds(1)));

    config.ReceiveEndpoint("shipment_queue",x =>
    {
      x.ConfigureConsumer<ShipmentCreatedConsumer>(context);
      x.BindDeadLetterQueue("dlq_shipment_exchange", "dlq_shipment_queue");
    });

    config.ConfigureEndpoints(context);
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
