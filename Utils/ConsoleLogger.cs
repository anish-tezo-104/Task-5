namespace EmployeeManagementSystem.Utils;

public class ConsoleLogger : ILogger
{
    public void LogError(string message, bool newLine = true)
    {
        LogMessage(message+"\n", ConsoleColor.Red, newLine);
    }

    public void LogSuccess(string message, bool newLine = true)
    {
        LogMessage(message + "\n", ConsoleColor.Green, newLine);
    }

    public void LogInfo(string message, bool newLine = true)
    {
        LogMessage(message, ConsoleColor.White, newLine);
    }

    public void LogWarning(string message, bool newLine = true)
    {
        LogMessage(message + "\n", ConsoleColor.DarkYellow, newLine);
    }

    private static void LogMessage(string message, ConsoleColor color, bool newLine)
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

        Console.ResetColor();
    }
}
