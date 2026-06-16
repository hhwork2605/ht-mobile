# Cài đặt môi trường dev

## Yêu cầu
- .NET SDK 9.x · Node 20+ · Docker (cho Redis) · 1 project Supabase (Postgres).

## 1. Redis (Docker)
```powershell
docker compose up -d        # khởi động redis:7 (+ pgadmin nếu bật)
```
> Không có Docker/Redis? Đặt `ConnectionStrings:Redis = "memory"` (hoặc bỏ trống) để chạy **cache in-memory**
> — `appsettings.Development.json` đã set sẵn `"memory"` nên dev chạy được ngay không cần Redis. Production vẫn dùng Redis thật.

## 2. Connection string (KHÔNG commit — dùng user-secrets)
Lấy connection từ Supabase → Project Settings → Database.

- **Direct** (cho migrations): host `db.<ref>.supabase.co`, port `5432`.
- **Transaction pooler** (cho runtime): host `aws-0-...pooler.supabase.com`, port `6543`.

```powershell
cd src/HtMobile.Web
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Default"    "Host=...pooler...;Port=6543;Database=postgres;Username=postgres.<ref>;Password=<PW>;SSL Mode=Require;Trust Server Certificate=true"
dotnet user-secrets set "ConnectionStrings:Migrations" "Host=db.<ref>.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=<PW>;SSL Mode=Require;Trust Server Certificate=true"
dotnet user-secrets set "ConnectionStrings:Redis"      "localhost:6379"
```

> Khi dùng pooler (pgBouncer transaction mode), Npgsql nên bật `No Reset On Close=true` và tránh prepared
> statements (`Max Auto Prepare=0`). Migrations luôn chạy trên connection **Direct** (`Migrations`).

## 3. Migrate + seed
```powershell
scripts\migrate.ps1 update     # áp schema lên Supabase
scripts\seed.ps1               # seed vùng/danh mục/SP mẫu + tài khoản admin
```

## 4. Chạy
```powershell
scripts\dev.ps1                # Web + tailwind --watch
# hoặc:
dotnet run --project src/HtMobile.Web
```
Mở `https://localhost:5001` → `/`, `/iphone`, PDP. Admin: `/admin` (tài khoản seed trong `docs/setup.md`/seeder).

## Biến môi trường khi deploy
Đặt các key `ConnectionStrings__Default`, `ConnectionStrings__Redis`, `ASPNETCORE_ENVIRONMENT=Production`.
