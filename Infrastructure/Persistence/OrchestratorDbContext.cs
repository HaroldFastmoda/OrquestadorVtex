using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class OrchestratorDbContext : DbContext
    {
        public OrchestratorDbContext(DbContextOptions<OrchestratorDbContext> options)
            : base(options)
        {
        }

        // Mapeo de Tablas (DbSet)
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<Audit> Audits { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<NewDetail> NewDetails { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RoleAsPermission> RoleAsPermissions { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Packing> Packings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones adicionales si son necesarias
            // Por ejemplo, asegurar índices únicos o valores por defecto

            modelBuilder.Entity<Inventory>()
                .HasIndex(i => i.Sku)
                .IsUnique(false); // O true si el SKU debe ser único
        }
    }
}
