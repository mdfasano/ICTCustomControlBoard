using System;
using System.Configuration;
using System.Collections.Generic;

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
            // names are defined in app.config file, default to Dev1, Dev2, etc
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
            byte b4_port1 = board4.GetBits("port1");
            byte b4_port2 = board4.GetBits("port2");

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
        public CustomBoard.BoardInfo[] GetBoardInfo()
        {
            CustomBoard.BoardInfo board1info = board1.GetIOID();
            CustomBoard.BoardInfo board2info = board2.GetIOID();
            CustomBoard.BoardInfo board3info = board3.GetIOID();
            CustomBoard.BoardInfo board4info = board4.GetIOID();

            return [board1info, board2info, board3info, board4info];
        }
    }
}