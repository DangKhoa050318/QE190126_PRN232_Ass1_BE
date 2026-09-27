// The scaffolded entity TaskTrack.Repo.Models.Task clashes with System.Threading.Tasks.Task.
// Bare "Task" always means the async type; the entity is referred to as TaskItem.
global using Task = System.Threading.Tasks.Task;
global using TaskItem = TaskTrack.Repo.Models.Task;
