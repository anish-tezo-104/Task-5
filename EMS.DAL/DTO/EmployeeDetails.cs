using EMS.DAL.DBO;

namespace EMS.DAL.DTO;

public class EmployeeDetails : Employee
{
    public string? LocationName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; } = string.Empty;
    public string? StatusName { get; set; } = string.Empty;
    public string? AssignManagerName { get; set; } = string.Empty;
    public string? AssignProjectName { get; set; } = string.Empty;
    public string? RoleName { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"Employee ID: {EmpNo}\n" +
               $"Name: {FirstName} {LastName}\n" +
               $"Date of Birth: {(Dob.HasValue ? Dob.Value.ToShortDateString() : string.Empty)}\n" +
               $"Email: {Email}\n" +
               $"Mobile Number: {MobileNumber}\n" +
               $"Joining Date: {(JoiningDate.HasValue ? JoiningDate.Value.ToShortDateString() : string.Empty)}\n" +
               $"Location: {LocationName}\n" +
               $"Role: {RoleName}\n" +
               $"Department: {DepartmentName}\n" +
               $"Assign Manager: {AssignManagerName}\n" +
               $"Assign Project: {AssignProjectName}\n" +
               $"Status: {StatusName}\n";
    }
}
