using System.Configuration;
using System.Drawing;

namespace IctCustomControlBoard
{
    public class BoardManager
    {
        private readonly CustomBoard board1;
        private readonly CustomBoard board2;
        private readonly CustomBoard board3;
        private readonly CustomBoard board4;

        #region bitmaps to translate between logical and physical bit references
        // array location = logical bit (marks bitmap),
        // integer = physical bit
        // device2 starts at bit 24
        private readonly int[] outputBitmap =
        [
            14 /*0*/, 13, 15, 12, 16, 20/*5*/, 19, 18, 17, 0, 1/*10*/, 2, 3, 4, 23, 22/*15*/, 21,
            5, 6, 7, 8/*20*/, 9, 39, 38, 37, 47/*25*/, 46, 45, 44, 43, 42/*30*/, 41, 40
        ];

        // array location = physical bit (pin),
        // integer = logical bit (mark's bitmap),
        // device2 starts at bit 24
        private readonly int[] inputBitmap =
        [
            11/*0*/, 12, 13, 14, 15, 16/*5*/, 25, 26, 27, 28, 29/*10*/, 30, 2, 33, 1, 0/*15*/, 10, 9, 8,
            7, 6/*20*/, 5, 35, 34, 31, 32/*25*/, 17, 18, 19, 20, 21/*30*/, 22, 23, 24, 4
        ];
        #endregion

        public BoardManager()
        {
            //names are defined in app.config file, default to Dev1, Dev2, etc
            board1 = new CustomBoard(ConfigurationManager.AppSettings["Board1Name"] ?? "Dev1");
            board2 = new CustomBoard(ConfigurationManager.AppSettings["Board2Name"] ?? "Dev2");
            board3 = new CustomBoard(ConfigurationManager.AppSettings["Board3Name"] ?? "Dev3");
            board4 = new CustomBoard(ConfigurationManager.AppSettings["Board4Name"] ?? "Dev4");

            SetBits((ulong)0); // initialize everything to be off
        }

        #region Public Functions
        // setting bits on exactly board1 and board2
        public void SetBits(ulong bits)
        {
            //remap the incoming ulong to something the hardware understands
            bits = RemapBits(bits, outputBitmap);

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

        // reading from board3 and board4
        public ulong GetBits()
        {
            // -------- Board 3 --------
            // this has pull-up resistors, so no signal means high reading
            // which means we need to invert the readback
            byte b3_port0 = (byte)~board3.GetBits("port0");
            byte b3_port1 = (byte)~board3.GetBits("port1");
            byte b3_port2 = (byte)~board3.GetBits("port2");

            // -------- Board 4 --------
            // this is a different device (6002) and has pull-down resistors instead
            // board was physically changed to also have pullup resistors, so we are inverting here too to get appropriate readings
            byte b4_port0 = (byte)~board4.GetBits("port0");
            byte b4_port1 = (byte)~board4.GetBits("port1"); // Do I care that these ports are 4 bit
            byte b4_port2 = (byte)~board4.GetBits("port2"); // and 1 bit in size?

            // Pack everything into a single ulong
            ulong packed = 0;
            packed |= (ulong)b3_port0 << 0;
            packed |= (ulong)b3_port1 << 8;
            packed |= (ulong)b3_port2 << 16;
            packed |= (ulong)b4_port0 << 24;
            packed |= (ulong)b4_port1 << 32;
            packed |= (ulong)b4_port2 << 40;

            // translate physical bit data to user-expected logical organization
            packed = RemapBits(packed, inputBitmap);

            return packed;
        }

        // only two voltages to read, so return as a tuple
        // ADC Channel 0 - Leakage
        // Output of current sense donut with 10 wraps, works from 5ma to 25ma represented by
        // 1V to 5V(10% to 50%) ( 5ma to 25ma)
        //
        // ADC Channel 1 - Load
        // Output of current sense donut with 1 wraps, works from 5A to 25A represented by
        // 1V to 5V(10% to 50%) (5A to 25A)
        public (double Current1, double Current2, double Current3, double Current4) GetCurrents()
        {
            // convert the read voltage to current before returning
            double Current1 = (board4.GetVoltage(0) - 1) * 5; // AIN0: Leakage
            double Current2 = (board4.GetVoltage(1) - 1) * 5; // AIN1: Load
            double Current3 = (board4.GetVoltage(2) - 1) * 5; // AIN2: 24V_OK
            double Current4 = (board4.GetVoltage(3) - 1) * 5; // 24VESTOP_OK

            return (Current1, Current2, Current3, Current4);
        }

        // for testing: reading just voltages
        public (double Voltage0, double Voltage1, double Voltage2, double Voltage3) GetVoltages()
        {
            double Voltage0 = board4.GetVoltage(0);
            double Voltage1 = board4.GetVoltage(1);
            double Voltage2 = board4.GetVoltage(2);
            double Voltage3 = board4.GetVoltage(3);

            return (Voltage0, Voltage1, Voltage2, Voltage3);
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
        #endregion

        #region Helpers
        // translates between physical and logical bit 'locations'
        private static ulong RemapBits(ulong bits, int[] map)
        {
            ulong translatedBits = 0;

            for (int i = 0; i < map.Length; i++) // iterate through the map
            {
                if (((bits >> i) & 1UL) == 1) // checks if the bit at 'i' is set to 1.
                {
                    int n = map[i];
                    translatedBits |= (1UL << n); // flips "output's" nth bit
                }
            }

            return translatedBits;
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
        #endregion
    }
}