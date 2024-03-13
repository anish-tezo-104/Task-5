using EmployeeManagementSystem.DAL;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Utils;
namespace EmployeeManagementSystem.BAL;

public class EmployeeBAL : IEmployeeBAL
{
    public readonly IEmployeeDAL employeeDal;
    public readonly ILogger logger;

    public EmployeeBAL(ILogger logger, IEmployeeDAL employeeDal)
    {
        this.employeeDal = employeeDal;
        this.logger = logger;
    }
    public (List<EmployeeDetails>? employees, bool status) GetAllEmployees()
    {
        bool status;
        List<EmployeeDetails> employees = new List<EmployeeDetails>();
        try
        {
            (employees, status) = employeeDal.RetrieveAllEmployees();
            if (!status && (employees == null || employees.Count == 0))
            {
                logger.LogError("No Employee(s) Found");
                return (null, true);
            }
        }
        catch (Exception)
        {
            status = false;
        }
        return (employees, status);
    }

    public bool AddEmployee(Employee employee)
    {
        bool status;
        try
        {
            status = employeeDal.Insert(employee);
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
            status = employeeDal.Delete(empNo);
            if (status)
            {
                logger.LogSuccess("Employee deleted successfully.");
            }
            else
            {
                logger.LogError("Employee not found. Unable to delete.");
            }
            status = true;
        }
        catch (Exception)
        {

            status = false;
        }

        return status;
    }

    public bool UpdateEmployee(Employee updatedEmployee, string empNo)
    {
        bool status;
        try
        {
            status = employeeDal.Update(updatedEmployee, empNo);
            if (status)
            {
                logger.LogSuccess("Employee data updated successfully.");
            }
        }
        catch (Exception)
        {
            status = false;
        }
        return status;
    }

    public (List<EmployeeDetails>? employees, bool status) SearchEmployees(List<string> keywords)
    {
        bool status;
        List<EmployeeDetails> employees = new List<EmployeeDetails>();
        try
        {
            (employees, status) = employeeDal.SearchOrFilter(keywords, null);
            if (status && employees != null)
            {
                logger.LogSuccess($"Found {employees.Count} employees.");
            }
            else
            {
                logger.LogSuccess("No employee(s) Found");
            }
            status = true;
        }
        catch (Exception)
        {
            status = false;
        }

        return (employees, status);
    }

    public (List<EmployeeDetails>? employees, bool status) FilterEmployees(EmployeeFilters filters)
    {
        bool status;
        List<EmployeeDetails> employees = new List<EmployeeDetails>();
        try
        {
            (employees, status) = employeeDal.SearchOrFilter(null, filters);
            if (status && employees != null)
            {
                logger.LogSuccess($"Filtering completed successfully.\nFound {employees.Count} matching employees.\n");
            }
            else
            {
                logger.LogSuccess("Filtering completed successfully.\nNo matching employees found.\n");
            }
            status = true;
        }
        catch (Exception)
        {
            status = false;
        }

        return (employees, status);
    }

    public int CountEmployees()
    {
        return employeeDal.Count();
    }
}

