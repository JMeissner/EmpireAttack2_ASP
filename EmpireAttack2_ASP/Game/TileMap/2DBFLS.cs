using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmpireAttack2_ASP.Game.TileMap
{
    public class _2DBFLS
    {
        #region Private Fields

        private int maxX;
        private int maxY;
        private Queue<Point> queue;

        private Tile[][] workingCopyMap;

        #endregion Private Fields

        #region Public Constructors

        public _2DBFLS()
        {
        }

        #endregion Public Constructors

        #region Public Methods

        public Tile[] GetTilesInRadius(Tile[][] map, int x, int y, int radius)
        {
            maxX = map.Length;
            maxY = map[0].Length;
            //Reset Map
            for (int i = 0; i < map.Length; i++)
            {
                for (int j = 0; j < map[0].Length; j++)
                {
                        map[i][j].IsConnected = false;
                        map[i][j].IsVisited = false;
                }
            }

            List<Tile> tiles = new List<Tile>();

            //Init BFS
            workingCopyMap = map;
            queue = new Queue<Point>();
            map[x][y].IsConnected = true;
            map[x][y].IsVisited = true;
            GetNeighbors(x, y, map);
            tiles.Add(map[x][y]);
            //BFS Loop
            for (int i = 0; i < radius; i++)
            {
                Queue<Point> tempqueue = new Queue<Point>();
                while (queue.Any())
                {
                    Point p = queue.Dequeue();
                    map[p.x][p.y].IsVisited = true;
                    tempqueue.Enqueue(p);
                    tiles.Add(map[p.x][p.y]);
                }
                while (tempqueue.Any())
                {
                    Point p = tempqueue.Dequeue();
                    GetNeighbors(p.x, p.y, map);
                }
            }
            //CleanUp
            workingCopyMap = null;
            maxX = 0;
            maxY = 0;
            queue = null;
            return tiles.ToArray();
        }

        #endregion Public Methods

        #region Private Methods

        private void GetNeighbors(int x, int y, Tile[][] map)
        {
            //Up
            if (x + 1 < maxX)
            {
                if (!map[x + 1][y].IsVisited && !queue.Contains(new Point(x + 1, y)))
                {
                    queue.Enqueue(new Point(x + 1, y));
                }
            }
            //Down
            if (x - 1 >= 0)
            {
                if (!map[x - 1][y].IsVisited && !queue.Contains(new Point(x - 1, y)))
                {
                    queue.Enqueue(new Point(x - 1, y));
                }
            }
            //Right
            if (y + 1 < maxY)
            {
                if (!map[x][y + 1].IsVisited && !queue.Contains(new Point(x, y + 1)))
                {
                    queue.Enqueue(new Point(x, y + 1));
                }
            }
            //Left
            if (y - 1 >= 0)
            {
                if (!map[x][y - 1].IsVisited && !queue.Contains(new Point(x, y - 1)))
                {
                    queue.Enqueue(new Point(x, y - 1));
                }
            }
        }

        #endregion Private Methods
    }
}
