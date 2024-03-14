using EmployeeManagementSystem.Models;
namespace EmployeeManagementSystem.DAL;

public interface IEmployeeDAL
{
    public bool Insert(Employee employee);
    public List<EmployeeDetails>? RetrieveAllEmployees();
    public bool Update(string empNo, Employee updatedEmployee);
    public bool Delete(string empNo);
    public List<EmployeeDetails>? SearchOrFilter(List<string>? keywords = null, EmployeeFilters? filters = null);
    public int Count();
}

