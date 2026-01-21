using System;
using NationalInstruments.DAQmx;

// a class for interfacing with the usb-6501 Digital IO board from national instruments.
// allows for reading and writing of bits on the device
// also should report its device name when asked
namespace IctCustomControlBoard
{
    public class CustomDIOBoard : IDisposable
    {
        private readonly string _deviceName;

        public CustomDIOBoard(string deviceName)
        {
            _deviceName = deviceName;

            // Configure ports once during construction
            switch (_deviceName)
            {
                case "Dev1":
                    ConfigurePort("port0", output);
                    ConfigurePort("port1", output);
                    ConfigurePort("port2", output);
                    break;

                case "Dev2":
                    ConfigurePort("port0", output);
                    ConfigurePort("port1", output);
                    ConfigurePort("port2", output);
                    break;
                case "Dev3":
                    ConfigurePort("port0", input);
                    ConfigurePort("port1", input);
                    ConfigurePort("port2", input);
                    break;
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
            writer.WriteSingleSamplePort(true, value);
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

        // GetIOID: returns device name
        public string GetIOID()
        {
            return _deviceName;
        }

        public void ConfigurePort(string portName, bool isOutput = false)
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


        public void Dispose()
        {
            // Nothing persistent yet — provided for future resource cleanup
        }
    }
}
