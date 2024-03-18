using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Utils;
using EmployeeManagementSystem.DAL;

namespace EmployeeManagementSystem.BAL;



public class RoleBAL : IRoleBAL
{
    private readonly IRoleDAL _roleDal;
    public readonly ILogger _logger;

    public RoleBAL(ILogger logger, IRoleDAL roleDal)
    {
        _roleDal = roleDal;
        _logger = logger;
    }

    public bool AddRole(Role role)
    {
        Console.WriteLine("inside role BAl");
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
}
