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
    private readonly static IEmployeeBAL _employeeBal;
    private static JSONUtils _jsonUtils;
    private static readonly ILogger _logger;
    private static readonly string _employeeJsonPath;

    public static partial Employee GetEmployeeDataFromUser();
    public static partial void PrintEmployeesDetails(List<EmployeeDetails> employees);
    public static partial IConfiguration GetIConfiguration();
    public static partial EmployeeFilters? GetEmployeeFiltersFromConsole();
    public static partial EmployeeFilters? GetSearchKeywordFromConsole();
    public static partial Employee GetUpdatedDataFromUser();

    static EMS()
    {
        _jsonUtils = new JSONUtils();
        _logger = new ConsoleLogger();
        _employeeJsonPath = GetIConfiguration()["EmployeeJsonPath"];
        _employeeDal = new EmployeeDAL(_logger, _jsonUtils, _employeeJsonPath);
        _employeeBal = new EmployeeBAL(_logger, _employeeDal);
    }

    public static int Main(string[] args)
    {
        var rootCommand = new RootCommand("Employee Management System");
        return HandleCommandArgs(args, rootCommand);
    }

    private static int HandleCommandArgs(string[] args, RootCommand rootCommand)
    {
        var addCommand = new Command("--add", "Add a new employee")
        {
            Handler = CommandHandler.Create(() => AddEmployee())
        };
        var showCommand = new Command("--show", "Show employees list")
        {
            Handler = CommandHandler.Create(() => DisplayEmployees())
        };
        var filterCommand = new Command("--filter", "Filter employees list")
        {
            Handler = CommandHandler.Create(() => FilterEmployees())
        };
        var deleteCommand = new Command("--delete", "Delete an employee [Input : Employee Number]")
        {
            new Argument<string>("empNo", "EmployeeNumber")
        };
        deleteCommand.Handler = CommandHandler.Create((string empNo) => DeleteEmployee(empNo));

        var updateCommand = new Command("--update", "Update an employee detail"){
            new Argument<string>("empNo", "EmployeeNumber")
        };
        updateCommand.Handler = CommandHandler.Create((string empNo) => UpdateEmployee(empNo));

        var searchCommand = new Command("--search", "Search an employee details")
        {
            Handler = CommandHandler.Create(() => SearchEmployee())
        };

        var countEmployees = new Command("--count", "Count of employees")
        {
            Handler = CommandHandler.Create(() => CountEmployees())
        };

        rootCommand.AddOption(new Option<string>("-o", "Display all operations"));
        rootCommand.AddCommand(addCommand);
        rootCommand.AddCommand(showCommand);
        rootCommand.AddCommand(deleteCommand);
        rootCommand.AddCommand(updateCommand);
        rootCommand.AddCommand(searchCommand);
        rootCommand.AddCommand(filterCommand);
        rootCommand.AddCommand(countEmployees);

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
        Employee employee = GetEmployeeDataFromUser();
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
            if (employees != null && employees.Count > 0)
            {
                _logger.LogSuccess($"{employees.Count} {Constants.SearchEmployeeSuccess}");
                PrintEmployeesDetails(employees);
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
            if (employees != null && employees.Count > 0)
            {
                _logger.LogSuccess(Constants.FilterEmployeesSuccess);
                PrintEmployeesDetails(employees);
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
}
