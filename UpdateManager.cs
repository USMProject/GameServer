using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class UpdateManager
    {
        public Data GameData { get; set; } = new Data();

        public void IncomingStream(Socket incomingSocket)
        {
            try
            {
                StreamReader stream = new StreamReader(new NetworkStream(incomingSocket));

                while (!stream.EndOfStream)
                {
                    // Parse the incoming data
                    string line = stream.ReadLine();

                    // Do stuff
                    if (line == "Initializing map size")
                    {
                        InitializeMap(40, 40);
                    }
                    else if (line == "Fill map")
                    {
                        FillMap(line);
                    }
                    else if (line == "Init players")
                    {
                        InitializePlayers(line);
                    }
                    else if (line == "Update players")
                    {
                        UpdatePlayers(line);
                    }
                    else if (line == "Update cookies")
                    {
                        UpdateCookies(line);
                    }
                }
            }
            catch (Exception)
            {
                // Tell the player there's a network issue.
            }
        }

        private void InitializeMap(int sizeX, int sizeY)
        {
            // Setup the map
            GameData.MapSizeX = sizeX;
            GameData.MapSizeY = sizeY;
            GameData.map = new bool[sizeX, sizeY];
            GameData.players = new List<Player>();
        }

        private void FillMap(string binaryMap)
        {
            int x = 0;
            int y = 0;

            foreach (char c in binaryMap)
            {
                // parse stuff in.
                GameData.map[x, y] = (c == 1);
                y++;

                if (y > GameData.MapSizeY)
                {
                    x++;
                    y = 0;
                }

                if (x > GameData.MapSizeX)
                {
                    // out of bounds
                }
            }
        }

        // Init players
        private void InitializePlayers(string s)
        {
            // Parse in new players
            string[] playerParams = s.Split(',');
            Player newGuy = new Player(playerParams[0],
                int.Parse(playerParams[1]),
                int.Parse(playerParams[2]),
                int.Parse(playerParams[3]));

            GameData.players.Add(newGuy);
        }

        private void UpdatePlayers(string line)
        {
            //
        }

        private void UpdateCookies(string line)
        {
            //
        }
    }
}