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
        // private string _rootDirectory = "C:\\";
        private string _portNumber = "80";
        private int _port;

        public HTTPService(string[] args)
        {
        }

        protected void OnStart(string[] args)
        {
            // Args
            if (args.Count() > 0)
            {
                _rootDirectory = args[0];
            }

            if (args.Count() > 1)
            {
                _portNumber = args[1];
            }

            if (!int.TryParse(_portNumber, out _port))
            {
                // Default port 80
                _port = 80;
            }

            if (Directory.Exists(_rootDirectory))
            {

                _hostThread = new Thread(RunHTTPService);
                _hostThread.Start();
            }
            else
            {
                OnStop();
            }
        }

        protected void OnStop()
        {
            _listeningSocket?.Close();
        }

        private void RunHTTPService()
        {
            // Run the service
            _listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listeningSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), _port)); // Always localhost
            _listeningSocket.Listen(_port);

            while (true)
            {
                /*WebRequest newWebRequest = new WebRequest(_listeningSocket.Accept())
                {
                    RootDirectory = _rootDirectory
                };*/
                HTTPService service = new HTTPService(null);
                Thread thread = new Thread(() => service.OnStart(null));
                thread.Start();
            }
        }
        void Main()
        {

        }
    }
}