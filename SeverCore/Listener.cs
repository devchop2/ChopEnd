using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class Listener
    {
        Socket? _listenerSocket;
        Func<Session>? _sessionFactory;

        public void Init(string host, int port, Func<Session> _handler, int register = 10, int backlog = 100)
        {

            this._sessionFactory = _handler;
            _listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress address;
            if (host.Equals("0.0.0.0")) address = IPAddress.Any;
            else address = IPAddress.Parse(host);

            IPEndPoint endpoint = new IPEndPoint(address, port);

            _listenerSocket.Bind(endpoint);
            _listenerSocket.Listen(backlog);

            for (int i = 0; i < register; i++)
            {
                var socketArgs = new SocketAsyncEventArgs();
                socketArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);

                RegisterAccept(socketArgs);
            }

            Console.WriteLine("[Server] listener init complete.");

        }

        void RegisterAccept(SocketAsyncEventArgs socketArgs)
        {
            if (_listenerSocket == null) return;

            socketArgs.AcceptSocket = null; //reset
            bool pending = _listenerSocket.AcceptAsync(socketArgs);
            if (!pending) OnAcceptCompleted(null, socketArgs);

        }

        void OnAcceptCompleted(object? sender, SocketAsyncEventArgs args)
        {
            if (_sessionFactory == null)
            {
                Console.WriteLine("session factory not exist.");
                return;
            }
            if (args.SocketError != SocketError.Success)
            {
                Console.WriteLine("socket listen fail. " + args.SocketError.ToString());
                return;
            }

            Console.WriteLine("[server] success connecting");
            Session session = _sessionFactory.Invoke();
            if (args == null || args.AcceptSocket == null)
            {
                Console.WriteLine("socket not exist.");
                return;
            }

            session.Start(args.AcceptSocket);

            RegisterAccept(args);
        }
    }
}

