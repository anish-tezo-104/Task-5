using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Microsoft.Extensions.Configuration;
using EmployeeManagementSystem.Utils;
using EmployeeManagementSystem.DAL;
using EmployeeManagementSystem.BAL;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem;

public partial class EMS
{
    private readonly static IEmployeeDAL _employeeDal;
    private readonly static IRoleDAL _roleDal;
    private readonly static IDropdownDAL _dropdownDal;
    private readonly static IEmployeeBAL _employeeBal;
    private readonly static IDropdownBAL _dropdownBal;
    private readonly static IRoleBAL _roleBal;
    private static JSONUtils _jsonUtils;
    private static readonly ILogger _logger;
    private static readonly IConfiguration _configuration;

    private static partial Employee GetEmployeeDataFromConsole();
    private static partial Role GetRoleDataFromConsole();

    private static partial void PrintEmployeesDetails(List<EmployeeDetails> employees);
    private static partial IConfiguration GetIConfiguration();
    private static partial EmployeeFilters? GetEmployeeFiltersFromConsole();
    private static partial EmployeeFilters? GetSearchKeywordFromConsole();
    private static partial Employee GetUpdatedDataFromUser();
    private static partial void PrintRoles(List<Role> roles);

    static EMS()
    {
        _configuration = GetIConfiguration();
        _logger = new ConsoleLogger();
        _jsonUtils = new JSONUtils();
        _dropdownDal = new DropdownDAL(_configuration);
        _dropdownBal = new DropdownBAL(_dropdownDal);
        _employeeDal = new EmployeeDAL(_logger, _jsonUtils, _configuration);
        _roleDal = new RoleDAL(_logger, _jsonUtils, _configuration);
        _employeeBal = new EmployeeBAL(_logger, _employeeDal, _dropdownBal);
        _roleBal = new RoleBAL(_logger, _roleDal, _dropdownDal);
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
        var updateEmployeesCommand = new Command("--update-emp", "Update an employee detail")
        {
            new Argument<string>("empNo", "EmployeeNumber")
        };
        updateEmployeesCommand.Handler = CommandHandler.Create((string empNo) => UpdateEmployee(empNo));

        var searchEmployeesCommand = new Command("--search-emp", "Search an employee details [Employee Number, Employee Name]")
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

        var showRolesCommand = new Command("--show-role", "Show roles list")
        {
            Handler = CommandHandler.Create(() => DisplayRoles())
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
        rootCommand.AddCommand(showRolesCommand);

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
        try
        {
            Employee employee = GetEmployeeDataFromConsole();
            if (employee == null)
            {
                PrintError(Constants.GettingDataFromConsoleError);
                return;
            }
            bool status = _employeeBal.AddEmployee(employee);
            if (status)
            {
                PrintSuccess(Constants.AddEmployeeSuccess);
            }
            else
            {
                PrintError(Constants.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"{Constants.ErrorMessage} : {ex.Message}");
        }
    }

    private static void DisplayEmployees()
    {
        Console.WriteLine(_configuration["BasePath"] + _configuration["LocationJsonPath"]);
        try
        {
            List<EmployeeDetails> employees = _employeeBal.GetAll();
            if (employees != null)
            {
                PrintSuccess($"{employees.Count} {Constants.RetrieveAllEmployeesSuccess}");
                if (employees.Count > 0)
                {
                    PrintEmployeesDetails(employees);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"{Constants.ErrorMessage} : {ex.Message}");
        }
    }

    private static void DeleteEmployee(string empNo)
    {
        try
        {
            List<EmployeeDetails> employees;
            var filters = new EmployeeFilters { Search = empNo };
            employees = _employeeBal.SearchEmployees(filters);

            if (employees == null || employees.Count == 0)
            {
                PrintSuccess(Constants.EmpNoNotFound);
                return;
            }

            bool status = _employeeBal.DeleteEmployees(empNo);
            if (status)
            {
                PrintSuccess(Constants.DeleteEmployeeSuccess);
            }
            else
            {
                PrintError(Constants.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"{Constants.ErrorMessage} : {ex.Message}");
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
        catch (Exception ex)
        {
            _logger.LogError($"{Constants.ErrorMessage}: {ex.Message}");
            return;
        }

        if (employees == null || employees.Count == 0)
        {
            PrintSuccess(Constants.EmpNoNotFound);
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
                PrintSuccess(Constants.UpdateEmployeeSuccess);
            }
            else
            {
                PrintError(Constants.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"{Constants.ErrorMessage} : {ex.Message}");
        }
    }

    private static void SearchEmployee()
    {
        EmployeeFilters? keyword = GetSearchKeywordFromConsole();
        if (keyword == null)
        {
            PrintError(Constants.ErrorMessage);
            return;
        }
        try
        {
            List<EmployeeDetails> employees = _employeeBal.SearchEmployees(keyword);
            if (employees != null)
            {
                PrintSuccess($"{Constants.SearchEmployeeSuccess} {employees.Count}");
                if (employees.Count > 0)
                {
                    PrintEmployeesDetails(employees);
                }

            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"{Constants.ErrorMessage} : {ex.Message}");
        }
    }

    private static void FilterEmployees()
    {
        EmployeeFilters? filters = GetEmployeeFiltersFromConsole();
        if (filters == null)
        {
            PrintError(Constants.GettingDataFromConsoleError);
            return;
        }
        try
        {
            List<EmployeeDetails> employees = _employeeBal.FilterEmployees(filters);
            if (employees != null)
            {
                PrintSuccess($"{Constants.FilterEmployeesSuccess} {employees.Count}");
                if (employees.Count > 0)
                {
                    PrintEmployeesDetails(employees);
                }

            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"{Constants.ErrorMessage} : {ex.Message}");
        }
        ResetFilters(filters);
    }

    private static void CountEmployees()
    {
        try
        {
            int count = _employeeBal.CountEmployees();
            PrintSuccess($"{Constants.CountEmployeesSuccess} {count}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"{Constants.ErrorMessage} : {ex.Message}");
        }
    }

    // Roles Command Functions
    private static void AddRoles()
    {
        try
        {
            Role role = GetRoleDataFromConsole();
            if (role == null)
            {
                PrintError(Constants.GettingDataFromConsoleError);
                return;
            }
            bool status = _roleBal.AddRole(role);
            if (status)
            {
                PrintSuccess(Constants.AddRoleSuccess);
            }
            else
            {
                PrintError(Constants.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"{Constants.ErrorMessage}: {ex.Message}");
        }
    }

    private static void DisplayRoles()
    {
        try
        {
            List<Role> roles = _roleBal.GetAll();
            if (roles != null)
            {
                PrintSuccess($"{roles.Count} {Constants.RetrieveAllRolesSuccess}");
                if (roles.Count > 0)
                {
                    PrintRoles(roles);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"{Constants.ErrorMessage} : {e.Message}");
        }
    }
}
