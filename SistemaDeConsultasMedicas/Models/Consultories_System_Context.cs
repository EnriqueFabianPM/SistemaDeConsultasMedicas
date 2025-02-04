﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SistemaDeConsultasMedicas.Models;

public partial class Consultories_System_Context : DbContext
{
    public Consultories_System_Context()
    {
    }

    public Consultories_System_Context(DbContextOptions<Consultories_System_Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Consultories> Consultories { get; set; }

    public virtual DbSet<Medical_Appointments> Medical_Appointments { get; set; }

    public virtual DbSet<Roles> Roles { get; set; }

    public virtual DbSet<Schedules> Schedules { get; set; }

    public virtual DbSet<Sexes> Sexes { get; set; }

    public virtual DbSet<Types> Types { get; set; }

    public virtual DbSet<Users> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=192.168.192.193,1433;Initial Catalog=Consultories_System_Dev;Persist Security Info=True;User ID=UTSC_USER;Password=S1stem@5.UTSC2025;Encrypt=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Consultories>(entity =>
        {
            entity.HasKey(e => e.Id_Consultory).HasName("PK__Consulto__F3FE3AA11764B14F");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.Name)
                .IsRequired()
                .IsUnicode(false);
        });

        modelBuilder.Entity<Medical_Appointments>(entity =>
        {
            entity.HasKey(e => e.Id_Appointment).HasName("PK__Medical___6ECCF902700E950F");

            entity.HasOne(d => d.fk_DoctorNavigation).WithMany(p => p.Medical_Appointmentsfk_DoctorNavigation)
                .HasForeignKey(d => d.fk_Doctor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MEDICAL_APPOINTMENTS_DOCTOR_USERS");

            entity.HasOne(d => d.fk_PatientNavigation).WithMany(p => p.Medical_Appointmentsfk_PatientNavigation)
                .HasForeignKey(d => d.fk_Patient)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MEDICAL_APPOINTMENTS_PATIENT_USERS");

            entity.HasOne(d => d.fk_ScheduleNavigation).WithMany(p => p.Medical_Appointments)
                .HasForeignKey(d => d.fk_Schedule)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MEDICAL_APPOINTMENTS_SCHEDULES");
        });

        modelBuilder.Entity<Roles>(entity =>
        {
            entity.HasKey(e => e.Id_Role).HasName("PK__Roles__34ADFA60FC262DD5");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.Name)
                .IsRequired()
                .IsUnicode(false);
        });

        modelBuilder.Entity<Schedules>(entity =>
        {
            entity.HasKey(e => e.Id_Schedule).HasName("PK__Schedule__1E84FB4B78F8EE21");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.Schedule_Name)
                .IsRequired()
                .HasMaxLength(50);
        });

        modelBuilder.Entity<Sexes>(entity =>
        {
            entity.HasKey(e => e.Id_Sex).HasName("PK__Sexes__552797C2748D6071");

            entity.Property(e => e.Name)
                .IsRequired()
                .IsUnicode(false);
        });

        modelBuilder.Entity<Types>(entity =>
        {
            entity.HasKey(e => e.Id_Type).HasName("PK__Types__1A20A3D5D77AADA6");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.Name)
                .IsRequired()
                .IsUnicode(false);
        });

        modelBuilder.Entity<Users>(entity =>
        {
            entity.HasKey(e => e.Id_User).HasName("PK__Users__D03DEDCBC90AC850");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.Email)
                .IsRequired()
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .IsRequired()
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .IsRequired()
                .IsUnicode(false);

            entity.HasOne(d => d.fk_ConsultoryNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.fk_Consultory)
                .HasConstraintName("FK_USERS_CONSULTORIES");

            entity.HasOne(d => d.fk_RoleNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.fk_Role)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_USERS_ROLES");

            entity.HasOne(d => d.fk_ScheduleNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.fk_Schedule)
                .HasConstraintName("FK_USERS_Schedule");

            entity.HasOne(d => d.fk_SexNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.fk_Sex)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_USERS_SEXES");

            entity.HasOne(d => d.fk_TypeNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.fk_Type)
                .HasConstraintName("FK_USERS_TYPES");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}