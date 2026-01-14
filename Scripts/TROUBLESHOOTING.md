# دليل تشغيل Scripts قاعدة البيانات
# Database Scripts Execution Guide

## المشكلة
`psql` command غير موجود في PATH

## الحلول المتاحة

### الحل 1: إضافة PostgreSQL إلى PATH (موصى به)

1. ابحث عن مجلد تثبيت PostgreSQL (عادة):
   ```
   C:\Program Files\PostgreSQL\15\bin
   أو
   C:\Program Files\PostgreSQL\14\bin
   ```

2. أضف المجلد إلى PATH:
   - افتح System Properties > Environment Variables
   - في System Variables، اختر Path
   - اضغط Edit
   - اضغط New
   - الصق مسار bin folder
   - اضغط OK

3. أعد فتح PowerShell وجرب:
   ```powershell
   psql --version
   ```

---

### الحل 2: استخدام المسار الكامل

بدلاً من `psql`، استخدم المسار الكامل:

```powershell
# مثال (عدّل المسار حسب نسختك)
& "C:\Program Files\PostgreSQL\15\bin\psql.exe" --version
```

ثم شغّل Scripts:

```powershell
cd "C:\Users\hp\Desktop\IT\سنه رابعه .IT\دراسات مشروع التخرج\قاعدة البيانات\MyAPIv3\Scripts"

$env:PGPASSWORD = "admin"

# Step 1
& "C:\Program Files\PostgreSQL\15\bin\psql.exe" -h localhost -p 5432 -U postgres -d sass_inventory_db -f .\check-permissions.sql

# Step 2
& "C:\Program Files\PostgreSQL\15\bin\psql.exe" -h localhost -p 5432 -U postgres -d sass_inventory_db -f .\seed-permissions.sql

# Step 3
& "C:\Program Files\PostgreSQL\15\bin\psql.exe" -h localhost -p 5432 -U postgres -d sass_inventory_db -f .\seed-default-roles.sql

$env:PGPASSWORD = $null
```

---

### الحل 3: استخدام pgAdmin أو DBeaver

1. افتح pgAdmin أو DBeaver
2. اتصل بقاعدة البيانات `sass_inventory_db`
3. افتح Query Tool
4. انسخ محتوى كل ملف SQL وشغّله بالترتيب:
   - `check-permissions.sql`
   - `seed-permissions.sql`
   - `seed-default-roles.sql`

---

### الحل 4: استخدام API مباشرة

إذا كان API يعمل، يمكنك استخدام PowerShell script الموجود:

```powershell
cd "C:\Users\hp\Desktop\IT\سنه رابعه .IT\دراسات مشروع التخرج\قاعدة البيانات\MyAPIv3\Scripts"

# تأكد أن API يعمل على http://localhost:5011
.\create-admin.ps1
```

---

## الترتيب الصحيح

مهما كانت الطريقة، شغّل Scripts بهذا الترتيب:

1. ✅ `check-permissions.sql` - فحص الوضع الحالي (اختياري)
2. ✅ `seed-permissions.sql` - إضافة الصلاحيات (مطلوب)
3. ✅ `seed-default-roles.sql` - إنشاء الأدوار (مطلوب)
4. ⚠️ `create-admin-user.sql` - إنشاء مستخدم Admin (اختياري - يحتاج تعديل)

---

## التحقق من النجاح

بعد تشغيل Scripts، تحقق من النتائج:

```sql
-- عدد الصلاحيات (يجب أن يكون 46)
SELECT COUNT(*) FROM permissions;

-- عدد الأدوار (يجب أن يكون 5)
SELECT COUNT(*) FROM roles;

-- صلاحيات كل دور
SELECT 
    r.role_name,
    COUNT(rp.permission_id) as permission_count
FROM roles r
LEFT JOIN role_permissions rp ON r.id = rp.role_id
GROUP BY r.role_name
ORDER BY r.role_name;
```

النتائج المتوقعة:
- Admin: 46 permissions
- Manager: 32 permissions
- Cashier: 7 permissions
- Accountant: 11 permissions
- Inventory Manager: 14 permissions

---

## ماذا بعد؟

بعد نجاح Scripts:

1. ✅ شغّل API: `dotnet run`
2. ✅ شغّل `create-admin.ps1` لإنشاء مستخدم Admin
3. ✅ جرّب تسجيل الدخول من Flutter app
4. ✅ اختبر الصلاحيات

---

## المساعدة

إذا واجهت مشاكل:
1. تأكد من تشغيل PostgreSQL service
2. تأكد من صحة بيانات الاتصال في appsettings.json
3. تحقق من Logs في Console
