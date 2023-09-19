using System.Net.Sockets;
using System.Net;
using ServerCore;

public class Program
{
    static Listener _listener = new Listener();

    public static void Main(string[] args)
    {
        try
        {
            string ipAdd = "0.0.0.0";
            if (args != null && args.Length >= 1) ipAdd = args[0];

            Thread.Sleep(1000);
            //string ipAdd = "143.244.189.72";
            int port = 8080;

            _listener.Init(ipAdd, port, () =>
            {
                return SessionManager.Instance.Generate();
            });


            while (true)
            {
                Console.WriteLine("Waiting..");
                Thread.Sleep(500);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("[server] main thread exception. " + e.Message);
        }
    }
}