# تعليمات إعداد وتشغيل نظام إدارة البحوث العلمية

## 📋 المتطلبات الأساسية

### 1. تثبيت .NET SDK
```bash
# تحميل وتثبيت .NET 9.0 SDK من الموقع الرسمي
# https://dotnet.microsoft.com/download/dotnet/9.0

# للتحقق من التثبيت
dotnet --version
```

### 2. تثبيت SQL Server
```bash
# تثبيت SQL Server Express (مجاني)
# أو استخدام SQL Server LocalDB
# أو استخدام SQL Server في Docker:

docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
   -p 1433:1433 --name sqlserver \
   -d mcr.microsoft.com/mssql/server:2022-latest
```

### 3. أدوات التطوير (اختياري)
- Visual Studio 2022 Community (مجاني)
- أو Visual Studio Code مع C# Extension
- أو JetBrains Rider

## 🚀 خطوات الإعداد

### 1. استنساخ المشروع
```bash
git clone https://github.com/Mohamed-sabah/ResearchManagement25.git
cd ResearchManagement25
```

### 2. تكوين قاعدة البيانات

#### أ. تحديث Connection String
افتح ملف `src/ResearchManagement.Web/appsettings.json` وحدث:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=ResearchManagementDB;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

#### ب. تطبيق Migrations
```bash
# تثبيت أدوات Entity Framework
dotnet tool install --global dotnet-ef

# تطبيق migrations لإنشاء قاعدة البيانات
dotnet ef database update --project src/ResearchManagement.Infrastructure --startup-project src/ResearchManagement.Web
```

### 3. تكوين الإعدادات الإضافية

#### أ. إعدادات البريد الإلكتروني
في `appsettings.json`:
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderPassword": "your-app-password",
    "EnableSsl": true
  }
}
```

#### ب. إعدادات رفع الملفات
```json
{
  "FileUploadSettings": {
    "MaxFileSize": 10485760,
    "AllowedExtensions": [".pdf", ".doc", ".docx", ".ppt", ".pptx"],
    "UploadPath": "wwwroot/uploads"
  }
}
```

### 4. تشغيل المشروع

#### أ. من سطر الأوامر
```bash
cd src/ResearchManagement.Web
dotnet run
```

#### ب. من Visual Studio
1. افتح ملف `ResearchManagement.sln`
2. اختر `ResearchManagement.Web` كمشروع البدء
3. اضغط F5 أو Ctrl+F5

### 5. الوصول للتطبيق
- افتح المتصفح على: `https://localhost:5001`
- أو: `http://localhost:5000`

## 👤 بيانات الدخول الافتراضية

### المدير العام
- **البريد الإلكتروني:** admin@research.com
- **كلمة المرور:** Admin123!

### باحث تجريبي
- **البريد الإلكتروني:** researcher@research.com
- **كلمة المرور:** Researcher123!

### مراجع تجريبي
- **البريد الإلكتروني:** reviewer@research.com
- **كلمة المرور:** Reviewer123!

## 🔧 استكشاف الأخطاء

### مشكلة الاتصال بقاعدة البيانات
```bash
# تحقق من تشغيل SQL Server
# تحقق من صحة Connection String
# تأكد من وجود قاعدة البيانات

# إعادة تطبيق migrations
dotnet ef database drop --project src/ResearchManagement.Infrastructure --startup-project src/ResearchManagement.Web
dotnet ef database update --project src/ResearchManagement.Infrastructure --startup-project src/ResearchManagement.Web
```

### مشكلة في الحزم
```bash
# استعادة الحزم
dotnet restore

# تنظيف وإعادة البناء
dotnet clean
dotnet build
```

### مشكلة في الملفات الثابتة
```bash
# تأكد من وجود مجلد uploads
mkdir -p src/ResearchManagement.Web/wwwroot/uploads

# تعيين الصلاحيات (Linux/Mac)
chmod 755 src/ResearchManagement.Web/wwwroot/uploads
```

## 🐳 تشغيل باستخدام Docker

### 1. إنشاء Docker Compose
إنشاء ملف `docker-compose.yml`:
```yaml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "YourStrong@Passw0rd"
      ACCEPT_EULA: "Y"
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql

  web:
    build: .
    ports:
      - "5000:80"
      - "5001:443"
    depends_on:
      - sqlserver
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=ResearchManagementDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;

volumes:
  sqlserver_data:
```

### 2. إنشاء Dockerfile
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/ResearchManagement.Web/ResearchManagement.Web.csproj", "src/ResearchManagement.Web/"]
COPY ["src/ResearchManagement.Application/ResearchManagement.Application.csproj", "src/ResearchManagement.Application/"]
COPY ["src/ResearchManagement.Domain/ResearchManagement.Domain.csproj", "src/ResearchManagement.Domain/"]
COPY ["src/ResearchManagement.Infrastructure/ResearchManagement.Infrastructure.csproj", "src/ResearchManagement.Infrastructure/"]
RUN dotnet restore "src/ResearchManagement.Web/ResearchManagement.Web.csproj"
COPY . .
WORKDIR "/src/src/ResearchManagement.Web"
RUN dotnet build "ResearchManagement.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ResearchManagement.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ResearchManagement.Web.dll"]
```

### 3. تشغيل Docker
```bash
docker-compose up -d
```

## 🧪 تشغيل الاختبارات

### Unit Tests
```bash
dotnet test tests/ResearchManagement.UnitTests
```

### Integration Tests
```bash
dotnet test tests/ResearchManagement.IntegrationTests
```

### جميع الاختبارات
```bash
dotnet test
```

## 📊 مراقبة الأداء

### تفعيل Logging
في `appsettings.json`:
```json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/app-.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

### Health Checks
الوصول إلى: `https://localhost:5001/health`

## 🔒 إعدادات الأمان للإنتاج

### 1. تحديث كلمات المرور
```bash
# تغيير كلمات المرور الافتراضية
# استخدام كلمات مرور قوية
# تفعيل Two-Factor Authentication
```

### 2. تكوين HTTPS
```bash
# تثبيت شهادة SSL صالحة
# تفعيل HSTS
# تكوين HTTPS Redirection
```

### 3. تكوين قاعدة البيانات
```bash
# استخدام مستخدم قاعدة بيانات محدود الصلاحيات
# تفعيل تشفير قاعدة البيانات
# إعداد النسخ الاحتياطية
```

## 📞 الدعم والمساعدة

### المشاكل الشائعة
- [قائمة المشاكل الشائعة](docs/troubleshooting.md)
- [الأسئلة المتكررة](docs/faq.md)

### التواصل
- **GitHub Issues:** [رابط المشاكل](https://github.com/Mohamed-sabah/ResearchManagement25/issues)
- **البريد الإلكتروني:** mohamed.sabah@example.com

---

**ملاحظة:** تأكد من تحديث جميع كلمات المرور والإعدادات الافتراضية قبل النشر في بيئة الإنتاج.