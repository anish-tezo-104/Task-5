namespace EmployeeManagementSystem.Models;

public class Role{
    public int Id { get; set; }
    public required string RoleName { get; set; }
    public int? DepartmentId { get; set; }
}