using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using MyAPIv3.Data;
using MyAPIv3.Models;
// ============================================================
// Permission Authorization Attribute
// سمة التحقق من الصلاحيات
// ============================================================
// Date Created: 2025-12-15
// Last Modified: 2025-12-15 20:46
// Purpose: Validate user permissions before executing API actions
//          to prevent unauthorized access
// Usage: [RequirePermission("Users.Delete")]
// ============================================================

namespace MyAPIv3.Attributes
{
    /// <summary>
    /// Attribute to require specific permission for API endpoint access
    /// سمة للتحقق من صلاحية معينة قبل الوصول لنقطة نهاية API
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _requiredPermission;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="permission">Required permission name (e.g., "Users.Delete")</param>
        public RequirePermissionAttribute(string permission)
        {
            _requiredPermission = permission ?? throw new ArgumentNullException(nameof(permission));
        }

        /// <summary>
        /// Authorization logic - executed before action (ASYNC)
        /// منطق التحقق - يُنفذ قبل تنفيذ الـ Action (بشكل غير متزامن)
        /// </summary>
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;

            // السماح بالطلبات من نوع OPTIONS (CORS preflight) بدون تحقق
            // Allow OPTIONS requests (CORS preflight) without permission check
            if (httpContext.Request.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            try
            {
                // ============================================================
                // 1. الحصول على هوية المستخدم من الهيدر
                // 1. Get current user identity from header
                // ============================================================

                // ملاحظة 18/12/2025:
                // نعتمد مؤقتاً على هيدر مخصص X-User-Id يتم إرساله من تطبيق Flutter
                // بعد تسجيل الدخول. مستقبلاً يمكن استبداله بـ JWT أو Cookie آمن.
                if (!httpContext.Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) ||
                    string.IsNullOrWhiteSpace(userIdHeader))
                {
                    // لم يتم تمرير هوية مستخدم → غير مصرح
                    // No user identity provided → Unauthorized
                    context.Result = new UnauthorizedResult();
                    return;
                }

                if (!long.TryParse(userIdHeader.ToString(), out var userId))
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                // ============================================================
                // 2. جلب السياق (AppDbContext) من الـ DI
                // 2. Resolve AppDbContext from DI container
                // ============================================================
                var dbContext = httpContext.RequestServices.GetService(typeof(AppDbContext)) as AppDbContext;
                if (dbContext == null)
                {
                    // في حال عدم توفر الـ DbContext نعيد خطأ خادم
                    // If DbContext is not available, return 500
                    context.Result = new StatusCodeResult(500);
                    return;
                }

                // ============================================================
                // 3. جلب المستخدم مع الأدوار والصلاحيات
                // 3. Load user with roles and permissions
                // ============================================================
                var user = await dbContext.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                            .ThenInclude(r => r!.RolePermissions)
                                .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                // جمع جميع أسماء الصلاحيات الفعّالة لهذا المستخدم
                // Collect all active permission names for this user
                var userPermissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var userRole in user.UserRoles ?? Enumerable.Empty<UserRole>())
                {
                    if (userRole.Role == null) continue;

                    foreach (var rolePerm in userRole.Role.RolePermissions ?? Enumerable.Empty<RolePermission>())
                    {
                        if (rolePerm.Permission != null && rolePerm.Permission.IsActive)
                        {
                            userPermissions.Add(rolePerm.Permission.PermissionName);
                        }
                    }
                }

                // ============================================================
                // 4. التحقق من امتلاك الصلاحية المطلوبة
                // 4. Check if user has the required permission
                // ============================================================
                var hasPermission = userPermissions.Contains(_requiredPermission);

                Console.WriteLine($"🔒 Permission Check for user {user.Username} (ID={user.Id}): required = '{_requiredPermission}', has = {hasPermission}");

                if (!hasPermission)
                {
                    // المستخدم لا يمتلك الصلاحية المطلوبة
                    // User does NOT have the required permission
                    context.Result = new ForbidResult();
                    return;
                }

                // في حال النجاح: لا نُعيّن context.Result → يُسمح بتنفيذ الـ Action
            }
            catch (Exception ex)
            {
                // أي خطأ غير متوقع نعامله كخطأ خادم
                // Any unexpected error is treated as server error
                Console.WriteLine($"❌ RequirePermissionAttribute error: {ex}");
                context.Result = new StatusCodeResult(500);
            }
        }
    }
}
