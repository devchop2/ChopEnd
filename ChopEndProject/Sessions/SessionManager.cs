using System;
using chopsFirstServer;
using ServerCore;

public class SessionManager
{

    static SessionManager _instance = new SessionManager();
    public static SessionManager Instance { get { return _instance; } }

    int sessionId = 0;
    Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();

    object lockObj = new object();

    public ClientSession Generate()
    {
        lock (lockObj)
        {
            int id = ++sessionId;
            ClientSession sess = new ClientSession(id);

            _sessions.Add(id, sess);
            return sess;
        }
    }

    public ClientSession Find(int id)
    {
        lock (lockObj)
        {
            _sessions.TryGetValue(id, out var session);
            if (session != null) return session;
            else throw new Exception("session not exist");
        }

    }

    public void Remove(ClientSession session)
    {
        lock (lockObj)
        {
            _sessions.Remove(session.sessionId);
        }
    }
}