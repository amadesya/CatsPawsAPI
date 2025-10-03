using CatsPawsAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // ---------- DbSet ----------
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<Material> Materials { get; set; }
    public DbSet<MaterialTopic> MaterialTopics { get; set; }
    public DbSet<Task> Tasks { get; set; }
    public DbSet<TaskStatus> TaskStatuses { get; set; }
    public DbSet<TaskStatusHistory> TaskStatusHistory { get; set; }
    public DbSet<Test> Tests { get; set; }
    public DbSet<TestQuestion> TestQuestions { get; set; }
    public DbSet<TestOption> TestOptions { get; set; }
    public DbSet<TestResult> TestResults { get; set; }
    public DbSet<TaskStatusHistory> TaskStatusHistories { get; set; }

    // ---------- OnModelCreating ----------
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------- Составной ключ для MaterialTopic ----------
        modelBuilder.Entity<MaterialTopic>()
            .HasKey(mt => new { mt.MaterialId, mt.TopicId });

        modelBuilder.Entity<MaterialTopic>()
            .HasOne(mt => mt.Material)
            .WithMany(m => m.MaterialTopics)
            .HasForeignKey(mt => mt.MaterialId);

        modelBuilder.Entity<MaterialTopic>()
            .HasOne(mt => mt.Topic)
            .WithMany(t => t.MaterialTopics)
            .HasForeignKey(mt => mt.TopicId);

        // ---------- Явные ключи ----------
        modelBuilder.Entity<Role>()
            .HasKey(r => r.RoleId);

        modelBuilder.Entity<User>()
            .HasKey(u => u.UserId);

        modelBuilder.Entity<Topic>()
            .HasKey(t => t.TopicId);

        modelBuilder.Entity<Material>()
            .HasKey(m => m.MaterialId);

        modelBuilder.Entity<Task>()
            .HasKey(t => t.TaskId);

        modelBuilder.Entity<TaskStatus>()
            .HasKey(ts => ts.StatusId);

        modelBuilder.Entity<TaskStatusHistory>()
            .HasKey(tsh => tsh.TaskStatusId);

        modelBuilder.Entity<Test>()
            .HasKey(t => t.TestId);

        modelBuilder.Entity<TestQuestion>()
            .HasKey(q => q.QuestionId);

        modelBuilder.Entity<TestOption>()
            .HasKey(o => o.OptionId);

        modelBuilder.Entity<TestResult>()
            .HasKey(tr => tr.UserTestId);

        // ---------- Связи ----------
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId);

        modelBuilder.Entity<Task>()
            .HasOne(t => t.Topic)
            .WithMany(tp => tp.Tasks)
            .HasForeignKey(t => t.TopicId);

        modelBuilder.Entity<TaskStatusHistory>()
            .HasOne(tsh => tsh.User)
            .WithMany(u => u.TaskStatusHistory)
            .HasForeignKey(tsh => tsh.UserId);

        modelBuilder.Entity<TaskStatusHistory>()
            .HasOne(tsh => tsh.Task)
            .WithMany(t => t.TaskStatusHistory)
            .HasForeignKey(tsh => tsh.TaskId);

        modelBuilder.Entity<TaskStatusHistory>()
            .HasOne(tsh => tsh.Status)
            .WithMany(ts => ts.TaskStatusHistory)
            .HasForeignKey(tsh => tsh.StatusId);

        modelBuilder.Entity<Test>()
            .HasOne(t => t.Topic)
            .WithMany(tp => tp.Tests)
            .HasForeignKey(t => t.TopicId);

        modelBuilder.Entity<TestQuestion>()
            .HasOne(q => q.Test)
            .WithMany(t => t.TestQuestions)
            .HasForeignKey(q => q.TestId);

        modelBuilder.Entity<TestOption>()
            .HasOne(o => o.Question)
            .WithMany(q => q.TestOptions)
            .HasForeignKey(o => o.QuestionId);

        modelBuilder.Entity<TestResult>()
            .HasOne(tr => tr.User)
            .WithMany(u => u.TestResults)
            .HasForeignKey(tr => tr.UserId);

        modelBuilder.Entity<TestResult>()
            .HasOne(tr => tr.Test)
            .WithMany(t => t.TestResults)
            .HasForeignKey(tr => tr.TestId);
    }
}
