using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ShipmentAPI.Data
{
  public class SampleDbContext : DbContext
  {
    public DbSet<Shipment> Shipments { get; set; }


    public SampleDbContext(DbContextOptions<SampleDbContext> opt) : base(opt)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.AddOutboxMessageEntity();
      modelBuilder.AddOutboxStateEntity();

      base.OnModelCreating(modelBuilder);
    }
  }
}
