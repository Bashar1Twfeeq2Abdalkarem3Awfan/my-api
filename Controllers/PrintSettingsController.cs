using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyAPIv3.Data;
using MyAPIv3.Models;

namespace MyAPIv3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrintSettingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PrintSettingsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: api/PrintSettings
        /// استرجاع إعدادات الطباعة
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PrintSettings>> GetSettings()
        {
            var settings = await _context.PrintSettings.FirstOrDefaultAsync();
            
            if (settings == null)
            {
                // إنشاء إعدادات افتراضية وحفظها للمرة الأولى
                // Create and save default settings for first time
                settings = new PrintSettings 
                { 
                    PrinterType = "thermal_80",
                    ShowLogo = true,
                    ShowCompanyInfo = true,
                    FontSize = 12,
                    FooterText = "شكراً لتعاملكم معنا",
                    CreatedAt = DateTime.UtcNow
                };
                
                _context.PrintSettings.Add(settings);
                await _context.SaveChangesAsync();
            }
            
            return Ok(settings);
        }

        /// <summary>
        /// PUT: api/PrintSettings
        /// تحديث إعدادات الطباعة
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateSettings([FromBody] PrintSettings settings)
        {
            try
            {
                var existing = await _context.PrintSettings.FirstOrDefaultAsync();
                
                if (existing == null)
                {
                    // إنشاء جديد
                    settings.CreatedAt = DateTime.UtcNow;
                    _context.PrintSettings.Add(settings);
                }
                else
                {
                    // تحديث موجود
                    existing.PrinterType = settings.PrinterType;
                    existing.ShowLogo = settings.ShowLogo;
                    existing.ShowCompanyInfo = settings.ShowCompanyInfo;
                    existing.FooterText = settings.FooterText;
                    existing.FontSize = settings.FontSize;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
                
                await _context.SaveChangesAsync();
                return Ok(new { message = "تم تحديث إعدادات الطباعة بنجاح" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "خطأ في تحديث الإعدادات", error = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/PrintSettings/Reset
        /// إعادة تعيين إعدادات الطباعة للإعدادات الافتراضية
        /// </summary>
        [HttpPost("Reset")]
        public async Task<IActionResult> ResetSettings()
        {
            try
            {
                var existing = await _context.PrintSettings.FirstOrDefaultAsync();
                
                if (existing != null)
                {
                    existing.PrinterType = "thermal_80";
                    existing.ShowLogo = true;
                    existing.ShowCompanyInfo = true;
                    existing.FontSize = 12;
                    existing.FooterText = "شكراً لتعاملكم معنا";
                    existing.UpdatedAt = DateTime.UtcNow;
                    
                    await _context.SaveChangesAsync();
                }
                
                return Ok(new { message = "تم إعادة تعيين الإعدادات" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "خطأ في إعادة التعيين", error = ex.Message });
            }
        }
    }
}
