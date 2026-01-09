using IctCustomControlBoard;
using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Board Manager Test Console (with Direction Safety) ===");

        using (BoardManager manager = new())
        {
            while (true)
            {
                Console.WriteLine("\nSelect a board (1-4) or Q to quit:");
                Console.WriteLine("0: Board1 (DIO, output)");
                Console.WriteLine("1: Board2 (DIO, output)");
                Console.WriteLine("2: Board3 (DIO, input)");
                Console.WriteLine("3: Board4 (AI and DIO, analog and digital input)");
                Console.Write("> ");

                string? input = Console.ReadLine()?.Trim().ToUpper();
                if (input == "Q") break;

                if (!int.TryParse(input, out int boardIndex) || boardIndex < 0 || boardIndex > 3)
                {
                    Console.WriteLine("Invalid selection. Try again.");
                    continue;
                }

                try
                {
                    switch (boardIndex)
                    {
                        case 0:
                            TestDigitalBoard(manager.Board1, "Board1");
                            break;
                        case 1:
                            TestDigitalBoard(manager.Board2, "Board2");
                            break;
                        case 2:
                            TestDigitalBoard(manager.Board3, "Board3");
                            break;
                        case 3:
                            TestAIBoard(manager.Board4);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Error: {ex.Message}");
                }
            }
        }

        Console.WriteLine("\nExiting program.");
    }

    // ==========================================================
    // TEST: Digital I/O Board
    // ==========================================================
    static void TestDigitalBoard(CustomDIOBoard board, string name)
    {
        Console.WriteLine($"\n{name} selected ({board.GetIOID()}).");

        Console.WriteLine("Enter port name (e.g., port0) or Q to go back:");
        string? port = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(port) || port.ToUpper() == "Q") return;

        Console.WriteLine("Choose action: (R)ead or (W)rite:");
        string? action = Console.ReadLine()?.Trim().ToUpper();

        try
        {
            if (action == "W")
            {
                Console.Write("Enter value to write (0-255): ");
                string? valStr = Console.ReadLine()?.Trim();
                if (!byte.TryParse(valStr, out byte value))
                {
                    Console.WriteLine("Invalid value.");
                    return;
                }

                board.SetBits(port, value);
                Console.WriteLine($" Wrote 0x{value:X2} to {port}");
            }
            else if (action == "R")
            {
                byte bits = board.GetBits(port);
                Console.WriteLine($" Read {port}: 0x{bits:X2}");
            }
            else
            {
                Console.WriteLine("Invalid action.");
            }
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($" Operation blocked: {ex.Message}");
        }
    }

    // ==========================================================
    // TEST: 6002 Board (Analog + Digital)
    // ==========================================================
    static void TestAIBoard(CustomAIBoard board)
    {
        Console.WriteLine($"\nBoard3 (6002) selected ({board.GetIOID()}).");
        Console.WriteLine("1: Digital I/O");
        Console.WriteLine("2: Analog Input");
        Console.WriteLine("Q: Back");
        Console.Write("> ");

        string? choice = Console.ReadLine()?.Trim().ToUpper();
        if (choice == "Q") return;

        try
        {
            switch (choice)
            {
                case "1":
                    Console.Write("Enter port name (e.g., port0): ");
                    string? port = Console.ReadLine()?.Trim();
                    if (string.IsNullOrEmpty(port)) return;

                    Console.Write("Choose action: (R)ead or (W)rite: ");
                    string? action = Console.ReadLine()?.Trim().ToUpper();

                    if (action == "W")
                    {
                        Console.Write("Enter value to write (0-255): ");
                        string? valStr = Console.ReadLine()?.Trim();
                        if (!byte.TryParse(valStr, out byte value))
                        {
                            Console.WriteLine("Invalid value.");
                            return;
                        }

                        board.SetBits(port, value);
                        Console.WriteLine($" Wrote 0x{value:X2} to {port}");
                    }
                    else if (action == "R")
                    {
                        byte bits = board.GetBits(port);
                        Console.WriteLine($" Read {port}: 0x{bits:X2}");
                    }
                    else
                    {
                        Console.WriteLine("Invalid action.");
                    }
                    break;

                case "2":
                    Console.Write("Enter analog channel number (0-7): ");
                    string? chStr = Console.ReadLine()?.Trim();
                    if (!int.TryParse(chStr, out int channel))
                    {
                        Console.WriteLine("Invalid channel number.");
                        return;
                    }

                    double voltage = board.GetVoltage(channel);
                    Console.WriteLine($" CH{channel}: {voltage:F3} V");
                    break;

                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($" Operation blocked: {ex.Message}");
        }
    }
}
