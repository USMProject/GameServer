using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class Player
    {
        public string username;
        public int x, y;
        public int cookieCount;

        //May not need this
        //public Data GameData { get; set; } = new Data();

        Socket _incomingSocket;

        public Player(Socket incomingSocket)
        {
            Console.WriteLine("Connected!");
            _incomingSocket = incomingSocket;
        }

        public void Run()
        {
            try
            {
                
                //NetworkStream stream = new NetworkStream(_incomingSocket);
                StreamReader stream = new StreamReader(new NetworkStream(_incomingSocket));
                bool loggedIn = false;
                while (stream != null && !stream.EndOfStream)
                {
                    // Parse the incoming data
                    string line = stream.ReadLine();
                    Console.WriteLine("Readin: " + line);
                    if(!loggedIn)
                    {
                        loggedIn = true;
                        Login(line);
                        
                    }
                    Move(line);
                    Throw(line);
                }
            }
            catch (Exception)
            {
                // Tell the player there's a network issue.
            }
        }
        public void Login(string line)
        {
            if (line.StartsWith("LOGIN "))
            {
                if (line.Length >= 6)
                {
                    username = line.Substring(6, line.Length - 6);
                    /*foreach (Player pl in Server.players.Where(n => n.username == username))
                    {
                        Server.Update(500, "Username already taken");
                        return;
                    }*/

                    /*foreach (Player pl in Server.players)
                    {
                        if(pl.username == username)
                        {
                            Server.Update(500, "username already taken");
                            return;
                        }
                    }*/
                    Server.Update(200, Server.gameState.GetMapSizeX() + ", " + Server.gameState.GetMapSizeY());
                    Console.WriteLine("Logged in as: " + username);
                }
            }
            Server.SendMap();
            //Server.Update(102, "0, 0, 3, 3, 1, -1, 1, 1, 1, 1, -1, 1, 1, 1, 1, 1, -1, -1, -1, -1");

            Random randomizer = new Random();
            bool placed = false;
            while (!placed)
            {
                int xPos = randomizer.Next(0, Server.gameState.GetMapSizeX());
                int yPos = randomizer.Next(0, Server.gameState.GetMapSizeY());
                if (CheckCollision(xPos, yPos))
                {
                    Server.Update(104, username + ", " + xPos + ", " + yPos + ", " + cookieCount);
                    x = xPos;
                    y = yPos;
                    placed = true;
                }
            }
        }
        public void Throw(string line)
        {
            if (line.ToLower().StartsWith("throw ") || line.ToLower().StartsWith("t "))
            {
                line = line.Substring(line.IndexOf(" ") + 1);
                char throwDir = line[0];
                if (throwDir == 'u')
                {
                   Server.AddCookie(x, y, directions.up, Server.GetCookieID());
                }
                else if (throwDir == 'd')
                {
                        Server.AddCookie(x, y, directions.down, Server.GetCookieID());
                }
                else if (throwDir == 'l')
                {
                        Server.AddCookie(x, y, directions.left, Server.GetCookieID());
                }
                else if (throwDir == 'r')
                {
                        Server.AddCookie(x, y, directions.right, Server.GetCookieID());
                }
            }
        }
        public void Move(string line)
        {
            bool moved = false;
            if (line.ToLower().StartsWith("move ") || line.ToLower().StartsWith("m "))
            {
                line = line.Substring(line.IndexOf(" ") + 1);
                char moveDir = line[0];
                if (moveDir == 'u')
                {
                    if(CheckCollision(x, y+1))
                    {
                        y++;
                        y %= Server.gameState.GetMapSizeY();
                        moved = true;
                    }
                    

                }
                else if (moveDir == 'd')
                {
                    if (CheckCollision(x, y -1 ))
                    {
                        y--;
                        if (y < 0)
                            y = Server.gameState.GetMapSizeY() - 1;
                        moved = true;
                    }
                }
                else if (moveDir == 'l')
                {
                    if (CheckCollision(x - 1, y))
                    {
                        x--;
                        if (x < 0)
                            x = Server.gameState.GetMapSizeX()-1;
                        moved = true;
                    }
                }
                else if (moveDir == 'r')
                {
                    if (CheckCollision(x + 1, y))
                    {
                        x++;
                        x %= Server.gameState.GetMapSizeX();
                        moved = true;
                    }
                }
            }
            if(moved)
            {
                Server.Update(104, username + ", " + x + ", " + y + ", " + cookieCount);
            }
            
        }
        bool CheckCollision(int x, int y)
        {
            if (x < 0) x = Server.gameState.GetMapSizeX() - 1;
            if (y < 0) y = Server.gameState.GetMapSizeY() - 1;
            return Server.gameState.map[x % (Server.gameState.GetMapSizeX()), y % (Server.gameState.GetMapSizeY())] >= 0;
        }
        public void SendUpdate(string sent)
        {
            _incomingSocket.Send(Encoding.ASCII.GetBytes(sent));
        }
        public void InvalidCommand()
        {
            Console.WriteLine("Invalid Command!");
        }
    }
}