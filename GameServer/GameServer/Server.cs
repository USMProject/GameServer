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
        public const double cookieRefreshRate = 0.3;
        private static int id;

        private void RunService()
        {
            players = new List<Player>();
            cookies = new List<Cookie>();
            gameState = new Data(64, 64);
            //Create new thread for updating cookies
            new Timer(CookieTick, null, TimeSpan.Zero, TimeSpan.FromSeconds(0.3));

            string localHost = Environment.OSVersion.Platform == PlatformID.Unix ? "0.0.0.0" : "127.0.0.1";

            // Run the service
            _listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listeningSocket.Bind(new IPEndPoint(IPAddress.Parse(localHost), _port)); // Always localhost
            _listeningSocket.Listen(_port);

            //Debug info
            Console.WriteLine("Listening for connections on port " + _port + " and localhost IP: " + localHost);

            
            
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
        public static int GetCookieID()
        {
            id++;
            return id;
        }
        public static void AddCookie(int x, int y, directions dir, int id)
        {
            cookies.Add(new Cookie(id, x, y, dir));
        }
        public static void Update(int code, string sent)
        {
            foreach(Player player in players)
            {
                player.SendUpdate(code + " " + sent + "\r\n");
            }
        }
        

        private void CookieTick(object obj)
        {
            Cookie delete = null;
            foreach(Cookie ck in cookies)
            {
                switch (ck.direction)
                {
                    case directions.down:
                        if (CheckCollision(ck.x, ck.y-1))
                        {
                            if (ck.y > 0)
                                ck.y--;
                            else
                                ck.y = gameState.GetMapSizeY();
                        }
                        else
                        {
                            ck.ttl = 0;
                        }
                        break;
                    case directions.up:
                        if (CheckCollision(ck.x, ck.y+1))
                        {
                            if (ck.y < gameState.GetMapSizeY())
                                ck.y++;
                            else
                                ck.y = 0;
                        }
                        else
                        {
                            ck.ttl = 0;
                        }
                        break;
                    case directions.left:
                        if (CheckCollision(ck.x - 1, ck.y))
                        {
                            if (ck.x > 0)
                                ck.x--;
                            else
                                ck.x = gameState.GetMapSizeY();
                        }
                        else
                        {
                            ck.ttl = 0;
                        }
                        break;
                    case directions.right:
                        if (CheckCollision(ck.x + 1, ck.y))
                        {
                            if (ck.x < gameState.GetMapSizeX())
                                ck.x++;
                            else
                                ck.x = 0;
                        }
                        else
                        {
                            ck.ttl = 0;
                        }
                        break;
                }
                ck.SendUpdate();
                if(ck.ttl == 0)
                {
                    delete = ck;
                }
            }
            if(delete != null)
            {
                cookies.Remove(delete);
            }
        }
        bool CheckCollision(int x, int y)
        {
            if (x < 0) x = Server.gameState.GetMapSizeX() - 1;
            if (y < 0) y = Server.gameState.GetMapSizeY() - 1;
            return Server.gameState.map[x % (Server.gameState.GetMapSizeX()), y % (Server.gameState.GetMapSizeY())] >= 0;
        }
        void DeleteCookies()
        {
        }
        public static void Main()
        {
            Server server = new Server();
            server.RunService();
        }
    }
}