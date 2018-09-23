using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AdefHelpDeskBase.Models.DataContext
{
    public partial class ADefHelpDeskContext : DbContext
    {
        public virtual DbSet<AdefHelpDeskApiSecurity> AdefHelpDeskApiSecurity { get; set; }
        public virtual DbSet<AdefHelpDeskAttachments> AdefHelpDeskAttachments { get; set; }
        public virtual DbSet<AdefHelpDeskCategories> AdefHelpDeskCategories { get; set; }
        public virtual DbSet<AdefHelpDeskLastSearch> AdefHelpDeskLastSearch { get; set; }
        public virtual DbSet<AdefHelpDeskLog> AdefHelpDeskLog { get; set; }
        public virtual DbSet<AdefHelpDeskRiausers> AdefHelpDeskRiausers { get; set; }
        public virtual DbSet<AdefHelpDeskRoles> AdefHelpDeskRoles { get; set; }
        public virtual DbSet<AdefHelpDeskSettings> AdefHelpDeskSettings { get; set; }
        public virtual DbSet<AdefHelpDeskSystemLog> AdefHelpDeskSystemLog { get; set; }
        public virtual DbSet<AdefHelpDeskTaskAssociations> AdefHelpDeskTaskAssociations { get; set; }
        public virtual DbSet<AdefHelpDeskTaskCategories> AdefHelpDeskTaskCategories { get; set; }
        public virtual DbSet<AdefHelpDeskTaskDetails> AdefHelpDeskTaskDetails { get; set; }
        public virtual DbSet<AdefHelpDeskTasks> AdefHelpDeskTasks { get; set; }
        public virtual DbSet<AdefHelpDeskUserRoles> AdefHelpDeskUserRoles { get; set; }
        public virtual DbSet<AdefHelpDeskUsers> AdefHelpDeskUsers { get; set; }
        public virtual DbSet<AdefHelpDeskVersion> AdefHelpDeskVersion { get; set; }
        public virtual DbSet<AspNetRoleClaims> AspNetRoleClaims { get; set; }
        public virtual DbSet<AspNetRoles> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaims> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogins> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUserRoles> AspNetUserRoles { get; set; }
        public virtual DbSet<AspNetUsers> AspNetUsers { get; set; }
        public virtual DbSet<AspNetUserTokens> AspNetUserTokens { get; set; }

        public ADefHelpDeskContext(DbContextOptions<ADefHelpDeskContext> options) :
        base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AdefHelpDeskApiSecurity>(entity =>
            {
                entity.ToTable("ADefHelpDesk_ApiSecurity");

                entity.Property(e => e.ContactCompany).HasMaxLength(500);

                entity.Property(e => e.ContactEmail).HasMaxLength(500);

                entity.Property(e => e.ContactName)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.ContactPhone).HasMaxLength(256);

                entity.Property(e => e.ContactWebsite).HasMaxLength(2000);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<AdefHelpDeskAttachments>(entity =>
            {
                entity.HasKey(e => e.AttachmentId);

                entity.ToTable("ADefHelpDesk_Attachments");

                entity.HasIndex(e => e.DetailId)
                    .HasName("IX_ADefHelpDesk_Attachments");

                entity.Property(e => e.AttachmentId).HasColumnName("AttachmentID");

                entity.Property(e => e.AttachmentPath)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(e => e.DetailId).HasColumnName("DetailID");

                entity.Property(e => e.FileName)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.OriginalFileName)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Detail)
                    .WithMany(p => p.AdefHelpDeskAttachments)
                    .HasForeignKey(d => d.DetailId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ADefHelpDesk_Attachments_ADefHelpDesk_TaskDetails");
            });

            modelBuilder.Entity<AdefHelpDeskCategories>(entity =>
            {
                entity.HasKey(e => e.CategoryId);

                entity.ToTable("ADefHelpDesk_Categories");

                entity.HasIndex(e => e.PortalId)
                    .HasName("IX_ADefHelpDesk_Categories");

                entity.Property(e => e.CategoryId).HasColumnName("CategoryID");

                entity.Property(e => e.CategoryName).HasMaxLength(50);

                entity.Property(e => e.ParentCategoryId).HasColumnName("ParentCategoryID");

                entity.Property(e => e.PortalId).HasColumnName("PortalID");
            });

            modelBuilder.Entity<AdefHelpDeskLastSearch>(entity =>
            {
                entity.ToTable("ADefHelpDesk_LastSearch");

                entity.HasIndex(e => new { e.UserId, e.PortalId })
                    .HasName("IX_ADefHelpDesk_LastSearch");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.AssignedRoleId).HasColumnName("AssignedRoleID");

                entity.Property(e => e.Categories).HasMaxLength(2000);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.DueDate).HasColumnType("datetime");

                entity.Property(e => e.PortalId).HasColumnName("PortalID");

                entity.Property(e => e.Priority).HasMaxLength(50);

                entity.Property(e => e.SearchText).HasMaxLength(150);

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.UserId).HasColumnName("UserID");
            });

            modelBuilder.Entity<AdefHelpDeskLog>(entity =>
            {
                entity.HasKey(e => e.LogId);

                entity.ToTable("ADefHelpDesk_Log");

                entity.HasIndex(e => e.TaskId)
                    .HasName("IX_ADefHelpDesk_Log");

                entity.Property(e => e.LogId).HasColumnName("LogID");

                entity.Property(e => e.DateCreated).HasColumnType("datetime");

                entity.Property(e => e.LogDescription)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.TaskId).HasColumnName("TaskID");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Task)
                    .WithMany(p => p.AdefHelpDeskLog)
                    .HasForeignKey(d => d.TaskId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ADefHelpDesk_Log_ADefHelpDesk_Tasks");
            });

            modelBuilder.Entity<AdefHelpDeskRiausers>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.ToTable("ADefHelpDesk_RIAUsers");

                entity.Property(e => e.UserId)
                    .HasColumnName("UserID")
                    .ValueGeneratedNever();

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Ipaddress)
                    .IsRequired()
                    .HasColumnName("IPAddress")
                    .HasMaxLength(50);

                entity.Property(e => e.Riapassword)
                    .IsRequired()
                    .HasColumnName("RIAPassword")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<AdefHelpDeskRoles>(entity =>
            {
                entity.ToTable("ADefHelpDesk_Roles");

                entity.HasIndex(e => e.PortalId)
                    .HasName("IX_ADefHelpDesk_Roles");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.PortalId).HasColumnName("PortalID");

                entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<AdefHelpDeskSettings>(entity =>
            {
                entity.HasKey(e => e.SettingId);

                entity.ToTable("ADefHelpDesk_Settings");

                entity.HasIndex(e => e.PortalId);

                entity.Property(e => e.SettingId).HasColumnName("SettingID");

                entity.Property(e => e.PortalId).HasColumnName("PortalID");

                entity.Property(e => e.SettingName)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.SettingValue)
                    .IsRequired()
                    .HasMaxLength(250);
            });

            modelBuilder.Entity<AdefHelpDeskSystemLog>(entity =>
            {
                entity.HasKey(e => e.LogId);

                entity.ToTable("ADefHelpDesk_SystemLog");

                entity.HasIndex(e => e.LogType)
                    .HasName("IX_ADefHelpDesk_SystemLog");

                entity.Property(e => e.LogId).HasColumnName("LogId");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.LogMessage)
                    .IsRequired()
                    .HasMaxLength(4000);

                entity.Property(e => e.LogType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.UserName).HasMaxLength(350);
            });

            modelBuilder.Entity<AdefHelpDeskTaskAssociations>(entity =>
            {
                entity.HasKey(e => e.TaskRelationId);

                entity.ToTable("ADefHelpDesk_TaskAssociations");

                entity.HasIndex(e => e.TaskId)
                    .HasName("IX_ADefHelpDesk_TaskAssociations");

                entity.Property(e => e.TaskRelationId).HasColumnName("TaskRelationID");

                entity.Property(e => e.AssociatedId).HasColumnName("AssociatedID");

                entity.Property(e => e.TaskId).HasColumnName("TaskID");

                entity.HasOne(d => d.Task)
                    .WithMany(p => p.AdefHelpDeskTaskAssociations)
                    .HasForeignKey(d => d.TaskId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ADefHelpDesk_TaskAssociations_ADefHelpDesk_Tasks");
            });

            modelBuilder.Entity<AdefHelpDeskTaskCategories>(entity =>
            {
                entity.ToTable("ADefHelpDesk_TaskCategories");

                entity.HasIndex(e => e.TaskId)
                    .HasName("IX_ADefHelpDesk_TaskCategories");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CategoryId).HasColumnName("CategoryID");

                entity.Property(e => e.TaskId).HasColumnName("TaskID");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.AdefHelpDeskTaskCategories)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ADefHelpDesk_TaskCategories_ADefHelpDesk_Categories");

                entity.HasOne(d => d.Task)
                    .WithMany(p => p.AdefHelpDeskTaskCategories)
                    .HasForeignKey(d => d.TaskId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ADefHelpDesk_TaskCategories_ADefHelpDesk_Tasks");
            });

            modelBuilder.Entity<AdefHelpDeskTaskDetails>(entity =>
            {
                entity.HasKey(e => e.DetailId);

                entity.ToTable("ADefHelpDesk_TaskDetails");

                entity.HasIndex(e => e.TaskId)
                    .HasName("IX_ADefHelpDesk_TaskDetails");

                entity.Property(e => e.DetailId).HasColumnName("DetailID");

                entity.Property(e => e.ContentType).HasMaxLength(50);

                entity.Property(e => e.Description).IsRequired();

                entity.Property(e => e.DetailType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.InsertDate).HasColumnType("datetime");

                entity.Property(e => e.StartTime).HasColumnType("datetime");

                entity.Property(e => e.StopTime).HasColumnType("datetime");

                entity.Property(e => e.TaskId).HasColumnName("TaskID");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Task)
                    .WithMany(p => p.AdefHelpDeskTaskDetails)
                    .HasForeignKey(d => d.TaskId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ADefHelpDesk_TaskDetails_ADefHelpDesk_Tasks");
            });

            modelBuilder.Entity<AdefHelpDeskTasks>(entity =>
            {
                entity.HasKey(e => e.TaskId);

                entity.ToTable("ADefHelpDesk_Tasks");

                entity.HasIndex(e => e.AssignedRoleId);

                entity.HasIndex(e => e.CreatedDate);

                entity.HasIndex(e => e.Status);

                entity.Property(e => e.TaskId).HasColumnName("TaskID");

                entity.Property(e => e.AssignedRoleId).HasColumnName("AssignedRoleID");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.DueDate).HasColumnType("datetime");

                entity.Property(e => e.EstimatedCompletion).HasColumnType("datetime");

                entity.Property(e => e.EstimatedStart).HasColumnType("datetime");

                entity.Property(e => e.PortalId).HasColumnName("PortalID");

                entity.Property(e => e.Priority)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.RequesterEmail).HasMaxLength(350);

                entity.Property(e => e.RequesterName).HasMaxLength(350);

                entity.Property(e => e.RequesterPhone).HasMaxLength(50);

                entity.Property(e => e.RequesterUserId).HasColumnName("RequesterUserID");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.TicketPassword)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<AdefHelpDeskUserRoles>(entity =>
            {
                entity.HasKey(e => e.UserRoleId);

                entity.ToTable("ADefHelpDesk_UserRoles");

                entity.Property(e => e.UserRoleId).HasColumnName("UserRoleID");

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AdefHelpDeskUserRoles)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserRoles_ADefHelpDesk_Roles");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AdefHelpDeskUserRoles)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ADefHelpDesk_UserRoles_ADefHelpDesk_Users");
            });

            modelBuilder.Entity<AdefHelpDeskUsers>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.ToTable("ADefHelpDesk_Users");

                entity.HasIndex(e => e.Email)
                    .IsUnique();

                entity.HasIndex(e => e.Username)
                    .HasName("IX_ADefHelpDesk_Users");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Riapassword)
                    .HasColumnName("RIAPassword")
                    .HasMaxLength(50);

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.VerificationCode).HasMaxLength(50);
            });

            modelBuilder.Entity<AdefHelpDeskVersion>(entity =>
            {
                entity.HasKey(e => e.VersionNumber);

                entity.ToTable("ADefHelpDesk_Version");

                entity.Property(e => e.VersionNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .ValueGeneratedNever();
            });

            modelBuilder.Entity<AspNetRoleClaims>(entity =>
            {
                entity.HasIndex(e => e.RoleId);

                entity.Property(e => e.RoleId).IsRequired();

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AspNetRoleClaims)
                    .HasForeignKey(d => d.RoleId);
            });

            modelBuilder.Entity<AspNetRoles>(entity =>
            {
                entity.HasIndex(e => e.NormalizedName)
                    .HasName("RoleNameIndex");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name).HasMaxLength(256);

                entity.Property(e => e.NormalizedName).HasMaxLength(256);
            });

            modelBuilder.Entity<AspNetUserClaims>(entity =>
            {
                entity.HasIndex(e => e.UserId);

                entity.Property(e => e.UserId).IsRequired();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserClaims)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserLogins>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

                entity.HasIndex(e => e.UserId);

                entity.Property(e => e.UserId).IsRequired();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserLogins)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserRoles>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });

                entity.HasIndex(e => e.RoleId);

                entity.HasIndex(e => e.UserId);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AspNetUserRoles)
                    .HasForeignKey(d => d.RoleId);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserRoles)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUsers>(entity =>
            {
                entity.HasIndex(e => e.NormalizedEmail)
                    .HasName("EmailIndex");

                entity.HasIndex(e => e.NormalizedUserName)
                    .HasName("UserNameIndex")
                    .IsUnique()
                    .HasFilter("([NormalizedUserName] IS NOT NULL)");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Email).HasMaxLength(256);

                entity.Property(e => e.NormalizedEmail).HasMaxLength(256);

                entity.Property(e => e.NormalizedUserName).HasMaxLength(256);

                entity.Property(e => e.UserName).HasMaxLength(256);
            });

            modelBuilder.Entity<AspNetUserTokens>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });
            });
        }
    }
}
