using Microsoft.EntityFrameworkCore;
using MyAPIv3.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;


namespace MyAPIv3.Data
{
    // ============================
    // DbContext
    // ============================
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Person> Persons => Set<Person>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Unit> Units => Set<Unit>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductUnit> ProductUnits => Set<ProductUnit>();
        public DbSet<Inventory> Inventories => Set<Inventory>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<InvoiceProduct> InvoiceProducts => Set<InvoiceProduct>();
        public DbSet<ReturnTbl> Returns => Set<ReturnTbl>();
        public DbSet<ReturnProduct> ReturnProducts => Set<ReturnProduct>();
        public DbSet<Expense> Expenses => Set<Expense>();
        public DbSet<Debt> Debts => Set<Debt>();
        public DbSet<CompanySettings> CompanySettings => Set<CompanySettings>();
        public DbSet<PrintSettings> PrintSettings => Set<PrintSettings>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => v.ToUniversalTime(),      // عند الكتابة إلى DB
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)); // عند القراءة من DB

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?));

                foreach (var property in properties)
                {
                    modelBuilder.Entity(entityType.Name)
                        .Property(property.Name)
                        .HasConversion(dateTimeConverter);
                }
            }

            // Convert enums to string
            modelBuilder.Entity<Person>().Property(p => p.PersonType)
                .HasColumnName("person_type_enum")
                .HasConversion<string>();        // حتى يتم حفظ Enum كـ string

            modelBuilder.Entity<Invoice>().Property(i => i.InvoiceType)
                .HasColumnName("invoice_type")
                .HasConversion<string>();        // حتى يتم حفظ Enum كـ string

            modelBuilder.Entity<ReturnTbl>().Property(r => r.ReturnType)
                .HasColumnName("return_type")
                .HasConversion<string>();        // حتى يتم حفظ Enum كـ string

            // Composite PKs for junction tables
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            // Map table and relationships for junctions explicitly
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles!)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles!)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions!)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions!)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Uniques and indexes
            modelBuilder.Entity<Person>()
                .HasIndex(p => new { p.FirstName, p.ThirdWithLastname })
                .HasDatabaseName("idx_person_name");

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.ProductName)
                .HasDatabaseName("idx_product_name");

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.QrCode)
                .HasDatabaseName("idx_product_qr");

            // product_unit unique constraint
            modelBuilder.Entity<ProductUnit>()
                .HasIndex(pu => new { pu.ProductId, pu.UnitId })
                .IsUnique();

            // invoice relationships
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Client)
                .WithMany(p => p.ClientInvoices)
                .HasForeignKey(i => i.ClientId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Supplier)
                .WithMany(p => p.SupplierInvoices)
                .HasForeignKey(i => i.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.CreatedByPerson)
                .WithMany(p => p.CreatedInvoices)
                .HasForeignKey(i => i.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // invoice_product FKs
            modelBuilder.Entity<InvoiceProduct>()
                .HasOne(ip => ip.Invoice)
                .WithMany(i => i.InvoiceProducts)
                .HasForeignKey(ip => ip.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InvoiceProduct>()
                .HasOne(ip => ip.Product)
                .WithMany(p => p.InvoiceProducts)
                .HasForeignKey(ip => ip.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InvoiceProduct>()
                .HasOne(ip => ip.Unit)
                .WithMany(u => u.InvoiceProducts)
                .HasForeignKey(ip => ip.UnitId)
                .OnDelete(DeleteBehavior.Restrict);

            // return relations
            modelBuilder.Entity<ReturnTbl>()
                .HasOne(r => r.OriginalInvoice)
                .WithMany() // no direct Invoice -> Return inverse in SQL
                .HasForeignKey(r => r.OriginalInvoiceId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ReturnProduct>()
                .HasOne(rp => rp.Return)
                .WithMany(r => r.ReturnProducts)
                .HasForeignKey(rp => rp.ReturnId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReturnProduct>()
                .HasOne(rp => rp.Product)
                .WithMany(p => p.ReturnProducts)
                .HasForeignKey(rp => rp.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // inventory
            modelBuilder.Entity<Inventory>()
                .HasOne(inv => inv.Product)
                .WithMany(p => p.Inventories)
                .HasForeignKey(inv => inv.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // expense - created_by
            modelBuilder.Entity<Expense>()
                .HasOne(e => e.CreatedByPerson)
                .WithMany(p => p.ExpensesCreated)
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // debt foreigns
            modelBuilder.Entity<Debt>()
                .HasOne(d => d.ClientPerson)
                .WithMany(p => p.DebtsAsClient)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Debt>()
                .HasOne(d => d.SupplierPerson)
                .WithMany(p => p.DebtsAsSupplier)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Debt>()
                .HasOne(d => d.CreatedByPerson)
                .WithMany(p => p.DebtsCreated)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // Ensure string length and precision constraints are honored by EF migrations where possible.
        }
    }
}
