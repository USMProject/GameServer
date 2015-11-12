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
        //public bool dirty;
        public int[,] map;

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
            Random random = new Random();
            map = new int[sizeX, sizeY];
            //Generate a random map
            for(int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    if(random.Next(0, 101) < 5)
                    {
                        map[x, y] = -1;  
                    }
                    else
                    {
                        map[x, y] = 1;
                    }
                }
            }
        }
    }

    public class Server
    {
        private Socket _listeningSocket;
        private int _port = 11000;
        public static Data gameState;
        public static List<Player> players;
        private static List<Cookie> cookies;
        private void MainThread()
        {
            while (true)
            {
                /*if (gameState.dirty)
                {
                    string playersString = "";
                    foreach(Player pl in players)
                    {
                        playersString += pl.username + "|" + pl.x + "|" + pl.y + "|";
                    }
                    Update(playersString);
                    gameState.dirty = false;
                }*/
            }
        }
        private void RunService()
        {
            players = new List<Player>();
            gameState = new Data(64, 64);

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
        public static void SendMap()
        {
            for(int y = 0; y < gameState.GetMapSizeY(); y++)
            {
                string sent = "0, " + y + ", " + (gameState.GetMapSizeX()-1) + ", " + y + ", ";
                for (int x = 0; x < gameState.GetMapSizeX(); x++)
                {
                    sent += gameState.map[x, y] + ", ";
                }
                sent = sent.Substring(0, sent.Length - 2);
                Update(102, sent);
            }
            
        }
        public static void Update(int code, string sent)
        {
            foreach(Player player in players)
            {
                player.SendUpdate(code + " " + sent + "\r\n");
            }
        }
        public static void Main()
        {
            Server server = new Server();
            server.RunService();
        }
    }
}