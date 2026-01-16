using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace NotificationWorker.Models.DBModels;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Notification> Notifications { get; set; }
    public virtual DbSet<OrganizationClient> OrganizationClients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notifications_pk");
            entity.ToTable("notifications");

            entity.Property(e => e.Id)
                .HasColumnType("character varying")
                .HasColumnName("id");
            entity.Property(e => e.ClientId)
                .HasColumnType("character varying")
                .HasColumnName("client_id");
            entity.Property(e => e.Channel)
                .HasColumnType("character varying")
                .HasColumnName("channel");
            entity.Property(e => e.ContactInfo)
                .HasColumnType("character varying")
                .HasColumnName("contact_info");
            entity.Property(e => e.Subject)
                .HasColumnType("character varying")
                .HasColumnName("subject");
            entity.Property(e => e.Message)
                .HasColumnType("text")
                .HasColumnName("message");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'new'")
                .HasColumnType("character varying")
                .HasColumnName("status");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.ProcessedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("processed_at");
            entity.Property(e => e.SentAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("sent_at");
            entity.Property(e => e.RetryCount)
                .HasDefaultValueSql("0")
                .HasColumnName("retry_count");
            entity.Property(e => e.ErrorMessage)
                .HasColumnType("text")
                .HasColumnName("error_message");
            entity.Property(e => e.Metadata)
                .HasColumnType("text")
                .HasColumnName("metadata");

            entity.HasOne(d => d.ClientNavigation).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.ClientId)
                .HasConstraintName("notifications_fk");
        });

        modelBuilder.Entity<OrganizationClient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("organization_cients_pk");
            entity.ToTable("organization_clients");

            entity.Property(e => e.Id)
                .HasColumnType("character varying")
                .HasColumnName("id");
            entity.Property(e => e.ClientAdres)
                .HasColumnType("character varying")
                .HasColumnName("client_adres");
            entity.Property(e => e.ClientBalance)
                .HasColumnName("client_balance");
            entity.Property(e => e.ClientEmail)
                .HasColumnType("character varying")
                .HasColumnName("client_email");
            entity.Property(e => e.ClientInn)
                .HasColumnType("character varying")
                .HasColumnName("client_inn");
            entity.Property(e => e.ClientLogo)
                .HasColumnType("character varying")
                .HasColumnName("client_logo");
            entity.Property(e => e.ClientName)
                .HasColumnType("character varying")
                .HasColumnName("client_name");
            entity.Property(e => e.ClientPhone)
                .HasColumnType("character varying")
                .HasColumnName("client_phone");
            entity.Property(e => e.ClientStatus)
                .HasDefaultValueSql("0")
                .HasColumnName("client_status");
            entity.Property(e => e.ClinetType)
                .HasColumnType("character varying")
                .HasColumnName("clinet_type");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Organization)
                .HasColumnType("character varying")
                .HasColumnName("organization");
            entity.Property(e => e.UpdatedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_date");
            entity.Property(e => e.UserCreater)
                .HasColumnType("character varying")
                .HasColumnName("user_creater");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

