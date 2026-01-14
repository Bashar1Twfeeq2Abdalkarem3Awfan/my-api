using Microsoft.EntityFrameworkCore;
using MyAPIv3.Models;
using MyAPIv3.Helpers;

namespace MyAPIv3.Data
{
    /// <summary>
    /// مُبذِّر البيانات الأولية لنظام الصلاحيات
    /// Database seeder for User-Role-Permission system
    /// </summary>
    public static class AuthSeeder
    {
        /// <summary>
        /// تطبيق البيانات الأولية
        /// Apply initial seed data
        /// </summary>
        public static async Task SeedAuthData(AppDbContext context)
        {
            // ============================================================
            // 1. إنشاء الأدوار الأساسية
            // 1. Create default roles
            // ============================================================
            
            if (!await context.Roles.AnyAsync())
            {
                var roles = new List<Role>
                {
                    new Role
                    {
                        RoleName = "مدير",
                        Description = "صلاحيات كاملة على النظام بأكمله",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Role
                    {
                        RoleName = "محاسب مالي",
                        Description = "إدارة المبيعات، المشتريات، المديونيات، والتقارير المالية",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Role
                    {
                        RoleName = "كاشير",
                        Description = "نقاط البيع - إنشاء فواتير البيع وعرض المخزون فقط",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Roles seeded successfully!");
            }

            // ============================================================
            // 2. إنشاء الصلاحيات
            // 2. Create permissions
            // ============================================================
            
            if (!await context.Permissions.AnyAsync())
            {
                var permissions = new List<Permission>
                {
                    // ========== Sales & Invoices (المبيعات والفواتير) ==========
                    new Permission { PermissionName = "view_sales", Category = "Sales", Module = "Invoices", Description = "عرض فواتير البيع", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "create_invoice", Category = "Sales", Module = "Invoices", Description = "إنشاء فاتورة", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "edit_invoice", Category = "Sales", Module = "Invoices", Description = "تعديل فاتورة", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "delete_invoice", Category = "Sales", Module = "Invoices", Description = "حذف فاتورة", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "export_invoice", Category = "Sales", Module = "Invoices", Description = "تصدير/طباعة فاتورة", IsActive = true, CreatedAt = DateTime.UtcNow },

                    // ========== Purchases (المشتريات) ==========
                    new Permission { PermissionName = "view_purchases", Category = "Purchases", Module = "Invoices", Description = "عرض فواتير الشراء", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "create_purchase", Category = "Purchases", Module = "Invoices", Description = "إنشاء فاتورة شراء", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "edit_purchase", Category = "Purchases", Module = "Invoices", Description = "تعديل فاتورة شراء", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "delete_purchase", Category = "Purchases", Module = "Invoices", Description = "حذف فاتورة شراء", IsActive = true, CreatedAt = DateTime.UtcNow },

                    // ========== Returns (المرتجعات) ==========
                    new Permission { PermissionName = "view_returns", Category = "Returns", Module = "Returns", Description = "عرض المرتجعات", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "create_return", Category = "Returns", Module = "Returns", Description = "إنشاء مرتجع", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "edit_return", Category = "Returns", Module = "Returns", Description = "تعديل مرتجع", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "delete_return", Category = "Returns", Module = "Returns", Description = "حذف مرتجع", IsActive = true, CreatedAt = DateTime.UtcNow },

                    // ========== Inventory (المخزون) ==========
                    new Permission { PermissionName = "view_inventory", Category = "Inventory", Module = "Inventory", Description = "عرض المخزون", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "update_inventory", Category = "Inventory", Module = "Inventory", Description = "تحديث المخزون", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "adjust_inventory", Category = "Inventory", Module = "Inventory", Description = "تعديل المخزون يدوياً", IsActive = true, CreatedAt = DateTime.UtcNow },

                    // ========== Debts (المديونيات) ==========
                    new Permission { PermissionName = "view_debts", Category = "Debts", Module = "Debts", Description = "عرض المديونيات", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "create_debt", Category = "Debts", Module = "Debts", Description = "إنشاء دين", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "edit_debt", Category = "Debts", Module = "Debts", Description = "تعديل دين", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "delete_debt", Category = "Debts", Module = "Debts", Description = "حذف دين", IsActive = true, CreatedAt = DateTime.UtcNow },

                    // ========== Products (المنتجات) ==========
                    new Permission { PermissionName = "view_products", Category = "Products", Module = "Products", Description = "عرض المنتجات", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "create_product", Category = "Products", Module = "Products", Description = "إضافة منتج جديد", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "edit_product", Category = "Products", Module = "Products", Description = "تعديل منتج", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "delete_product", Category = "Products", Module = "Products", Description = "حذف منتج", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "manage_products", Category = "Products", Module = "Products", Description = "إدارة وحدات وأسعار المنتجات", IsActive = true, CreatedAt = DateTime.UtcNow },

                    // ========== Customers (العملاء) ==========
                    new Permission { PermissionName = "view_customers", Category = "Customers", Module = "Persons", Description = "عرض العملاء", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "create_customer", Category = "Customers", Module = "Persons", Description = "إضافة عميل جديد", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "edit_customer", Category = "Customers", Module = "Persons", Description = "تعديل بيانات عميل", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "delete_customer", Category = "Customers", Module = "Persons", Description = "حذف عميل", IsActive = true, CreatedAt = DateTime.UtcNow },

                    // ========== Suppliers (الموردين) ==========
                    new Permission { PermissionName = "view_suppliers", Category = "Suppliers", Module = "Persons", Description = "عرض الموردين", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "create_supplier", Category = "Suppliers", Module = "Persons", Description = "إضافة مورد جديد", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "edit_supplier", Category = "Suppliers", Module = "Persons", Description = "تعديل بيانات مورد", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "delete_supplier", Category = "Suppliers", Module = "Persons", Description = "حذف مورد", IsActive = true, CreatedAt = DateTime.UtcNow },

                    // ========== Reports (التقارير) ==========
                    new Permission { PermissionName = "view_reports", Category = "Reports", Module = "Reports", Description = "عرض التقارير", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "export_reports", Category = "Reports", Module = "Reports", Description = "تصدير التقارير", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "view_sales_reports", Category = "Reports", Module = "Reports", Description = "عرض تقارير المبيعات", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "view_inventory_reports", Category = "Reports", Module = "Reports", Description = "عرض تقارير المخزون", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "view_financial_reports", Category = "Reports", Module = "Reports", Description = "عرض التقارير المالية", IsActive = true, CreatedAt = DateTime.UtcNow },

                    // ========== Users (المستخدمين) ==========
                    new Permission { PermissionName = "view_users", Category = "Users", Module = "Users", Description = "عرض المستخدمين", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "create_user", Category = "Users", Module = "Users", Description = "إضافة مستخدم جديد", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "edit_user", Category = "Users", Module = "Users", Description = "تعديل مستخدم", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "delete_user", Category = "Users", Module = "Users", Description = "حذف مستخدم", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "change_password", Category = "Users", Module = "Users", Description = "تغيير كلمة المرور", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "assign_roles", Category = "Users", Module = "Users", Description = "تعيين أدوار للمستخدمين", IsActive = true, CreatedAt = DateTime.UtcNow },

                    // ========== Roles (الأدوار) ==========
                    new Permission { PermissionName = "view_roles", Category = "Roles", Module = "Roles", Description = "عرض الأدوار", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "create_role", Category = "Roles", Module = "Roles", Description = "إنشاء دور جديد", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "edit_role", Category = "Roles", Module = "Roles", Description = "تعديل دور", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "delete_role", Category = "Roles", Module = "Roles", Description = "حذف دور", IsActive = true, CreatedAt = DateTime.UtcNow },

                    // ========== Permissions (الصلاحيات) ==========
                    new Permission { PermissionName = "view_permissions", Category = "Permissions", Module = "Permissions", Description = "عرض قائمة الصلاحيات", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "create_permission", Category = "Permissions", Module = "Permissions", Description = "إضافة صلاحية جديدة", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "edit_permission", Category = "Permissions", Module = "Permissions", Description = "تعديل صلاحية", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "delete_permission", Category = "Permissions", Module = "Permissions", Description = "حذف صلاحية", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "manage_permissions", Category = "Permissions", Module = "Permissions", Description = "إدارة ربط الأدوار بالصلاحيات", IsActive = true, CreatedAt = DateTime.UtcNow },

                    // ========== Expenses (المصروفات) ==========
                    new Permission { PermissionName = "view_expenses", Category = "Expenses", Module = "Expenses", Description = "عرض المصروفات", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "create_expense", Category = "Expenses", Module = "Expenses", Description = "إضافة مصروف", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "edit_expense", Category = "Expenses", Module = "Expenses", Description = "تعديل مصروف", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Permission { PermissionName = "delete_expense", Category = "Expenses", Module = "Expenses", Description = "حذف مصروف", IsActive = true, CreatedAt = DateTime.UtcNow },
                };

                await context.Permissions.AddRangeAsync(permissions);
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Permissions seeded successfully!");
            }

            // ============================================================
            // 3. ربط الصلاحيات بالأدوار
            // 3. Link permissions to roles
            // ============================================================
            
            if (!await context.RolePermissions.AnyAsync())
            {
                var managerRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "مدير");
                var accountantRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "محاسب مالي");
                var cashierRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "كاشير");

                var allPermissions = await context.Permissions.ToListAsync();

                var rolePermissions = new List<RolePermission>();

                // المدير ← كل الصلاحيات
                // Manager gets ALL permissions
                if (managerRole != null)
                {
                    foreach (var perm in allPermissions)
                    {
                        rolePermissions.Add(new RolePermission
                        {
                            RoleId = managerRole.Id,
                            PermissionId = perm.Id,
                            GrantedAt = DateTime.UtcNow
                        });
                    }
                }

                // المحاسب ← كل شيء ماعدا إدارة المستخدمين
                // Accountant gets everything except user management
                if (accountantRole != null)
                {
                    var accountantPermissions = allPermissions
                        .Where(p => p.Category != "Users")
                        .ToList();

                    foreach (var perm in accountantPermissions)
                    {
                        rolePermissions.Add(new RolePermission
                        {
                            RoleId = accountantRole.Id,
                            PermissionId = perm.Id,
                            GrantedAt = DateTime.UtcNow
                        });
                    }
                }

                // الكاشير ← بيع + عرض فقط
                // Cashier gets sales + view only
                if (cashierRole != null)
                {
                    var cashierPermNames = new[]
                    {
                        "view_sales", "create_invoice",
                        "view_inventory",
                        "view_products",
                        "view_customers"
                    };

                    var cashierPermissions = allPermissions
                        .Where(p => cashierPermNames.Contains(p.PermissionName))
                        .ToList();

                    foreach (var perm in cashierPermissions)
                    {
                        rolePermissions.Add(new RolePermission
                        {
                            RoleId = cashierRole.Id,
                            PermissionId = perm.Id,
                            GrantedAt = DateTime.UtcNow
                        });
                    }
                }

                await context.RolePermissions.AddRangeAsync(rolePermissions);
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Role-Permission mappings seeded successfully!");
            }

            Console.WriteLine("🎉 Auth Seed Data completed!");
        }
    }
}
