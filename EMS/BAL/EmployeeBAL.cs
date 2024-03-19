using EmployeeManagementSystem.DAL;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Utils;
namespace EmployeeManagementSystem.BAL;

public class EmployeeBAL : IEmployeeBAL
{
    private readonly IEmployeeDAL _employeeDal;
    private readonly IDropdownDAL _dropdownDal;
    private readonly ILogger _logger;

    public EmployeeBAL(ILogger logger, IEmployeeDAL employeeDal, IDropdownDAL dropdownDAL)
    {
        _employeeDal = employeeDal;
        _dropdownDal = dropdownDAL;
        _logger = logger;
    }

    public List<EmployeeDetails>? GetAll()
    {
        List<EmployeeDetails> employeeDetailsList;
        try
        {
            employeeDetailsList = GetEmployeeDetails();
        }
        catch (Exception)
        {
            throw; 
        }
        return employeeDetailsList;
    }

    public bool AddEmployee(Employee employee)
    {
        bool status;
        try
        {
            status = _employeeDal.Insert(employee);
        }
        catch (Exception)
        {
            status = false;
        }
        return status;
    }

    public bool DeleteEmployees(string empNo)
    {
        bool status;
        try
        {
            status = _employeeDal.Delete(empNo);
        }
        catch (Exception)
        {
            status = false;
        }

        return status;
    }

    public bool UpdateEmployee(string empNo, Employee employee)
    {
        try
        {
            return _employeeDal.Update(empNo, employee);
        }
        catch (Exception ex)
        {
            throw new Exception("Error occurred while updating employee data.", ex);
        }
    }

    public List<EmployeeDetails>? SearchEmployees(EmployeeFilters keyword)
    {
        List<EmployeeDetails> employeeDetailsList, resultEmployees;
        try
        {
            employeeDetailsList =  employeeDetailsList = GetEmployeeDetails();
            resultEmployees = _employeeDal.Filter(keyword, employeeDetailsList);
        }
        catch (Exception)
        {
            throw;
        }

        return resultEmployees;
    }

    public List<EmployeeDetails>? FilterEmployees(EmployeeFilters filters)
    {
        List<EmployeeDetails> employeeDetailsList, filteredEmployees;
        try
        {
            employeeDetailsList = GetEmployeeDetails();
            filteredEmployees = _employeeDal.Filter(filters, employeeDetailsList);
        }
        catch (Exception)
        {
            throw;
        }

        return filteredEmployees;
    }

    public int CountEmployees()
    {
        return _employeeDal.Count();
    }

    private List<EmployeeDetails> GetEmployeeDetails()
    {
        List<Employee> employees;
        List<EmployeeDetails> employeeDetailsList;
        Dictionary<int, string> locationNames = _dropdownDal.GetLocations();
        Dictionary<int, string> roleNames = _dropdownDal.GetRoles();
        Dictionary<int, string> departmentNames = _dropdownDal.GetDepartments();
        Dictionary<int, string> managerNames = _dropdownDal.GetManagers();
        Dictionary<int, string> projectNames = _dropdownDal.GetProjects();
        Dictionary<int, string> statusNames = _dropdownDal.GetStatus();

        employees = _employeeDal.RetrieveAll();
        if (employees == null || employees.Count == 0)
        {
            return [];
        }
        employeeDetailsList = employees.Select(employee => PopulateEmployeeDetails(employee, locationNames!, roleNames!, departmentNames!, managerNames!, projectNames!, statusNames!)).ToList();
        return employeeDetailsList;
    }

    private static EmployeeDetails PopulateEmployeeDetails(Employee employee, Dictionary<int, string> locationNames, Dictionary<int, string> roleNames, Dictionary<int, string> departmentNames, Dictionary<int, string> managerNames, Dictionary<int, string> projectNames, Dictionary<int, string> statusNames)
    {
        EmployeeDetails employeeDetails = new EmployeeDetails()
        {
            EmpNo = employee.EmpNo,
            StatusId = employee.StatusId,
            StatusName = statusNames.TryGetValue(employee.StatusId, out string? statusValue) ? statusValue : null,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Dob = employee.Dob,
            Email = employee.Email,
            MobileNumber = employee.MobileNumber,
            LocationId = employee.LocationId,
            LocationName = employee.LocationId.HasValue && locationNames.TryGetValue(employee.LocationId.Value, out string? locationValue) ? locationValue : null,
            JoiningDate = employee.JoiningDate,
            RoleId = employee.RoleId,
            RoleName = employee.RoleId.HasValue && roleNames.TryGetValue(employee.RoleId.Value, out string? roleValue) ? roleValue : null,
            DepartmentId = employee.DepartmentId,
            DepartmentName = employee.DepartmentId.HasValue && departmentNames.TryGetValue(employee.DepartmentId.Value, out string? departmentValue) ? departmentValue : null,
            AssignManagerId = employee.AssignManagerId,
            AssignManagerName = employee.AssignManagerId.HasValue && managerNames.TryGetValue(employee.AssignManagerId.Value, out string? managerValue) ? managerValue : null,
            AssignProjectId = employee.AssignProjectId,
            AssignProjectName = employee.AssignProjectId.HasValue && projectNames.TryGetValue(employee.AssignProjectId.Value, out string? projectValue) ? projectValue : null
        };
        return employeeDetails;
    }
}

