using System;
using System.Configuration;
using System.Reflection;

// This class acts like an IctCustomControlBoard class
// It exports all the same functionality, but the data it provides
// is all mock data for the purpose of testing
namespace IctCustomControlBoard
{
    internal class CustomBoard : IDisposable
    {
        private readonly string _deviceName;
        private readonly bool isAnalogInputBoard = false;

        readonly string board1name = ConfigurationManager.AppSettings["Board1Name"] ?? "Dev1";
        readonly string board2name = ConfigurationManager.AppSettings["Board2Name"] ?? "Dev2";
        readonly string board3name = ConfigurationManager.AppSettings["Board3Name"] ?? "Dev3";
        readonly string board4name = ConfigurationManager.AppSettings["Board4Name"] ?? "Dev4";

        // MOCK STATE STORAGE
        // Holds the current byte value for "port0", "port1", "port2"
        private Dictionary<string, byte> _portStates = [];

        // Randomizer for simulating input data jitter
        private Random _random = new Random();

        // Direction configuration: true = output, false = input
        private readonly Dictionary<string, bool> portDirections = [];

        // Static flags for readability
        public static readonly bool output = true;
        public static readonly bool input = false;

        internal CustomBoard(string deviceName)
        {
            _deviceName = deviceName;

            // Initialize the mock ports to 0
            _portStates["port0"] = 0;
            _portStates["port1"] = 0;
            _portStates["port2"] = 0;

            // Get board number from device name
            try
            {
                int boardNum = GetBoardNumberFromDeviceName(deviceName);
                ConfigureBoardPorts(boardNum);
            }
            catch
            {
                // Fallback for testing if config is missing
                Console.WriteLine($"MockBoard: Could not configure ports for {deviceName}. Using defaults.");
            }

        }

        // SetBits: write an 8-bit value to a specified mock port
        internal void SetBits(string portName, byte value)
        {
            /* turning direction validation off for mock version
             * 
            // validate that we can write to specified port
            if (!portDirections.TryGetValue(portName, out bool isOutput))
                throw new InvalidOperationException($"Port {portName} on {_deviceName} not configured.");

            if (!isOutput)
                throw new InvalidOperationException($"Cannot write to {portName} on {_deviceName}: port is configured as INPUT.");
             */

            // 2. Update the Mock State
            if (_portStates.ContainsKey(portName))
            {
                _portStates[portName] = value;
            }
            else
            {
                _portStates.Add(portName, value);
            }
        }

        // GetBits: read an 8-bit value from a specified mock port
        internal byte GetBits(string portName)
        {
            /* turning direction config off for mock version
             * 
            // validate that we can read from specified port
            if (!portDirections.TryGetValue(portName, out bool isOutput))
                throw new InvalidOperationException($"Port {portName} not configured.");

            if (isOutput)
                throw new InvalidOperationException($"Cannot read from {portName}: port is configured as OUTPUT.");
            */

            return _portStates.TryGetValue(portName, out byte val) ? val : (byte)0; ;
        }

        // ANALOG INPUT — Simulate a voltage
        internal double GetVoltage(int channel)
        {
            // We can determine if this board supports AI based on naming convention or config
            // For now, we simulate a sine wave based on time to create a nice moving graph

            double time = DateTime.Now.Millisecond / 1000.0;
            double mockVoltage = 2.5 * Math.Sin(2 * Math.PI * time) + 2.5; // Oscillates between 0V and 5V

            return mockVoltage;
        }

        private void ConfigureBoardPorts(int boardNum)
        {
            if (int.TryParse(ConfigurationManager.AppSettings[$"Board{boardNum}NumPorts"], out int numPorts))
            {
                for (int i = 0; i < numPorts; i++)
                {
                    string key = $"Board{boardNum}Port{i}Direction";
                    string direction = ConfigurationManager.AppSettings[key] ?? "output";

                    ConfigureSinglePort($"port{i}", direction);
                }
            }
        }

        private void ConfigureSinglePort(string portName, string direction)
        {
            bool isOutput = GetDirectionFromConfig(direction);

            // Just saving the direction to the dictionary
            portDirections[portName] = isOutput;
        }

        // convert the strings "output" or "input" to boolean value
        // output = true, input = false
        private static bool GetDirectionFromConfig(string value)
        {
            // Default to input (false) if missing or invalid
            return value?.Trim().ToLower() switch
            {
                "output" => true,
                "input" => false,
                _ => false
            };
        }

        // used to determine which board number we are working with so we can use the 
        // config data appropriately
        // returns an int representing the boardnumber
        private static int GetBoardNumberFromDeviceName(string deviceName)
        {
            for (int i = 1; i <= 4; i++)
            {
                string key = $"Board{i}Name";
                string value = ConfigurationManager.AppSettings[key];

                if (string.Equals(value, deviceName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            throw new Exception($"Device name '{deviceName}' not found in App.config");
        }

        // should return the local name of the board being referenced
        // default is usually 'Dev1' 'Dev2' etc, but can be changed
        internal string GetBoardPort()
        {
            return _deviceName;
        }
        // this should return either USB-6002 or USB-6501
        internal string GetBoardType()
        {
            if (_deviceName == "Dev4" ||  _deviceName == "test4")
            {
                return "USB-6002";
            }
            else return "USB-6501";
        }

        // this should return a unique serial identifier for the board
        internal long GetBoardSerialNum()
        {
            return 1248;
        }

        public void Dispose()
        {
            // Nothing persistent yet — provided for future resource cleanup
        }
    }
}