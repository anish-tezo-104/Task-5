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
    private static JSONUtils jsonUtils;
    private static readonly ILogger _logger;
    private static readonly string employeeFilePath = GetIConfiguration()["environmentVariables:EMPLOYEE__JSON__PATH"];

    public static partial Employee GetEmployeeDataFromUser();
    public static partial void PrintEmployeesDetails(List<EmployeeDetails> employees);
    public static partial IConfiguration GetIConfiguration();
    public static partial EmployeeFilters? GetEmployeeFiltersFromConsole();
    public static partial EmployeeFilters? GetSearchKeywordFromConsole();
    public static partial Employee GetUpdatedDataFromUser();

    static EMS()
    {
        jsonUtils = new JSONUtils();
        _logger = new ConsoleLogger();
        _employeeDal = new EmployeeDAL(_logger, jsonUtils, employeeFilePath);
        _employeeBal = new EmployeeBAL(_logger, _employeeDal);
    }

    public static int Main(string[] args)
    {
        var rootCommand = new RootCommand("Employee Management System");
        return HandleCommandArgs(args, rootCommand);
    }

    public static void DisplayAvailableCommands(RootCommand rootCommand)
    {
        _logger.LogInfo("\nAvailable Commands:");
        foreach (var command in rootCommand.Children)
        {
            if (command.Name != "o")
            {
                _logger.LogInfo($"{command.Name}: {command.Description}");
            }
        }
    }

    public static void AddEmployee()
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
            _logger.LogError(Constants.AddEmployeeError);
        }
    }

    public static void GetAllEmployees()
    {
        try
        {
            List<EmployeeDetails> employees = _employeeBal.GetAll();
            if (employees != null && employees.Count > 0)
            {
                _logger.LogSuccess($"{employees.Count} {Constants.RetrieveAllEmployeesSuccess}");
                PrintEmployeesDetails(employees);
            }
        }
        catch (Exception)
        {

            _logger.LogError(Constants.RetrieveAllEmployeesError);
        }
    }

    public static void DeleteEmployee(string empNo)
    {
        bool status = _employeeBal.DeleteEmployees(empNo);
        if (status)
        {
            _logger.LogSuccess(Constants.DeleteEmployeeSuccess);

        }
        else
        {
            _logger.LogError(Constants.DeleteEmployeeError);
        }
    }

    public static void UpdateEmployee(string empNo)
    {
        List<EmployeeDetails> employees;
        try
        {
            var filters = new EmployeeFilters { Search = empNo };
            employees = _employeeBal.SearchEmployees(filters);
        }
        catch (Exception)
        {
            _logger.LogError(Constants.SearchEmployeeError);
            return;
        }

        if (employees == null || employees.Count == 0)
        {
            _logger.LogSuccess(Constants.EmpNoNotFound);
            return;
        }

        PrintEmployeesDetails(employees);
        _logger.LogInfo("\nPress 'Enter' to keep the original value.");
        _logger.LogInfo("\nEnter '--d' to delete the original value.\n");
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
                _logger.LogError(Constants.UpdateEmployeeError);
            }
        }
        catch (Exception)
        {
            _logger.LogError(Constants.UpdateEmployeeError);
        }
    }

    public static void SearchEmployee()
    {
        EmployeeFilters? keyword = GetSearchKeywordFromConsole();
        if (keyword == null)
        {
            _logger.LogError(Constants.SearchNoKeywordError);
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
            _logger.LogError(Constants.SearchEmployeeError);
        }
    }

    public static void FilterEmployees()
    {
        EmployeeFilters? filters = GetEmployeeFiltersFromConsole();
        if (filters == null)
        {
            _logger.LogError(Constants.GetEmployeeFiltersFromConsoleError);
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
            _logger.LogError(Constants.FilterEmployeesError);
        }
        ResetFilters(filters);
    }

    public static void CountEmployees()
    {
        int count = _employeeBal.CountEmployees();
        _logger.LogSuccess($"{Constants.CountEmployeesSuccess} {count}");
    }

    private static int HandleCommandArgs(string[] args, RootCommand rootCommand)
    {
        var addCommand = new Command("--add", "Add a new employee")
        {
            Handler = CommandHandler.Create(() => AddEmployee())
        };
        var showCommand = new Command("--show", "Show employees list")
        {
            Handler = CommandHandler.Create(() => GetAllEmployees())
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
}
