//COS 460 Computer Networks
//Toss Your Cookies Server
//Pavel Gorelov and Samuel Capotosto
//Server.cs

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;

namespace GameServer
{
    /// <summary>
    /// Directions enum
    /// </summary>
    public enum directions
    {
        up,
        down,
        left,
        right
    }

    /// <summary>
    /// Data class to hold the map
    /// </summary>
    public class Data
    {
        /// <summary>
        /// Map array of ints
        /// </summary>
        public int[,] map;

        /// <summary>
        /// Map size X
        /// </summary>
        public int GetMapSizeX()
        {
            return map.GetLength(0);
        }

        /// <summary>
        /// Map size Y
        /// </summary>
        public int GetMapSizeY()
        {
            return map.GetLength(1);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Data(int sizeX, int sizeY)
        {
            Random random = new Random();
            map = new int[sizeX, sizeY];

            //Generate a random map
            for(int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    // 5% populated
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

    /// <summary>
    /// Server class to manage activities
    /// </summary>
    public class Server
    {
        // Verver variables
        private Socket _listeningSocket;
        private int _port = 11000;
        public static Data gameState;
        public static List<Player> players;
        private static List<Cookie> cookies;
        public const double cookieRefreshRate = 0.3;
        private static int id;

        // Servers most important running method
        private void RunService()
        {
            // Important variable instantiation
            players = new List<Player>();
            cookies = new List<Cookie>();
            gameState = new Data(64, 64);

            //Create new thread for updating cookies
            new Timer(CookieTick, null, TimeSpan.Zero, TimeSpan.FromSeconds(0.3));

            // Windows or Unix localhost
            string localHost = Environment.OSVersion.Platform == PlatformID.Unix ? "0.0.0.0" : "127.0.0.1";

            // Setup the TCP socket
            _listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listeningSocket.Bind(new IPEndPoint(IPAddress.Parse(localHost), _port)); // Always localhost
            _listeningSocket.Listen(_port);

            //Debug info
            Console.WriteLine("Listening for connections on port " + _port + " and localhost IP: " + localHost);
            
            // Socket listening loop
            while (true)
            {
                Player player = new Player(_listeningSocket.Accept());
                players.Add(player);
                Thread thread = new Thread(() => player.Run());
                thread.Start();
            }
        }

        /// <summary>
        /// Send the map to specific player
        /// </summary>
        public static void SendMap(string player)
        {
            // Send one row at a time
            for(int y = 0; y < gameState.GetMapSizeY(); y++)
            {
                string sent = "0, " + y + ", " + (gameState.GetMapSizeX()-1) + ", " + y + ", ";
                for (int x = 0; x < gameState.GetMapSizeX(); x++)
                {
                    sent += gameState.map[x, y] + ", ";
                }
                sent = sent.Substring(0, sent.Length - 2);
                Update(102, sent, player);
            }
        }

        /// <summary>
        /// Get and increment a cookie id
        /// </summary>
        /// <returns></returns>
        public static int GetCookieID()
        {
            id++;
            return id;
        }

        /// <summary>
        /// Add a cookie to the jar
        /// </summary>
        public static void AddCookie(int x, int y, directions dir, int id, string throwerName)
        {
            cookies.Add(new Cookie(id, x, y, dir, throwerName));
        }

        /// <summary>
        /// Update each player with a global game update
        /// </summary>
        public static void Update(int code, string sent)
        {
            foreach(Player player in players)
            {
                player.SendUpdate(code + " " + sent + "\r\n");
            }
        }

        /// <summary>
        /// Specific user messaging or update
        /// </summary>
        public static void Update(int code, string sent, string sendTo)
        {
            foreach (Player player in players)
            {
                if(player.username == sendTo)
                {
                    player.SendUpdate(code + " " + sent + "\r\n");
                    return;
                }
            }
        }

        /// <summary>
        /// Update the cookies using an interval
        /// </summary>
        private void CookieTick(object obj)
        {
            Cookie delete = null;
            foreach(Cookie ck in cookies)
            {
                switch (ck.direction)
                {
                    // Check for wall collisions
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
                            // Time to live is out
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

                // Check if cookie hits a player
                string decrPlayersCookies = string.Empty;
                foreach (Player pl in players)
                {
                    if (ck.x == pl.x && ck.y == pl.y && ck.thrower != pl.username)
                    {
                        // Saving the username
                        decrPlayersCookies = pl.username;
                    }
                }

                // Update the cookie or delete it
                if (decrPlayersCookies == string.Empty)
                {
                    ck.SendUpdate();
                    if (ck.ttl == 0)
                    {
                        delete = ck;
                    }
                }
                else
                {
                    DecrementCookieCount(ck.thrower);
                    delete = ck;
                }
            }

            // Delete the cookie if needed
            if(delete != null)
            {
                cookies.Remove(delete);
            }
        }

        /// <summary>
        /// Check for wall collision
        /// </summary>
        bool CheckCollision(int x, int y)
        {
            if (x < 0) x = gameState.GetMapSizeX() - 1;
            if (y < 0) y = gameState.GetMapSizeY() - 1;
            return gameState.map[x % (gameState.GetMapSizeX()), y % (gameState.GetMapSizeY())] >= 0;
        }

        /// <summary>
        /// Decrement cookie count of a player and check if a winner
        /// </summary>
        private static void DecrementCookieCount(string username)
        {
            Player pl = players.Find(x => x.username.Equals(username));
            pl.cookieCount--;
            if (pl.cookieCount <= 0)
            {
                Update(100, pl.username + " won this game!");
                // Reset the game....
                //Thread.Sleep(2000);
            }
        }

        /// <summary>
        /// Main method
        /// </summary>
        public static void Main()
        {
            Server server = new Server();
            server.RunService();
        }

        /// <summary>
        /// Messaging system
        /// </summary>
        public static void SendMessage(string sendTo, string message)
        {
            Console.WriteLine("'" + sendTo + "'   '" + message + "'");
            if(sendTo == "all")
            {
                Update(500, message);
            }
            else
            {
                Update(500, message, sendTo);
            }
        }
    }
}