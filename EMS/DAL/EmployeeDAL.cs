using EmployeeManagementSystem.Utils;
using EmployeeManagementSystem.Models;
using Microsoft.Extensions.Configuration;
namespace EmployeeManagementSystem.DAL;

public class EmployeeDAL : IEmployeeDAL
{
    private readonly string _filePath = "";
    private readonly JSONUtils _jsonUtils;
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;

    public EmployeeDAL(ILogger logger, JSONUtils jsonUtils, IConfiguration configuration)
    {
        _jsonUtils = jsonUtils;
        _logger = logger;
        _configuration = configuration;
        _filePath = _configuration["BasePath"] + _configuration["EmployeesJsonPath"];
    }

    public bool Insert(Employee employee)
    {
        List<Employee> existingEmployees = _jsonUtils.ReadJSON<Employee>(_filePath);
        existingEmployees.Add(employee);
        try
        {
            _jsonUtils.WriteJSON(existingEmployees, _filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error writing to file '{_filePath}': {ex.Message}");
            return false;
        }
        return true;
    }

    public List<EmployeeDetails>? RetrieveAll()
    {
        List<EmployeeDetails> employees = _jsonUtils.ReadJSON<EmployeeDetails>(_filePath);
        if (employees == null || employees.Count == 0)
        {
            return new List<EmployeeDetails>();
        }
        return employees;
    }

    public bool Update(string empNo, Employee employee)
    {
        List<Employee> existingEmployees = _jsonUtils.ReadJSON<Employee>(_filePath);
        Employee dbEmployee = existingEmployees.FirstOrDefault(emp => emp.EmpNo == empNo) ?? throw new Exception("Employee not found.");
        dbEmployee.FirstName = GetUpdatedValue(employee.FirstName, dbEmployee.FirstName);
        dbEmployee.LastName = GetUpdatedValue(employee.LastName, dbEmployee.LastName);
        dbEmployee.Dob = employee.Dob ?? dbEmployee.Dob;
        dbEmployee.Email = GetUpdatedValue(employee.Email, dbEmployee.Email);
        dbEmployee.MobileNumber = GetUpdatedValue(employee.MobileNumber, dbEmployee.MobileNumber);
        dbEmployee.JoiningDate = employee.JoiningDate ?? dbEmployee.JoiningDate;
        dbEmployee.LocationId = GetUpdatedValue(employee.LocationId, dbEmployee.LocationId);
        dbEmployee.RoleId = GetUpdatedValue(employee.RoleId, dbEmployee.RoleId);
        dbEmployee.DepartmentId = GetUpdatedValue(employee.DepartmentId, dbEmployee.DepartmentId);
        dbEmployee.AssignManagerId = GetUpdatedValue(employee.AssignManagerId, dbEmployee.AssignManagerId);
        dbEmployee.AssignProjectId = GetUpdatedValue(employee.AssignProjectId, dbEmployee.AssignProjectId);
        try
        {
            _jsonUtils.WriteJSON(existingEmployees, _filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error writing to file '{_filePath}': {ex.Message}");
            return false;
        }
        return true;
    }

    public bool Delete(string empNo)
    {
        List<Employee> existingEmployees = _jsonUtils.ReadJSON<Employee>(_filePath);
        Employee employeeToDelete = existingEmployees.FirstOrDefault(employee => employee.EmpNo == empNo);

        if (employeeToDelete != null)
        {
            existingEmployees.Remove(employeeToDelete);
            try
            {
                _jsonUtils.WriteJSON(existingEmployees, _filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error writing to file '{_filePath}': {ex.Message}");
                return false;
            }
        }
        return true;
    }

    public List<EmployeeDetails>? Filter(EmployeeFilters? filters)
    {
        List<EmployeeDetails> employees = RetrieveAll();
        if (employees == null || employees.Count == 0)
        {
            return [];
        }
        if (filters != null)
        {
            // Apply filters
            employees = ApplyFilter(employees, filters);

            // Apply search
            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                employees = ApplySearch(employees, filters.Search);
            }
        }

        return employees;
    }

    public int Count()
    {
        List<Employee> existingEmployees = _jsonUtils.ReadJSON<Employee>(_filePath);
        int count = existingEmployees.Count;
        return count;
    }

    private static List<EmployeeDetails> ApplyFilter(List<EmployeeDetails> employees, EmployeeFilters filters)
    {
        List<Func<Employee, bool>> filterConditions = [];

        if (filters.Alphabet != null && filters.Alphabet.Count != 0)
        {
            filterConditions.Add(e => e.FirstName != null && filters.Alphabet.Contains(char.ToLower(e.FirstName[0])));
        }

        if (filters.Locations != null && filters.Locations.Count != 0)
        {
            filterConditions.Add(e => e.LocationId.HasValue && filters.Locations.Contains(e.LocationId.Value));
        }

        if (filters.Departments != null && filters.Departments.Count != 0)
        {
            filterConditions.Add(e => e.DepartmentId.HasValue && filters.Departments.Contains(e.DepartmentId.Value));
        }

        if (filters.Status != null && filters.Status.Count != 0)
        {
            filterConditions.Add(e => filters.Status.Contains(e.StatusId));
        }

        return employees.Where(e => filterConditions.All(condition => condition(e))).ToList();
    }

    private static List<EmployeeDetails> ApplySearch(List<EmployeeDetails> employees, string searchKeyword)
    {
        if (string.IsNullOrWhiteSpace(searchKeyword))
        {
            return employees;
        }

        string keyword = searchKeyword.ToLower();
        return employees.Where(e =>
            e.EmpNo?.Contains(keyword, StringComparison.CurrentCultureIgnoreCase) == true ||
            e.FirstName?.Contains(keyword, StringComparison.CurrentCultureIgnoreCase) == true ||
            e.LastName?.Contains(keyword, StringComparison.CurrentCultureIgnoreCase) == true
            ).ToList();
    }

    private static T? GetUpdatedValue<T>(T? newValue, T? oldValue)
    {
        if (newValue == null || (newValue is string && string.IsNullOrEmpty((string)(object)newValue)))
        {
            return oldValue;
        }
        return newValue;
    }
}