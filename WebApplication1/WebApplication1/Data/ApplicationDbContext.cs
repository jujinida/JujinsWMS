using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Employee> Employees { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<VacationRequest> VacationRequests { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductLocation> ProductLocations { get; set; }
        public DbSet<InventoryLog> InventoryLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Employee와 관련 엔티티들 간의 관계 설정
            modelBuilder.Entity<Employee>()
                .HasMany(e => e.AttendanceRecords)
                .WithOne(ar => ar.Employee)
                .HasForeignKey(ar => ar.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.VacationRequests)
                .WithOne(vr => vr.Employee)
                .HasForeignKey(vr => vr.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.Payrolls)
                .WithOne(p => p.Employee)
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product와 관련 엔티티들 간의 관계 설정
            modelBuilder.Entity<Product>()
                .HasMany(p => p.ProductLocations)
                .WithOne(pl => pl.Product)
                .HasForeignKey(pl => pl.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.InventoryLogs)
                .WithOne(il => il.Product)
                .HasForeignKey(il => il.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ProductLocation 복합 키 설정
            modelBuilder.Entity<ProductLocation>()
                .HasKey(pl => new { pl.ProductId, pl.LocationId });

            // Employee 필드 설정
            modelBuilder.Entity<Employee>()
                .Property(e => e.RemainingVacationDays)
                .HasDefaultValue(0m)
                .ValueGeneratedOnAddOrUpdate();

            modelBuilder.Entity<Employee>()
                .Property(e => e.TotalVacationDays)
                .HasDefaultValue(0)
                .ValueGeneratedOnAddOrUpdate();

            // 인덱스 설정
            modelBuilder.Entity<AttendanceRecord>()
                .HasIndex(ar => ar.RecordDate);

            modelBuilder.Entity<InventoryLog>()
                .HasIndex(il => il.LogDate);
        }
    }
}
