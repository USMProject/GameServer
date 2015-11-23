//COS 460 Computer Networks
//Toss Your Cookies Server
//Pavel Gorelov and Samuel Capotosto
//Player.cs

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
                    Message(line);
                }
            }
            catch (Exception)
            {
                // Tell the player there's a network issue.
            }
        }
        public void Login(string line)
        {
            if (line.ToLower().StartsWith("login "))
            {
                if (line.Length >= 6)
                {
                    username = line.Substring(6, line.Length - 6);
                    Server.Update(200, Server.gameState.GetMapSizeX() + ", " + Server.gameState.GetMapSizeY());
                    Console.WriteLine("Logged in as: " + username);
                    
                }
            }

            Server.SendMap(username);

            Random randomizer = new Random();
            bool placed = false;
            while (!placed)
            {
                int xPos = randomizer.Next(0, Server.gameState.GetMapSizeX());
                int yPos = randomizer.Next(0, Server.gameState.GetMapSizeY());
                if (CheckCollision(xPos, yPos))
                {
                    cookieCount = 20;
                    Server.Update(104, username + ", " + xPos + ", " + yPos + ", " + cookieCount);
                    x = xPos;
                    y = yPos;
                    placed = true;
                }
            }

        }
        public void Message(string line)
        {
            if (line.ToLower().StartsWith("msg "))
            {
                string player = line.Split(' ')[1];
                string message = line.Substring(4 + player.Length + 1);
                Server.SendMessage(player, message);
            }
        }
        public void Throw(string line)
        {
            if (line.ToLower().StartsWith("throw ") || line.ToLower().StartsWith("t "))
            {
                line = line.Substring(line.IndexOf(" ") + 1);
                if (line.StartsWith("u") || line.StartsWith("up"))
                {
                    Server.AddCookie(x, y, directions.up, Server.GetCookieID(), username);
                }
                else if (line.StartsWith("d") || line.StartsWith("down"))
                {
                    Server.AddCookie(x, y, directions.down, Server.GetCookieID(), username);
                }
                else if (line.StartsWith("l") || line.StartsWith("left"))
                {
                    Server.AddCookie(x, y, directions.left, Server.GetCookieID(), username);
                }
                else if (line.StartsWith("r") || line.StartsWith("right"))
                {
                    Server.AddCookie(x, y, directions.right, Server.GetCookieID(), username);
                }
            }
        }
        public void Move(string line)
        {
            bool moved = false;
            if (line.ToLower().StartsWith("move ") || line.ToLower().StartsWith("m "))
            {
                line = line.Substring(line.IndexOf(" ") + 1);
                if (line.StartsWith("u") || line.StartsWith("up"))
                {
                    if(CheckCollision(x, y+1))
                    {
                        y++;
                        y %= Server.gameState.GetMapSizeY();
                        moved = true;
                    }
                    

                }
                else if (line.StartsWith("d") || line.StartsWith("down"))
                {
                    if (CheckCollision(x, y -1 ))
                    {
                        y--;
                        if (y < 0)
                            y = Server.gameState.GetMapSizeY() - 1;
                        moved = true;
                    }
                }
                else if (line.StartsWith("l") || line.StartsWith("left"))
                {
                    if (CheckCollision(x - 1, y))
                    {
                        x--;
                        if (x < 0)
                            x = Server.gameState.GetMapSizeX()-1;
                        moved = true;
                    }
                }
                else if (line.StartsWith("r") || line.StartsWith("right"))
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