using EmployeeManagementSystem.DAL;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Utils;
namespace EmployeeManagementSystem.BAL;

public class EmployeeBAL : IEmployeeBAL
{
    private readonly IEmployeeDAL _employeeDal;
    public readonly ILogger _logger;

    public EmployeeBAL(ILogger logger, IEmployeeDAL employeeDal)
    {
        _employeeDal = employeeDal;
        _logger = logger;
    }

    public List<EmployeeDetails>? GetAll()
    {
        List<EmployeeDetails> employees;
        try
        {
            employees = _employeeDal.RetrieveAll();
        }
        catch (Exception)
        {
            throw;
        }
        return employees;
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
        List<EmployeeDetails> employees;
        try
        {
            employees = _employeeDal.Filter(keyword);
            if (employees == null || employees.Count == 0)
            {
                throw new Exception("No matching employees found.");
            }
        }
        catch (Exception)
        {
            throw;
        }

        return employees;
    }

    public List<EmployeeDetails>? FilterEmployees(EmployeeFilters filters)
    {
        List<EmployeeDetails> employees;
        try
        {
            employees = _employeeDal.Filter(filters);
            if (employees == null || employees.Count == 0)
            {
                throw new Exception("No matching employees found.");
            }
        }
        catch (Exception)
        {
            throw;
        }

        return employees;
    }

    public int CountEmployees()
    {
        return _employeeDal.Count();
    }
}

