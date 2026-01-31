using System;
using IctCustomControlBoard;

namespace BoardTestApp
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== Board Manager Test ===\n");

            // Create the board manager
            BoardManager manager = new();

            // 1. Read voltages
            var (v1, v2) = manager.GetVoltages();
            Console.WriteLine($"Voltage 1: {v1:F2} V");
            Console.WriteLine($"Voltage 2: {v2:F2} V\n");

            // 2. Get board info
            var boards = manager.GetBoardInfo();
            Console.WriteLine("Board Info:");
            for (int i = 0; i < boards.Length; i++)
            {
                var b = boards[i];
                Console.WriteLine($"  Board {i + 1}:");
                Console.WriteLine($"    Manufacturer ID: {b.Board_type}");
                Console.WriteLine($"    Board Number: {b.Board_serial_number}");
                Console.WriteLine($"    Board Port: {b.Board_port}");
            }
            Console.WriteLine();

            // 3. Read coil / relay status
            ulong coilStatus = manager.GetBits();
            Console.WriteLine($"Current Coil Status (binary): {Convert.ToString((long)coilStatus, 2).PadLeft(48, '0')}\n");

            // 4. Set relays with a test value
            ulong testRelays = 0b101010101010101010101010; // example value
            Console.WriteLine($"Setting relays to: {Convert.ToString((long)testRelays, 2).PadLeft(48, '0')}");
            manager.SetBits(testRelays);
            Console.WriteLine("Relays set successfully.\n");

            Console.WriteLine("=== Test Complete ===");
            Console.ReadLine();
        }
    }
}
