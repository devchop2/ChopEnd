namespace ChopEnd
{
    using System;
    using System.Net;
    using ServerCore;

    public class ChopEndClient
    {
        static string ipAddr = "143.244.189.72";
        static int portNum = 8080;
        static ServerSession _session = new ServerSession();

        public static void ConnectServer(Action<StatusCode> result)
        {
            ConnectWithServer(result);
        }

        public static void Login(string nickName, Action<StatusCode> callback)
        {
            if (!_session.IsConnected)
            {
                callback?.Invoke(StatusCode.NotConnected);
                return;
            }

            C_Login login = new C_Login();
            login.nickname = nickName;
            _session.Send(login.Serialize());
        }


        static void ConnectWithServer(Action<StatusCode> handler)
        {
            if (_session != null && _session.IsConnected) return;
            if (_session == null) _session = new ServerSession();

            var host = IPAddress.Parse(ipAddr);
            IPEndPoint endPoint = new IPEndPoint(host, portNum);

            Connector connector = new Connector();
            connector.SetHandler(handler, () => _session);
            connector.Connect(endPoint);
        }
    }
}

