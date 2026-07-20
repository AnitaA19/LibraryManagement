using LibraryManagement.Core.Exceptions;

namespace LibraryManagement.App.UI
{
    internal static class ConsoleIO
    {
        public static string ReadNonEmptyString(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input))
                {
                    return input.Trim();
                }
                WriteError("Input cannot be empty.");
            }
        }

        public static int ReadInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var input = Console.ReadLine();
                if (int.TryParse(input, out var value))
                {
                    return value;
                }
                WriteError("Please enter a valid whole number.");
            }
        }

        public static decimal ReadDecimal(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var input = Console.ReadLine();
                if (decimal.TryParse(input, out var value))
                {
                    return value;
                }
                WriteError("Please enter a valid number.");
            }
        }

        public static DateTime ReadDate(string prompt, string format = "yyyy-MM-dd")
        {
            while (true)
            {
                Console.Write($"{prompt} ({format}): ");
                var input = Console.ReadLine();
                if (DateTime.TryParseExact(input, format, null, System.Globalization.DateTimeStyles.None, out var value))
                {
                    return value;
                }
                WriteError($"Please enter a valid date in {format} format.");
            }
        }

        public static int ReadMenuChoice(string prompt, int min, int max)
        {
            while (true)
            {
                var value = ReadInt(prompt);
                if (value >= min && value <= max)
                {
                    return value;
                }
                WriteError($"Please enter a number between {min} and {max}.");
            }
        }

        public static void RunSafely(Action action)
        {
            try
            {
                action();
            }
            catch (LibraryException ex)
            {
                WriteError(ex.Message);
            }
            catch (Exception ex)
            {
                WriteError($"Unexpected error: {ex.Message}");
            }
        }

        public static void WaitForKey(string prompt = "Press Enter to continue...")
        {
            Console.WriteLine();
            Console.Write(prompt);
            Console.ReadLine();
        }

        public static void Clear()
        {
            try
            {
                Console.Clear();
            }
            catch (IOException)
            {
            }
        }

        public static void WriteError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void WriteSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}

