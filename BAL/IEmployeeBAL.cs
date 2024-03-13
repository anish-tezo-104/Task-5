using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.BAL;
public interface IEmployeeBAL
{
    public (List<EmployeeDetails>? employees, bool status) GetAllEmployees();
    public bool AddEmployee(Employee employee);
    public bool DeleteEmployees(string empNo);
    public bool UpdateEmployee(Employee updatedEmployee, string empNo);
    public (List<EmployeeDetails>? employees, bool status) SearchEmployees(List<string> keywords);
    public (List<EmployeeDetails>? employees, bool status) FilterEmployees(EmployeeFilters filters);
    public int CountEmployees();
}