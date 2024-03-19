using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Microsoft.Extensions.Configuration;
using EmployeeManagementSystem.Utils;
using EmployeeManagementSystem.DAL;
using EmployeeManagementSystem.BAL;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Data;

namespace EmployeeManagementSystem;

public partial class EMS
{
    private readonly static IEmployeeDAL _employeeDal;
    private readonly static IRoleDAL _roleDal;
    private readonly static IEmployeeBAL _employeeBal;
    private readonly static IRoleBAL _roleBal;

    private readonly static DataManager _dataManager;
    private static JSONUtils _jsonUtils;
    private static readonly ILogger _logger;
    private static readonly string _employeeJsonPath;
    private static readonly string _roleJsonPath;

    private static partial Employee GetEmployeeDataFromConsole();
    private static partial Role GetRoleDataFromConsole();

    private static partial void PrintEmployeesDetails(List<EmployeeDetails> employees);
    private static partial IConfiguration GetIConfiguration();
    private static partial EmployeeFilters? GetEmployeeFiltersFromConsole();
    private static partial EmployeeFilters? GetSearchKeywordFromConsole();
    private static partial Employee GetUpdatedDataFromUser();

    static EMS()
    {
        _jsonUtils = new JSONUtils();
        _logger = new ConsoleLogger();
        _dataManager = new DataManager(GetIConfiguration());
        _employeeJsonPath = GetIConfiguration()["EmployeesJsonPath"];
        _roleJsonPath = GetIConfiguration()["RoleJsonPath"];
        _employeeDal = new EmployeeDAL(_logger, _jsonUtils, _employeeJsonPath, _dataManager);
        _roleDal = new RoleDAL(_logger, _jsonUtils, _roleJsonPath, _dataManager);
        _employeeBal = new EmployeeBAL(_logger, _employeeDal);
        _roleBal = new RoleBAL(_logger, _roleDal);
        _roleDal = new RoleDAL(_logger, _jsonUtils, _roleJsonPath, _dataManager);
    }

    public static int Main(string[] args)
    {
        var rootCommand = new RootCommand("Employee Management System");
        return HandleCommandArgs(args, rootCommand);
    }

    private static int HandleCommandArgs(string[] args, RootCommand rootCommand)
    {

        //Employees Command
        var addEmployeesCommand = new Command("--add-emp", "Add a new employee")
        {
            Handler = CommandHandler.Create(() => AddEmployee())
        };
        var showEmployeesCommand = new Command("--show-emp", "Show employees list")
        {
            Handler = CommandHandler.Create(() => DisplayEmployees())
        };
        var filterEmployeesCommand = new Command("--filter-emp", "Filter employees list")
        {
            Handler = CommandHandler.Create(() => FilterEmployees())
        };
        var deleteEmployeesCommand = new Command("--delete-emp", "Delete an employee [Input : Employee Number]")
    {
        new Argument<string>("empNo", "EmployeeNumber")
    };
        deleteEmployeesCommand.Handler = CommandHandler.Create((string empNo) => DeleteEmployee(empNo));

        var updateEmployeesCommand = new Command("--update-emp", "Update an employee detail"){
        new Argument<string>("empNo", "EmployeeNumber")
    };
        updateEmployeesCommand.Handler = CommandHandler.Create((string empNo) => UpdateEmployee(empNo));

        var searchEmployeesCommand = new Command("--search-emp", "Search an employee details")
        {
            Handler = CommandHandler.Create(() => SearchEmployee())
        };

        var countEmployees = new Command("--count-emp", "Count of employees")
        {
            Handler = CommandHandler.Create(() => CountEmployees())
        };

        // Roles Command

        var addRolesCommand = new Command("--add-role", "Add new Role")
        {
            Handler = CommandHandler.Create(() => AddRoles())
        };

        rootCommand.AddOption(new Option<string>("-o", "Display all operations"));
        rootCommand.AddCommand(addEmployeesCommand);
        rootCommand.AddCommand(showEmployeesCommand);
        rootCommand.AddCommand(deleteEmployeesCommand);
        rootCommand.AddCommand(updateEmployeesCommand);
        rootCommand.AddCommand(searchEmployeesCommand);
        rootCommand.AddCommand(filterEmployeesCommand);
        rootCommand.AddCommand(countEmployees);

        rootCommand.AddCommand(addRolesCommand);

        if (args.Length == 1 && (args[0] == "-o" || args[0] == "-options"))
        {
            DisplayAvailableCommands(rootCommand);
            return 0;
        }
        return rootCommand.Invoke(args);
    }

    private static void DisplayAvailableCommands(RootCommand rootCommand)
    {
        PrintConsoleMessage("\nAvailable Commands:");
        foreach (var command in rootCommand.Children)
        {
            if (command.Name != "o")
            {
                PrintConsoleMessage($"{command.Name}: {command.Description}");
            }
        }
    }

