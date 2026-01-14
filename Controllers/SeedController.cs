using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MyAPIv3.Data;

namespace MyAPIv3.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    [ApiController]
    public class SeedController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SeedController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// تطبيق البيانات الأولية لنظام الصلاحيات
        /// Apply seed data for authentication system
        /// </summary>
        /// <returns></returns>
        [HttpPost("auth")]
        public async Task<IActionResult> SeedAuthData()
        {
            try
            {
                await AuthSeeder.SeedAuthData(_context);
                return Ok(new
                {
                    success = true,
                    message = "✅ تم تطبيق البيانات الأولية بنجاح!",
                    details = new
                    {
                        roles = "3 أدوار (مدير، محاسب مالي، كاشير)",
                        permissions = "38 صلاحية",
                        mappings = "مدير: كل الصلاحيات، محاسب: بدون إدارة المستخدمين، كاشير: بيع + عرض"
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "❌ حدث خطأ أثناء تطبيق البيانات الأولية",
                    error = ex.Message
                });
            }
        }
    }
}


