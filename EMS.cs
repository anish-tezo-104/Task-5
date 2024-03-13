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
    private readonly static IEmployeeDAL employeeDal;
    private readonly static IEmployeeBAL employeeBal;
    internal static JSONUtils jsonUtils;
    private static ILogger logger;

    //Defining functions present in partial class
    public static partial Employee GetEmployeeDataFromUser();

    public static partial void PrintEmployeesDetails(List<EmployeeDetails> employees);

    public static partial IConfiguration GetIConfiguration();

    public static partial (bool status, EmployeeFilters?) GetEmployeeFiltersFromConsole();

    public static partial Employee GetUpdatedDataFromUser();

    internal static string EmployeeFilePath = GetIConfiguration()["environmentVariables:EMPLOYEE__JSON__PATH"];

    static EMS()
    {
        jsonUtils = new JSONUtils();
        logger = new ConsoleLogger();
        employeeDal = new EmployeeDAL(logger, jsonUtils, EmployeeFilePath);
        employeeBal = new EmployeeBAL(logger, employeeDal);
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
            new Argument<List<string>>("keywords", "Employee Number, Name, Date of Birth, Location, etc.")
            {
                Arity = ArgumentArity.OneOrMore
            }
        };
        searchCommand.Handler = CommandHandler.Create((List<string> keywords) => SearchEmployee(keywords));

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

    public static void DisplayAvailableCommands(RootCommand rootCommand)
    {
        Console.WriteLine("\nAvailable Commands:");
        foreach (var command in rootCommand.Children)
        {
            if (command.Name != "o")
            {
                logger.LogInfo($"{command.Name}: {command.Description}");
            }
        }
    }

    public static void AddEmployee()
    {
        Employee employee = GetEmployeeDataFromUser();
        if(employee == null)
        {
            logger.LogError("Error retrieving employee data from console.");
            return;
        }
        bool status = employeeBal.AddEmployee(employee);
        if (status)
        {
            logger.LogSuccess("Employee added successfully.");
        }
        else
        {
            logger.LogError("Some error occur while adding employee. Please try again!");
        }
    }

    public static void GetAllEmployees()
    {
        (List<EmployeeDetails> employees, bool status) = employeeBal.GetAllEmployees();
        if (status && employees != null)
        {
            logger.LogSuccess($"Found {employees.Count} employees : \n");
            PrintEmployeesDetails(employees);
        }
        else if (!status)
        {
            logger.LogError("Something Went Wrong. Please try again!");
        }
    }

    public static void DeleteEmployee(string empNo)
    {
        if (empNo == null)
        {
            logger.LogError("Please provide the employee number.");
            return;
        }
        bool status = employeeBal.DeleteEmployees(empNo);
        if (!status)
        {
            logger.LogError("Some error occur while deleting employee. Please try again!");
        }
    }

    public static void UpdateEmployee(string empNo)
    {
        bool status;
        (List<EmployeeDetails> employees, status) = employeeBal.SearchEmployees([empNo]);
        if (status && employees != null && employees.Count > 0)
        {
            PrintEmployeesDetails(employees);
        }
        else if (!status)
        {
            logger.LogError("Something went wrong. Please try again!");
            return;
        }

        if (employees == null || employees.Count == 0)
        {
            return;
        }

        logger.LogInfo("\nPress 'Enter' to keep the original value.\n");
        Employee updatedEmployee = GetUpdatedDataFromUser();

        status = employeeBal.UpdateEmployee(updatedEmployee, empNo);
        if (!status)
        {
            logger.LogError("Some error occur while updating employee data. Please try again");
        }
    }

    public static void SearchEmployee(List<string> keywords)
    {
        if (keywords == null || keywords.Count == 0)
        {
            logger.LogError("No keywords provided for search.");
            return;
        }

        (List<EmployeeDetails> employees, bool status) = employeeBal.SearchEmployees(keywords);
        if (status && employees != null)
        {
            PrintEmployeesDetails(employees);
        }
        else if (!status)
        {
            logger.LogError("Some error occur while searching employees. Please try again!");
        }
    }

    public static void FilterEmployees()
    {
        (bool status, EmployeeFilters? filters) = GetEmployeeFiltersFromConsole();
        if (!status)
        {
            logger.LogError("Error retrieving employee filters from console.");
            if (filters != null)
            {
                ResetFilters(filters);
            }
            return;
        }

        if (filters != null)
        {
            (List<EmployeeDetails> employees, bool filterStatus) = employeeBal.FilterEmployees(filters);
            if (filterStatus && employees != null && employees.Count > 0)
            {
                PrintEmployeesDetails(employees);
            }
            else if (!filterStatus)
            {
                logger.LogError("Some error occur while filtering employees. Please try again!");
                ResetFilters(filters);
                return;
            }
            ResetFilters(filters);
        }
        else
        {
            logger.LogError("Error filtering employees.");
        }
    }

    public static void CountEmployees()
    {
        int count = employeeBal.CountEmployees();
        logger.LogSuccess($"Total Employees: {count}");
    }

}