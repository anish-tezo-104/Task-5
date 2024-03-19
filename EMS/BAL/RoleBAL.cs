using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Utils;
using EmployeeManagementSystem.DAL;

namespace EmployeeManagementSystem.BAL;

public class RoleBAL : IRoleBAL
{
    private readonly IRoleDAL _roleDal;
    private readonly ILogger _logger;
    private readonly IDropdownDAL _dropdownDAL;

    public RoleBAL(ILogger logger, IRoleDAL roleDal, IDropdownDAL dropdownDAL)
    {
        _roleDal = roleDal;
        _logger = logger;
        _dropdownDAL = dropdownDAL;
    }

    public bool AddRole(Role role)
    {
        bool status;
        try
        {
            status = _roleDal.Insert(role);
        }
        catch (Exception)
        {
            status = false;
        }
        return status;
    }

    public List<Role>? GetAll()
    {
        List<Role> roles;
        try
        {
            roles = _roleDal.RetrieveAll();
        }
        catch (Exception)
        {
            throw;
        }
        return roles;
    }
}
