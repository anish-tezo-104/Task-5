
using EMS.DAL.DBO;
using EMS.DAL.DTO;

namespace EMS.BAL.Interfaces;

public interface IEmployeeBAL
{
    public List<EmployeeDetails>? GetAll();
    public bool AddEmployee(Employee employee);
    public bool DeleteEmployees(string empNo);
    public bool UpdateEmployee(string empNo, Employee employee);
    public List<EmployeeDetails>? SearchEmployees(EmployeeFilters keyword);
    public List<EmployeeDetails>? FilterEmployees(EmployeeFilters filters);
    public int CountEmployees();
}