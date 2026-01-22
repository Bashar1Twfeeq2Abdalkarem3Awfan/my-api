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
        /// الحصول على قائمة جميع الصلاحيات (57 صلاحية - مطابقة لـ Flutter)
        /// Create list of all permissions (matching Flutter PermissionConstants)
        /// </summary>
        private List<Permission> CreatePermissions()
        {
            return new List<Permission>
            {
                // ============ إدارة المستخدمين (Users) ============
                new Permission { PermissionName = "view_users", Description = "عرض المستخدمين" },
                new Permission { PermissionName = "create_user", Description = "إنشاء مستخدم" },
                new Permission { PermissionName = "edit_user", Description = "تعديل مستخدم" },
                new Permission { PermissionName = "delete_user", Description = "حذف مستخدم" },
                new Permission { PermissionName = "change_password", Description = "تغيير كلمة المرور" },
                new Permission { PermissionName = "assign_roles", Description = "تعيين الأدوار" },

                // ============ إدارة الأدوار (Roles) ============
                new Permission { PermissionName = "view_roles", Description = "عرض الأدوار" },
                new Permission { PermissionName = "create_role", Description = "إنشاء دور" },
                new Permission { PermissionName = "edit_role", Description = "تعديل دور" },
                new Permission { PermissionName = "delete_role", Description = "حذف دور" },

                // ============ إدارة الصلاحيات (Permissions) ============
                new Permission { PermissionName = "view_permissions", Description = "عرض الصلاحيات" },
                new Permission { PermissionName = "create_permission", Description = "إنشاء صلاحية" },
                new Permission { PermissionName = "edit_permission", Description = "تعديل صلاحية" },
                new Permission { PermissionName = "delete_permission", Description = "حذف صلاحية" },
                new Permission { PermissionName = "manage_permissions", Description = "إدارة الصلاحيات" },

                // ============ المبيعات (Sales) ============
                new Permission { PermissionName = "view_sales", Description = "عرض المبيعات" },
                new Permission { PermissionName = "create_invoice", Description = "إنشاء فاتورة" },
                new Permission { PermissionName = "edit_invoice", Description = "تعديل فاتورة" },
                new Permission { PermissionName = "delete_invoice", Description = "حذف فاتورة" },
                new Permission { PermissionName = "export_invoice", Description = "تصدير فاتورة" },

                // ============ المشتريات (Purchases) ============
                new Permission { PermissionName = "view_purchases", Description = "عرض المشتريات" },
                new Permission { PermissionName = "create_purchase", Description = "إنشاء فاتورة شراء" },
                new Permission { PermissionName = "edit_purchase", Description = "تعديل فاتورة شراء" },
                new Permission { PermissionName = "delete_purchase", Description = "حذف فاتورة شراء" },

                // ============ المرتجعات (Returns) ============
                new Permission { PermissionName = "view_returns", Description = "عرض المرتجعات" },
                new Permission { PermissionName = "create_return", Description = "إنشاء مرتجع" },
                new Permission { PermissionName = "edit_return", Description = "تعديل مرتجع" },
                new Permission { PermissionName = "delete_return", Description = "حذف مرتجع" },

                // ============ المخزون (Inventory) ============
                new Permission { PermissionName = "view_inventory", Description = "عرض المخزون" },
                new Permission { PermissionName = "update_inventory", Description = "تحديث المخزون" },
                new Permission { PermissionName = "adjust_inventory", Description = "تسوية المخزون" },

                // ============ الديون (Debts) ============
                new Permission { PermissionName = "view_debts", Description = "عرض الديون" },
                new Permission { PermissionName = "create_debt", Description = "إضافة دين" },
                new Permission { PermissionName = "edit_debt", Description = "تعديل دين" },
                new Permission { PermissionName = "delete_debt", Description = "حذف دين" },

                // ============ المنتجات (Products) ============
                new Permission { PermissionName = "view_products", Description = "عرض المنتجات" },
                new Permission { PermissionName = "create_product", Description = "إنشاء منتج" },
                new Permission { PermissionName = "edit_product", Description = "تعديل منتج" },
                new Permission { PermissionName = "delete_product", Description = "حذف منتج" },
                new Permission { PermissionName = "manage_products", Description = "إدارة المنتجات" },

                // ============ العملاء (Customers) ============
                new Permission { PermissionName = "view_customers", Description = "عرض العملاء" },
                new Permission { PermissionName = "create_customer", Description = "إنشاء عميل" },
                new Permission { PermissionName = "edit_customer", Description = "تعديل عميل" },
                new Permission { PermissionName = "delete_customer", Description = "حذف عميل" },

                // ============ الموردين (Suppliers) ============
                new Permission { PermissionName = "view_suppliers", Description = "عرض الموردين" },
                new Permission { PermissionName = "create_supplier", Description = "إنشاء مورد" },
                new Permission { PermissionName = "edit_supplier", Description = "تعديل مورد" },
                new Permission { PermissionName = "delete_supplier", Description = "حذف مورد" },

                // ============ التقارير (Reports) ============
                new Permission { PermissionName = "view_reports", Description = "عرض التقارير" },
                new Permission { PermissionName = "export_reports", Description = "تصدير التقارير" },
                new Permission { PermissionName = "view_sales_reports", Description = "عرض تقارير المبيعات" },
                new Permission { PermissionName = "view_inventory_reports", Description = "عرض تقارير المخزون" },
                new Permission { PermissionName = "view_financial_reports", Description = "عرض التقارير المالية" },

                // ============ المصروفات (Expenses) ============
                new Permission { PermissionName = "view_expenses", Description = "عرض المصروفات" },
                new Permission { PermissionName = "create_expense", Description = "إنشاء مصروف" },
                new Permission { PermissionName = "edit_expense", Description = "تعديل مصروف" },
                new Permission { PermissionName = "delete_expense", Description = "حذف مصروف" },
                
                // ============ إعدادات النظام & النسخ الاحتياطي (System) ============
                new Permission { PermissionName = "view_settings", Description = "عرض الإعدادات" },
                new Permission { PermissionName = "manage_settings", Description = "إدارة الإعدادات" },
                new Permission { PermissionName = "manage_backup", Description = "إدارة النسخ الاحتياطي" },
                new Permission { PermissionName = "view_dashboard", Description = "عرض لوحة التحكم" }
            };
        }
    }
}
