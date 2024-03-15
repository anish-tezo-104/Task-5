using Microsoft.Extensions.Configuration;
using EmployeeManagementSystem.Models;
using System.Globalization;

namespace EmployeeManagementSystem;

public delegate bool IsEmpNoDuplicate(string empNo);
public partial class EMS
{
    private static partial Employee GetEmployeeDataFromUser()
    {
        PrintConsoleMessage("Enter employee details:\n", true);
        bool required = true;
        Employee employee = new();
        string empNo = GetDataFromField("Employee Number", required);
        employee.EmpNo = string.IsNullOrWhiteSpace(empNo) ? null : empNo;
        employee.StatusId = 1;
        string firstName = GetDataFromField("First Name", required);
        employee.FirstName = string.IsNullOrWhiteSpace(firstName) ? null : firstName;
        string lastName = GetDataFromField("Last Name");
        employee.LastName = string.IsNullOrWhiteSpace(lastName) ? null : lastName;
        string dob = GetDataFromField("Date of Birth (YYYY-MM-DD)", required);
        DateTime.TryParse(dob, out DateTime dobValue);
        employee.Dob = dobValue;
        string email = GetDataFromField("Email", required);
        employee.Email = string.IsNullOrWhiteSpace(email) ? null : email;
        string mobileNumber = GetDataFromField("Mobile Number");
        employee.MobileNumber = string.IsNullOrWhiteSpace(mobileNumber) ? null : mobileNumber;
        string joiningDate = GetDataFromField("Joining Date (YYYY-MM-DD)", required);
        DateTime.TryParse(joiningDate, out DateTime joiningDateValue);
        employee.JoiningDate = joiningDateValue;
        employee.LocationId = GetValidEnumInput<Location>("Location");
        string jobTitle = GetDataFromField("Job Title");
        employee.JobTitle = string.IsNullOrWhiteSpace(jobTitle) ? null : jobTitle;
        employee.DepartmentId = GetValidEnumInput<Department>("Department");
        employee.AssignManagerId = GetValidEnumInput<Project>("Assign Project");
        employee.AssignProjectId = GetValidEnumInput<Manager>("Assign Manager");

        return employee;
    }

    private static partial EmployeeFilters? GetEmployeeFiltersFromConsole()
    {
        DisplayEnumOptions<Location>();
        DisplayEnumOptions<Department>();
        DisplayEnumOptions<Status>();
        EmployeeFilters filters = new();
        PrintConsoleMessage("\nEnter the filter criteria:\n\n");

        string alphabetInput = GetDataFromField("Enter alphabet letters (separated by comma if multiple)");
        filters.Alphabet = string.IsNullOrEmpty(alphabetInput) ? null : alphabetInput.Split(',').SelectMany(x => x.Trim().Select(char.ToLower)).ToList();

        if (ValidateFiltersWithEnum<Location>("Location", out var locationIds))
        {
            filters.Locations = locationIds;
        }

        if (ValidateFiltersWithEnum<Department>("Department", out var departmentIds))
        {
            filters.Departments = departmentIds;
        }

        if (ValidateFiltersWithEnum<Status>("Status", out var statusIds))
        {
            filters.Status = statusIds;
        }

        return filters;
    }

    private static partial EmployeeFilters? GetSearchKeywordFromConsole()
    {
        EmployeeFilters filters = new();

        PrintConsoleMessage("Enter the search keyword:");
        filters.Search = Console.ReadLine()?.Trim();

        return filters;
    }

    private static void ResetFilters(EmployeeFilters Filters)
    {
        if (Filters != null)
        {
            Filters.Alphabet = [];
            Filters.Locations = [];
            Filters.Departments = [];
            Filters.Status = [];
            Filters.Search = "";
        }
    }

    private static partial void PrintEmployeesDetails(List<EmployeeDetails> employees)
    {
        PrintEmployeesTableHeader();
        foreach (EmployeeDetails employee in employees)
        {
            string fullName = $"{employee.FirstName ?? null} {employee.LastName ?? null}";
            string dob = employee.Dob?.ToString("dd-MM-yyyy") ?? null;
            string email = employee.Email ?? null;
            string mobileNumber = employee.MobileNumber ?? null;
            string locationName = employee.LocationName ?? null;
            string jobTitle = employee.JobTitle ?? null;
            string departmentName = employee.DepartmentName ?? null;

            Console.WriteLine($" {employee.EmpNo}\t\t|{fullName,-20}\t|{employee.StatusName,-10}\t|{dob}\t|{email,-30}\t|{mobileNumber}\t|{locationName,-10}\t\t|{jobTitle,-30}\t|{departmentName}");
        }
        Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------\n");
    }

