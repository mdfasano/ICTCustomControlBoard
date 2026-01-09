
class Program
{
    static void Main()
    {
        BoardManagerServer server = new BoardManagerServer();
        server.Run(); // loop indefinitely and handle pipe requests
    }
}
