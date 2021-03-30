using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

#nullable disable

namespace RemoteControl.Models
{
    public partial class KGNGPEmployeesContext : DbContext
    {
        public KGNGPEmployeesContext()
        {
        }

        public KGNGPEmployeesContext(DbContextOptions<KGNGPEmployeesContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Check> Checks { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<WorkingTime> WorkingTimes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Check>(entity =>
            {
                //entity.HasComment("Проверки нахождения сотрудников на рабочем месте");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("code")
                    .IsFixedLength(true);

                entity.Property(e => e.EmployeeId).HasColumnName("employeeID");

                entity.Property(e => e.Info)
                    .HasMaxLength(300)
                    .HasColumnName("info");

                entity.Property(e => e.Ip)
                    .HasMaxLength(30)
                    .HasColumnName("ip")
                    .IsFixedLength(true);

                entity.Property(e => e.SentDate)
                    .HasColumnType("datetime")
                    .HasColumnName("sentDate");

                entity.Property(e => e.TypedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("typedDate");

                entity.Property(e => e.PlanDate)
                    .IsRequired()
                    .HasColumnType("datetime")
                    .HasColumnName("planDate");
                entity.Property(e => e.WorkingTimeEndDate)
                    .IsRequired()
                    .HasColumnType("datetime")
                    .HasColumnName("WorkingTimeEndDate");

                entity.Property(e => e.WrongTypedCount).HasColumnName("wrongTypedCount");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.Checks)
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Checks_Employees");
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasComment("Подразделения и отделы");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Idguid1C).HasColumnName("IDGuid1C");

                entity.Property(e => e.Name).HasMaxLength(300);

                entity.Property(e => e.ParentId).HasColumnName("ParentID");

                entity.Property(e => e.Level).IsRequired().HasColumnName("Level");

                entity.Property(e => e.SortOrder).IsRequired().HasColumnName("SortOrder");

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.Children)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_Departments_Parent");
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasComment("Работники");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Address).HasMaxLength(100);

                entity.Property(e => e.Birthsday).HasColumnType("datetime");

                entity.Property(e => e.City).HasMaxLength(50);

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Login).HasMaxLength(50);

                entity.Property(e => e.GazPhone).HasMaxLength(50);

                entity.Property(e => e.Idguid1C).HasColumnName("IDGuid1C");

                entity.Property(e => e.InnerPhone).HasMaxLength(50);

                entity.Property(e => e.MobilePhone).HasMaxLength(50);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.CanManage).HasColumnName("CanManage");

                entity.Property(e => e.IsControlled).HasColumnName("IsControlled");

                entity.Property(e => e.SyncAD).HasColumnName("SyncAD");

                entity.Property(e => e.Position)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.StatusTill).HasColumnType("datetime");

                entity.Property(e => e.WorkPhone).HasMaxLength(50);

                entity.HasOne(d => d.Department)
                    .WithMany(p => p.Employees)
                    .HasForeignKey(d => d.DepartmentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Employees_Departments");
            });

            modelBuilder.Entity<WorkingTime>(entity =>
            {
                entity.HasComment("Рабочие часы по дням");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.EmployeeId).HasColumnName("employeeID");

                entity.Property(e => e.LunchEnd)
                    .HasColumnType("datetime")
                    .HasColumnName("lunchEnd");

                entity.Property(e => e.LunchStart)
                    .HasColumnType("datetime")
                    .HasColumnName("lunchStart");

                entity.Property(e => e.TimeHoursDifToServer).HasColumnName("timeHoursDifToServer");

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasComment("Тип работы: 0-на раб.месте, 1-удаленно, 2-отпуск, 3-командировка, 4-больничный, 5-прочее");

                entity.Property(e => e.WorkEnd)
                    .HasColumnType("datetime")
                    .HasColumnName("workEnd");

                entity.Property(e => e.WorkStart)
                    .HasColumnType("datetime")
                    .HasColumnName("workStart");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.WorkingTimes)
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_WorkingTimes_Employees");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
