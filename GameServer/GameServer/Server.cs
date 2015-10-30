using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace GameServer
{
    public enum directions
    {
        up,
        down,
        left,
        right
    }

    public class Cookie
    {
        public int x, y;
        public directions direction;
    }

    public class Player
    {
        public string username;
        public int x, y;
        public int cookies;
        public Player (string named, int px, int py, int ck)
        {
            username = named;
            x = px;
            y = py;
            cookies = ck;
        }
    }

    public class Data
    {
        public List<Player> players;
        public List<Cookie> cookies;
        public bool[,] map;

        /*public Data(string mapin)
        {
            map = new bool 
        }*/
    }

    public class Server
    {
        private Socket _listeningSocket;
        private Thread _hostThread;
        private int _port = 80;
        private bool _isRunning;

        protected void Start()
        {
            _isRunning = true;
            _hostThread = new Thread(RunService);
            _hostThread.Start();
        }

        protected void Stop()
        {
            _listeningSocket?.Close();
        }

        private void RunService()
        {
            // Run the service
            _listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listeningSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), _port)); // Always localhost
            _listeningSocket.Listen(_port);

            while (_isRunning)
            {
                // I think that we should simply modify the UpdateManager class with the extra features a server would handle
                // once the client-side gets figured out?

                // Use modified client update manager here.
                // UpdateManager.Something(_listeningSocket.Accept())
                //Thread thread = new Thread(() => service.OnStart(null));
                //thread.Start();
            }
        }

        public static void Main()
        {

        }
    }
}