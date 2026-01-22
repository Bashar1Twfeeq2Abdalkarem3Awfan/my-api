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
                ThirdWithLastname = "User", // ✨ إصلاح مشكلة Flutter Crash
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
        /// الحصول على قائمة جميع الصلاحيات (snake_case لتوافق مع API)
        /// Create list of all permissions (snake_case)
        /// </summary>
        private List<Permission> CreatePermissions()
        {
            return new List<Permission>
            {
                // Dashboard
                new Permission { PermissionName = "view_dashboard", Description = "عرض لوحة التحكم" },
                
                // Products
                new Permission { PermissionName = "view_products", Description = "عرض المنتجات" },
                new Permission { PermissionName = "create_product", Description = "إضافة منتج" },
                new Permission { PermissionName = "edit_product", Description = "تعديل منتج" },
                new Permission { PermissionName = "delete_product", Description = "حذف منتج" },
                
                // Categories
                new Permission { PermissionName = "view_categories", Description = "عرض الفئات" },
                new Permission { PermissionName = "manage_categories", Description = "إدارة الفئات" },
                
                // Units
                new Permission { PermissionName = "view_units", Description = "عرض الوحدات" },
                new Permission { PermissionName = "manage_units", Description = "إدارة الوحدات" },
                
                // Invoices
                new Permission { PermissionName = "view_invoices", Description = "عرض الفواتير" },
                new Permission { PermissionName = "create_invoice", Description = "إنشاء فاتورة" },
                new Permission { PermissionName = "edit_invoice", Description = "تعديل فاتورة" },
                new Permission { PermissionName = "delete_invoice", Description = "حذف فاتورة" },
                
                // Sales
                new Permission { PermissionName = "view_sales", Description = "عرض المبيعات" },
                new Permission { PermissionName = "manage_sales", Description = "إدارة المبيعات" },
                
                // Purchases
                new Permission { PermissionName = "view_purchases", Description = "عرض المشتريات" },
                new Permission { PermissionName = "manage_purchases", Description = "إدارة المشتريات" },
                
                // Returns
                new Permission { PermissionName = "view_returns", Description = "عرض المرتجعات" },
                new Permission { PermissionName = "manage_returns", Description = "إدارة المرتجعات" },
                
                // Customers & Suppliers
                new Permission { PermissionName = "view_persons", Description = "عرض العملاء والموردين" },
                new Permission { PermissionName = "manage_persons", Description = "إدارة العملاء والموردين" },
                
                // Debts
                new Permission { PermissionName = "view_debts", Description = "عرض الديون" },
                new Permission { PermissionName = "manage_debts", Description = "إدارة الديون" },
                
                // Users
                new Permission { PermissionName = "view_users", Description = "عرض المستخدمين" },
                new Permission { PermissionName = "manage_users", Description = "إدارة المستخدمين" }, // For general management
                new Permission { PermissionName = "create_user", Description = "إنشاء مستخدم" },
                new Permission { PermissionName = "edit_user", Description = "تعديل مستخدم" },
                new Permission { PermissionName = "delete_user", Description = "حذف مستخدم" },
                
                // Roles
                new Permission { PermissionName = "view_roles", Description = "عرض الأدوار" },
                new Permission { PermissionName = "manage_roles", Description = "إدارة الأدوار" },
                
                // Reports
                new Permission { PermissionName = "view_reports", Description = "عرض التقارير" },
                new Permission { PermissionName = "generate_reports", Description = "إنشاء التقارير" },
                
                // Settings
                new Permission { PermissionName = "view_settings", Description = "عرض الإعدادات" },
                new Permission { PermissionName = "manage_settings", Description = "إدارة الإعدادات" },
                
                // Backup
                new Permission { PermissionName = "manage_backup", Description = "إدارة النسخ الاحتياطي" }
            };
        }
    }
}
