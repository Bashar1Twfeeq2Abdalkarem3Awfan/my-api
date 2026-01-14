# Database Scripts - نصوص قاعدة البيانات

هذا المجلد يحتوي على نصوص SQL لإعداد نظام الصلاحيات في قاعدة البيانات.

## 📋 الملفات المتوفرة

### 1. `check-permissions.sql`
**الغرض**: فحص الصلاحيات والأدوار الموجودة حالياً في قاعدة البيانات

**متى تستخدمه**: قبل تشغيل أي scripts أخرى للتحقق من الوضع الحالي

**كيفية الاستخدام**:
```bash
psql -U your_username -d your_database -f check-permissions.sql
```

---

### 2. `seed-permissions.sql`
**الغرض**: إضافة جميع الصلاحيات المعرفة في `permission_constants.dart` إلى قاعدة البيانات

**متى تستخدمه**: عند إعداد قاعدة البيانات لأول مرة أو عند إضافة صلاحيات جديدة

**ملاحظات مهمة**:
- يستخدم `ON CONFLICT DO NOTHING` لتجنب التكرار
- آمن للتشغيل عدة مرات
- يضيف 46 صلاحية موزعة على 11 فئة

**كيفية الاستخدام**:
```bash
psql -U your_username -d your_database -f seed-permissions.sql
```

---

### 3. `seed-default-roles.sql`
**الغرض**: إنشاء الأدوار الافتراضية وتعيين الصلاحيات المناسبة لكل دور

**الأدوار المُنشأة**:
1. **Admin** - جميع الصلاحيات (46 صلاحية)
2. **Manager** - صلاحيات إدارية (32 صلاحية)
3. **Cashier** - صلاحيات نقطة البيع (7 صلاحيات)
4. **Accountant** - صلاحيات مالية (11 صلاحية)
5. **Inventory Manager** - صلاحيات المخزون (14 صلاحية)

**متطلبات**:
- يجب تشغيل `seed-permissions.sql` أولاً

**كيفية الاستخدام**:
```bash
psql -U your_username -d your_database -f seed-default-roles.sql
```

---

### 4. `create-admin-user.sql`
**الغرض**: إنشاء مستخدم Admin للاختبار

**بيانات المستخدم**:
- **Username**: `admin`
- **Password**: `admin123` (يجب تغييرها بعد أول تسجيل دخول)
- **Role**: Admin (جميع الصلاحيات)

**متطلبات**:
- يجب تشغيل `seed-permissions.sql` و `seed-default-roles.sql` أولاً

**ملاحظة هامة**:
⚠️ هذا Script يحتاج تعديل! يجب استبدال `$2a$11$YourHashedPasswordHere` بكلمة مرور مشفرة فعلية.

**كيفية الاستخدام**:
```bash
# بعد تعديل password_hash
psql -U your_username -d your_database -f create-admin-user.sql
```

---

### 5. `fix_user_role_table.sql`
**الغرض**: إصلاح جدول `user_role` بإضافة Primary Key

**متى تستخدمه**: إذا كان جدول `user_role` لا يحتوي على Primary Key

---

## 🚀 الترتيب الموصى به للتشغيل

### للإعداد الأولي (First Time Setup):

```bash
# 1. فحص الوضع الحالي
psql -U postgres -d your_database -f check-permissions.sql

# 2. إضافة جميع الصلاحيات
psql -U postgres -d your_database -f seed-permissions.sql

# 3. إنشاء الأدوار الافتراضية
psql -U postgres -d your_database -f seed-default-roles.sql

# 4. (اختياري) إنشاء مستخدم Admin للاختبار
# تأكد من تعديل password_hash أولاً!
psql -U postgres -d your_database -f create-admin-user.sql
```

### للتحديث (Update):

```bash
# إذا أضفت صلاحيات جديدة في permission_constants.dart
psql -U postgres -d your_database -f seed-permissions.sql

# إذا أردت إعادة تعيين صلاحيات الأدوار
psql -U postgres -d your_database -f seed-default-roles.sql
```

---

## ⚠️ تحذيرات مهمة

### 1. كلمة مرور Admin
- Script `create-admin-user.sql` يحتاج تعديل password_hash
- استخدم PowerShell script لتوليد hash صحيح:
  ```powershell
  # في MyAPIv3/Scripts/create-admin.ps1
  dotnet run --project ../MyAPIv3.csproj hash-password admin123
  ```

### 2. البيئة الإنتاجية
- **لا تستخدم** مستخدم admin بكلمة مرور افتراضية في الإنتاج
- غيّر كلمة المرور فوراً بعد أول تسجيل دخول
- احذف أو عطّل المستخدمين الافتراضيين في الإنتاج

### 3. النسخ الاحتياطي
- **دائماً** قم بعمل backup لقاعدة البيانات قبل تشغيل أي scripts
- خاصة قبل تشغيل `seed-default-roles.sql` لأنه قد يعيد تعيين الصلاحيات

---

## 🔍 التحقق من النجاح

بعد تشغيل جميع Scripts، يمكنك التحقق من النجاح:

```sql
-- عدد الصلاحيات (يجب أن يكون 46)
SELECT COUNT(*) FROM permissions;

-- عدد الأدوار (يجب أن يكون 5)
SELECT COUNT(*) FROM roles;

-- صلاحيات كل دور
SELECT 
    r.role_name,
    COUNT(rp.permission_id) AS permission_count
FROM roles r
LEFT JOIN role_permissions rp ON r.id = rp.role_id
GROUP BY r.role_name
ORDER BY r.role_name;

-- المستخدمين وأدوارهم
SELECT 
    u.username,
    r.role_name
FROM users u
JOIN user_roles ur ON u.id = ur.user_id
JOIN roles r ON ur.role_id = r.id;
```

---

## 📝 ملاحظات إضافية

### تطابق الصلاحيات
تأكد من تطابق أسماء الصلاحيات في:
- ✅ `seed-permissions.sql` (قاعدة البيانات)
- ✅ `lib/utils/permission_constants.dart` (Flutter)
- ✅ `Controllers/*.cs` (ASP.NET Core API)

### إضافة صلاحيات جديدة
عند إضافة صلاحية جديدة:
1. أضفها في `permission_constants.dart`
2. أضفها في `seed-permissions.sql`
3. أضفها للأدوار المناسبة في `seed-default-roles.sql`
4. استخدمها في Controllers مع `[RequirePermission("permission_name")]`

---

## 🆘 استكشاف الأخطاء

### خطأ: "relation does not exist"
- تأكد من أن جداول Permissions, Roles, RolePermissions موجودة
- قم بتشغيل migrations أولاً

### خطأ: "duplicate key value"
- هذا طبيعي إذا كانت البيانات موجودة مسبقاً
- Scripts تستخدم `ON CONFLICT DO NOTHING` لتجنب هذا

### خطأ: "Admin role not found"
- تأكد من تشغيل `seed-default-roles.sql` قبل `create-admin-user.sql`

---

## 📞 الدعم

إذا واجهت أي مشاكل، تحقق من:
1. Logs في Console
2. تشغيل `check-permissions.sql` لفحص الوضع الحالي
3. التأكد من تشغيل Scripts بالترتيب الصحيح
