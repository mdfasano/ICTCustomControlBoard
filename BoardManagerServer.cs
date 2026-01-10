using System;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using IctCustomControlBoard.Remote;

namespace IctCustomControlBoard
{
    public class BoardManagerServer
    {
        private readonly CustomDIOBoard board1;
        private readonly CustomDIOBoard board2;
        private readonly CustomDIOBoard board3;
        private readonly CustomAIBoard board4;

        public BoardManagerServer()
        {
            board1 = new CustomDIOBoard("Dev1");
            board2 = new CustomDIOBoard("Dev2");
            board3 = new CustomDIOBoard("Dev3");
            board4 = new CustomAIBoard("Dev4");
        }

        public void Run()
        {
            Console.WriteLine("Board Manager Server Running (Named Pipes with JSON)...");

            while (true)
            {
                using NamedPipeServerStream pipe = new("BoardPipe", PipeDirection.InOut);
                pipe.WaitForConnection();
                try
                {
                    using var sr = new StreamReader(pipe);
                    using var sw = new StreamWriter(pipe) { AutoFlush = true };
                    // Read JSON request from client
                    string jsonRequest = sr.ReadLine()!;
                    BoardRequest request = JsonSerializer.Deserialize<BoardRequest>(jsonRequest)!;

                    // Process the request
                    BoardResponse response = ProcessRequest(request);

                    // Send JSON response back
                    string jsonResponse = JsonSerializer.Serialize(response);
                    sw.WriteLine(jsonResponse);
                }
                catch (Exception ex)
                {
                    using var sw = new StreamWriter(pipe) { AutoFlush = true };
                    var errorResponse = new BoardResponse
                    {
                        Success = false,
                        Message = ex.Message
                    };
                    sw.WriteLine(JsonSerializer.Serialize(errorResponse));
                }
            }
        }

        private BoardResponse ProcessRequest(BoardRequest request)
        {
            BoardResponse response = new();

            try
            {
                switch (request.BoardIndex)
                {
                    case 1: HandleBoard(board1, request, response); break;
                    case 2: HandleBoard(board2, request, response); break;
                    case 3: HandleBoard(board3, request, response); break;
                    case 4: HandleBoard(board4, request, response); break;
                    default: throw new ArgumentException("Invalid board index");
                }

                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        private static void HandleBoard(dynamic board, BoardRequest request, BoardResponse response)
        {
            switch (request.Command)
            {
                case BoardCommand.GetBits: response.Bits = board.GetBits(PortNumberToName(request.Port!)); break;
                case BoardCommand.SetBits: board.SetBits(PortNumberToName(request.Port!), request.Value); break;
                case BoardCommand.GetVoltage: response.Voltage = board.GetVoltage(request.Channel); break;
                case BoardCommand.GetIOID: response.Message = board.GetIOID(); break;
                default: throw new ArgumentException("Unknown command");
            }
        }
        private static string PortNumberToName(int portNumber)
        {
            return $"port{portNumber}";
        }
    }
}