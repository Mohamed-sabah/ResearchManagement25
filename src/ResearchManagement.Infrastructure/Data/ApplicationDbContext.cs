using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ResearchManagement.Domain.Entities;
using ResearchManagement.Infrastructure.Data.Configurations;



namespace ResearchManagement.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Research> Researches { get; set; }
        public DbSet<ResearchAuthor> ResearchAuthors { get; set; }
        public DbSet<ResearchFile> ResearchFiles { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<TrackManager> TrackManagers { get; set; }
        public DbSet<TrackReviewer> TrackReviewers { get; set; }
        public DbSet<ResearchStatusHistory> ResearchStatusHistories { get; set; }
        public DbSet<EmailNotification> EmailNotifications { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ========================================
            // User Configuration
            // ========================================
            builder.Entity<User>(entity =>
            {
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FirstNameEn).HasMaxLength(100);
                entity.Property(e => e.LastNameEn).HasMaxLength(100);
                entity.Property(e => e.Institution).HasMaxLength(200);
                entity.Property(e => e.AcademicDegree).HasMaxLength(100);
                entity.Property(e => e.OrcidId).HasMaxLength(50);
                entity.Property(e => e.Role).HasConversion<int>();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Role);
            });

            // ========================================
            // Research Configuration
            // ========================================
            builder.Entity<Research>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Title).IsRequired().HasMaxLength(500);
                entity.Property(r => r.TitleEn).HasMaxLength(500);
                entity.Property(r => r.AbstractAr).IsRequired().HasMaxLength(2000);
                entity.Property(r => r.AbstractEn).HasMaxLength(2000);
                entity.Property(r => r.Keywords).HasMaxLength(500);
                entity.Property(r => r.KeywordsEn).HasMaxLength(500);
                entity.Property(r => r.Methodology).HasMaxLength(200);
                entity.Property(r => r.RejectionReason).HasMaxLength(1000);
                entity.Property(r => r.SubmittedById).IsRequired();

                // العلاقات - حل مشكلة CASCADE
                entity.HasOne(r => r.SubmittedBy)
                    .WithMany(u => u.Researches)
                    .HasForeignKey(r => r.SubmittedById)
                    .OnDelete(DeleteBehavior.Restrict); // منع CASCADE

                entity.HasOne(r => r.AssignedTrackManager)
                    .WithMany(tm => tm.ManagedResearches)
                    .HasForeignKey(r => r.AssignedTrackManagerId)
                    .OnDelete(DeleteBehavior.SetNull);

                // تحويل الـ Enums
                entity.Property(r => r.ResearchType).HasConversion<int>();
                entity.Property(r => r.Language).HasConversion<int>();
                entity.Property(r => r.Status).HasConversion<int>();
                entity.Property(r => r.Track).HasConversion<int>();

                // الفهارس
                entity.HasIndex(r => r.SubmittedById);
                entity.HasIndex(r => r.Status);
                entity.HasIndex(r => r.Track);
                entity.HasIndex(r => r.SubmissionDate);
            });

            // ========================================
            // ResearchAuthor Configuration
            // ========================================
            builder.Entity<ResearchAuthor>(entity =>
            {
                entity.HasKey(ra => ra.Id);
                entity.Property(ra => ra.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(ra => ra.LastName).IsRequired().HasMaxLength(100);
                entity.Property(ra => ra.FirstNameEn).HasMaxLength(100);
                entity.Property(ra => ra.LastNameEn).HasMaxLength(100);
                entity.Property(ra => ra.Email).IsRequired().HasMaxLength(200);
                entity.Property(ra => ra.Institution).HasMaxLength(200);
                entity.Property(ra => ra.AcademicDegree).HasMaxLength(100);
                entity.Property(ra => ra.OrcidId).HasMaxLength(50);

                entity.HasOne(ra => ra.Research)
                    .WithMany(r => r.Authors)
                    .HasForeignKey(ra => ra.ResearchId)
                    .OnDelete(DeleteBehavior.Cascade); // آمن

                entity.HasOne(ra => ra.User)
                    .WithMany(u => u.AuthoredResearches)
                    .HasForeignKey(ra => ra.UserId)
                    .OnDelete(DeleteBehavior.SetNull); // منع CASCADE

                entity.HasIndex(ra => new { ra.ResearchId, ra.Order }).IsUnique();
            });

            // ========================================
            // Review Configuration
            // ========================================
            builder.Entity<Review>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.CommentsToAuthor).HasMaxLength(2000);
                entity.Property(r => r.CommentsToTrackManager).HasMaxLength(2000);
                entity.Property(r => r.Decision).HasConversion<int>();

                entity.HasOne(r => r.Research)
                    .WithMany(res => res.Reviews)
                    .HasForeignKey(r => r.ResearchId)
                    .OnDelete(DeleteBehavior.Cascade); // آمن

                entity.HasOne(r => r.Reviewer)
                    .WithMany(u => u.Reviews)
                    .HasForeignKey(r => r.ReviewerId)
                    .OnDelete(DeleteBehavior.Restrict); // منع CASCADE

                entity.HasIndex(r => r.ResearchId);
                entity.HasIndex(r => r.ReviewerId);
                entity.HasIndex(r => r.IsCompleted);
            });

            // ========================================
            // ResearchFile Configuration - المشكلة هنا!
            // ========================================
            builder.Entity<ResearchFile>(entity =>
            {
                entity.HasKey(rf => rf.Id);
                entity.Property(rf => rf.FileName).IsRequired().HasMaxLength(255);
                entity.Property(rf => rf.OriginalFileName).IsRequired().HasMaxLength(255);
                entity.Property(rf => rf.FilePath).IsRequired().HasMaxLength(500);
                entity.Property(rf => rf.ContentType).IsRequired().HasMaxLength(50);
                entity.Property(rf => rf.Description).HasMaxLength(500);
                entity.Property(rf => rf.FileType).HasConversion<int>();

                // العلاقة الرئيسية - Research
                entity.HasOne(rf => rf.Research)
                    .WithMany(r => r.Files)
                    .HasForeignKey(rf => rf.ResearchId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired(true); // المسار الرئيسي

                // العلاقة الثانوية - Review - هنا الحل!
                entity.HasOne(rf => rf.Review)
                    .WithMany(r => r.ReviewFiles)
                    .HasForeignKey(rf => rf.ReviewId)
                    .OnDelete(DeleteBehavior.SetNull) // بدلاً من Cascade
                    .IsRequired(false); // جعل ReviewId اختيارية

                entity.HasIndex(rf => rf.ResearchId);
                entity.HasIndex(rf => rf.ReviewId);
                entity.HasIndex(rf => rf.FileType);
            });

            // ========================================
            // TrackManager Configuration
            // ========================================
            builder.Entity<TrackManager>(entity =>
            {
                entity.HasKey(tm => tm.Id);
                entity.Property(tm => tm.TrackDescription).HasMaxLength(200);
                entity.Property(tm => tm.Track).HasConversion<int>();

                entity.HasOne(tm => tm.User)
                    .WithMany(u => u.ManagedTracks)
                    .HasForeignKey(tm => tm.UserId)
                    .OnDelete(DeleteBehavior.Restrict); // منع CASCADE

                entity.HasIndex(tm => tm.Track);
                entity.HasIndex(tm => tm.UserId);
            });

            // ========================================
            // TrackReviewer Configuration
            // ========================================
            builder.Entity<TrackReviewer>(entity =>
            {
                entity.HasKey(tr => tr.Id);
                entity.Property(tr => tr.Track).HasConversion<int>();

                entity.HasOne(tr => tr.TrackManager)
                    .WithMany(tm => tm.TrackReviewers)
                    .HasForeignKey(tr => tr.TrackManagerId)
                    .OnDelete(DeleteBehavior.Cascade); // آمن

                entity.HasOne(tr => tr.Reviewer)
                    .WithMany()
                    .HasForeignKey(tr => tr.ReviewerId)
                    .OnDelete(DeleteBehavior.Restrict); // منع CASCADE

                entity.HasIndex(tr => new { tr.TrackManagerId, tr.ReviewerId, tr.Track }).IsUnique();
            });

            // ========================================
            // ResearchStatusHistory Configuration
            // ========================================
            builder.Entity<ResearchStatusHistory>(entity =>
            {
                entity.HasKey(sh => sh.Id);
                entity.Property(sh => sh.Notes).HasMaxLength(500);
                entity.Property(sh => sh.FromStatus).HasConversion<int>();
                entity.Property(sh => sh.ToStatus).HasConversion<int>();

                entity.HasOne(sh => sh.Research)
                    .WithMany(r => r.StatusHistory)
                    .HasForeignKey(sh => sh.ResearchId)
                    .OnDelete(DeleteBehavior.Cascade); // آمن

                entity.HasOne(sh => sh.ChangedBy)
                    .WithMany()
                    .HasForeignKey(sh => sh.ChangedById)
                    .OnDelete(DeleteBehavior.Restrict); // منع CASCADE

                entity.HasIndex(sh => sh.ResearchId);
                entity.HasIndex(sh => sh.ChangedAt);
            });

            // ========================================
            // Global Query Filters للـ Soft Delete
            // ========================================
            builder.Entity<Research>().HasQueryFilter(r => !r.IsDeleted);
            builder.Entity<ResearchAuthor>().HasQueryFilter(ra => !ra.IsDeleted);
            builder.Entity<ResearchFile>().HasQueryFilter(rf => !rf.IsDeleted);
            builder.Entity<Review>().HasQueryFilter(r => !r.IsDeleted);
            builder.Entity<TrackManager>().HasQueryFilter(tm => !tm.IsDeleted);
            builder.Entity<TrackReviewer>().HasQueryFilter(tr => !tr.IsDeleted);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // تحديث تواريخ الإنشاء والتعديل تلقائياً
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }

            // تحديث User منفصلاً
            foreach (var entry in ChangeTracker.Entries<User>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}