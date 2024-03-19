using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.DAL;

public interface IRoleDAL
{
    public bool Insert(Role role);
    public List<Role>? RetrieveAll();
}