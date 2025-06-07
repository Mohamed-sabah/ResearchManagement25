using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ResearchManagement.Domain.Entities;

namespace ResearchManagement.Infrastructure.Data.Configurations
{
    public class ResearchConfiguration : IEntityTypeConfiguration<Research>
    {
        public void Configure(EntityTypeBuilder<Research> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Title)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(r => r.AbstractAr)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(r => r.SubmittedById)
                .IsRequired();

            // العلاقات مع حل مشكلة Cascade
            builder.HasOne(r => r.SubmittedBy)
                .WithMany(u => u.Researches)
                .HasForeignKey(r => r.SubmittedById)
                .OnDelete(DeleteBehavior.Restrict); // منع CASCADE

            builder.HasOne(r => r.AssignedTrackManager)
                .WithMany(tm => tm.ManagedResearches)
                .HasForeignKey(r => r.AssignedTrackManagerId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(r => r.Authors)
                .WithOne(ra => ra.Research)
                .HasForeignKey(ra => ra.ResearchId)
                .OnDelete(DeleteBehavior.Cascade); // آمن - لا يوجد مسار متعدد

            builder.HasMany(r => r.Files)
                .WithOne(rf => rf.Research)
                .HasForeignKey(rf => rf.ResearchId)
                .OnDelete(DeleteBehavior.Cascade); // آمن - هذا هو المسار الرئيسي

            builder.HasMany(r => r.Reviews)
                .WithOne(rv => rv.Research)
                .HasForeignKey(rv => rv.ResearchId)
                .OnDelete(DeleteBehavior.Cascade); // آمن

            builder.HasMany(r => r.StatusHistory)
                .WithOne(sh => sh.Research)
                .HasForeignKey(sh => sh.ResearchId)
                .OnDelete(DeleteBehavior.Cascade); // آمن

            // الفهارس
            builder.HasIndex(r => r.SubmittedById);
            builder.HasIndex(r => r.Status);
            builder.HasIndex(r => r.Track);
            builder.HasIndex(r => r.SubmissionDate);

            // تحويل الـ Enums
            builder.Property(r => r.ResearchType).HasConversion<int>();
            builder.Property(r => r.Language).HasConversion<int>();
            builder.Property(r => r.Status).HasConversion<int>();
            builder.Property(r => r.Track).HasConversion<int>();
        }
    }

    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.CommentsToAuthor)
                .HasMaxLength(2000);

            builder.Property(r => r.CommentsToTrackManager)
                .HasMaxLength(2000);

            builder.Property(r => r.Decision)
                .HasConversion<int>();

            // العلاقات
            builder.HasOne(r => r.Reviewer)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict); // منع CASCADE

            builder.HasOne(r => r.Research)
                .WithMany(res => res.Reviews)
                .HasForeignKey(r => r.ResearchId)
                .OnDelete(DeleteBehavior.Cascade); // آمن

            // علاقة ReviewFiles - هنا المشكلة!
            builder.HasMany(r => r.ReviewFiles)
                .WithOne(rf => rf.Review)
                .HasForeignKey(rf => rf.ReviewId)
                .OnDelete(DeleteBehavior.SetNull); // الحل: SetNull بدلاً من Cascade

            builder.HasIndex(r => r.ResearchId);
            builder.HasIndex(r => r.ReviewerId);
            builder.HasIndex(r => r.IsCompleted);
        }
    }

    public class ResearchFileConfiguration : IEntityTypeConfiguration<ResearchFile>
    {
        public void Configure(EntityTypeBuilder<ResearchFile> builder)
        {
            builder.HasKey(rf => rf.Id);

            builder.Property(rf => rf.FileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(rf => rf.OriginalFileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(rf => rf.FilePath)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(rf => rf.ContentType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(rf => rf.Description)
                .HasMaxLength(500);

            builder.Property(rf => rf.FileType)
                .HasConversion<int>();

            // العلاقات - الحل هنا
            builder.HasOne(rf => rf.Research)
                .WithMany(r => r.Files)
                .HasForeignKey(rf => rf.ResearchId)
                .OnDelete(DeleteBehavior.Cascade); // المسار الرئيسي

            builder.HasOne(rf => rf.Review)
                .WithMany(r => r.ReviewFiles)
                .HasForeignKey(rf => rf.ReviewId)
                .OnDelete(DeleteBehavior.SetNull); // حل المشكلة: SetNull

            builder.HasIndex(rf => rf.ResearchId);
            builder.HasIndex(rf => rf.ReviewId);
            builder.HasIndex(rf => rf.FileType);
        }
    }

    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.FirstNameEn)
                .HasMaxLength(100);

            builder.Property(u => u.LastNameEn)
                .HasMaxLength(100);

            builder.Property(u => u.Institution)
                .HasMaxLength(200);

            builder.Property(u => u.AcademicDegree)
                .HasMaxLength(100);

            builder.Property(u => u.OrcidId)
                .HasMaxLength(50);

            builder.Property(u => u.Role)
                .HasConversion<int>();

            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasIndex(u => u.Role);
        }
    }

    public class ResearchAuthorConfiguration : IEntityTypeConfiguration<ResearchAuthor>
    {
        public void Configure(EntityTypeBuilder<ResearchAuthor> builder)
        {
            builder.HasKey(ra => ra.Id);

            builder.Property(ra => ra.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(ra => ra.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(ra => ra.Email)
                .IsRequired()
                .HasMaxLength(200);

            // العلاقات
            builder.HasOne(ra => ra.Research)
                .WithMany(r => r.Authors)
                .HasForeignKey(ra => ra.ResearchId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ra => ra.User)
                .WithMany(u => u.AuthoredResearches)
                .HasForeignKey(ra => ra.UserId)
                .OnDelete(DeleteBehavior.SetNull); // منع مشكلة cascade

            builder.HasIndex(ra => new { ra.ResearchId, ra.Order }).IsUnique();
        }
    }

    public class TrackManagerConfiguration : IEntityTypeConfiguration<TrackManager>
    {
        public void Configure(EntityTypeBuilder<TrackManager> builder)
        {
            builder.HasKey(tm => tm.Id);

            builder.Property(tm => tm.TrackDescription)
                .HasMaxLength(200);

            builder.Property(tm => tm.Track)
                .HasConversion<int>();

            builder.HasOne(tm => tm.User)
                .WithMany(u => u.ManagedTracks)
                .HasForeignKey(tm => tm.UserId)
                .OnDelete(DeleteBehavior.Restrict); // منع cascade

            builder.HasIndex(tm => tm.Track);
            builder.HasIndex(tm => tm.UserId);
        }
    }

    public class TrackReviewerConfiguration : IEntityTypeConfiguration<TrackReviewer>
    {
        public void Configure(EntityTypeBuilder<TrackReviewer> builder)
        {
            builder.HasKey(tr => tr.Id);

            builder.Property(tr => tr.Track)
                .HasConversion<int>();

            builder.HasOne(tr => tr.TrackManager)
                .WithMany(tm => tm.TrackReviewers)
                .HasForeignKey(tr => tr.TrackManagerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(tr => tr.Reviewer)
                .WithMany()
                .HasForeignKey(tr => tr.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict); // منع cascade

            builder.HasIndex(tr => new { tr.TrackManagerId, tr.ReviewerId, tr.Track }).IsUnique();
        }
    }

    public class ResearchStatusHistoryConfiguration : IEntityTypeConfiguration<ResearchStatusHistory>
    {
        public void Configure(EntityTypeBuilder<ResearchStatusHistory> builder)
        {
            builder.HasKey(sh => sh.Id);

            builder.Property(sh => sh.Notes)
                .HasMaxLength(500);

            builder.Property(sh => sh.FromStatus)
                .HasConversion<int>();

            builder.Property(sh => sh.ToStatus)
                .HasConversion<int>();

            builder.HasOne(sh => sh.Research)
                .WithMany(r => r.StatusHistory)
                .HasForeignKey(sh => sh.ResearchId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(sh => sh.ChangedBy)
                .WithMany()
                .HasForeignKey(sh => sh.ChangedById)
                .OnDelete(DeleteBehavior.Restrict); // منع cascade

            builder.HasIndex(sh => sh.ResearchId);
            builder.HasIndex(sh => sh.ChangedAt);
        }
    }

    public class EmailNotificationConfiguration : IEntityTypeConfiguration<EmailNotification>
    {
        public void Configure(EntityTypeBuilder<EmailNotification> builder)
        {
            builder.HasKey(en => en.Id);

            builder.Property(en => en.ToEmail)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(en => en.Subject)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(en => en.Body)
                .IsRequired();

            builder.Property(en => en.ErrorMessage)
                .HasMaxLength(500);

            builder.Property(en => en.Type)
                .HasConversion<int>();

            builder.Property(en => en.Status)
                .HasConversion<int>();

            // العلاقات
            builder.HasOne(en => en.Research)
                .WithMany()
                .HasForeignKey(en => en.ResearchId)
                .OnDelete(DeleteBehavior.SetNull); // منع cascade

            builder.HasOne(en => en.User)
                .WithMany()
                .HasForeignKey(en => en.UserId)
                .OnDelete(DeleteBehavior.SetNull); // منع cascade

            builder.HasIndex(en => en.Status);
            builder.HasIndex(en => en.Type);
            builder.HasIndex(en => en.CreatedAt);
        }
    }
}

