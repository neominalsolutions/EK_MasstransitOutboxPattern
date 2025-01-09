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
      // Not: Gönderim aşamasında bir transaction bloğu içerisinde önce eventi gönderip daha sonra event sonrasında veritabanına yazılması gereken kaydı transaction scope içerisinde kaydediyoruz.
      // sıra önemli

      using (var transaction = await this.db.Database.BeginTransactionAsync())
      {
        try
        {

          var entity = new Shipment();
          entity.Address = "ISTANBUL";

          // publish kodu saveChanges öncesi tanımlanması lazım.
          // SaveChanges sonrasında yazılırsa event outbox table atılmıyor.
          await this.publishEndpoint.Publish<IShipmentCreated>(new { Id = entity.Id, Address = entity.Address },action =>
          {
            action.TimeToLive = TimeSpan.FromMinutes(3);
            // Dead letter queue 3 dakika içerisinde eğer mesaj iletimi consumer olmazsa bunu dead letter queue taşır.
            // UsemessageReDelivery gibi bir poliçe uygulandığında poliçeye uyulmadığı takdirde
            // messaj boyutu aşıldığı taktirde
            // mesaj rejected nact yaptığı takdirde. 
            // yukarıdaki tüm işlemlerde iletiyi kaybetmemek için Dead letter queue  ileti taşınıyor.
          });

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
