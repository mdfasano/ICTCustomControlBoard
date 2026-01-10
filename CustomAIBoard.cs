using NationalInstruments.DAQmx;
using System;
using System.DirectoryServices;

// a class for interfacing with the usb-6002 Digital IO board from national instruments.
// allows for reading and writing of bits on the device
// allows for reading differential voltages from an analog input
// also should report its device name when asked
namespace IctCustomControlBoard
{
    public class CustomAIBoard : IDisposable
    {
        private readonly string _deviceName;

        public CustomAIBoard(string deviceName)
        {
            _deviceName = deviceName;
            ConfigurePort("port0", input);
            ConfigurePort("port1", input);
            ConfigurePort("port2", input);
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

            // debug statement: remove later
            Console.WriteLine($"[{_deviceName}] Read {portName} = 0x{value:X2}");
            return value;
        }

        // ANALOG INPUT — Read voltage from an AI channel
        public double GetVoltage(int channel)
        {
            using NationalInstruments.DAQmx.Task aiTask = new NationalInstruments.DAQmx.Task();
            string channelName = $"{_deviceName}/ai{channel}";

            aiTask.AIChannels.CreateVoltageChannel(
                channelName,
                "",
                AITerminalConfiguration.Differential,   // Differential mode
                -5.0, 5.0,                              // Input range
                AIVoltageUnits.Volts);

            AnalogSingleChannelReader reader = new(aiTask.Stream);
            double voltage = reader.ReadSingleSample();

            // debug statement: remove later
            Console.WriteLine($"[{_deviceName}] CH{channel} = {voltage:F3} V");
            return voltage;
        }

        public void ConfigurePort(string portName, bool isOutput)
        {
            using NationalInstruments.DAQmx.Task configTask = new();
            string channel = $"{_deviceName}/{portName}";
            if (isOutput)
            {
                configTask.DOChannels.CreateChannel(channel, "", ChannelLineGrouping.OneChannelForAllLines);
                Console.WriteLine($"[{_deviceName}] {portName} configured as OUTPUT");
            }
            else
            {
                configTask.DIChannels.CreateChannel(channel, "", ChannelLineGrouping.OneChannelForAllLines);
                Console.WriteLine($"[{_deviceName}] {portName} configured as INPUT");
            }

            configTask.Start();
            configTask.Stop();

            // Store configuration in dictionary
            portDirections[portName] = isOutput;
        }

        // GetIOID: returns device name
        public string GetIOID()
        {
            return _deviceName;
        }

        public void Dispose()
        {
            // Nothing persistent yet — provided for future resource cleanup
        }
    }
}