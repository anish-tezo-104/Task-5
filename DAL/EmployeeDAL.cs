using EmployeeManagementSystem.Utils;
using EmployeeManagementSystem.Models;
namespace EmployeeManagementSystem.DAL;
public class EmployeeDAL : IEmployeeDAL
{
    private readonly string _filePath = "";
    private readonly JSONUtils _jsonUtils;
    public readonly ILogger _logger;

    public EmployeeDAL(ILogger logger, JSONUtils jsonUtils, string filePath)
    {
        _jsonUtils = jsonUtils;
        _logger = logger;
        _filePath = filePath;
    }

    public bool Insert(Employee employee)
    {
        List<Employee> existingEmployees = _jsonUtils.ReadJSON<Employee>(_filePath);
        existingEmployees.Add(employee);
        _jsonUtils.WriteJSON(existingEmployees, _filePath);
        return true;
    }

    public List<EmployeeDetails>? RetrieveAllEmployees()
    {
        List<Employee> existingEmployees = _jsonUtils.ReadJSON<Employee>(_filePath);
        if (existingEmployees == null || existingEmployees.Count == 0)
        {
            throw new Exception("No employee(s) found.");
        }
        List<EmployeeDetails> employeeDetailsList = new List<EmployeeDetails>();

        foreach (var employee in existingEmployees)
        {
            EmployeeDetails employeeDetails = ConvertToEmployeeDetails(employee);
            employeeDetailsList.Add(employeeDetails);
        }
        return employeeDetailsList;
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
        dbEmployee.JobTitle = GetUpdatedValue(employee.JobTitle, dbEmployee.JobTitle);
        dbEmployee.DepartmentId = GetUpdatedValue(employee.DepartmentId, dbEmployee.DepartmentId);
        dbEmployee.AssignManager = GetUpdatedValue(employee.AssignManager, dbEmployee.AssignManager);
        dbEmployee.AssignProject = GetUpdatedValue(employee.AssignProject, dbEmployee.AssignProject);

        _jsonUtils.WriteJSON(existingEmployees, _filePath);
        return true;
    }

    public bool Delete(string empNo)
    {
        List<Employee> existingEmployees = _jsonUtils.ReadJSON<Employee>(_filePath);
        Employee employeeToDelete = existingEmployees.FirstOrDefault(employee => employee.EmpNo == empNo);

        if (employeeToDelete != null)
        {
            existingEmployees.Remove(employeeToDelete);
            _jsonUtils.WriteJSON(existingEmployees, _filePath);
            return true;
        }
        else
        {
            throw new Exception($"Employee with EmpNo '{empNo}' not found.");
        }
    }

    public List<EmployeeDetails>? SearchOrFilter(List<string>? keywords = null, EmployeeFilters? filters = null)
    {
        List<Employee> employees = _jsonUtils.ReadJSON<Employee>(_filePath);

        List<EmployeeDetails> employeeDetailsList = employees.Select(ConvertToEmployeeDetails).ToList();

        // Search logic
        if (keywords != null && keywords.Count > 0)
        {
            List<EmployeeDetails> foundEmployees = employeeDetailsList
                .Where(employee =>
                    keywords.Any(kw =>
                        EmployeeContainsSearchTerm(employee, kw)
                    )
                ).ToList();

            if (foundEmployees.Count == 0)
            {
                throw new Exception("No employees found.");
            }

            return foundEmployees;
        }

        // Filter logic
        if (filters != null)
        {
            if (filters.Alphabet != null && filters.Alphabet.Count != 0)
            {
                employeeDetailsList = employeeDetailsList
                    .Where(e =>
                        e.FirstName != null && filters.Alphabet.Contains(char.ToLower(e.FirstName[0])))
                    .ToList();
            }
            if (filters.Locations != null && filters.Locations.Count != 0)
            {
                employeeDetailsList = employeeDetailsList
                    .Where(e =>
                        e.LocationName != null && filters.Locations
                            .Any(location => location.ToString().Equals(e.LocationName, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }
            if (filters.Departments != null && filters.Departments.Count != 0)
            {
                employeeDetailsList = employeeDetailsList
                    .Where(e =>
                        e.DepartmentName != null && filters.Departments
                            .Any(department => department.ToString().Equals(e.DepartmentName, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }
            if (filters.Status != null && filters.Status.Count != 0)
            {
                employeeDetailsList = employeeDetailsList
                    .Where(e =>
                        e.StatusName != null && filters.Status
                            .Any(status => status.ToString().Equals(e.StatusName, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }
        }

        if (employeeDetailsList.Count == 0)
        {
            throw new Exception("No employees found.");
        }
        return employeeDetailsList;
    }

    public int Count()
    {
        List<Employee> existingEmployees = _jsonUtils.ReadJSON<Employee>(_filePath);
        int count = existingEmployees.Count;
        return count;
    }

    private static bool EmployeeContainsSearchTerm(EmployeeDetails employee, string searchTerm)
    {
        return (employee.EmpNo != null && employee.EmpNo.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
               (employee.FirstName != null && employee.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
               (employee.LastName != null && employee.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
               (employee.Email != null && employee.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
               (employee.MobileNumber != null && employee.MobileNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (employee.LocationName != null && employee.LocationName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
               (employee.JobTitle != null && employee.JobTitle.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
               (employee.DepartmentName != null && employee.DepartmentName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
               (employee.AssignManager != null && employee.AssignManager.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
               (employee.AssignProject != null && employee.AssignProject.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
    }

    private static EmployeeDetails ConvertToEmployeeDetails(Employee employee)
    {

        EmployeeDetails employeeDetails = new()
        {
            EmpNo = employee.EmpNo,
            StatusId = employee.StatusId,
            StatusName = ((Status)employee.StatusId).ToString(),
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Dob = employee.Dob,
            Email = employee.Email,
            MobileNumber = employee.MobileNumber,
            LocationId = employee.LocationId,
            LocationName = employee.LocationId.HasValue ? ((Location)employee.LocationId).ToString() : null,
            JoiningDate = employee.JoiningDate,
            JobTitle = employee.JobTitle,
            DepartmentId = employee.DepartmentId,
            DepartmentName = employee.DepartmentId.HasValue ? ((Department)employee.DepartmentId).ToString() : null,
            AssignManager = employee.AssignManager,
            AssignProject = employee.AssignProject
        };
        return employeeDetails;
    }

    private static T? GetUpdatedValue<T>(T? newValue, T? oldValue)
    {
        return newValue?.Equals(default(T)) == true ? oldValue : newValue;
    }
}