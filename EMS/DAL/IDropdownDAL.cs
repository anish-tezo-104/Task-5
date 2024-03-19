namespace EmployeeManagementSystem.DAL;

public interface IDropdownDAL
{
    Dictionary<int, string>? GetLocations();
    Dictionary<int, string>? GetDepartments();
    Dictionary<int, string>? GetManagers();
    Dictionary<int, string>? GetProjects();
    Dictionary<int, string>? GetStatus();
    Dictionary<int, string>? GetRoles();
}