using NationalInstruments.DAQmx;
using System;
using System.Configuration;
using System.DirectoryServices;
using System.Reflection;

// a class for interfacing with the usb-6002 Digital IO board from national instruments.
// allows for reading and writing of bits on the device
// allows for reading differential voltages from an analog input
// also should report its device name when asked
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

        internal CustomBoard(string deviceName)
        {
            _deviceName = deviceName;

            // get boardnumber from device name
            int boardNum = GetBoardNumberFromDeviceName(deviceName);
            ConfigureBoardPorts(boardNum);

        }
        // true = output, false = input
        private readonly Dictionary<string, bool> portDirections = [];

        // Static flags for readability
        public static readonly bool output = true;
        public static readonly bool input = false;

        // SetBits: write an 8-bit value to a digital output port
        internal void SetBits(string portName, byte value)
        {
            // validate that we can write to specified port
            if (!portDirections.TryGetValue(portName, out bool isOutput))
                throw new InvalidOperationException($"Port {portName} on {_deviceName} not configured.");

            if (!isOutput)
                throw new InvalidOperationException($"Cannot write to {portName} on {_deviceName}: port is configured as INPUT.");

            using NationalInstruments.DAQmx.Task doTask = new();
            string channel = $"{_deviceName}/{portName}";

            doTask.DOChannels.CreateChannel(
                channel,
                "",
                ChannelLineGrouping.OneChannelForAllLines);

            DigitalSingleChannelWriter writer = new(doTask.Stream);
            doTask.Start();
            writer.WriteSingleSamplePort(false, value);
            doTask.Stop();


            // debug statement: remove later
            Console.WriteLine($"[{_deviceName}] Set {portName} = 0x{value:X2}");
        }

        // GetBits: read an 8-bit value from a digital input port
        internal byte GetBits(string portName)
        {
            // validate that we can read from specified port
            if (!portDirections.TryGetValue(portName, out bool isOutput))
                throw new InvalidOperationException($"Port {portName} not configured.");

            if (isOutput)
                throw new InvalidOperationException($"Cannot read from {portName}: port is configured as OUTPUT.");

            using NationalInstruments.DAQmx.Task diTask = new();
            string channel = $"{_deviceName}/{portName}";

            diTask.DIChannels.CreateChannel(
                channel,
                "",
                ChannelLineGrouping.OneChannelForAllLines);

            DigitalSingleChannelReader reader = new(diTask.Stream);
            byte value = reader.ReadSingleSamplePortByte();

            return value;
        }

        // ANALOG INPUT — Read voltage from an AI channel
        internal double GetVoltage(int channel)
        {
            if (!isAnalogInputBoard)
                throw new InvalidOperationException(($"{_deviceName}: Cannot read voltage — invalid board type"));

            using NationalInstruments.DAQmx.Task aiTask = new();
            string channelName = $"{_deviceName}/ai{channel}";

            aiTask.AIChannels.CreateVoltageChannel(
                channelName,
                "",
                AITerminalConfiguration.Differential,   // Differential mode
                -5.0, 5.0,                              // Input range
                AIVoltageUnits.Volts);

            AnalogSingleChannelReader reader = new(aiTask.Stream);
            double voltage = reader.ReadSingleSample();

            return voltage;
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

            using NationalInstruments.DAQmx.Task configTask = new();
            string channel = $"{_deviceName}/{portName}";
            if (isOutput)
            {
                configTask.DOChannels.CreateChannel(channel, "", ChannelLineGrouping.OneChannelForAllLines);
            }
            else
            {
                configTask.DIChannels.CreateChannel(channel, "", ChannelLineGrouping.OneChannelForAllLines);
            }

            configTask.Start();
            configTask.Stop();

            MessageBox.Show($"portname{portName} getting direction{isOutput}");
            // Store configuration in dictionary
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
            Device board = DaqSystem.Local.LoadDevice(_deviceName);

            return board.DeviceID;
        }
        // this should return either USB-6002 or USB-6501
        internal string GetBoardType()
        {
            Device board = DaqSystem.Local.LoadDevice(_deviceName);

            return board.ProductType;
        }

        // this should return a unique serial identifier for the board
        internal long GetBoardSerialNum()
        {
            Device board = DaqSystem.Local.LoadDevice(_deviceName);

            return board.SerialNumber;
        }

        public void Dispose()
        {
            // Nothing persistent yet — provided for future resource cleanup
        }
    }
}