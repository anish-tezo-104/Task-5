
using EmployeeManagementSystem.DAL;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.BAL;

public class DropdownBAL : IDropdownBAL
{
    private readonly IDropdownDAL _dropdownDAL;

    public DropdownBAL(IDropdownDAL dropdownDAL)
    {
        _dropdownDAL = dropdownDAL;
    }
    public Dictionary<int, string> GetDepartments()
    {
        List<Dropdown> departmentsList = _dropdownDAL.GetDepartmentsList();
        if (departmentsList == null)
        {
            return [];
        }
        Dictionary<int, string> departments = ConvertListToDictionary(departmentsList);
        return departments;
    }

    public Dictionary<int, string> GetLocations()
    {
        List<Dropdown> locationsList = _dropdownDAL.GetLocationsList();
        if (locationsList == null)
        {
            return [];
        }
        Dictionary<int, string> locations = ConvertListToDictionary(locationsList);
        return locations;
    }

    public Dictionary<int, string> GetManagers()
    {
        List<Dropdown> managersList = _dropdownDAL.GetManagersList();
        if (managersList == null)
        {
            return [];
        }
        Dictionary<int, string> managers = ConvertListToDictionary(managersList);
        return managers;
    }

    public Dictionary<int, string> GetProjects()
    {
        List<Dropdown> projectsList = _dropdownDAL.GetProjectsList();
        if (projectsList == null)
        {
            return [];
        }
        Dictionary<int, string> projects = ConvertListToDictionary(projectsList);
        return projects;
    }

    public Dictionary<int, string> GetRoles()
    {
        List<Role> rolesList = _dropdownDAL.GetRolesList();
        if (rolesList == null)
        {
            return [];
        }
        Dictionary<int, string> roles = ConvertListToDictionary(rolesList);
        return roles;
    }

    public Dictionary<int, string> GetStatus()
    {
        List<Dropdown> statusList = _dropdownDAL.GetStatusList();
        if (statusList == null)
        {
            return [];
        }
        Dictionary<int, string> status = ConvertListToDictionary(statusList);
        return status;
    }

    public static Dictionary<int, string> ConvertListToDictionary<T>(List<T> items)
    {
        if (items == null)
        {
            return [];
        }

        var dictionary = new Dictionary<int, string>();
        foreach (var item in items)
        {
            var idProperty = typeof(T).GetProperty("Id");
            var nameProperty = typeof(T).GetProperty("Name");

            if (idProperty != null && nameProperty != null)
            {
                var idValue = idProperty.GetValue(item);
                var nameValue = nameProperty.GetValue(item);

                if (idValue != null && nameValue != null && idValue is int && nameValue is string)
                {
                    dictionary[(int)idValue] = (string)nameValue;
                }
            }
        }
        return dictionary;
    }
}