using Microsoft.Extensions.Configuration;
using EmployeeManagementSystem.Models;
using System.Globalization;
using System.Reflection;

namespace EmployeeManagementSystem;

public partial class EMS
{
    //Employees related Partial Functions
    private static partial Employee GetEmployeeDataFromConsole()
    {
        PrintConsoleMessage("Enter employee details:\n");
        bool required = true;
        Employee employee = new();
        string empNo = ValidateEmployeeNo("Employee Number", required);
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
        employee.LocationId = GetIdFromUser<Location>("Location", "LocationJsonPath");
        employee.DepartmentId = GetIdFromUser<Department>("Department", "DepartmentJsonPath");
        employee.RoleId = GetRoleIdFromUserForDepartment(employee.DepartmentId);
        employee.AssignManagerId = GetIdFromUser<Manager>("Assign Manager", "ManagerJsonPath");
        employee.AssignProjectId = GetIdFromUser<Project>("Assign Project", "ProjectJsonPath");
        return employee;
    }

    private static string? ValidateEmployeeNo(string fieldName, bool isRequired = false)
    {
        PrintConsoleMessage($"{fieldName}: ", ConsoleColor.White, false);
        string empNo = Console.ReadLine();
        if (isRequired && (string.IsNullOrEmpty(empNo) || string.IsNullOrWhiteSpace(empNo)))
        {
            _logger.LogWarning("Field is required. Please enter a value.\n");
            return GetDataFromField(fieldName, isRequired);
        }

        if (empNo != null && IsEmpNoDuplicate(empNo))
        {
            _logger.LogWarning($"Employee already exists.");
            ValidateEmployeeNo(fieldName, isRequired);
        }
        return empNo;
    }

    private static bool IsEmpNoDuplicate(string empNo)
    {
        string employeeJsonPath = _configuration["EmployeeJsonPath"];
        List<Employee> employees = _jsonUtils.ReadJSON<Employee>(employeeJsonPath);
        return employees.Any(x => x.EmpNo == empNo);
    }

    private static int? GetRoleIdFromUserForDepartment(int? departmentId)
    {
        string departmentFilePath = GetIConfiguration()["DepartmentJsonPath"];
        string roleFilePath = GetIConfiguration()["RoleJsonPath"];

        // Read departments and roles
        List<Department> departments = _jsonUtils.ReadJSON<Department>(departmentFilePath);
        List<Role> roles = _jsonUtils.ReadJSON<Role>(roleFilePath);

        // Filter roles based on the selected department
        var department = departments.FirstOrDefault(d => d.Id == departmentId);
        if (department == null)
        {
            _logger.LogError($"Department with ID '{departmentId}' not found.");
            return null;
        }

        List<int> validRoleIds = roles.Where(role => role.DepartmentId == departmentId && role.Id != 0)
                                  .Select(role => role.Id)
                                  .Distinct()
                                  .ToList();

        if (validRoleIds.Count == 0)
        {
            _logger.LogWarning($"No roles found for the department '{department.DepartmentName}'.");
            return null;
        }

        // Display valid roles for the selected department
        PrintConsoleMessage($"\nValid roles for department '{department.DepartmentName}':\n", ConsoleColor.Cyan);
        foreach (var validRoleId in validRoleIds)
        {
            var role = roles.FirstOrDefault(r => r.Id == validRoleId);
            if (role != null)
            {
                PrintConsoleMessage($"- {role.RoleName}", ConsoleColor.Cyan);
            }
        }
        PrintConsoleMessage("\n");

        // Prompt user to select a role
        string userInput;
        int? roleId;
        do
        {
            userInput = GetDataFromField("Role");
            if (string.IsNullOrWhiteSpace(userInput)) return null;

            roleId = FindIdInJson<Role>(userInput, roleFilePath);
            if (!validRoleIds.Contains(roleId ?? -1))
            {
                _logger.LogError($"Invalid role. Please select from the roles listed for department '{department.DepartmentName}'.");
            }
        } while (!validRoleIds.Contains(roleId ?? -1));

        return roleId;
    }

