//COS 460 Computer Networks
//Toss Your Cookies Server
//Pavel Gorelov and Samuel Capotosto
//Cookie.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    /// <summary>
    /// Cookie class
    /// </summary>
    class Cookie
    {
        // Local variables
        public int id;
        public int x, y;
        public directions direction;
        public int ttl = 20;
        public string thrower;

        /// <summary>
        /// Cookie constructor
        /// </summary>
        public Cookie(int idck, int xp, int yp, directions dir, string throwerName)
        {
            id = idck;
            x = xp;
            y = yp;
            direction = dir;
            thrower = throwerName;
            SendUpdate();
        }
        
        /// <summary>
        /// Send this cookies updated info
        /// </summary>
        public void SendUpdate()
        {
            // Check time to live
            if(ttl > 0)
            {
                ttl--;
                string dirStr = "";
                switch (direction)
                {
                    case directions.up:
                        dirStr = "u";
                        break;
                    case directions.down:
                        dirStr = "d";
                        break;
                    case directions.left:
                        dirStr = "l";
                        break;
                    case directions.right:
                        dirStr = "r";
                        break;
                }
                Server.Update(103, id + ", " + x + ", " + y + ", " + dirStr);
            }
        }
    }
}
