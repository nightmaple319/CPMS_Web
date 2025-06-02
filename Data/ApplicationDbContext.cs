using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CPMS_Web.Models;

namespace CPMS_Web.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for your models
        public DbSet<SparePart> SpareParts { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<MaterialRequest> MaterialRequests { get; set; }
        public DbSet<MaterialRequestDetail> MaterialRequestDetails { get; set; }
        public DbSet<StockCount> StockCounts { get; set; }
        public DbSet<StockCountDetail> StockCountDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure SparePart
            modelBuilder.Entity<SparePart>(entity =>
            {
                entity.HasKey(e => e.No);
                entity.Property(e => e.No).ValueGeneratedOnAdd();
                entity.Property(e => e.PlantId).HasColumnName("plant_id");
                entity.Property(e => e.PositionId).HasColumnName("position_id");
                entity.Property(e => e.SubPositionId).HasColumnName("subposition_id");
                entity.Property(e => e.Category).HasColumnName("category");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.Specification).HasColumnName("specification");
                entity.Property(e => e.Quantity).HasColumnName("quantity");
                entity.Property(e => e.LastUpdated).HasColumnName("last_updated");
                entity.Property(e => e.Remarks).HasColumnName("remarks");
            });

            // Configure UserProfile
            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure InventoryTransaction
            modelBuilder.Entity<InventoryTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasOne(e => e.SparePart)
                      .WithMany(s => s.InventoryTransactions)
                      .HasForeignKey(e => e.SparePartNo)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure MaterialRequest
            modelBuilder.Entity<MaterialRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasOne(e => e.Requester)
                      .WithMany()
                      .HasForeignKey(e => e.RequesterId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Approver)
                      .WithMany()
                      .HasForeignKey(e => e.ApproverId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure MaterialRequestDetail
            modelBuilder.Entity<MaterialRequestDetail>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasOne(e => e.MaterialRequest)
                      .WithMany(mr => mr.MaterialRequestDetails)
                      .HasForeignKey(e => e.RequestId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.SparePart)
                      .WithMany(s => s.MaterialRequestDetails)
                      .HasForeignKey(e => e.SparePartNo)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure StockCount
            modelBuilder.Entity<StockCount>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasOne(e => e.Counter)
                      .WithMany()
                      .HasForeignKey(e => e.CounterId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure StockCountDetail
            modelBuilder.Entity<StockCountDetail>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasOne(e => e.StockCount)
                      .WithMany(sc => sc.StockCountDetails)
                      .HasForeignKey(e => e.CountId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.SparePart)
                      .WithMany(s => s.StockCountDetails)
                      .HasForeignKey(e => e.SparePartNo)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.Ignore(e => e.Difference); // 計算欄位不存入資料庫
            });

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // 這裡可以加入初始資料
            // 例如預設的使用者角色等
        }
    }
}