    private static int? GetIdFromUser<T>(string fieldName, string jsonFilePathKey, bool isRequired = false) where T : class
    {
        string filePath = GetIConfiguration()[jsonFilePathKey];

        string userInput;
        int? id;
        do
        {
            userInput = GetDataFromField(fieldName, isRequired);
            if (string.IsNullOrWhiteSpace(userInput)) return null;

            id = FindIdInJson<T>(userInput, filePath);
            if (id == null)
            {
                _logger.LogError($"Invalid {fieldName}. Please try again.");
            }
        } while (id == null);

        return id;
    }

    private static int? FindIdInJson<T>(string userInput, string jsonFilePath) where T : class
    {
        try
        {
            if (File.Exists(jsonFilePath))
            {
                var items = _jsonUtils.ReadJSON<T>(jsonFilePath);
                if (items != null)
                {
                    PropertyInfo[] properties = typeof(T).GetProperties();
                    foreach (var item in items)
                    {
                        foreach (var property in properties)
                        {
                            if (property.PropertyType == typeof(string)) // to get the name property, Location name, manager name etc
                            {
                                string value = (string)property.GetValue(item);
                                if (value != null && value.Equals(userInput, StringComparison.OrdinalIgnoreCase))
                                {
                                    PropertyInfo idProperty = typeof(T).GetProperty("Id");
                                    if (idProperty != null)
                                    {
                                        object idValue = idProperty.GetValue(item);
                                        if (idValue is int id)
                                        {
                                            return id;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                _logger.LogError(Constants.FileNotFoundExceptionMessage);
                throw new FileNotFoundException($"Error: File '{jsonFilePath}' is not present. No Data Available");
            }
        }
        catch (IOException)
        {
            _logger.LogError(Constants.IOExceptionMessage);
        }
        return null;
    }

    private static partial EmployeeFilters? GetEmployeeFiltersFromConsole()
    {
        DisplayJsonOptions<Location>(GetIConfiguration()["LocationJsonPath"]);
        DisplayJsonOptions<Department>(GetIConfiguration()["DepartmentJsonPath"]);
        DisplayJsonOptions<Status>(GetIConfiguration()["StatusJsonPath"]);

        EmployeeFilters filters = new();
        PrintConsoleMessage("\nEnter the filter criteria:\n\n");

        string alphabetInput = GetDataFromField("Enter alphabet letters (separated by comma if multiple)");
        filters.Alphabet = string.IsNullOrEmpty(alphabetInput) ? null : alphabetInput.Split(',').SelectMany(x => x.Trim().Select(char.ToLower)).ToList();

        ValidateFilters<Location>("Location", "LocationJsonPath", out var locationIds);
        filters.Locations = locationIds;

        ValidateFilters<Department>("Department", "DepartmentJsonPath", out var departmentIds);
        filters.Departments = departmentIds;

        ValidateFilters<Status>("Status", "StatusJsonPath", out var statusIds);
        filters.Status = statusIds;

        return filters;
    }

    private static void DisplayJsonOptions<T>(string filePath) where T : class
    {
        try
        {
            if (File.Exists(filePath))
            {
                List<T> items = _jsonUtils.ReadJSON<T>(filePath);

                if (items != null && items.Count > 0)
                {
                    PrintConsoleMessage($"\n{typeof(T).Name}:\n");
                    foreach (var item in items)
                    {
                        PropertyInfo[] properties = typeof(T).GetProperties();
                        foreach (var property in properties)
                        {
                            object value = property.GetValue(item);
                            PrintConsoleMessage($"{value}  ", ConsoleColor.Cyan, false);
                        }
                        PrintConsoleMessage("\n");
                    }
                }
            }
            else
            {
                PrintConsoleMessage(Constants.ExceptionMessage);
            }
        }
        catch (IOException)
        {
            PrintConsoleMessage(Constants.IOExceptionMessage);
        }
    }

    private static void ValidateFilters<T>(string fieldName, string jsonFilePathKey, out List<int> ids) where T : class
    {
        string input;
        ids = [];
        string filePath = GetIConfiguration()[jsonFilePathKey];
        do
        {
            input = GetDataFromField(fieldName);

            if (string.IsNullOrWhiteSpace(input))
            {
                break;
            }

            var filterValues = input.Split(',').Select(value => value.Trim());
            if (!filterValues.Any(value => !string.IsNullOrEmpty(value)))
            {
                break;
            }

            foreach (var filterValue in filterValues)
            {
                var id = FindIdInJson<T>(filterValue, filePath);
                if (id.HasValue)
                {
                    ids.Add(id.Value);
                }
                else
                {
                    _logger.LogError($"Invalid value '{filterValue}'. Please enter a valid {fieldName}.");
                }
            }
        } while (true);
    }

    private static partial EmployeeFilters? GetSearchKeywordFromConsole()
    {
        EmployeeFilters filters = new();

        PrintConsoleMessage("Enter the search keyword:");
        filters.Search = Console.ReadLine()?.Trim();

        return filters;
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
            LocationId = GetIdFromUser<Location>("Location", "LocationJsonPath"),
            DepartmentId = GetIdFromUser<Department>("Department", "DepartmentJsonPath")
        };
        employee.RoleId = GetRoleIdFromUserForDepartment(employee.DepartmentId);
        employee.AssignManagerId = GetIdFromUser<Manager>("Assign Manager", "ManagerJsonPath");
        employee.AssignProjectId = GetIdFromUser<Project>("Assign Project", "ProjectJsonPath");

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

    private static string? GetDataFromField(string message, bool isRequired = false)
    {
        PrintConsoleMessage($"{message}: ", ConsoleColor.White, false);
        string fieldInput = Console.ReadLine();
        if (isRequired && (string.IsNullOrEmpty(fieldInput) || string.IsNullOrWhiteSpace(fieldInput)))
        {
            _logger.LogWarning("Field is required. Please enter a value.\n");
            return GetDataFromField(message, isRequired);
        }
        return fieldInput;
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
            string jobTitle = employee.RoleName ?? null;
            string departmentName = employee.DepartmentName ?? null;

            Console.WriteLine($" {employee.EmpNo}\t\t|{fullName,-20}\t|{employee.StatusName,-10}\t|{dob}\t|{email,-30}\t|{mobileNumber}\t|{locationName,-10}\t\t|{jobTitle,-30}\t|{departmentName}");
        }
        Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------\n");
    }

    private static void PrintEmployeesTableHeader()
    {
        Console.WriteLine("\nEmployee Details:\n");
        Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
        Console.WriteLine(" Employee ID\t|Name\t\t\t|Status\t\t|Date of Birth\t|Email\t\t\t\t|Mobile Number\t|Location\t\t|Job Title\t\t\t|Department");
        Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
    }

    // Roles related partial functions
    private static partial Role GetRoleDataFromConsole()
    {
        PrintConsoleMessage("Enter Role details:\n");
        bool required = true;

        string roleFilePath = GetIConfiguration()["RoleJsonPath"];
        List<Role> roles = _jsonUtils.ReadJSON<Role>(roleFilePath);

        Role role = new()
        {
            Id = roles.Count + 1,
            DepartmentId = GetIdFromUser<Department>("Department", "DepartmentJsonPath", required),
            RoleName = GetDataFromField("Role Name", required)!
        };

        return role;
    }

    private static void PrintRolesTableHeader()
    {
        Console.WriteLine("\nRoles Details:\n");
        Console.WriteLine("------------------------------------------------------------------------------------");
        Console.WriteLine(" Role ID\t|Role Name\t\t\t|Department Name");
        Console.WriteLine("------------------------------------------------------------------------------------");
    }

    private static partial void PrintRoles(List<Role> roles)
    {
        Dictionary<int, string>? departmentNames = _dropdownDal.GetDepartments();

        if (departmentNames != null)
        {
            PrintRolesTableHeader();
            foreach (Role role in roles)
            {
                int id = role.Id;
                string roleName = role.RoleName;
                int departmentId = role.DepartmentId ?? 0;
                string? departmentName = departmentNames.TryGetValue(departmentId, out string? value) ? value : null;
                Console.WriteLine($" {id}\t\t|{roleName,-30}\t|{departmentName}");
            }
            Console.WriteLine("--------------------------------------------------------------------------------------\n");
        }
        else
        {
            _logger.LogError(Constants.DropdownListError);
        }
    }

    private static void PrintConsoleMessage(string message, ConsoleColor color = ConsoleColor.White, bool newLine = true)
    {
        Console.ForegroundColor = color;
        if (newLine)
        {
            Console.WriteLine(message);
        }
        else
        {
            Console.Write(message);
        }
        Console.ForegroundColor = ConsoleColor.White;
    }
}
