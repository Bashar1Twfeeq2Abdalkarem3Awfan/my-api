using System.Diagnostics;
using Npgsql;

namespace MyAPIv3.Services
{
    /// <summary>
    /// خدمة النسخ الاحتياطي والاستعادة
    /// Backup and Restore Service for PostgreSQL Database
    /// </summary>
    public class BackupService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly string _backupDirectory;

        public BackupService(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            
            // إنشاء مجلد للنسخ الاحتياطية
            _backupDirectory = Path.Combine(_environment.ContentRootPath, "Backups");
            if (!Directory.Exists(_backupDirectory))
            {
                Directory.CreateDirectory(_backupDirectory);
            }
        }

        /// <summary>
        /// إنشاء نسخة احتياطية من قاعدة البيانات
        /// Create database backup using pg_dump
        /// </summary>
        public async Task<string> CreateBackupAsync()
        {
            try
            {
                // الحصول على معلومات الاتصال
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                var builder = new NpgsqlConnectionStringBuilder(connectionString);

                // اسم الملف بالتاريخ والوقت
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"backup_{timestamp}.sql";
                var backupPath = Path.Combine(_backupDirectory, fileName);

                // تجهيز pg_dump command
                var pgDumpPath = GetPgDumpPath();
                
                // Arguments لـ pg_dump
                var arguments = $"-h {builder.Host} " +
                              $"-p {builder.Port} " +
                              $"-U {builder.Username} " +
                              $"-d {builder.Database} " +
                              $"-F p " + // plain text SQL format
                              $"--no-owner " + // don't dump ownership commands
                              $"--no-privileges " + // don't dump privileges
                              $"-f \"{backupPath}\"";

                // تشغيل pg_dump
                var processInfo = new ProcessStartInfo
                {
                    FileName = pgDumpPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // إضافة كلمة المرور كـ environment variable
                processInfo.EnvironmentVariables["PGPASSWORD"] = builder.Password;

                using (var process = Process.Start(processInfo))
                {
                    if (process == null)
                    {
                        throw new Exception("فشل في بدء عملية النسخ الاحتياطي");
                    }

                    // قراءة الأخطاء إن وجدت
                    var error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    if (process.ExitCode != 0)
                    {
                        throw new Exception($"فشل النسخ الاحتياطي: {error}");
                    }
                }

                return backupPath;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطأ في إنشاء النسخة الاحتياطية: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// استعادة قاعدة البيانات من نسخة احتياطية
        /// Restore database from backup file using psql
        /// </summary>
        public async Task RestoreBackupAsync(string backupFilePath)
        {
            try
            {
                if (!File.Exists(backupFilePath))
                {
                    throw new FileNotFoundException("ملف النسخة الاحتياطية غير موجود");
                }

                // الحصول على معلومات الاتصال
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                var builder = new NpgsqlConnectionStringBuilder(connectionString);

                // تجهيز psql command
                var psqlPath = GetPsqlPath();

                // Arguments لـ psql
                var arguments = $"-h {builder.Host} " +
                              $"-p {builder.Port} " +
                              $"-U {builder.Username} " +
                              $"-d {builder.Database} " +
                              $"-f \"{backupFilePath}\"";

                // تشغيل psql
                var processInfo = new ProcessStartInfo
                {
                    FileName = psqlPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // إضافة كلمة المرور
                processInfo.EnvironmentVariables["PGPASSWORD"] = builder.Password;

                using (var process = Process.Start(processInfo))
                {
                    if (process == null)
                    {
                        throw new Exception("فشل في بدء عملية الاستعادة");
                    }

                    var error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    if (process.ExitCode != 0)
                    {
                        throw new Exception($"فشل الاستعادة: {error}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"خطأ في استعادة النسخة الاحتياطية: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// الحصول على قائمة النسخ الاحتياطية المتاحة
        /// Get list of available backup files
        /// </summary>
        public List<BackupInfo> GetAvailableBackups()
        {
            var backups = new List<BackupInfo>();

            if (!Directory.Exists(_backupDirectory))
            {
                return backups;
            }

            var files = Directory.GetFiles(_backupDirectory, "backup_*.sql")
                                .OrderByDescending(f => File.GetCreationTime(f));

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                backups.Add(new BackupInfo
                {
                    FileName = fileInfo.Name,
                    FilePath = fileInfo.FullName,
                    CreatedAt = fileInfo.CreationTime,
                    SizeInBytes = fileInfo.Length,
                    SizeFormatted = FormatFileSize(fileInfo.Length)
                });
            }

            return backups;
        }

        /// <summary>
        /// حذف نسخة احتياطية
        /// Delete a backup file
        /// </summary>
        public void DeleteBackup(string fileName)
        {
            var filePath = Path.Combine(_backupDirectory, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        /// <summary>
        /// الحصول على مسار pg_dump
        /// Get pg_dump executable path
        /// </summary>
        private string GetPgDumpPath()
        {
            // 1. محاولة في PATH أولاً
            try
            {
                var testProcess = new ProcessStartInfo
                {
                    FileName = "pg_dump",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                
                using (var process = Process.Start(testProcess))
                {
                    if (process != null)
                    {
                        process.WaitForExit();
                        if (process.ExitCode == 0)
                        {
                            return "pg_dump";
                        }
                    }
                }
            }
            catch { }

            // 2. البحث في مجلدات PostgreSQL
            var basePaths = new[]
            {
                @"C:\Program Files\PostgreSQL",
                @"C:\Program Files (x86)\PostgreSQL"
            };

            foreach (var basePath in basePaths)
            {
                if (Directory.Exists(basePath))
                {
                    // البحث في جميع الإصدارات
                    var versionDirs = Directory.GetDirectories(basePath)
                        .OrderByDescending(d => d); // أحدث إصدار أولاً

                    foreach (var versionDir in versionDirs)
                    {
                        var pgDumpPath = Path.Combine(versionDir, "bin", "pg_dump.exe");
                        if (File.Exists(pgDumpPath))
                        {
                            return pgDumpPath;
                        }
                    }
                }
            }

            throw new Exception("لم يتم العثور على pg_dump. تأكد من تثبيت PostgreSQL بشكل صحيح");
        }

        /// <summary>
        /// الحصول على مسار psql
        /// Get psql executable path
        /// </summary>
        private string GetPsqlPath()
        {
            // 1. محاولة في PATH أولاً
            try
            {
                var testProcess = new ProcessStartInfo
                {
                    FileName = "psql",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                
                using (var process = Process.Start(testProcess))
                {
                    if (process != null)
                    {
                        process.WaitForExit();
                        if (process.ExitCode == 0)
                        {
                            return "psql";
                        }
                    }
                }
            }
            catch { }

            // 2. البحث في مجلدات PostgreSQL
            var basePaths = new[]
            {
                @"C:\Program Files\PostgreSQL",
                @"C:\Program Files (x86)\PostgreSQL"
            };

            foreach (var basePath in basePaths)
            {
                if (Directory.Exists(basePath))
                {
                    var versionDirs = Directory.GetDirectories(basePath)
                        .OrderByDescending(d => d);

                    foreach (var versionDir in versionDirs)
                    {
                        var psqlPath = Path.Combine(versionDir, "bin", "psql.exe");
                        if (File.Exists(psqlPath))
                        {
                            return psqlPath;
                        }
                    }
                }
            }

            throw new Exception("لم يتم العثور على psql. تأكد من تثبيت PostgreSQL بشكل صحيح");
        }

        /// <summary>
        /// تنسيق حجم الملف
        /// Format file size to human-readable format
        /// </summary>
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }

    /// <summary>
    /// معلومات النسخة الاحتياطية
    /// Backup file information
    /// </summary>
    public class BackupInfo
    {
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public long SizeInBytes { get; set; }
        public string SizeFormatted { get; set; } = null!;
    }
}
