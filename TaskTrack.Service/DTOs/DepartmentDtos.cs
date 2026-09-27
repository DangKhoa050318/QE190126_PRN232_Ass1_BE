using System.ComponentModel.DataAnnotations;

namespace TaskTrack.Service.DTOs;

public class DepartmentDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string DepartmentDescription { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int ProjectCount { get; set; }
}

public class DepartmentDetailDto : DepartmentDto
{
    public List<ProjectDto> Projects { get; set; } = [];
}

public class CreateDepartmentRequest
{
    [Required(ErrorMessage = "Department name is required.")]
    [StringLength(100, ErrorMessage = "Department name must not exceed 100 characters.")]
    public string DepartmentName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(300, ErrorMessage = "Description must not exceed 300 characters.")]
    public string DepartmentDescription { get; set; } = string.Empty;
}

public class UpdateDepartmentRequest : CreateDepartmentRequest
{
    public bool IsActive { get; set; } = true;
}