    private static void AddEmployee()
    {
        Employee employee = GetEmployeeDataFromConsole();
        if (employee == null)
        {
            _logger.LogError(Constants.GettingDataFromConsoleError);
            return;
        }
        bool status = _employeeBal.AddEmployee(employee);
        if (status)
        {
            _logger.LogSuccess(Constants.AddEmployeeSuccess);
        }
        else
        {
            _logger.LogError(Constants.ExceptionMessage);
        }
    }

    private static void DisplayEmployees()
    {
        try
        {
            List<EmployeeDetails> employees = _employeeBal.GetAll();
            if (employees != null)
            {
                _logger.LogSuccess($"{employees.Count} {Constants.RetrieveAllEmployeesSuccess}");
                if (employees.Count > 0)
                {
                    PrintEmployeesDetails(employees);
                }
            }
        }
        catch (Exception)
        {
            _logger.LogError(Constants.ExceptionMessage);
        }
    }

    private static void DeleteEmployee(string empNo)
    {
        List<EmployeeDetails> employees;
        try
        {
            var filters = new EmployeeFilters { Search = empNo };
            employees = _employeeBal.SearchEmployees(filters);
        }
        catch (Exception)
        {
            _logger.LogError(Constants.ExceptionMessage);
            return;
        }

        if (employees == null || employees.Count == 0)
        {
            _logger.LogSuccess(Constants.EmpNoNotFound);
            return;
        }

        bool status = _employeeBal.DeleteEmployees(empNo);
        if (status)
        {
            _logger.LogSuccess(Constants.DeleteEmployeeSuccess);

        }
        else
        {
            _logger.LogError(Constants.ExceptionMessage);
        }
    }

    private static void UpdateEmployee(string empNo)
    {
        List<EmployeeDetails> employees;
        try
        {
            var filters = new EmployeeFilters { Search = empNo };
            employees = _employeeBal.SearchEmployees(filters);
        }
        catch (Exception)
        {
            _logger.LogError(Constants.ExceptionMessage);
            return;
        }

        if (employees == null || employees.Count == 0)
        {
            _logger.LogSuccess(Constants.EmpNoNotFound);
            return;
        }

        PrintEmployeesDetails(employees);
        PrintConsoleMessage("\nPress 'Enter' to keep the original value.");
        Employee updatedEmployee = GetUpdatedDataFromUser();
        try
        {
            bool status = _employeeBal.UpdateEmployee(empNo, updatedEmployee);
            if (status)
            {
                _logger.LogSuccess(Constants.UpdateEmployeeSuccess);
            }
            else
            {
                _logger.LogError(Constants.ExceptionMessage);
            }
        }
        catch (Exception)
        {
            _logger.LogError(Constants.ExceptionMessage);
        }
    }

    private static void SearchEmployee()
    {
        EmployeeFilters? keyword = GetSearchKeywordFromConsole();
        if (keyword == null)
        {
            _logger.LogError(Constants.ExceptionMessage);
            return;
        }
        try
        {
            List<EmployeeDetails> employees = _employeeBal.SearchEmployees(keyword);
            if (employees != null)
            {
                _logger.LogSuccess($"{Constants.SearchEmployeeSuccess} {employees.Count}");
                if (employees.Count > 0)
                {
                    PrintEmployeesDetails(employees);
                }

            }
        }
        catch (Exception)
        {
            _logger.LogError(Constants.ExceptionMessage);
        }
    }

    private static void FilterEmployees()
    {
        EmployeeFilters? filters = GetEmployeeFiltersFromConsole();
        if (filters == null)
        {
            _logger.LogError(Constants.GettingDataFromConsoleError);
            return;
        }
        try
        {
            List<EmployeeDetails> employees = _employeeBal.FilterEmployees(filters);
            if (employees != null)
            {
                _logger.LogSuccess($"{Constants.FilterEmployeesSuccess} {employees.Count}");
                if (employees.Count > 0)
                {
                    PrintEmployeesDetails(employees);
                }

            }
        }
        catch (Exception)
        {
            _logger.LogError(Constants.ExceptionMessage);
        }
        ResetFilters(filters);
    }

    private static void CountEmployees()
    {
        int count = _employeeBal.CountEmployees();
        _logger.LogSuccess($"{Constants.CountEmployeesSuccess} {count}");
    }

    // Roles Command Functions
    private static void AddRoles()
    {
        Role role = GetRoleDataFromConsole();
        if (role == null)
        {
            _logger.LogError(Constants.GettingDataFromConsoleError);
            return;
        }
        bool status = _roleBal.AddRole(role);
        if (status)
        {
            _logger.LogSuccess(Constants.AddRoleSuccess);
        }
        else
        {
            _logger.LogError(Constants.ExceptionMessage);
        }
    }
}
