using EmployeeManagementSystem.Models;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace EmployeeManagementSystem.Data;

public class DataManager : IDataManager
{
    private readonly IConfiguration _configuration;
    private readonly Dictionary<string, int>? _locations;
    private readonly Dictionary<string, int>? _departments;
    private readonly Dictionary<string, int>? _managers;
    private readonly Dictionary<string, int>? _projects;
    private readonly Dictionary<string, int>? _status;
    private readonly Dictionary<string, int>? _roles;

    public DataManager(IConfiguration configuration)
    {
        _configuration = configuration;

        _locations = LoadLocations();
        _departments = LoadDepartments();
        _managers = LoadManagers();
        _projects = LoadProjects();
        _status = LoadStatus();
        _roles = LoadRoles();
    }

    public string GetStatusName(int? statusId)
    {
        return GetValueByKey(_status, statusId);
    }

    public string GetLocationName(int? locationId)
    {
        return GetValueByKey(_locations, locationId);
    }

    public string GetDepartmentName(int? departmentId)
    {
        return GetValueByKey(_departments, departmentId);
    }

    public string GetManagerName(int? managerId)
    {
        return GetValueByKey(_managers, managerId);
    }

    public string GetProjectName(int? projectId)
    {
        return GetValueByKey(_projects, projectId);
    }

    public string GetRoleName(int? roleId)
    {
        return GetValueByKey(_roles, roleId);
    }

    private static string GetValueByKey(Dictionary<string, int>? dictionary, int? id)
    {
        if (id.HasValue && dictionary != null && dictionary.ContainsValue(id.Value))
        {
            foreach (var kvp in dictionary)
            {
                if (kvp.Value == id)
                    return kvp.Key;
            }
        }
        return string.Empty;
    }

    private Dictionary<string, int>? LoadLocations()
    {
        return LoadData<Location>("LocationJsonPath", l => l.LocationName, l => l.Id);
    }

    private Dictionary<string, int>? LoadDepartments()
    {
        return LoadData<Department>("DepartmentJsonPath", d => d.DepartmentName, d => d.Id);
    }

    private Dictionary<string, int>? LoadManagers()
    {
        return LoadData<Manager>("ManagerJsonPath", m => m.ManagerName, m => m.Id);
    }

    private Dictionary<string, int>? LoadProjects()
    {
        return LoadData<Project>("ProjectJsonPath", p => p.ProjectTitle, p => p.Id);
    }

    private Dictionary<string, int>? LoadStatus()
    {
        return LoadData<Status>("StatusJsonPath", s => s.StatusName, s => s.Id);
    }

    private Dictionary<string, int>? LoadRoles()
    {
        return LoadData<Role>("RoleJsonPath", r => r.RoleName, r => r.Id);
    }

    private Dictionary<string, int>? LoadData<T>(string jsonFilePathKey, Func<T, string> keySelector, Func<T, int> valueSelector) where T : class
    {
        string jsonFilePath = _configuration[jsonFilePathKey];
        if (string.IsNullOrEmpty(jsonFilePath))
        {
            throw new ArgumentException($"{jsonFilePathKey} is not configured.");
        }

        if (!File.Exists(jsonFilePath))
        {
            throw new FileNotFoundException($"JSON file not found at {jsonFilePath}");
        }

        try
        {
            string json = File.ReadAllText(jsonFilePath);
            var items = JsonSerializer.Deserialize<List<T>>(json);
            if (items != null)
            {
                var dictionary = new Dictionary<string, int>();
                foreach (var item in items)
                {
                    var key = keySelector(item);
                    var value = valueSelector(item);
                    if (!string.IsNullOrEmpty(key) && value != default)
                    {
                        dictionary[key] = value;
                    }
                }
                return dictionary;
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error reading or deserializing JSON file: {ex.Message}", ex);
        }
        return null;
    }
}
