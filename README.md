## Database

Project sử dụng SQL Server + Entity Framework Core Migration.

Khi chạy project, `Program.cs` gọi:

```csharp
db.Database.Migrate();