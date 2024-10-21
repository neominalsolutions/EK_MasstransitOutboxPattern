using MassTransit;
using Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShipmentAPI.Data;

namespace ShipmentAPI.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class TestsController : ControllerBase
  {
    private readonly IPublishEndpoint publishEndpoint;
    private readonly SampleDbContext db;

    public TestsController(IPublishEndpoint publishEndpoint, SampleDbContext db)
    {
      this.publishEndpoint = publishEndpoint;
      this.db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Publish()
    {

      using (var transaction = await this.db.Database.BeginTransactionAsync())
      {
        try
        {

          var entity = new Shipment();
          entity.Address = "ISTANBUL";

          // publish kodu saveChanges öncesi tanımlanması lazım.
          // SaveChanges sonrasında yazılırsa event outbox table atılmıyor.
          await this.publishEndpoint.Publish<IShipmentCreated>(new { Id = entity.Id, Address = entity.Address });

          db.Shipments.Add(entity);
          db.SaveChanges();



    

          await transaction.CommitAsync();
        }
        catch (Exception)
        {
          await transaction.RollbackAsync();
          throw;
        }
      }

      return Ok();

    }

  }
}