    private static partial Employee GetUpdatedDataFromUser()
    {
        Employee employee = new()
        {
            FirstName = GetDataFromField("First Name")!,
            LastName = GetDataFromField("Last Name")!,
            Dob = ParseNullableDate(GetDataFromField("Date of Birth (YYYY-MM-DD)")),
            Email = GetDataFromField("Email")!,
            MobileNumber = GetDataFromField("Mobile Number")!,
            JoiningDate = ParseNullableDate(GetDataFromField("Joining Date (YYYY-MM-DD)")),
            LocationId = GetValidEnumInput<Location>("Location"),
            JobTitle = GetDataFromField("Job Title")!,
            DepartmentId = GetValidEnumInput<Department>("Department"),
            AssignManagerId = GetValidEnumInput<Manager>("Assign Manager"),
            AssignProjectId = GetValidEnumInput<Project>("Assign Project")
        };

        return employee;
    }

    private static partial IConfiguration GetIConfiguration()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        return configuration;
    }

    private static DateTime? ParseNullableDate(string? dateValue)
    {
        if (string.IsNullOrWhiteSpace(dateValue))
        {
            return null;
        }
        else
        {
            if (DateTime.TryParseExact(dateValue, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
            {
                return parsedDate;
            }
            else
            {
                throw new FormatException("Invalid date format. Please use the format YYYY-MM-DD or enter '--d' to indicate no date.");
            }
        }
    }

    private static int? GetValidEnumInput<TEnum>(string fieldName) where TEnum : struct, Enum
    {
        List<int> enumIds;
        do
        {
            string input = GetDataFromField(fieldName)?.Replace(" ", "");

            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            if (!EnumCheck<TEnum>(input, out enumIds))
            {
                _logger.LogError($"Invalid value. Please enter a valid {fieldName}.");
            }
        } while (enumIds.Count == 0);

        return enumIds[0];
    }

    private static bool ValidateFiltersWithEnum<TEnum>(string fieldName, out List<int> enumIds) where TEnum : struct, Enum
    {
        string input;
        enumIds = [];
        do
        {
            input = GetDataFromField(fieldName)?.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                return true;
            }

            if (!EnumCheck<TEnum>(input, out enumIds))
            {
                _logger.LogError($"Invalid value. Please enter a valid {fieldName}.");
            }
        } while (enumIds.Count == 0);

        return true;
    }

    private static bool EnumCheck<TEnum>(string input, out List<int> enumIds) where TEnum : struct, Enum
    {
        enumIds = new List<int>();
        string[] values = input.Split(',').Select(x => x.Trim()).ToArray();
        foreach (var value in values)
        {
            if (Enum.TryParse(value, true, out TEnum result))
            {
                enumIds.Add(Convert.ToInt32(result)); // Convert the enum value to its corresponding ID
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    private static void PrintEmployeesTableHeader()
    {
        Console.WriteLine("\nEmployee Details:\n");
        Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
        Console.WriteLine(" Employee ID\t|Name\t\t\t|Status\t\t|Date of Birth\t|Email\t\t\t\t|Mobile Number\t|Location\t\t|Job Title\t\t\t|Department");
        Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
    }

    private static string? GetDataFromField(string message, bool isRequired = false)
    {
        PrintConsoleMessage($"{message}: ", false);
        string fieldInput = Console.ReadLine();
        if (isRequired && (string.IsNullOrEmpty(fieldInput) || string.IsNullOrWhiteSpace(fieldInput)))
        {
            _logger.LogWarning("Field is required. Please enter a value.\n");
            return GetDataFromField(message, isRequired);
        }
        return fieldInput;
    }

    private static void DisplayEnumOptions<T>()
    {
        PrintConsoleMessage($"\n{typeof(T).Name}:\n");
        foreach (var value in Enum.GetValues(typeof(T)))
        {
            PrintConsoleMessage($"{(int)value} : {value}\n", false);
        }
    }

    private static void PrintConsoleMessage(string message, bool newLine = true)
    {
        if (newLine)
        {
            Console.WriteLine(message);
        }
        else
        {
            Console.Write(message);
        }
    }
}
