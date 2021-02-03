using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OBSSwitcher
{
    public class MouseCords
    {
        public int PosX { get; set; }
        public int PosY { get; set; }

        public MouseCords()
        {
            PosX = Cursor.Position.X;
            PosY = Cursor.Position.Y;
        }
    }
}
