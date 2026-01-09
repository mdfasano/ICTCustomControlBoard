using System;
using IctCustomControlBoard;

namespace IctCustomControlBoard
{
    using System;

    public class BoardManager : IDisposable
    {
        public CustomDIOBoard Board1 { get; private set; } = null!;
        public CustomDIOBoard Board2 { get; private set; } = null!;
        public CustomDIOBoard Board3 { get; private set; } = null!;
        public CustomAIBoard Board4 { get; private set; } = null!;

        public BoardManager()
        {
            InitializeBoards();
        }

        private void InitializeBoards()
        {
            InitializeBoard1();
            InitializeBoard2();
            InitializeBoard3();
            InitializeBoard4();
        }

        // port 0: 0.0 - 0.7  digital output
        // port 1: 1.0, 1.1, and 1.4 - 1.7  digital output. 1.2 and 1.3 not used
        // port 2: 2.0 - 2.7  digital output
        private void InitializeBoard1()
        {
            Board1 = new CustomDIOBoard("Dev1");

            Board1.ConfigurePort("port0", CustomDIOBoard.output);
            Board1.ConfigurePort("port1", CustomDIOBoard.output);
            Board1.ConfigurePort("port2", CustomDIOBoard.output);

            Console.WriteLine("Board1 (DIO) initialized as Dev1.");
        }

        // port 0: not used
        // port 1: 1.0 - 1.4 not used. 1.5 - 1.7 digital output
        // port 2: 2.0 - 2.7 digital output
        private void InitializeBoard2()
        {
            Board2 = new CustomDIOBoard("Dev2");

            Board2.ConfigurePort("port0", CustomDIOBoard.output);
            Board2.ConfigurePort("port1", CustomDIOBoard.output);
            Board2.ConfigurePort("port2", CustomDIOBoard.output);

            Console.WriteLine("Board2 (DIO) initialized as Dev2.");
        }


        // port 0: 0.0 - 0.7 digital input
        // port 1: 1.0 - 1.7 digital input
        // port 2: 2.0 - 2.7 digital input
        private void InitializeBoard3()
        {
            Board3 = new CustomDIOBoard("Dev3");

            Board3.ConfigurePort("port0", CustomDIOBoard.input);
            Board3.ConfigurePort("port1", CustomDIOBoard.input);
            Board3.ConfigurePort("port2", CustomDIOBoard.input);

            Console.WriteLine("Board3 (DIO) initialized as Dev3.");
        }

        // port 0: 0.0 - 0.7 digital input
        // port 1 (4 bit port): 1.0 - 1.2 digital input, 1.3, 1.4 unused
        // port 2 (1 bit port): 2.0 unused
        // AI 0 and AI 1 are active. AI 2 - are unused
        private void InitializeBoard4()
        {
            Board4 = new CustomAIBoard("Dev4");

            Board4.ConfigurePort("port0", CustomAIBoard.input);
            Board4.ConfigurePort("port1", CustomAIBoard.input);
            Board4.ConfigurePort("port2", CustomAIBoard.input);

            Console.WriteLine("Board4 (AI board) initialized as Dev4.");
        }

        public void Dispose()
        {
            Board1?.Dispose();
            Board2?.Dispose();
            Board3?.Dispose();
            Board4?.Dispose();
        }
    }


}