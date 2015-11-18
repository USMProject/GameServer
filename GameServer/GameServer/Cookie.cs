using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    class Cookie
    {
        public int id;
        public int x, y;
        public directions direction;
        public int ttl = 20;
        public string thrower;

        public Cookie(int idck, int xp, int yp, directions dir, string throwerName)
        {
            id = idck;
            x = xp;
            y = yp;
            direction = dir;
            thrower = throwerName;
            SendUpdate();
        }
            
        public void SendUpdate()
        {
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
