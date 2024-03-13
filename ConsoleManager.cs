using Microsoft.Extensions.Configuration;
using EmployeeManagementSystem.Models;
using System.Globalization;

namespace EmployeeManagementSystem;
public partial class EMS
{
    public static partial Employee GetEmployeeDataFromUser()
    {
        logger.LogInfo("Enter employee details:\n", true);
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

        string assignManager = GetDataFromField("Assign Manager");
        employee.AssignManager = string.IsNullOrWhiteSpace(assignManager) ? null : assignManager;

        string assignProject = GetDataFromField("Assign Project");
        employee.AssignProject = string.IsNullOrWhiteSpace(assignProject) ? null : assignProject;

        return employee;
    }

    public static partial (bool status, EmployeeFilters?) GetEmployeeFiltersFromConsole()
    {
        DisplayEnumOptions<Location>();
        DisplayEnumOptions<Department>();
        DisplayEnumOptions<Status>();
        EmployeeFilters filters = new();
        logger.LogInfo("\nEnter the filter criteria:\n\n");

        string alphabetInput = GetDataFromField("Enter alphabet letters (separated by comma if multiple)");
        filters.Alphabet = string.IsNullOrEmpty(alphabetInput) ? null : alphabetInput.Split(',').SelectMany(x => x.Trim().Select(char.ToLower)).ToList();
        var locationResult = ValidateFiltersWithEnum<Location>("Location");
        if (locationResult.isValid)
        {
            filters.Locations = locationResult.enumValue;
        }
        var departmentResult = ValidateFiltersWithEnum<Department>("Department");
        if (departmentResult.isValid)
        {
            filters.Departments = departmentResult.enumValue;
        }
        var statusResult = ValidateFiltersWithEnum<Status>("Status");
        if (statusResult.isValid)
        {
            filters.Status = statusResult.enumValue;
        }

        return (true, filters);
    }

    private static int? GetValidEnumInput<TEnum>(string fieldName) where TEnum : struct, Enum
    {
        (bool isValid, List<TEnum> enumValues) result;
        do
        {
            string input = GetDataFromField(fieldName)?.Replace(" ", "");

            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            if (input.Equals("--d"))
            {
                return -1;
            }

            result = EnumCheck<TEnum>(input);

            if (!result.isValid)
            {
                logger.LogError($"Invalid value. Please enter a valid {fieldName}.");
            }
        } while (!result.isValid);

        return result.isValid && result.enumValues.Count > 0 ? Convert.ToInt32(result.enumValues[0]) : null;
    }

    private static (bool isValid, List<TEnum> enumValue) ValidateFiltersWithEnum<TEnum>(string fieldName) where TEnum : struct, Enum
    {
        string input;
        (bool isValid, List<TEnum> enumValue) result;
        do
        {
            input = GetDataFromField(fieldName);

            if (string.IsNullOrWhiteSpace(input))
            {
                return (true, new List<TEnum>());
            }

            result = EnumCheck<TEnum>(input);
            if (!result.isValid)
            {
                logger.LogError($"Invalid value. Please enter a valid {fieldName}.");
            }
        } while (!result.isValid);

        return result;
    }

    private static (bool success, List<TEnum> enumValues) EnumCheck<TEnum>(string input) where TEnum : struct, Enum
    {
        List<TEnum> enumValues = new List<TEnum>();
        string[] values = input.Split(',').Select(x => x.Trim()).ToArray();
        foreach (var value in values)
        {
            if (Enum.TryParse(value, true, out TEnum result))
            {
                enumValues.Add(result);
            }
            else
            {
                return (false, []);
            }
        }
        return (true, enumValues);
    }

    public static void ResetFilters(EmployeeFilters Filters)
    {
        if (Filters != null)
        {
            Filters.Alphabet = [];
            Filters.Locations = [];
            Filters.Departments = [];
            Filters.Status = [];
        }
    }

    private static void DisplayEnumOptions<T>()
    {
        logger.LogInfo($"\n{typeof(T).Name}:\n");
        foreach (var value in Enum.GetValues(typeof(T)))
        {
            logger.LogInfo($"{(int)value} : {value}\n", false);
        }
    }

    private static string? GetDataFromField(string message, bool isRequired = false)
    {
        logger.LogInfo($"{message}: ", false);
        string fieldInput = Console.ReadLine();
        if (isRequired && (string.IsNullOrEmpty(fieldInput) || string.IsNullOrWhiteSpace(fieldInput)))
        {
            logger.LogWarning("Field is required. Please enter a value.\n");
            return GetDataFromField(message, isRequired);
        }
        return fieldInput;
    }

    public static partial void PrintEmployeesDetails(List<EmployeeDetails> employees)
    {
        PrintEmployeesTableHeader();
        foreach (EmployeeDetails employee in employees)
        {
            string fullName = $"{employee.FirstName ?? "--"} {employee.LastName ?? "--"}";
            string dob = employee.Dob?.ToString("dd-MM-yyyy") ?? "--";
            string email = employee.Email ?? "--";
            string mobileNumber = employee.MobileNumber ?? "--";
            string locationName = employee.LocationName ?? "--";
            string jobTitle = employee.JobTitle ?? "--";
            string departmentName = employee.DepartmentName ?? "--";

            Console.WriteLine($"{employee.EmpNo}\t\t{fullName,-20}\t{employee.StatusName,-10}\t{dob}\t{email,-30}\t{mobileNumber}\t{locationName,-10}\t\t{jobTitle,-30}\t{departmentName}");
        }
        Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------\n");
    }

    private static void PrintEmployeesTableHeader()
    {
        Console.WriteLine("\nEmployee Details:\n");
        Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
        Console.WriteLine("Employee ID\tName\t\t\tStatus\t\tDate of Birth\tEmail\t\t\t\tMobile Number\tLocation\t\tJob Title\t\t\tDepartment");
        Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
    }

    public static partial Employee GetUpdatedDataFromUser()
    {
        Employee employee = new();
        employee.FirstName = GetDataFromField("First Name")!;
        employee.LastName = GetDataFromField("Last Name")!;
        employee.Dob = ParseNullableDate(GetDataFromField("Date of Birth (YYYY-MM-DD)"));
        employee.Email = GetDataFromField("Email")!;
        employee.MobileNumber = GetDataFromField("Mobile Number")!;
        employee.JoiningDate = ParseNullableDate(GetDataFromField("Joining Date (YYYY-MM-DD)"));
        int? locationId = GetValidEnumInput<Location>("Location");
        if (locationId == -1)
        {
            employee.LocationId = null;
        }
        else
        {
            employee.LocationId = locationId;
        }
        employee.JobTitle = GetDataFromField("Job Title")!;
        int? departmentId = GetValidEnumInput<Department>("Department");
        if (departmentId == -1)
        {
            employee.DepartmentId = null;
        }
        else
        {
            employee.DepartmentId = departmentId;
        }
        employee.AssignManager = GetDataFromField("Assign Manager")!;
        employee.AssignProject = GetDataFromField("Assign Project")!;
        return employee;
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

    public static partial IConfiguration GetIConfiguration()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        return configuration;
    }
}
