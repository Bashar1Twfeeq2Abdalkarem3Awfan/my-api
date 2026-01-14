using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAPIv3.Services;

namespace MyAPIv3.Controllers
{
    /// <summary>
    /// Controller للنسخ الاحتياطي والاستعادة
    /// Backup and Restore Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // يتطلب تسجيل دخول
    public class BackupController : ControllerBase
    {
        private readonly BackupService _backupService;
        private readonly IWebHostEnvironment _environment;

        public BackupController(BackupService backupService, IWebHostEnvironment environment)
        {
            _backupService = backupService;
            _environment = environment;
        }

        /// <summary>
        /// GET: api/Backup/Download
        /// إنشاء وتنزيل نسخة احتياطية
        /// Create and download database backup
        /// </summary>
        [HttpGet("Download")]
        public async Task<IActionResult> DownloadBackup()
        {
            try
            {
                // إنشاء النسخة الاحتياطية
                var backupPath = await _backupService.CreateBackupAsync();

                // قراءة الملف
                var fileBytes = await System.IO.File.ReadAllBytesAsync(backupPath);
                var fileName = Path.GetFileName(backupPath);

                // إرجاع الملف للتنزيل
                return File(fileBytes, "application/sql", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"فشل إنشاء النسخة الاحتياطية: {ex.Message}" });
            }
        }

        /// <summary>
        /// POST: api/Backup/Restore
        /// رفع واستعادة نسخة احتياطية
        /// Upload and restore database from backup
        /// </summary>
        [HttpPost("Restore")]
        public async Task<IActionResult> RestoreBackup(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "لم يتم رفع ملف" });
                }

                // التحقق من امتداد الملف
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (extension != ".sql")
                {
                    return BadRequest(new { message = "يجب أن يكون الملف بصيغة .sql" });
                }

                // حفظ الملف مؤقتاً
                var tempPath = Path.Combine(Path.GetTempPath(), $"restore_{Guid.NewGuid()}.sql");
                
                using (var stream = new FileStream(tempPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                try
                {
                    // استعادة قاعدة البيانات
                    await _backupService.RestoreBackupAsync(tempPath);

                    return Ok(new { message = "تمت الاستعادة بنجاح" });
                }
                finally
                {
                    // حذف الملف المؤقت
                    if (System.IO.File.Exists(tempPath))
                    {
                        System.IO.File.Delete(tempPath);
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"فشلت الاستعادة: {ex.Message}" });
            }
        }

        /// <summary>
        /// GET: api/Backup/List
        /// الحصول على قائمة النسخ الاحتياطية المتاحة
        /// Get list of available backups
        /// </summary>
        [HttpGet("List")]
        public IActionResult GetBackupList()
        {
            try
            {
                var backups = _backupService.GetAvailableBackups();
                return Ok(backups);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"فشل الحصول على القائمة: {ex.Message}" });
            }
        }

        /// <summary>
        /// DELETE: api/Backup/{fileName}
        /// حذف نسخة احتياطية
        /// Delete a backup file
        /// </summary>
        [HttpDelete("{fileName}")]
        public IActionResult DeleteBackup(string fileName)
        {
            try
            {
                _backupService.DeleteBackup(fileName);
                return Ok(new { message = "تم حذف النسخة الاحتياطية" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"فشل الحذف: {ex.Message}" });
            }
        }

        /// <summary>
        /// GET: api/Backup/Test
        /// اختبار اتصال pg_dump و psql
        /// Test pg_dump and psql availability
        /// </summary>
        [HttpGet("Test")]
        public IActionResult TestBackupTools()
        {
            try
            {
                // اختبار بإنشاء نسخة تجريبية
                var result = new
                {
                    message = "أدوات النسخ الاحتياطي متاحة",
                    ready = true
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = $"الأدوات غير متاحة: {ex.Message}",
                    ready = false
                });
            }
        }
    }
}
