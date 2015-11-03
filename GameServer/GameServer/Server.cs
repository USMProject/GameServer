using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;

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

    public class Data
    {
        public bool dirty;
        public bool[,] map;

        public int GetMapSizeX()
        {
            return map.GetLength(0);
        }
        public int GetMapSizeY()
        {
            return map.GetLength(1);
        }
        public Data(int sizeX, int sizeY)
        {
            map = new bool[sizeX, sizeY];
        }

        /*public Data(string mapin)
        {
            map = new bool 
        }*/
    }

    public class Server
    {
        private Socket _listeningSocket;
        private int _port = 11000;
        public static Data gameState;
        private List<Player> players;
        private List<Cookie> cookies;
        private void MainThread()
        {
            while (true)
            {
                if (gameState.dirty)
                {
                    string playersString = "";
                    foreach(Player pl in players)
                    {
                        playersString += pl.x + "|" + pl.y + "|";
                    }
                    Update(playersString);
                    gameState.dirty = false;
                }
            }
        }
        private void RunService()
        {
            players = new List<Player>();
            gameState = new Data(12, 12);

            Thread mainThread = new Thread(MainThread);
            mainThread.Start();

            // Run the service
            _listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listeningSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), _port)); // Always localhost
            _listeningSocket.Listen(_port);

            //Debug info
            Console.WriteLine("Listening for connections on port " + _port);

            while (true)
            {
                Player player = new Player(_listeningSocket.Accept());
                players.Add(player);
                Thread thread = new Thread(() => player.Run());
                thread.Start();
            }
        }
        public void Update(string sent)
        {
            foreach(Player player in players)
            {
                player.SendUpdate(sent + "\r\n");
            }
        }
        public static void Main()
        {
            Server server = new Server();
            server.RunService();
        }
    }
}