using EmployeeManagementSystem.Utils;
using EmployeeManagementSystem.Models;
namespace EmployeeManagementSystem.DAL;
public class EmployeeDAL : IEmployeeDAL
{
    public string filePath = "";
    public readonly JSONUtils jsonUtils;
    public readonly ILogger _logger;

    public EmployeeDAL(ILogger logger, JSONUtils jsonUtils, string filePath)
    {
        this.jsonUtils = jsonUtils;
        _logger = logger;
        this.filePath = filePath;
    }

    public bool Insert(Employee employee)
    {
        List<Employee> existingEmployees = jsonUtils.ReadJSON<Employee>(filePath);
        existingEmployees.Add(employee);
        jsonUtils.WriteJSON(existingEmployees, filePath);
        return true;
    }

    public (List<EmployeeDetails>? employees, bool status) RetrieveAllEmployees()
    {
        List<Employee> existingEmployees = jsonUtils.ReadJSON<Employee>(filePath);
        if (existingEmployees == null || existingEmployees.Count == 0)
        {
            return (null, false);
        }
        List<EmployeeDetails> employeeDetailsList = new List<EmployeeDetails>();

        foreach (var employee in existingEmployees)
        {
            EmployeeDetails employeeDetails = ConvertToEmployeeDetails(employee);
            employeeDetailsList.Add(employeeDetails);
        }
        return (employeeDetailsList, true);
    }

    public bool Update(Employee updatedEmployee, string empNo)
    {
        List<Employee> existingEmployees = jsonUtils.ReadJSON<Employee>(filePath);
        Employee employeeToUpdate = existingEmployees.FirstOrDefault(emp => emp.EmpNo == empNo);

        if (employeeToUpdate == null)
        {
            return false;
        }

        // Update employee fields
        employeeToUpdate.FirstName = updatedEmployee.FirstName == "--d" ? null :
                                     string.IsNullOrWhiteSpace(updatedEmployee.FirstName) ? employeeToUpdate.FirstName :
                                     updatedEmployee.FirstName;
        employeeToUpdate.LastName = updatedEmployee.LastName == "--d" ? null :
                                    string.IsNullOrWhiteSpace(updatedEmployee.LastName) ? employeeToUpdate.LastName :
                                    updatedEmployee.LastName;
        employeeToUpdate.Dob = updatedEmployee.Dob == DateTime.MinValue ? null :
                            updatedEmployee.Dob ?? employeeToUpdate.Dob;
        employeeToUpdate.Email = updatedEmployee.Email == "--d" ? null :
                                 string.IsNullOrWhiteSpace(updatedEmployee.Email) ? employeeToUpdate.Email :
                                 updatedEmployee.Email;
        employeeToUpdate.MobileNumber = updatedEmployee.MobileNumber == "--d" ? null :
                                         string.IsNullOrWhiteSpace(updatedEmployee.MobileNumber) ? employeeToUpdate.MobileNumber :
                                         updatedEmployee.MobileNumber;
        employeeToUpdate.JoiningDate = updatedEmployee.JoiningDate == DateTime.MinValue ? null :
                                        updatedEmployee.JoiningDate ?? updatedEmployee.JoiningDate;
        employeeToUpdate.LocationId = updatedEmployee.LocationId == -1 ? null :
                                   updatedEmployee.LocationId ?? employeeToUpdate.LocationId;
        employeeToUpdate.JobTitle = updatedEmployee.JobTitle == "--d" ? null :
                                    string.IsNullOrWhiteSpace(updatedEmployee.JobTitle) ? employeeToUpdate.JobTitle :
                                    updatedEmployee.JobTitle;
        employeeToUpdate.DepartmentId = updatedEmployee.DepartmentId == -1 ? null :
                                     updatedEmployee.DepartmentId ?? employeeToUpdate.DepartmentId;
        employeeToUpdate.AssignManager = updatedEmployee.AssignManager == "--d" ? null :
                                          string.IsNullOrWhiteSpace(updatedEmployee.AssignManager) ? employeeToUpdate.AssignManager :
                                          updatedEmployee.AssignManager;
        employeeToUpdate.AssignProject = updatedEmployee.AssignProject == "--d" ? null :
                                          string.IsNullOrWhiteSpace(updatedEmployee.AssignProject) ? employeeToUpdate.AssignProject :
                                          updatedEmployee.AssignProject;

        int index = existingEmployees.FindIndex(emp => emp.EmpNo == empNo);
        existingEmployees[index] = employeeToUpdate;
        jsonUtils.WriteJSON(existingEmployees, filePath);
        return true;
    }

    public bool Delete(string empNo)
    {
        List<Employee> existingEmployees = jsonUtils.ReadJSON<Employee>(filePath);
        Employee employeeToDelete = existingEmployees.FirstOrDefault(employee => employee.EmpNo == empNo);
        if (employeeToDelete != null)
        {
            existingEmployees.Remove(employeeToDelete);
            jsonUtils.WriteJSON(existingEmployees, filePath);
            return true;
        }
        else
        {
            return false;
        }
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
            LocationName = employee.LocationId.HasValue ? ((Location)employee.LocationId).ToString() : "--",
            JoiningDate = employee.JoiningDate,
            JobTitle = employee.JobTitle,
            DepartmentId = employee.DepartmentId,
            DepartmentName = employee.DepartmentId.HasValue ? ((Department)employee.DepartmentId).ToString() : "--",
            AssignManager = employee.AssignManager,
            AssignProject = employee.AssignProject
        };
        return employeeDetails;
    }

    public (List<EmployeeDetails>? results, bool status) SearchOrFilter(List<string>? keywords = null, EmployeeFilters? filters = null)
    {
        List<Employee> employees = jsonUtils.ReadJSON<Employee>(filePath);

        List<EmployeeDetails> employeeDetailsList = employees.Select(ConvertToEmployeeDetails).ToList();

        // Search logic
        if (keywords != null && keywords.Any())
        {
            List<EmployeeDetails> foundEmployees = employeeDetailsList
                .Where(employee =>
                    keywords.Any(kw =>
                        EmployeeContainsSearchTerm(employee, kw)
                    )
                ).ToList();

            if (foundEmployees.Count == 0)
            {
                return (null, false);
            }

            return (foundEmployees, true);
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
            return (null, false);
        }

        return (employeeDetailsList, true);
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

    public int Count()
    {
        List<Employee> existingEmployees = jsonUtils.ReadJSON<Employee>(filePath);
        int count = existingEmployees.Count;
        return count;
    }
}