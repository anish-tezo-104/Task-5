namespace EmployeeManagementSystem.Data;

public interface IDataManager
{
    string GetStatusName(int? statusId);
    string GetLocationName(int? locationId);
    string GetDepartmentName(int? departmentId);
    string GetManagerName(int? managerId);
    string GetProjectName(int? projectId);
    string GetRoleName(int? roleId);
}