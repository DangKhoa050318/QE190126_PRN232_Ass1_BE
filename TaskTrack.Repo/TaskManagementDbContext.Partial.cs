using Microsoft.EntityFrameworkCore;
using TaskTrack.Repo.Models;

namespace TaskTrack.Repo;

// Kept separate from the scaffolded file so re-scaffolding does not overwrite it.
public partial class TaskManagementDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        // Columns with a database default are treated by EF as "store generated": a CLR default value
        // (0 / false) is then omitted from INSERT and the DB default wins, e.g. Priority = 0 (Low)
        // would be saved as 1 (Medium). The application always sets these values, so always send them.
        modelBuilder.Entity<Department>(e => e.Property(x => x.IsActive).ValueGeneratedNever());

        modelBuilder.Entity<Project>(e =>
        {
            e.Property(x => x.IsActive).ValueGeneratedNever();
            e.Property(x => x.Status).ValueGeneratedNever();
            e.Property(x => x.CreatedDate).ValueGeneratedNever();
        });

        modelBuilder.Entity<TaskItem>(e =>
        {
            e.Property(x => x.IsActive).ValueGeneratedNever();
            e.Property(x => x.Status).ValueGeneratedNever();
            e.Property(x => x.Priority).ValueGeneratedNever();
            e.Property(x => x.CreatedDate).ValueGeneratedNever();
        });
    }
}
