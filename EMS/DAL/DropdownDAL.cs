using EmployeeManagementSystem.Models;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace EmployeeManagementSystem.DAL;

public class DropdownDAL : IDropdownDAL
{
    public readonly IConfiguration _configuration;
    public readonly Dictionary<int, string>? _locations;
    public readonly Dictionary<int, string>? _departments;
    public readonly Dictionary<int, string>? _managers;
    public readonly Dictionary<int, string>? _projects;
    public readonly Dictionary<int, string>? _status;
    public readonly Dictionary<int, string>? _roles;

    public DropdownDAL(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Dictionary<int, string>? GetLocations()
    {
        return LoadData<Location>("LocationJsonPath", l => l.Id, l => l.LocationName);
    }

    public Dictionary<int, string>? GetDepartments()
    {
        return LoadData<Department>("DepartmentJsonPath", d => d.Id, d => d.DepartmentName);
    }

    public Dictionary<int, string>? GetManagers()
    {
        return LoadData<Manager>("ManagerJsonPath", m => m.Id, m => m.ManagerName);
    }

    public Dictionary<int, string>? GetProjects()
    {
        return LoadData<Project>("ProjectJsonPath", p => p.Id, p => p.ProjectTitle);
    }

    public Dictionary<int, string>? GetStatus()
    {
        return LoadData<Status>("StatusJsonPath", s => s.Id, s => s.StatusName);
    }

    public Dictionary<int, string>? GetRoles()
    {
        return LoadData<Role>("RoleJsonPath", r => r.Id, r => r.RoleName);
    }

    public Dictionary<int, string>? LoadData<T>(string jsonFilePathKey, Func<T, int> keySelector, Func<T, string> valueSelector) where T : class
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
                var dictionary = new Dictionary<int, string>();
                foreach (var item in items)
                {
                    var key = keySelector(item);
                    var value = valueSelector(item);
                    if (key != default && !string.IsNullOrEmpty(value))
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
