using Area.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;

namespace Area.Models
{
    public class AreaContext : DbContext
    {
        public virtual DbSet<Person> Persons { get; set; }
        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<Task> Tasks { get; set; }
        public virtual DbSet<Evaluation> Evaluations { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Permission> Permissions { get; set; }
        public virtual DbSet<RolePermission> RolePermissions { get; set; }
        public virtual DbSet<PersonNotification> PersonNotifications { get; set; }
        public virtual DbSet<LoginHistory> LoginHistory { get; set; }
        public virtual DbSet<DevicesToken> DevicesToken { get; set; }
        public virtual DbSet<ActivityLog> ActivityLogs { get; set; }
        public virtual DbSet<CheckListItem> CheckListItems { get; set; }
        public virtual DbSet<TaskCheckListItem> TaskCheckListItems { get; set; }
        public virtual DbSet<ChecklistReview> ChecklistReview { get; set; }
        public virtual DbSet<KPIS> KPIS { get; set; }
        public virtual DbSet<KPISelection> KPISelection { get; set; }
        public virtual DbSet<KPISEvaluation> KPISEvaluation { get; set; }


        public AreaContext(DbContextOptions<AreaContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Person>()
              .HasDiscriminator<string>("Discriminator")
              .HasValue<Manager>("Manager")
              .HasValue<ProductOwner>("Product_Owner")
              .HasValue<UI_UX>("UI_Employee")
              .HasValue<TeamLeader>("Team_Leader")
              .HasValue<Developer_Employee>("Developer_Employee")
              .HasValue<Test_Employee>("Test_Employee");

            modelBuilder.Entity<Person>()
            .HasIndex(p => p.UserName)
            .IsUnique();

            //
            modelBuilder.Entity<Person>()
            .HasIndex(p => p.Email)
            .IsUnique();

            modelBuilder.Entity<Project>()
               .HasOne(p => p.ProductOwner)
               .WithMany(po => po.Projects)
               .HasForeignKey(p => p.Product_Id)
               .HasPrincipalKey(po => po.NationalNumber);

            modelBuilder.Entity<Project>()
               .HasMany(p => p.Tasks)
               .WithOne(t => t.Project)
               .HasForeignKey(t => t.Project_Id)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Task>()
                .HasMany(t => t.ChildTasks)
                .WithOne(t => t.ParentTask)
                .HasForeignKey(t => t.ParentTaskId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<Task>()
                .HasOne(t => t.ProductOwner)
                .WithMany(po => po.ProductTasks)
                .HasForeignKey(t => t.Product_Id)
                .HasPrincipalKey(po => po.NationalNumber);
                
            modelBuilder.Entity<Task>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.Project_Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Task>()
                .HasOne(t => t.TeamLeader)
                .WithMany(tl => tl.TeamLeaderTasks)
                .HasForeignKey(t => t.Leader_Id)
                .HasPrincipalKey(tl => tl.NationalNumber)
                .OnDelete(DeleteBehavior.NoAction);
            
            modelBuilder.Entity<Task>()
                .HasOne(t => t.UI_UX)
                .WithMany(ui => ui.UI_EmployeeTasks)
                .HasForeignKey(t => t.UI_UXId)
                .HasPrincipalKey(ui => ui.NationalNumber)
                .HasConstraintName("FK_Tasks_Persons_UI_UX_NationalNumber");

            modelBuilder.Entity<Task>()
                .HasOne(t => t.Developer_Employee)
                .WithMany(dv => dv.Developer_EmployeeTasks)
                .HasForeignKey(t => t.Developer_EmployeeId)
                .HasPrincipalKey(dv => dv.NationalNumber)
                .HasConstraintName("FK_Tasks_Persons_Developer_Employee_NationalNumber");

            modelBuilder.Entity<Task>()
                .HasOne(t => t.Test_Employee)
                .WithMany(te => te.Test_EmployeeTasks)
                .HasForeignKey(t => t.Test_EmployeeId)
                .HasPrincipalKey(te => te.NationalNumber)
                .HasConstraintName("FK_Tasks_Persons_Test_Employee_NationalNumber");

            modelBuilder.Entity<Task>()
             .HasOne(t => t.EvaluationTask)
             .WithOne(e => e.TaskEvaluation)
             .HasForeignKey<Evaluation>(e => e.TaskEvaluationID)
             .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Project>()
             .HasOne(p => p.EvaluationProject)
             .WithOne(e => e.ProjectEvaluation)
             .HasForeignKey<Evaluation>(e => e.ProjectEvaluationID);
            
            modelBuilder.Entity<RolePermission>()
              .HasKey(rp => new { rp.Role_id, rp.Permission_id });

            modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermission)
            .HasForeignKey(rp => rp.Role_id);

            modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermission)
            .HasForeignKey(rp => rp.Permission_id);

            modelBuilder.Entity<PersonNotification>()
                .HasKey(PN => new { PN.person_ID, PN.notifi_ID });

            modelBuilder.Entity<PersonNotification>()
            .HasOne(PN => PN.Person)
            .WithMany(P => P.PersonNotifications)
            .HasForeignKey(PN => PN.person_ID);

            modelBuilder.Entity<PersonNotification>()
            .HasOne(PN => PN.Notification)
            .WithMany(P => P.PersonNotifications)
            .HasForeignKey(PN => PN.notifi_ID);

            modelBuilder.Entity<Notification>()
            .HasOne(n => n.Project)
            .WithMany(P => P.Notifications)
            .HasForeignKey(n => n.ProjectID);

            modelBuilder.Entity<Notification>()
              .HasOne(n => n.Task)
              .WithMany(t => t.NotificationsTask)
              .HasForeignKey(n => n.TaskID)
              .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Evaluation>()
              .HasOne(e => e.ProjectEvaluation)
              .WithOne(p => p.EvaluationProject)
              .HasForeignKey<Project>(p => p.EvaluatioProjectid); 

            modelBuilder.Entity<Role>().HasData(
              new Role { Role_id = "1", Role_name = "Manager" },
              new Role { Role_id = "2", Role_name = "Product_Owner" },
              new Role { Role_id = "3", Role_name = "Team_Leader" },
              new Role { Role_id = "4", Role_name = "UI_Employee" },
              new Role { Role_id = "5", Role_name = "Developer_Employee" },
              new Role { Role_id = "6", Role_name = "Test_Employee" }
              );

            modelBuilder.Entity<Permission>().HasData(
                new Permission { Permission_id = "1", Permission_name = "Create Project" },
                new Permission { Permission_id = "2", Permission_name = "Edit Project" },
                new Permission { Permission_id = "3", Permission_name = "Delete Project" },
                new Permission { Permission_id = "4", Permission_name = "Create Task" },
                new Permission { Permission_id = "5", Permission_name = "Edit Task" },
                new Permission { Permission_id = "6", Permission_name = "Delete Task" },
                new Permission { Permission_id = "7", Permission_name = "Add Member" },
                new Permission { Permission_id = "8", Permission_name = "View Projects" },
                new Permission { Permission_id = "9", Permission_name = "View Tasks" },
                new Permission { Permission_id = "10", Permission_name = "View Reports" },
                new Permission { Permission_id = "11", Permission_name = "Test Tasks" }
                 );
            //
            modelBuilder.Entity<RolePermission>().HasData(
                 new RolePermission { Role_id = "1", Permission_id = "1" },
                 new RolePermission { Role_id = "1", Permission_id = "2" },
                 new RolePermission { Role_id = "1", Permission_id = "3" },
                 new RolePermission { Role_id = "1", Permission_id = "7" },
                 new RolePermission { Role_id = "1", Permission_id = "8" },
                 new RolePermission { Role_id = "1", Permission_id = "10" },
                 new RolePermission { Role_id = "2", Permission_id = "4" },
                 new RolePermission { Role_id = "2", Permission_id = "5" },
                 new RolePermission { Role_id = "3", Permission_id = "4" },
                 new RolePermission { Role_id = "4", Permission_id = "9" },
                 new RolePermission { Role_id = "5", Permission_id = "9" },
                 new RolePermission { Role_id = "6", Permission_id = "11" }
                 );
        }
    }
}