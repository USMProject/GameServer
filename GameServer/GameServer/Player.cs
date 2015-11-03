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
        public int cookies;

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

                while (stream != null && !stream.EndOfStream)
                {
                    // Parse the incoming data
                    string line = stream.ReadLine();
                    Console.WriteLine("Readin: " + line);
                    Login(line);
                    Move(line);
                    
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
                    Console.WriteLine("Logged in as: " + username);
                }
            }
        }
        public void Move(string line)
        {
            if (line.StartsWith("MOVE "))
            {
                Console.WriteLine("Movement");
                if (line.Length == 6)
                {
                    char moveDir = line[5];
                    if (moveDir == 'u')
                    {
                        y++;
                        y %= Server.gameState.GetMapSizeY();
                        Console.WriteLine("Up");
                    }
                    else if (moveDir == 'd')
                    {
                        y--;
                        if(y < 0)
                            y = 11;
                        Console.WriteLine("Down");
                    }
                    else if (moveDir == 'l')
                    {
                        x--;
                        if (x < 0)
                            x = 11;
                        x %= Server.gameState.GetMapSizeX();
                        Console.WriteLine("Left");
                    }
                    else if (moveDir == 'r')
                    {
                        x++;
                        x %= Server.gameState.GetMapSizeX();
                        Console.WriteLine("Right");
                    }
                    Server.gameState.dirty = true;
                }
                Console.WriteLine("Player " + username + " moved to x" + x + " y" + y);
            }
            else
            {
                InvalidCommand();
            }
        }
        public void SendUpdate(string sent)
        {
            Console.WriteLine("Sending! " + sent);
            _incomingSocket.Send(Encoding.ASCII.GetBytes(sent));
        }
        public void InvalidCommand()
        {
            Console.WriteLine("Invalid Command!");
        }
    }
}