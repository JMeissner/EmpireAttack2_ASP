using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmpireAttack2_ASP.Game.TileMap
{
    public struct Point
    {
        #region Private Fields

        public int x;
        public int y;

        #endregion Private Fields

        #region Public Constructors

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        #endregion Public Constructors
    }
}
