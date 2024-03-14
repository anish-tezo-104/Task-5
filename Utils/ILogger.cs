namespace EmployeeManagementSystem.Utils;

public interface ILogger
{
    void LogError(string message, bool newLine = true);
    void LogSuccess(string message, bool newLine = true);
    void LogInfo(string message, bool newLine = true);
    void LogWarning(string message, bool newLine = true);
}