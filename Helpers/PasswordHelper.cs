using BCrypt.Net;

namespace MyAPIv3.Helpers
{
    /// <summary>
    /// مساعد لتشفير والتحقق من كلمات المرور بشكل آمن
    /// Password hashing and verification helper using BCrypt
    /// </summary>
    public static class PasswordHelper
    {
        /// <summary>
        /// تشفير كلمة المرور
        /// Hash a password using BCrypt
        /// </summary>
        /// <param name="password">كلمة المرور النصية</param>
        /// <returns>كلمة المرور المشفرة</returns>
        public static string HashPassword(string password)
        {
            // استخدام BCrypt مع WorkFactor = 12 (أمان عالي)
            // Use BCrypt with WorkFactor = 12 (high security)
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        /// <summary>
        /// التحقق من كلمة المرور
        /// Verify a password against a hash
        /// </summary>
        /// <param name="password">كلمة المرور النصية</param>
        /// <param name="hash">الـ Hash المخزن</param>
        /// <returns>true إذا كانت متطابقة</returns>
        public static bool VerifyPassword(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                // في حالة hash غير صحيح
                // In case of invalid hash format
                return false;
            }
        }
    }
}
