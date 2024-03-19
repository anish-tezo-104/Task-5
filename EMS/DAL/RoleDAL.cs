using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Utils;
using Microsoft.Extensions.Configuration;

namespace EmployeeManagementSystem.DAL;

public class RoleDAL : IRoleDAL
{
    private readonly string _filePath = "";
    private readonly JSONUtils _jsonUtils;
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;

    public RoleDAL(ILogger logger, JSONUtils jsonUtils, IConfiguration configuration)
    {
        _jsonUtils = jsonUtils;
        _logger = logger;
        _configuration = configuration;
        _filePath = _configuration["RoleJsonPath"];
    }

    public bool Insert(Role role)
    {
        List<Role> existingRoles = _jsonUtils.ReadJSON<Role>(_filePath);
        existingRoles.Add(role);
        _jsonUtils.WriteJSON(existingRoles, _filePath);
        return true;
    }

    public List<Role>? RetrieveAll()
    {
        List<Role> roles = _jsonUtils.ReadJSON<Role>(_filePath);
        if (roles == null || roles.Count == 0)
        {
            return [];
        }
        return roles;
    }
}
