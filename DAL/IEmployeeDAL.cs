using EmployeeManagementSystem.Models;
namespace EmployeeManagementSystem.DAL;

public interface IEmployeeDAL
{
    public bool Insert(Employee employee);
    public (List<EmployeeDetails>? employees, bool status) RetrieveAllEmployees();
    public bool Update(Employee updatedEmployee, string empNo);
    public bool Delete(string empNo);
    public (List<EmployeeDetails>? results, bool status) SearchOrFilter(List<string>? keywords = null, EmployeeFilters? filters = null);
    public int Count();
}

