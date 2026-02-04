using System;
using System.Collections.Generic;
using System.Configuration;

namespace IctCustomControlBoard
{
    public class BoardManager
    {
        private readonly CustomBoard board1;
        private readonly CustomBoard board2;
        private readonly CustomBoard board3;
        private readonly CustomBoard board4;


        public BoardManager()
        {
            //names are defined in app.config file, default to Dev1, Dev2, etc
            board1 = new CustomBoard(ConfigurationManager.AppSettings["Board1Name"] ?? "Dev1");
            board2 = new CustomBoard(ConfigurationManager.AppSettings["Board2Name"] ?? "Dev2");
            board3 = new CustomBoard(ConfigurationManager.AppSettings["Board3Name"] ?? "Dev3");
            board4 = new CustomBoard(ConfigurationManager.AppSettings["Board4Name"] ?? "Dev4");
        }

        // assumes setting bits on exactly board1 and board2 for now
        // make more dynamic via app.config later
        public void SetBits(ulong bits)
        {
            // -------- Board 1 --------
            byte b1_port0 = (byte)((bits >> 0) & 0xFF);
            byte b1_port1 = (byte)((bits >> 8) & 0xFF);
            byte b1_port2 = (byte)((bits >> 16) & 0xFF);

            board1.SetBits("port0", b1_port0);
            board1.SetBits("port1", b1_port1);
            board1.SetBits("port2", b1_port2);

            // -------- Board 2 --------
            byte b2_port0 = (byte)((bits >> 24) & 0xFF);
            byte b2_port1 = (byte)((bits >> 32) & 0xFF);
            byte b2_port2 = (byte)((bits >> 40) & 0xFF);

            board2.SetBits("port0", b2_port0);
            board2.SetBits("port1", b2_port1);
            board2.SetBits("port2", b2_port2);
        }

        // assumes reading from board3 and board4
        public ulong GetBits()
        {
            // -------- Board 1 --------
            byte b3_port0 = board3.GetBits("port0");
            byte b3_port1 = board3.GetBits("port1");
            byte b3_port2 = board3.GetBits("port2");

            // -------- Board 2 --------
            byte b4_port0 = board4.GetBits("port0");
            byte b4_port1 = board4.GetBits("port1"); // Do I care that these ports are 4 bit
            byte b4_port2 = board4.GetBits("port2"); // and 1 bit in size?

            // Pack everything into a single ulong
            ulong packed = 0;
            packed |= (ulong)b3_port0 << 0;
            packed |= (ulong)b3_port1 << 8;
            packed |= (ulong)b3_port2 << 16;
            packed |= (ulong)b4_port0 << 24;
            packed |= (ulong)b4_port1 << 32;
            packed |= (ulong)b4_port2 << 40;
            return packed;
        }

        // only two voltages to read, so return as a tuple
        public (double v1, double v2) GetVoltages()
        {
            double v1 = board4.GetVoltage(0);
            double v2 = board4.GetVoltage(1);

            return (v1, v2);
        }

        // returns an array holding four instances of the boardinfo struct
        public BoardInfo[] GetBoardInfo()
        {

            GetSingleBoardInfo(board1, out BoardInfo board1info);
            GetSingleBoardInfo(board2, out BoardInfo board2info);
            GetSingleBoardInfo(board3, out BoardInfo board3info);
            GetSingleBoardInfo(board4, out BoardInfo board4info);

            return [board1info, board2info, board3info, board4info];
        }

        // helper function for GetBoardInfo
        // populates the given 'info' struct with data from the provided 'board'
        private static void GetSingleBoardInfo(CustomBoard board, out BoardInfo info)
        {
            string Board_type = board.GetBoardType();
            long Board_number = board.GetBoardSerialNum();
            string Board_port = board.GetBoardPort();

            info = new(Board_type, Board_number, Board_port);
            return;
        }

        // struct holding relevant info about the board
        public readonly struct BoardInfo(string boardType, long boardSerialNumber, string boardPort)
        {
            public string Board_type { get; } = boardType; // USB-6002 or USB-6501
            public long Board_serial_number { get; } = boardSerialNumber; // unique serial identifier
            public string Board_port { get; } = boardPort; // Internal name of the board. defaults to Dev1, Dev2, etc
        }
    }
}