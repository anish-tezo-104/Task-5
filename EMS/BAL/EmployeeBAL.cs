using EmployeeManagementSystem.DAL;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Utils;
using AutoMapper;

namespace EmployeeManagementSystem.BAL;

public class EmployeeBAL : IEmployeeBAL
{
    private readonly IEmployeeDAL _employeeDal;
    private readonly IDropdownBAL _dropdownBal;
    private readonly ILogger _logger;

    public EmployeeBAL(ILogger logger, IEmployeeDAL employeeDal, IDropdownBAL dropdownBal)
    {
        _employeeDal = employeeDal;
        _dropdownBal = dropdownBal;
        _logger = logger;
    }

    public List<EmployeeDetails>? GetAll()
    {
        List<EmployeeDetails> employees;
        employees = _employeeDal.RetrieveAll();
        if (employees != null)
        {
            employees = GetEmployeeDetails(employees);
        }
        return employees;
    }

    public bool AddEmployee(Employee employee)
    {
        return _employeeDal.Insert(employee);
    }

    public bool DeleteEmployees(string empNo)
    {
        return _employeeDal.Delete(empNo);
    }

    public bool UpdateEmployee(string empNo, Employee employee)
    {
        return _employeeDal.Update(empNo, employee);
    }

    public List<EmployeeDetails>? SearchEmployees(EmployeeFilters keyword)
    {
        List<EmployeeDetails> searchedEmployees;
        searchedEmployees = _employeeDal.Filter(keyword);
        if (searchedEmployees != null)
        {
            searchedEmployees = GetEmployeeDetails(searchedEmployees);
        }
        return searchedEmployees;
    }

    public List<EmployeeDetails>? FilterEmployees(EmployeeFilters filters)
    {
        List<EmployeeDetails> filteredEmployees;
        filteredEmployees = _employeeDal.Filter(filters);
        if (filteredEmployees != null)
        {
            filteredEmployees = GetEmployeeDetails(filteredEmployees);
        }
        return filteredEmployees;
    }

    public int CountEmployees()
    {
        return _employeeDal.Count();
    }

    private List<EmployeeDetails> GetEmployeeDetails(List<EmployeeDetails> employees)
    {
        if (employees == null || employees.Count == 0)
        {
            return [];
        }

        Dictionary<int, string> locationNames = _dropdownBal.GetLocations();
        Dictionary<int, string> roleNames = _dropdownBal.GetRoles();
        Dictionary<int, string> departmentNames = _dropdownBal.GetDepartments();
        Dictionary<int, string> managerNames = _dropdownBal.GetManagers();
        Dictionary<int, string> projectNames = _dropdownBal.GetProjects();
        Dictionary<int, string> statusNames = _dropdownBal.GetStatus();

        foreach (EmployeeDetails employee in employees)
        {
            employee.StatusName = statusNames.TryGetValue(employee.StatusId, out string? statusValue) ? statusValue : null;
            employee.LocationName = employee.LocationId.HasValue && locationNames.TryGetValue(employee.LocationId.Value, out string? locationValue) ? locationValue : null;
            employee.RoleName = employee.RoleId.HasValue && roleNames.TryGetValue(employee.RoleId.Value, out string? roleValue) ? roleValue : null;
            employee.DepartmentName = employee.DepartmentId.HasValue && departmentNames.TryGetValue(employee.DepartmentId.Value, out string? departmentValue) ? departmentValue : null;
            employee.AssignManagerName = employee.AssignManagerId.HasValue && managerNames.TryGetValue(employee.AssignManagerId.Value, out string? managerValue) ? managerValue : null;
            employee.AssignProjectName = employee.AssignProjectId.HasValue && projectNames.TryGetValue(employee.AssignProjectId.Value, out string? projectValue) ? projectValue : null;
        }
        return employees;
    }
}

