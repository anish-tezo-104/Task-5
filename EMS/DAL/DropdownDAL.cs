using EmployeeManagementSystem.Models;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace EmployeeManagementSystem.DAL;

public class DropdownDAL : IDropdownDAL
{
    public readonly IConfiguration _configuration;

    public DropdownDAL(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public List<Dropdown>? GetLocationsList()
    {
        return LoadDataToList<List<Dropdown>>("LocationJsonPath");
    }

    public List<Dropdown>? GetDepartmentsList()
    {
        return LoadDataToList<List<Dropdown>>("DepartmentJsonPath");
    }

    public List<Dropdown>? GetManagersList()
    {
        return LoadDataToList<List<Dropdown>>("ManagerJsonPath");
    }

    public List<Dropdown>? GetProjectsList()
    {
        return LoadDataToList<List<Dropdown>>("ProjectJsonPath");
    }

    public List<Dropdown>? GetStatusList()
    {
        return LoadDataToList<List<Dropdown>>("StatusJsonPath");
    }

    public List<Role>? GetRolesList()
    {
        return LoadDataToList<List<Role>>("RoleJsonPath");
    }

    private T? LoadDataToList<T>(string jsonFilePathKey)
    {
        string jsonFilePath = _configuration["BasePath"] + _configuration[jsonFilePathKey];
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
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error reading or deserializing JSON file: {ex.Message}", ex);
        }
    }
}
