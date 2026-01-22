using Microsoft.EntityFrameworkCore;
using MyAPIv3.Data;
using MyAPIv3.Models;
using MyAPIv3.Helpers;

namespace MyAPIv3.Services
{
    /// <summary>
    /// خدمة لإنشاء البيانات الأولية (Seeding)
    /// Creates initial data when database is empty
    /// </summary>
    public class DatabaseSeeder
    {
        private readonly AppDbContext _context;

        public DatabaseSeeder(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// تهيئة قاعدة البيانات بالبيانات الأولية
        /// Initialize database with default data
        /// </summary>
        public async Task SeedAsync()
        {
            // التحقق من وجود مستخدمين - إذا كان هناك مستخدمين، لا نفعل شيء
            if (await _context.Users.AnyAsync())
            {
                return; // قاعدة البيانات ليست فارغة
            }

            Console.WriteLine("🌱 Database is empty. Seeding initial data...");

            // 1. إنشاء جميع الصلاحيات
            var permissions = CreatePermissions();
            await _context.Permissions.AddRangeAsync(permissions);
            await _context.SaveChangesAsync();

            // 2. إنشاء دور المدير
            var adminRole = new Role
            {
                RoleName = "Admin",
                Description = "مدير النظام - صلاحيات كاملة"
            };
            await _context.Roles.AddAsync(adminRole);
            await _context.SaveChangesAsync();

            // 3. ربط جميع الصلاحيات بدور المدير
            var rolePermissions = permissions.Select(p => new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = p.Id
            }).ToList();
            await _context.RolePermissions.AddRangeAsync(rolePermissions);
            await _context.SaveChangesAsync();

            // 4. إنشاء Person للمدير
            var adminPerson = new Person
            {
                FirstName = "Admin",
                SecondName = "System",
                PhoneNumber = "0000000000",
                Email = "admin@system.com",
                Address = "System",
                PersonType = PersonTypeEnum.Staff.ToString()
            };
            await _context.Persons.AddAsync(adminPerson);
            await _context.SaveChangesAsync();

            // 5. إنشاء مستخدم المدير
            var adminUser = new User
            {
                Username = "admin",
                PersonId = adminPerson.Id,
            };
            
            // تشفير كلمة المرور: admin123
            // ✨ استخدام PasswordHelper للحصول على BCrypt Hash متوافق
            adminUser.PasswordHash = PasswordHelper.HashPassword("admin123");
            
            await _context.Users.AddAsync(adminUser);
            await _context.SaveChangesAsync();

            // 6. ربط المستخدم بدور المدير
            var userRole = new UserRole
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id
            };
            await _context.UserRoles.AddAsync(userRole);
            await _context.SaveChangesAsync();

            // 7. إنشاء إعدادات الشركة الافتراضية
            var companySettings = new CompanySettings
            {
                CompanyName = "اسم الشركة",
                Address = "العنوان",
                PhoneNumber = "0000000000",
                Email = "info@company.com",
                TaxId = "000000000000000",
                LogoPath = null
            };
            await _context.CompanySettings.AddAsync(companySettings);
            await _context.SaveChangesAsync();

            Console.WriteLine("✅ Database seeded successfully!");
            Console.WriteLine("👤 Admin user created:");
            Console.WriteLine("   Username: admin");
            Console.WriteLine("   Password: admin123");
            Console.WriteLine("⚠️  Please change the password after first login!");
        }

        /// <summary>
        /// إنشاء قائمة بجميع الصلاحيات
        /// Create list of all permissions
        /// </summary>
        private List<Permission> CreatePermissions()
        {
            return new List<Permission>
            {
                // Dashboard
                new Permission { PermissionName = "ViewDashboard", Description = "عرض لوحة التحكم" },
                
                // Products
                new Permission { PermissionName = "ViewProducts", Description = "عرض المنتجات" },
                new Permission { PermissionName = "AddProduct", Description = "إضافة منتج" },
                new Permission { PermissionName = "EditProduct", Description = "تعديل منتج" },
                new Permission { PermissionName = "DeleteProduct", Description = "حذف منتج" },
                
                // Categories
                new Permission { PermissionName = "ViewCategories", Description = "عرض الفئات" },
                new Permission { PermissionName = "ManageCategories", Description = "إدارة الفئات" },
                
                // Units
                new Permission { PermissionName = "ViewUnits", Description = "عرض الوحدات" },
                new Permission { PermissionName = "ManageUnits", Description = "إدارة الوحدات" },
                
                // Invoices
                new Permission { PermissionName = "ViewInvoices", Description = "عرض الفواتير" },
                new Permission { PermissionName = "CreateInvoice", Description = "إنشاء فاتورة" },
                new Permission { PermissionName = "EditInvoice", Description = "تعديل فاتورة" },
                new Permission { PermissionName = "DeleteInvoice", Description = "حذف فاتورة" },
                
                // Sales
                new Permission { PermissionName = "ViewSales", Description = "عرض المبيعات" },
                new Permission { PermissionName = "ManageSales", Description = "إدارة المبيعات" },
                
                // Purchases
                new Permission { PermissionName = "ViewPurchases", Description = "عرض المشتريات" },
                new Permission { PermissionName = "ManagePurchases", Description = "إدارة المشتريات" },
                
                // Returns
                new Permission { PermissionName = "ViewReturns", Description = "عرض المرتجعات" },
                new Permission { PermissionName = "ManageReturns", Description = "إدارة المرتجعات" },
                
                // Customers & Suppliers
                new Permission { PermissionName = "ViewPersons", Description = "عرض العملاء والموردين" },
                new Permission { PermissionName = "ManagePersons", Description = "إدارة العملاء والموردين" },
                
                // Debts
                new Permission { PermissionName = "ViewDebts", Description = "عرض الديون" },
                new Permission { PermissionName = "ManageDebts", Description = "إدارة الديون" },
                
                // Users
                new Permission { PermissionName = "ViewUsers", Description = "عرض المستخدمين" },
                new Permission { PermissionName = "ManageUsers", Description = "إدارة المستخدمين" },
                
                // Roles
                new Permission { PermissionName = "ViewRoles", Description = "عرض الأدوار" },
                new Permission { PermissionName = "ManageRoles", Description = "إدارة الأدوار" },
                
                // Reports
                new Permission { PermissionName = "ViewReports", Description = "عرض التقارير" },
                new Permission { PermissionName = "GenerateReports", Description = "إنشاء التقارير" },
                
                // Settings
                new Permission { PermissionName = "ViewSettings", Description = "عرض الإعدادات" },
                new Permission { PermissionName = "ManageSettings", Description = "إدارة الإعدادات" },
                
                // Backup
                new Permission { PermissionName = "ManageBackup", Description = "إدارة النسخ الاحتياطي" }
            };
        }
    }
}
