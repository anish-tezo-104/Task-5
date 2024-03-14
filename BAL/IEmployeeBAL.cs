using EmployeeManagementSystem.Models;
namespace EmployeeManagementSystem.BAL;

public interface IEmployeeBAL
{
    public List<EmployeeDetails>? GetAllEmployees();
    public bool AddEmployee(Employee employee);
    public bool DeleteEmployees(string empNo);
    public bool UpdateEmployee(string empNo, Employee employee);
    public List<EmployeeDetails>? SearchEmployees(List<string> keywords);
    public List<EmployeeDetails>? FilterEmployees(EmployeeFilters filters);
    public int CountEmployees();
}