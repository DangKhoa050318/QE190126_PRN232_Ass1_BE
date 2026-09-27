namespace TaskTrack.Service.DTOs;

public class TaskDto
{
    public int TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public short Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public short Priority { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    public DateOnly? DueDate { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public List<TagDto> Tags { get; set; } = [];
}
