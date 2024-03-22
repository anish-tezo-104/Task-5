namespace EMS.DAL.DBO;

public class Employee
{
    public string? EmpNo { get; set; } = string.Empty;
    public int StatusId { get; set; }
    public string? FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; } = string.Empty;
    public DateTime? Dob { get; set; }
    public string? Email { get; set; } = string.Empty;
    public string? MobileNumber { get; set; } = string.Empty;
    public DateTime? JoiningDate { get; set; }
    public int? LocationId { get; set; }
    public int? RoleId { get; set; }
    public int? DepartmentId { get; set; }
    public int? AssignManagerId { get; set; }
    public int? AssignProjectId { get; set; }
    
    public override string ToString()
    {
        return $"Employee ID: {EmpNo}\n" +
               $"Name: {FirstName} {LastName}\n" +
               $"Date of Birth: {(Dob.HasValue ? Dob.Value.ToShortDateString() : string.Empty)}\n" +
               $"Email: {Email}\n" +
               $"Mobile Number: {MobileNumber}\n" +
               $"Joining Date: {(JoiningDate.HasValue ? JoiningDate.Value.ToShortDateString() : string.Empty)}\n" +
               $"LocationId: {LocationId}\n" +
               $"RoleId: {RoleId}\n" +
               $"DepartmentId: {DepartmentId}\n";
    }
}
