using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class Connector
    {
        Func<Session>? sessionFactory;
        Action<StatusCode>? handler;
        public void SetHandler(Action<StatusCode> handler, Func<Session> sessionFunc)
        {
            this.handler = handler;
            this.sessionFactory = sessionFunc;
        }

        public void Connect(IPEndPoint endPoint)
        {
            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.UserToken = socket;
            args.RemoteEndPoint = endPoint;
            args.Completed += OnConnectCompleted;

            RegisterConnect(args);
        }

        void RegisterConnect(SocketAsyncEventArgs args)
        {
            if (args == null || !(args.UserToken is Socket socket)) return;

            bool pending = socket.ConnectAsync(args);
            if (!pending) OnConnectCompleted(null, args);

        }

        void OnConnectCompleted(object? sender, SocketAsyncEventArgs args)
        {
            if (sessionFactory == null)
            {
                Console.WriteLine("session factory not exist.");
                handler?.Invoke(StatusCode.InvalidRequest);
                return;
            }

            if (args.SocketError != SocketError.Success)
            {
                Console.WriteLine("On Connect fail." + args.SocketError.ToString());
                handler?.Invoke(StatusCode.UnknownError);
                return;
            }


            Session session = sessionFactory.Invoke();
            if (args.ConnectSocket != null) session.Start(args.ConnectSocket);
            if (args.RemoteEndPoint != null) session.OnConnected(args.RemoteEndPoint);
            handler?.Invoke(StatusCode.Success);
            Console.WriteLine("success connection." + session);
        }
    }
}

