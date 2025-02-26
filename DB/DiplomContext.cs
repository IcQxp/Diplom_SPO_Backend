using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DiplomBackend.DB;

public partial class DiplomContext : DbContext
{
    public DiplomContext()
    {
    }

    public DiplomContext(DbContextOptions<DiplomContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Criterion> Criteria { get; set; }

    public virtual DbSet<Discipline> Disciplines { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<DocumentType> DocumentTypes { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Gender> Genders { get; set; }

    public virtual DbSet<Grade> Grades { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<Lesson> Lessons { get; set; }

    public virtual DbSet<LessonTime> LessonTimes { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-3SCKTGF;Database=Diplom;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Criterion>(entity =>
        {
            entity.HasKey(e => e.CriteriaId);

            entity.Property(e => e.CriteriaId).HasColumnName("Criteria_ID");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.MaxScore).HasColumnName("Max_score");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Discipline>(entity =>
        {
            entity.ToTable("Discipline");

            entity.Property(e => e.DisciplineId).HasColumnName("Discipline_ID");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("Document");

            entity.Property(e => e.DocumentId).HasColumnName("Document_ID");
            entity.Property(e => e.CriteriaId).HasColumnName("Criteria_ID");
            entity.Property(e => e.DocumentTypeId).HasColumnName("DocumentType_ID");
            entity.Property(e => e.DownloadDate).HasColumnType("datetime");
            entity.Property(e => e.EmployeeId).HasColumnName("Employee_ID");
            entity.Property(e => e.StatusId).HasColumnName("Status_ID");
            entity.Property(e => e.StudentId).HasColumnName("Student_ID");

            entity.HasOne(d => d.Criteria).WithMany(p => p.Documents)
                .HasForeignKey(d => d.CriteriaId)
                .HasConstraintName("FK_Document_Criteria");

            entity.HasOne(d => d.DocumentType).WithMany(p => p.Documents)
                .HasForeignKey(d => d.DocumentTypeId)
                .HasConstraintName("FK_Document_DocumentType");

            entity.HasOne(d => d.Employee).WithMany(p => p.Documents)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK_Document_Employee");

            entity.HasOne(d => d.Status).WithMany(p => p.Documents)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Document_Status");
        });

        modelBuilder.Entity<DocumentType>(entity =>
        {
            entity.ToTable("DocumentType");

            entity.Property(e => e.DocumentTypeId).HasColumnName("DocumentType_ID");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employee");

            entity.Property(e => e.EmployeeId).HasColumnName("Employee_ID");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Firstname).HasMaxLength(50);
            entity.Property(e => e.GenderCode)
                .HasMaxLength(1)
                .IsFixedLength()
                .HasColumnName("Gender_Code");
            entity.Property(e => e.Lastname).HasMaxLength(50);
            entity.Property(e => e.Login).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.Patronymic).HasMaxLength(50);
            entity.Property(e => e.RoleId).HasColumnName("Role_ID");
            entity.Property(e => e.Telephone)
                .HasMaxLength(11)
                .IsUnicode(false);

            entity.HasOne(d => d.GenderCodeNavigation).WithMany(p => p.Employees)
                .HasForeignKey(d => d.GenderCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Employee_Gender");

            entity.HasOne(d => d.Role).WithMany(p => p.Employees)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Employee_Role");
        });

        modelBuilder.Entity<Gender>(entity =>
        {
            entity.HasKey(e => e.GenderCode);

            entity.ToTable("Gender");

            entity.Property(e => e.GenderCode)
                .HasMaxLength(1)
                .IsFixedLength()
                .HasColumnName("Gender_Code");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Grade>(entity =>
        {
            entity.ToTable("Grade");

            entity.Property(e => e.GradeId).HasColumnName("Grade_ID");
            entity.Property(e => e.LessonId).HasColumnName("Lesson_ID");
            entity.Property(e => e.StudentId).HasColumnName("Student_ID");

            entity.HasOne(d => d.Lesson).WithMany(p => p.Grades)
                .HasForeignKey(d => d.LessonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Grade_Lesson");

            entity.HasOne(d => d.Student).WithMany(p => p.Grades)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Grade_Student");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.ToTable("Group");

            entity.Property(e => e.GroupId).HasColumnName("Group_ID");
            entity.Property(e => e.GroupNumber).HasMaxLength(20);
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.ToTable("Lesson");

            entity.Property(e => e.LessonId).HasColumnName("Lesson_ID");
            entity.Property(e => e.DisciplineId).HasColumnName("Discipline_ID");
            entity.Property(e => e.EmployeeId).HasColumnName("Employee_ID");
            entity.Property(e => e.GroupId).HasColumnName("Group_ID");
            entity.Property(e => e.LessonTimeId).HasColumnName("LessonTime_ID");

            entity.HasOne(d => d.Discipline).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.DisciplineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Lesson_Discipline");

            entity.HasOne(d => d.Employee).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Lesson_Employee");

            entity.HasOne(d => d.Group).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Lesson_Group");

            entity.HasOne(d => d.LessonTime).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.LessonTimeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Lesson_LessonTime");
        });

        modelBuilder.Entity<LessonTime>(entity =>
        {
            entity.ToTable("LessonTime");

            entity.Property(e => e.LessonTimeId).HasColumnName("LessonTime_ID");
            entity.Property(e => e.EndTime).HasPrecision(0);
            entity.Property(e => e.StartTime).HasPrecision(0);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role");

            entity.Property(e => e.RoleId).HasColumnName("Role_ID");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.ToTable("Status");

            entity.Property(e => e.StatusId).HasColumnName("Status_ID");
            entity.Property(e => e.Name).HasMaxLength(20);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Student");

            entity.Property(e => e.StudentId).HasColumnName("Student_ID");
            entity.Property(e => e.Firstname).HasMaxLength(50);
            entity.Property(e => e.GenderCode)
                .HasMaxLength(1)
                .IsFixedLength()
                .HasColumnName("Gender_Code");
            entity.Property(e => e.GroupId).HasColumnName("Group_ID");
            entity.Property(e => e.Lastname).HasMaxLength(50);
            entity.Property(e => e.Login).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.Patronymic).HasMaxLength(50);

            entity.HasOne(d => d.GenderCodeNavigation).WithMany(p => p.Students)
                .HasForeignKey(d => d.GenderCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Student_Gender");

            entity.HasOne(d => d.Group).WithMany(p => p.Students)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Student_Group");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
