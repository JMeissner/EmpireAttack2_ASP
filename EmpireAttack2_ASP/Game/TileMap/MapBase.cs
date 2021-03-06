﻿using EmpireAttackServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmpireAttack2_ASP.Game.TileMap
{
    public class MapBase : IMap
    {
        #region Public Fields

        public Tile[][] tileMap;

        #endregion Public Fields

        #region Public Constructors

        public MapBase()
        {
        }

        #endregion Public Constructors

        #region Public Methods

        public void AddPopulation(int x, int y, int amount)
        {
            tileMap[x][y].Population += amount;
        }

        public bool CanOccupyTile(Faction faction, int attackingForce, int x, int y)
        {
            if (tileMap[x][y].Faction.Equals(faction))
            {
                return false;
            }
            bool canOccupy = false;
            if (x + 1 < tileMap.Length)
            {
                if (tileMap[x + 1][y].Faction.Equals(faction) && tileMap[x][y].Population < attackingForce) { canOccupy = true; }
            }
            if (x - 1 >= 0)
            {
                if (tileMap[x - 1][y].Faction.Equals(faction) && tileMap[x][y].Population < attackingForce) { canOccupy = true; }
            }
            if (y + 1 < tileMap[0].Length)
            {
                if (tileMap[x][y + 1].Faction.Equals(faction) && tileMap[x][y].Population < attackingForce) { canOccupy = true; }
            }
            if (y - 1 >= 0)
            {
                if (tileMap[x][y - 1].Faction.Equals(faction) && tileMap[x][y].Population < attackingForce) { canOccupy = true; }
            }
            return canOccupy;
        }

        public bool CanAttackTile(int x, int y, Faction faction)
        {
            if(IsNeighbor(faction, x, y) && !tileMap[x][y].Faction.Equals(faction))
            {
                return true;
            }
            return false;
        }

        public void AttackTile(int x, int y, int attackingForce)
        {
            tileMap[x][y].Population -= attackingForce;
        }

        public int[] GetCapitals()
        {
            List<int> rList = new List<int>();
            for (int i = 0; i < tileMap.Length; i++)
            {
                for (int j = 0; j < tileMap[0].Length; j++)
                {
                    if (tileMap[i][j].Type.Equals(TileType.Capital))
                    {
                        rList.Add(i);
                        rList.Add(j);
                    }
                }
            }
            return rList.ToArray();
        }

        public void GetPopulation(Faction faction)
        {
            throw new NotImplementedException();
        }

        public TileType GetTileType(int x, int y)
        {
            return tileMap[x][y].Type;
        }

        public bool IsNeighbor(int x, int y)
        {
            bool isconnected = false;
            if (tileMap[x + 1][y].Faction.Equals(tileMap[x][y].Faction)) { isconnected = true; }
            if (tileMap[x - 1][y].Faction.Equals(tileMap[x][y].Faction)) { isconnected = true; }
            if (tileMap[x][y + 1].Faction.Equals(tileMap[x][y].Faction)) { isconnected = true; }
            if (tileMap[x][y - 1].Faction.Equals(tileMap[x][y].Faction)) { isconnected = true; }
            return isconnected;
        }

        public bool IsNeighbor(Faction faction, int x, int y)
        {
            bool isconnected = false;
            if (tileMap[x + 1][y].Faction.Equals(faction)) { isconnected = true; }
            if (tileMap[x - 1][y].Faction.Equals(faction)) { isconnected = true; }
            if (tileMap[x][y + 1].Faction.Equals(faction)) { isconnected = true; }
            if (tileMap[x][y - 1].Faction.Equals(faction)) { isconnected = true; }
            return isconnected;
        }

        //TODO: Apply modfiers for terrain
        public bool OccupyTile(Faction faction, int attackingForce, int x, int y)
        {
            int previousPop = tileMap[x][y].Population;
            tileMap[x][y].Faction = faction;
            tileMap[x][y].Population = attackingForce - previousPop;
            return true;
        }

        public void SetTileToFaction(Faction faction, int x, int y)
        {
            tileMap[x][y].Faction = faction;
        }

        public List<Tile> OvertakeEnemyTiles(Faction newf, Faction previous)
        {
            List<Tile> returnTiles = new List<Tile>();
            for (int i = 0; i < tileMap.Length; i++)
            {
                for (int j = 0; j < tileMap[0].Length; j++)
                {
                    if (tileMap[i][j].Faction.Equals(previous))
                    {
                        tileMap[i][j].Faction = newf;
                        if (tileMap[i][j].Type.Equals(TileType.Capital))
                        {
                            tileMap[i][j].Type = TileType.Normal;
                        }
                        returnTiles.Add(tileMap[i][j]);
                    }
                }
            }
            return returnTiles;
        }

        public void RemoveCapitals()
        {
            for (int i = 0; i < tileMap.Length; i++)
            {
                for (int j = 0; j < tileMap[0].Length; j++)
                {
                    if (tileMap[i][j].Type.Equals(TileType.Capital))
                    {
                        tileMap[i][j].Type = TileType.Normal;
                    }
                }
            }
        }

        public void SetCapitalAtPosition(int i, int j, Faction faction)
        {
            tileMap[i][j].Type = TileType.Capital;
            tileMap[i][j].Faction = faction;
            tileMap[i][j].Population = 1;
        }

        //public int UpdateMapPopulation(int x, int y)
        //{
        //    //BFS
        //    //Connected + 1
        //    //Unconnected - 1
        //    Faction faction = tileMap[x][y].Faction;
        //    int updatedTiles = 0;
        //    _2DBFS bfs = new _2DBFS();
        //    bfs.BFS(tileMap, x, y, 999999);

        //    for (int i = 0; i < tileMap.Length; i++)
        //    {
        //        for (int j = 0; j < tileMap[0].Length; j++)
        //        {
        //            if (tileMap[i][j].Faction == faction)
        //            {
        //                updatedTiles++;
        //                if (tileMap[i][j].IsConnected)
        //                {
        //                    tileMap[i][j].Population++;
        //                }
        //                else
        //                {
        //                    tileMap[i][j].Population--;
        //                    if (tileMap[i][j].Population <= 0)
        //                    {
        //                        //TODO: Update Lost tiles
        //                        tileMap[i][j].Faction = Faction.NONE;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return updatedTiles;
        //}

        public string UpdateMapPopulation(int x, int y)
        {
            //BFS
            //Connected + 1
            //Unconnected - 1
            Faction faction = tileMap[x][y].Faction;
            int updatedTiles = 0;
            _2DBFS bfs = new _2DBFS();
            bfs.BFS(tileMap, x, y, 999999);

            //stringbuilder
            List<string> updatedTilesString = new List<string>();
            for (int i = 0; i < tileMap.Length; i++)
            {
                for (int j = 0; j < tileMap[0].Length; j++)
                {
                    if (tileMap[i][j].Faction == faction)
                    {
                        updatedTiles++;
                        if (tileMap[i][j].IsConnected)
                        {
                            tileMap[i][j].Population++;
                        }
                        else
                        {
                            tileMap[i][j].Population--;
                            if (tileMap[i][j].Population <= 0)
                            {
                                tileMap[i][j].Faction = Faction.NONE;
                            }
                        }
                        string _t = "" + i + "," + j + "," + tileMap[i][j].Faction.ToString() + "," + tileMap[i][j].Population + "," + tileMap[i][j].Coin;
                        updatedTilesString.Add(_t);
                    }
                }
            }
            string concatTilesString = string.Join(";", updatedTilesString);
            return concatTilesString;
        }

        public Tile[] GetTilesFromCoin(int x, int y)
        {
            _2DBFLS bfs = new _2DBFLS();
            switch (tileMap[x][y].Coin)
            {
                case Coin.Bronze: return bfs.GetTilesInRadius(tileMap, x, y, 1);
                case Coin.Silver: return bfs.GetTilesInRadius(tileMap, x, y, 2);
                case Coin.Gold: return bfs.GetTilesInRadius(tileMap, x, y, 3);
            }
            return null;
        }

        public string GetSerializedMap()
        {
            string _return = tileMap.Length + ":" + tileMap[0].Length + "#";
            List<string> _tiles = new List<string>();
            for (int i = 0; i < tileMap.Length; i++)
            {
                for (int j = 0; j < tileMap[0].Length; j++)
                {
                    string _t = tileMap[i][j].GetShortType() + ",";
                    _t += tileMap[i][j].Faction.ToString() + ",";
                    _t += tileMap[i][j].Population.ToString() + ",";
                    _t += tileMap[i][j].Coin.ToString() + ",";
                    _tiles.Add(_t);
                }
            }
            _return += String.Join(";", _tiles);
            return _return;
        }

        public void SetCoinOnTile(int x, int y, Coin coin)
        {
            tileMap[x][y].Coin = coin;
        }

        #endregion Public Methods
    }
}