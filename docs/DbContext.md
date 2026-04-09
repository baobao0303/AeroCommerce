# DbContext Class trong CodePulse

## Tổng quan

```
Controller  ←──────→  DbContext  ←──────→  Database
  (API)               (EF Core)           (Supabase/PostgreSQL)
```

`DbContext` là lớp trung gian cốt lõi của **Entity Framework Core**, đóng vai trò là cầu nối giữa tầng ứng dụng (.NET) và cơ sở dữ liệu (PostgreSQL / Supabase).

---

## Cách hoạt động

```
HTTP Request
    │
    ▼
┌──────────────┐      ┌─────────────────┐      ┌──────────────┐
│  Controller  │ ───▶ │   DbContext     │ ───▶ │   Database   │
│  (API Layer) │      │  (EF Core ORM)  │      │  (Supabase   │
│              │ ◀─── │                 │ ◀─── │  PostgreSQL) │
└──────────────┘      └─────────────────┘      └──────────────┘
```

| Tầng | Vai trò |
|------|---------|
| **Controller** | Nhận HTTP request, gọi Service/DbContext, trả về response |
| **DbContext** | Quản lý entities, dịch LINQ → SQL, theo dõi thay đổi (Change Tracking) |
| **Database** | Lưu trữ dữ liệu thực tế (PostgreSQL trên Supabase) |

---

## DbContext trong CodePulse

Dự án có **2 DbContext** riêng biệt, mỗi service độc lập một database:

### 1. `UserDbContext` — UserService (port 5001)

```csharp
// src/CodePulse.UserService/Data/UserDbContext.cs

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

    // DbSet = ánh xạ đến bảng "Users" trong database
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();    // Email phải unique
            entity.HasIndex(u => u.Username).IsUnique(); // Username phải unique
        });
    }
}
```

**Database:** `codepulse_users`
**Bảng quản lý:** `Users`

---

### 2. `PostDbContext` — PostService (port 5002)

```csharp
// src/CodePulse.PostService/Data/PostDbContext.cs

public class PostDbContext : DbContext
{
    public PostDbContext(DbContextOptions<PostDbContext> options) : base(options) { }

    public DbSet<Post> Posts { get; set; }
    public DbSet<Tag>  Tags  { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.Slug).IsUnique(); // Slug phải unique
            entity.Property(p => p.Status).HasConversion<string>();

            // Tags là Owned Collection (lưu chung bảng với Post)
            entity.OwnsMany(p => p.Tags, tag =>
            {
                tag.WithOwner().HasForeignKey("PostId");
                tag.Property(t => t.Name).HasMaxLength(50).IsRequired();
            });
        });
    }
}
```

**Database:** `codepulse_posts`
**Bảng quản lý:** `Posts`, `Tags`

---

## Đăng ký DbContext vào DI Container

```csharp
// Program.cs — đọc connection string từ biến môi trường (Supabase)
var connectionString =
    Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.EnableRetryOnFailure(maxRetryCount: 3); // tự retry nếu mất kết nối
    }));
```

> **Lưu ý:** `DATABASE_URL` là connection string từ **Supabase Dashboard**
> → Settings → Database → URI

---

## Các thao tác CRUD qua DbContext

```csharp
// CREATE
var user = User.Create(username, email, passwordHash);
_db.Users.Add(user);
await _db.SaveChangesAsync();

// READ
var user = await _db.Users.FindAsync(id);
var users = await _db.Users.Where(u => u.IsActive).ToListAsync();

// UPDATE
user.ChangeRole("Admin");
await _db.SaveChangesAsync(); // Change Tracking tự detect thay đổi

// DELETE
_db.Users.Remove(user);
await _db.SaveChangesAsync();
```

---

## Change Tracking (Theo dõi thay đổi)

```
Entity được load từ DB
        │
        ▼
DbContext theo dõi trạng thái:
  ┌─────────────┬───────────────────────────────┐
  │ Unchanged   │ Chưa thay đổi gì              │
  │ Modified    │ Đã thay đổi property          │
  │ Added       │ Mới tạo, chưa lưu vào DB      │
  │ Deleted     │ Đã đánh dấu xóa               │
  └─────────────┴───────────────────────────────┘
        │
        ▼ SaveChangesAsync()
   EF Core sinh SQL tương ứng → gửi lên Supabase
```

---

## Connection String — Supabase

### Local Development
```
Host=localhost;Port=5432;Database=codepulse_users;Username=postgres;Password=postgres
```

### Supabase Production
```
postgresql://postgres:[PASSWORD]@db.[PROJECT-REF].supabase.co:5432/postgres?sslmode=require
```

> Lấy từ: **Supabase Dashboard** → Project → Settings → Database → Connection String (URI)

---

## Health Check

Mỗi service expose endpoint `/health` để kiểm tra kết nối database:

```bash
# Kiểm tra UserService có kết nối được Supabase không
curl http://localhost:5001/health

# Response OK
{ "status": "Healthy" }

# Response lỗi DB
{ "status": "Unhealthy" }
```

---

## Sơ đồ kiến trúc đầy đủ

```
                         API Gateway :5000 (Ocelot)
                         /api/users/*  →  UserService
                         /api/posts/*  →  PostService
                                │
              ┌─────────────────┴──────────────────┐
              │                                    │
   UserService :5001                    PostService :5002
   ┌─────────────────────┐         ┌─────────────────────┐
   │  AuthController     │         │  PostsController    │
   │       ↕             │         │       ↕             │
   │  AuthService        │         │  PostService        │
   │       ↕             │         │       ↕             │
   │  UserDbContext      │         │  PostDbContext       │
   └────────┬────────────┘         └────────┬────────────┘
            │                               │
            ▼                               ▼
   ┌─────────────────────────────────────────────────────┐
   │                  SUPABASE                           │
   │   codepulse_users (DB)  │  codepulse_posts (DB)    │
   │   └── Users table       │  └── Posts table         │
   │                         │      └── Tags table      │
   └─────────────────────────────────────────────────────┘
```

---

## Tài liệu tham khảo

- [EF Core DbContext — Microsoft Docs](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/)
- [Npgsql — PostgreSQL provider for EF Core](https://www.npgsql.org/efcore/)
- [Supabase — Getting connection string](https://supabase.com/docs/guides/database/connecting-to-postgres)
