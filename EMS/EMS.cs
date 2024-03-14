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
    internal static JSONUtils jsonUtils;
    private static readonly ILogger _logger;

    //Defining functions present in partial class
    public static partial Employee GetEmployeeDataFromUser();

    public static partial void PrintEmployeesDetails(List<EmployeeDetails> employees);

    public static partial IConfiguration GetIConfiguration();

    public static partial EmployeeFilters? GetEmployeeFiltersFromConsole();

    public static partial EmployeeFilters? GetSearchKeywordFromConsole();

    public static partial Employee GetUpdatedDataFromUser();

    internal static string EmployeeFilePath = GetIConfiguration()["environmentVariables:EMPLOYEE__JSON__PATH"];

    static EMS()
    {
        jsonUtils = new JSONUtils();
        _logger = new ConsoleLogger();
        _employeeDal = new EmployeeDAL(_logger, jsonUtils, EmployeeFilePath);
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
            _logger.LogError("Error retrieving employee data from console.");
            return;
        }
        bool status = _employeeBal.AddEmployee(employee);
        if (status)
        {
            _logger.LogSuccess("Employee added successfully.");
        }
        else
        {
            _logger.LogError("Some error occur while adding employee. Please try again!");
        }
    }

    public static void GetAllEmployees()
    {
        try
        {
            List<EmployeeDetails> employees = _employeeBal.GetAll();
            if (employees != null && employees.Count > 0)
            {
                _logger.LogSuccess($"Found {employees.Count} employees : \n");
                PrintEmployeesDetails(employees);
            }
        }
        catch (Exception ex)
        {

            _logger.LogError($"Error while retrieving employees: {ex.Message}");
        }
    }

    public static void DeleteEmployee(string empNo)
    {
        if (empNo == null)
        {
            _logger.LogError("Please provide the employee number.");
            return;
        }
        bool status = _employeeBal.DeleteEmployees(empNo);
        if (status)
        {
            _logger.LogSuccess("Employee deleted successfully.");

        }
        else
        {
            _logger.LogError("Some error occur while deleting employee. Please try again!");
        }
    }

    public static void UpdateEmployee(string empNo)
    {
        List<EmployeeDetails> employees = [];
        try
        {
            var filters = new EmployeeFilters { Search = empNo };
            employees = _employeeBal.SearchEmployees(filters);
        }
        catch (Exception)
        {
            _logger.LogError("Error occurred while searching for employees. Please try again!");
            return;
        }

        if (employees == null || employees.Count == 0)
        {
            _logger.LogSuccess("No employee found with the provided employee number.");
            return;
        }

        PrintEmployeesDetails(employees);
        _logger.LogInfo("\nPress 'Enter' to keep the original value.\n");
        Employee updatedEmployee = GetUpdatedDataFromUser();
        try
        {
            bool status = _employeeBal.UpdateEmployee(empNo, updatedEmployee);
            if (status)
            {
                _logger.LogSuccess("Employee data updated successfully.");
            }
            else
            {
                _logger.LogError("Error occurred while updating employee data.");
            }
        }
        catch (Exception)
        {
            _logger.LogError("Error occurred while updating employee data.");
        }
    }

    public static void SearchEmployee()
    {
        EmployeeFilters? keyword = GetSearchKeywordFromConsole();
        if (keyword == null)
        {
            _logger.LogError("No keyword provided for search.");
            return;
        }
        try
        {
            List<EmployeeDetails> employees = _employeeBal.SearchEmployees(keyword);
            if (employees != null && employees.Count > 0)
            {
                _logger.LogSuccess($"Found {employees.Count} employees.");
                PrintEmployeesDetails(employees);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while searching employees: {ex.Message}");
        }
    }

    public static void FilterEmployees()
    {
        EmployeeFilters? filters = GetEmployeeFiltersFromConsole();
        if (filters == null)
        {
            _logger.LogError("Error retrieving employee filters from console.");
            return;
        }
        try
        {
            List<EmployeeDetails> employees = _employeeBal.FilterEmployees(filters);
            if (employees != null && employees.Count > 0)
            {
                PrintEmployeesDetails(employees);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Some error occur while filtering employees: {ex.Message}");
        }
        ResetFilters(filters);
    }

    public static void CountEmployees()
    {
        int count = _employeeBal.CountEmployees();
        _logger.LogSuccess($"Total Employees: {count}");
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
