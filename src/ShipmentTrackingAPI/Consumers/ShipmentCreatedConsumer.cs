using MassTransit;
using Messaging;

namespace ShipmentTrackingAPI.Consumers
{
  public class ShipmentCreatedConsumer : IConsumer<IShipmentCreated>
  {
    public async Task Consume(ConsumeContext<IShipmentCreated> context)
    {
      await Console.Out.WriteAsync("Shipment Tracking");
    }
  }
}
