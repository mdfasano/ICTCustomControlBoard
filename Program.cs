using IctCustomControlBoard;
class Program
{
    static void Main()
    {
        BoardManagerServer server = new();
        server.Run(); // loop indefinitely and handle pipe requests
    }
}
