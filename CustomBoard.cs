using NationalInstruments.DAQmx;
using System;
using System.Configuration;
using System.DirectoryServices;

// a class for interfacing with the usb-6002 Digital IO board from national instruments.
// allows for reading and writing of bits on the device
// allows for reading differential voltages from an analog input
// also should report its device name when asked
namespace IctCustomControlBoard
{
    public class CustomBoard : IDisposable
    {
        private readonly string _deviceName;
        private readonly bool isAnalogInputBoard = false;

        readonly string board1name = ConfigurationManager.AppSettings["Board1Name"] ?? "Dev1";
        readonly string board2name = ConfigurationManager.AppSettings["Board2Name"] ?? "Dev2";
        readonly string board3name = ConfigurationManager.AppSettings["Board3Name"] ?? "Dev3";
        readonly string board4name = ConfigurationManager.AppSettings["Board4Name"] ?? "Dev4";

        public CustomBoard(string deviceName)
        {
            _deviceName = deviceName;


            // move these settings to app.config 
            if (_deviceName == board1name)
            {
                ConfigurePort("port0", output);
                ConfigurePort("port1", output);
                ConfigurePort("port2", output);
            }
            else if (_deviceName == board2name)
            {
                ConfigurePort("port0", output);
                ConfigurePort("port1", output);
                ConfigurePort("port2", output);
            }
            else if (_deviceName == board3name)
            {
                ConfigurePort("port0", input);
                ConfigurePort("port1", input);
                ConfigurePort("port2", input);
            }
            else if (_deviceName == board4name)
            {
                ConfigurePort("port0", input);
                ConfigurePort("port1", input);
                ConfigurePort("port2", input);
                isAnalogInputBoard = true;
            }

        }
        // true = output, false = input
        private readonly Dictionary<string, bool> portDirections = [];

        // Static flags for readability
        public static readonly bool output = true;
        public static readonly bool input = false;

        // SetBits: write an 8-bit value to a digital output port
        public void SetBits(string portName, byte value)
        {
            // validate that we can write to specified port
            if (!portDirections.TryGetValue(portName, out bool isOutput))
                throw new InvalidOperationException($"Port {portName} not configured.");

            if (!isOutput)
                throw new InvalidOperationException($"Cannot write to {portName}: port is configured as INPUT.");

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
        public byte GetBits(string portName)
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
        public double GetVoltage(int channel)
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

        public void ConfigurePort(string portName, bool isOutput)
        {
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

            // Store configuration in dictionary
            portDirections[portName] = isOutput;
        }

        // GetIOID: returns device name
        public BoardInfo GetIOID()
        {
            Device board = DaqSystem.Local.LoadDevice(_deviceName);

            string Manufacture_Id = board.ProductType;
            long Board_number = board.SerialNumber; // not sure this is what we need
            string Board_port = board.DeviceID;

            BoardInfo info = new (Manufacture_Id, Board_number, Board_port);
            return info;
        }

        // struct holding relevant info about the board
        public readonly struct BoardInfo
        {
            public string Manufacture_Id { get; }
            public long Board_number { get; }
            public string Board_port { get; }

            public BoardInfo(string manufactureId, long boardNumber, string boardPort)
            {
                Manufacture_Id = manufactureId;
                Board_number = boardNumber;
                Board_port = boardPort;
            }
        }

            public void Dispose()
        {
            // Nothing persistent yet — provided for future resource cleanup
        }
    }
}