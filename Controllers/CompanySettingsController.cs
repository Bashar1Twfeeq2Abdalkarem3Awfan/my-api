using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyAPIv3.Data;
using MyAPIv3.Models;

namespace MyAPIv3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanySettingsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CompanySettingsController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        /// <summary>
        /// GET: api/CompanySettings
        /// استرجاع إعدادات الشركة
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<CompanySettings>> GetSettings()
        {
            var settings = await _context.CompanySettings.FirstOrDefaultAsync();
            
            if (settings == null)
            {
                // إنشاء إعدادات افتراضية وحفظها للمرة الأولى
                // Create and save default settings for first time
                settings = new CompanySettings 
                { 
                    CompanyName = "اسم المنشأة",
                    Address = "",
                    TaxId = "",
                    PhoneNumber = "",
                    Email = "",
                    CommercialRegistration = "",
                    CreatedAt = DateTime.UtcNow
                };
                
                _context.CompanySettings.Add(settings);
                await _context.SaveChangesAsync();
            }
            
            return Ok(settings);
        }

        /// <summary>
        /// PUT: api/CompanySettings
        /// تحديث إعدادات الشركة
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateSettings([FromBody] CompanySettings settings)
        {
            try
            {
                var existing = await _context.CompanySettings.FirstOrDefaultAsync();
                
                if (existing == null)
                {
                    // إنشاء جديد
                    settings.CreatedAt = DateTime.UtcNow;
                    _context.CompanySettings.Add(settings);
                }
                else
                {
                    // تحديث موجود
                    existing.CompanyName = settings.CompanyName;
                    existing.Address = settings.Address;
                    existing.TaxId = settings.TaxId;
                    existing.PhoneNumber = settings.PhoneNumber;
                    existing.Email = settings.Email;
                    existing.CommercialRegistration = settings.CommercialRegistration;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
                
                await _context.SaveChangesAsync();
                return Ok(new { message = "تم تحديث الإعدادات بنجاح" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "خطأ في تحديث الإعدادات", error = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/CompanySettings/UploadLogo
        /// رفع شعار الشركة
        /// </summary>
        [HttpPost("UploadLogo")]
        public async Task<IActionResult> UploadLogo(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "لم يتم رفع ملف" });

                // التحقق من نوع الملف
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(extension))
                    return BadRequest(new { message = "نوع الملف غير مدعوم. يرجى رفع صورة (jpg, png, gif)" });

                // التحقق من حجم الملف (أقل من 5 ميغا)
                if (file.Length > 5 * 1024 * 1024)
                    return BadRequest(new { message = "حجم الملف كبير جداً. الحد الأقصى 5 ميغابايت" });

                // إنشاء مجلد الشعارات إذا لم يكن موجوداً
                // Fix: Handle null WebRootPath
                var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
                var uploadsFolder = Path.Combine(webRoot, "logos");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // إنشاء اسم فريد للملف
                var fileName = $"logo_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // حفظ الملف
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // تحديث مسار الشعار في قاعدة البيانات
                var settings = await _context.CompanySettings.FirstOrDefaultAsync();
                var logoPath = $"/logos/{fileName}";
                
                if (settings != null)
                {
                    // حذف الشعار القديم إذا وجد
                    if (!string.IsNullOrEmpty(settings.LogoPath))
                    {
                        var oldLogoPath = Path.Combine(webRoot, settings.LogoPath.TrimStart('/'));
                        if (System.IO.File.Exists(oldLogoPath))
                        {
                            System.IO.File.Delete(oldLogoPath);
                        }
                    }
                    
                    settings.LogoPath = logoPath;
                    settings.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // إنشاء إعدادات جديدة
                    _context.CompanySettings.Add(new CompanySettings
                    {
                        CompanyName = "متجر جديد",
                        LogoPath = logoPath,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();

                return Ok(new { logoPath = logoPath, message = "تم رفع الشعار بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "خطأ في رفع الشعار", error = ex.Message });
            }
        }

        /// <summary>
        /// DELETE: api/CompanySettings/DeleteLogo
        /// حذف شعار الشركة
        /// </summary>
        [HttpDelete("DeleteLogo")]
        public async Task<IActionResult> DeleteLogo()
        {
            try
            {
                var settings = await _context.CompanySettings.FirstOrDefaultAsync();
                
                if (settings == null || string.IsNullOrEmpty(settings.LogoPath))
                    return NotFound(new { message = "لا يوجد شعار لحذفه" });

                // حذف الملف من الخادم
                var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
                var logoPath = Path.Combine(webRoot, settings.LogoPath.TrimStart('/'));
                if (System.IO.File.Exists(logoPath))
                {
                    System.IO.File.Delete(logoPath);
                }

                // إزالة المسار من قاعدة البيانات
                settings.LogoPath = null;
                settings.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { message = "تم حذف الشعار بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "خطأ في حذف الشعار", error = ex.Message });
            }
        }
    }
}
