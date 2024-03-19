using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Utils;

namespace EmployeeManagementSystem.DAL;

public class RoleDAL : IRoleDAL
{
    private readonly string _filePath = "";
    private readonly JSONUtils _jsonUtils;
    public readonly ILogger _logger;
    public readonly IDataManager _dataManager;

    public RoleDAL(ILogger logger, JSONUtils jsonUtils, string filePath, IDataManager dataManager)
    {
        _jsonUtils = jsonUtils;
        _logger = logger;
        _filePath = filePath;
        _dataManager = dataManager;
    }

    public bool Insert(Role role)
    {
        List<Role> existingRoles = _jsonUtils.ReadJSON<Role>(_filePath);
        existingRoles.Add(role);
        _jsonUtils.WriteJSON(existingRoles, _filePath);
        return true;
    }
}
