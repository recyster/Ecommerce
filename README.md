# ECommerce Case

Bu proje, **.NET 8, Docker, RabbitMQ, Redis, SQL Server ve Seq** kullanılarak geliştirilmiş basit bir e-ticaret mikroservis örneğidir.

## Proje Yapısı
```
ECommerceSolution/
  ECommerce.Api/         --> API projesi
  ECommerce.Worker/      --> Kuyruktan işleyen worker servisi
  ECommerce.Core/        --> Domain katmanı
  ECommerce.Infrastructure/ --> DbContext ve Migration’lar
  ECommerce.Shared/      --> Ortak sınıflar
```

---

## Kurulum Adımları

### 1. Projeyi Aç
Visual Studio içinde `ECommerce.Api` projesini **Startup Project** olarak seç.

### 2. Migration ve Veritabanı
Migration komutlarını çalıştır:
```powershell
# Migration hali hazırda ben oluşturduğum için oluştururken kullandığım komutu koydum. O yüzden direkt olarak bir aşağıdaki komutu uygulayabilirsiniz.
dotnet ef migrations add InitialCreate --project ..\ECommerce.Infrastructure --startup-project . --output-dir Migrations
dotnet ef database update --project ..\ECommerce.Infrastructure --startup-project .
```

### 3. Docker Compose ile Servisleri Ayağa Kaldır
Proje klasöründen:*Docker Compose Dosyası Proje İçerisinde Mevcut*
```powershell
docker compose up -d
```
Çalışması gereken servisler:
- **SQL Server** -> `localhost,1433`
- **Redis** -> `localhost:6379`
- **RabbitMQ** -> `http://localhost:15672` (guest/guest)
- **Seq** -> `http://localhost:5341` (authentication kapalı)

### 4. Loglama
Program.cs içinde log yapılandırması:
```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/worker-log-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Seq(builder.Configuration["Seq:Url"] ?? "http://localhost:5341")
    .MinimumLevel.Information()
    .CreateLogger();
```

`appsettings.json` içine:
```json
"Seq": {
  "Url": "http://localhost:5341"
}
```

### 5. API ve Worker
API’yi çalıştır:
```powershell
cd ECommerce.Api
dotnet run
```
Swagger: `http://localhost:{port}/swagger`

Worker’ı çalıştır: VS de çoklu star yapılabilir.
```powershell
cd ECommerce.Worker
dotnet run
```

---

## Postman Kullanımı
`ECommerce.postman_collection.json` dosyasını Postman’e import et.

Adımlar:
1. **Auth - Login** → token al
2. Gelen token’ı Postman’de `token` değişkenine yapıştır
3. **Orders - Create** → sipariş oluştur
4. **Orders - Get By User** → ilgili kullanıcının siparişlerini listele

> Varsayılan `baseUrl = http://localhost:5000` ama Visual Studio farklı port açabilir.

---

## Faydalı Komutlar
Migration:
```powershell
dotnet ef migrations add <MigrationName> --project ..\ECommerce.Infrastructure --startup-project . --output-dir Migrations
dotnet ef database update --project ..\ECommerce.Infrastructure --startup-project .
```

Docker:
```powershell
docker compose up -d
docker compose down
docker ps
docker logs <container_name>
```

---

## Kontrol Listesi
- [x] Migration eklendi ve veritabanı güncellendi
- [x] Docker Compose çalışıyor (SQL Server, Redis, RabbitMQ, Seq)
- [x] API ayağa kalkıyor
- [x] Postman ile test edilebiliyor
- [x] Seq log takibi aktif
