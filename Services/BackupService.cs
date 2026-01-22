using System.Text;
using Npgsql;

namespace MyAPIv3.Services
{
    /// <summary>
    /// خدمة النسخ الاحتياطي والاستعادة
    /// Backup and Restore Service for PostgreSQL Database
    /// ✨ يعمل على Railway وجميع خدمات الاستضافة السحابية
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
        /// Create database backup using Npgsql (works on Railway!)
        /// </summary>
        public async Task<string> CreateBackupAsync()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                
                // اسم الملف بالتاريخ والوقت
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"backup_{timestamp}.sql";
                var backupPath = Path.Combine(_backupDirectory, fileName);

                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    
                    var sqlBuilder = new StringBuilder();
                    
                    // Header
                    sqlBuilder.AppendLine("-- PostgreSQL Database Backup");
                    sqlBuilder.AppendLine($"-- Created: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    sqlBuilder.AppendLine($"-- Database: {connection.Database}");
                    sqlBuilder.AppendLine();
                    
                    // Get all tables
                    var tables = await GetAllTablesAsync(connection);
                    
                    foreach (var table in tables)
                    {
                        sqlBuilder.AppendLine($"-- Table: {table}");
                        sqlBuilder.AppendLine($"TRUNCATE TABLE \"{table}\" CASCADE;");
                        sqlBuilder.AppendLine();
                        
                        // Get table data
                        var tableData = await GetTableDataAsync(connection, table);
                        if (!string.IsNullOrEmpty(tableData))
                        {
                            sqlBuilder.AppendLine(tableData);
                            sqlBuilder.AppendLine();
                        }
                    }
                    
                    // Write to file
                    await File.WriteAllTextAsync(backupPath, sqlBuilder.ToString(), Encoding.UTF8);
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
        /// Restore database from backup file using Npgsql
        /// </summary>
        public async Task RestoreBackupAsync(string backupFilePath)
        {
            try
            {
                if (!File.Exists(backupFilePath))
                {
                    throw new FileNotFoundException("ملف النسخة الاحتياطية غير موجود");
                }

                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                var sqlContent = await File.ReadAllTextAsync(backupFilePath, Encoding.UTF8);

                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    
                    // ✨ الخطوة 1: حذف جميع البيانات من كل الجداول
                    var tables = await GetAllTablesAsync(connection);
                    
                    // حذف البيانات بترتيب عكسي لتجنب مشاكل Foreign Keys
                    foreach (var table in tables.AsEnumerable().Reverse())
                    {
                        try
                        {
                            using (var truncateCmd = new NpgsqlCommand($"TRUNCATE TABLE \"{table}\" RESTART IDENTITY CASCADE;", connection))
                            {
                                await truncateCmd.ExecuteNonQueryAsync();
                            }
                        }
                        catch
                        {
                            // إذا فشل TRUNCATE، جرب DELETE
                            using (var deleteCmd = new NpgsqlCommand($"DELETE FROM \"{table}\";", connection))
                            {
                                await deleteCmd.ExecuteNonQueryAsync();
                            }
                        }
                    }
                    
                    // ✨ الخطوة 2: تنفيذ SQL من ملف النسخة الاحتياطية
                    var statements = sqlContent
                        .Split(new[] { ";\r\n", ";\n" }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(s => !string.IsNullOrWhiteSpace(s) && !s.TrimStart().StartsWith("--"));

                    foreach (var statement in statements)
                    {
                        var trimmedStatement = statement.Trim();
                        if (!string.IsNullOrEmpty(trimmedStatement))
                        {
                            try
                            {
                                using (var command = new NpgsqlCommand(trimmedStatement + ";", connection))
                                {
                                    await command.ExecuteNonQueryAsync();
                                }
                            }
                            catch (Exception ex)
                            {
                                // تجاهل أخطاء TRUNCATE المكررة من ملف الـ backup
                                if (!ex.Message.Contains("TRUNCATE") && !ex.Message.Contains("does not exist"))
                                {
                                    throw;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"خطأ في استعادة النسخة الاحتياطية: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// الحصول على قائمة جميع الجداول
        /// Get all table names from database
        /// </summary>
        private async Task<List<string>> GetAllTablesAsync(NpgsqlConnection connection)
        {
            var tables = new List<string>();
            
            var query = @"
                SELECT table_name 
                FROM information_schema.tables 
                WHERE table_schema = 'public' 
                AND table_type = 'BASE TABLE'
                ORDER BY table_name;";

            using (var command = new NpgsqlCommand(query, connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }
            }

            return tables;
        }

        /// <summary>
        /// الحصول على بيانات جدول معين
        /// Get data from a specific table as INSERT statements
        /// </summary>
        private async Task<string> GetTableDataAsync(NpgsqlConnection connection, string tableName)
        {
            var sqlBuilder = new StringBuilder();
            
            // Get column names
            var columns = await GetTableColumnsAsync(connection, tableName);
            if (columns.Count == 0) return string.Empty;

            // Get data
            var selectQuery = $"SELECT * FROM \"{tableName}\";";
            using (var command = new NpgsqlCommand(selectQuery, connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var values = new List<string>();
                    
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        if (reader.IsDBNull(i))
                        {
                            values.Add("NULL");
                        }
                        else
                        {
                            var value = reader.GetValue(i);
                            var formattedValue = FormatSqlValue(value);
                            values.Add(formattedValue);
                        }
                    }

                    var columnNames = string.Join(", ", columns.Select(c => $"\"{c}\""));
                    var valuesList = string.Join(", ", values);
                    
                    sqlBuilder.AppendLine($"INSERT INTO \"{tableName}\" ({columnNames}) VALUES ({valuesList});");
                }
            }

            return sqlBuilder.ToString();
        }

        /// <summary>
        /// الحصول على أسماء أعمدة جدول
        /// Get column names for a table
        /// </summary>
        private async Task<List<string>> GetTableColumnsAsync(NpgsqlConnection connection, string tableName)
        {
            var columns = new List<string>();
            
            var query = @"
                SELECT column_name 
                FROM information_schema.columns 
                WHERE table_schema = 'public' 
                AND table_name = @tableName
                ORDER BY ordinal_position;";

            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("tableName", tableName);
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        columns.Add(reader.GetString(0));
                    }
                }
            }

            return columns;
        }

        /// <summary>
        /// تنسيق القيمة للاستخدام في SQL
        /// Format value for SQL statement
        /// </summary>
        private string FormatSqlValue(object value)
        {
            if (value == null || value is DBNull)
                return "NULL";

            if (value is string str)
                return $"'{str.Replace("'", "''")}'";

            if (value is DateTime dt)
                return $"'{dt:yyyy-MM-dd HH:mm:ss}'";

            if (value is bool b)
                return b ? "TRUE" : "FALSE";

            if (value is byte[] bytes)
                return $"'\\x{BitConverter.ToString(bytes).Replace("-", "")}'";

            // Numbers and other types
            return value.ToString()!;
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
