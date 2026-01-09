using System;

namespace IctCustomControlBoard.Remote
{
    [Serializable]
    public enum BoardCommand { GetBits, SetBits, GetVoltage, GetIOID }

    [Serializable]
    public class BoardRequest
    {
        public int BoardIndex { get; set; }
        public BoardCommand Command { get; set; }
        public string? Port { get; set; }
        public byte Value { get; set; }
        public int Channel { get; set; }
    }

    [Serializable]
    public class BoardResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public byte? Bits { get; set; }
        public double? Voltage { get; set; }
    }
}
