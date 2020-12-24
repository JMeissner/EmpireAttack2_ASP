﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EmpireAttack2_ASP.Game.TileMap
{
    public class MapTextImport : MapBase
    {
        public MapTextImport(string path)
        {
            MapDataToTileMap(path);
        }

        private void MapDataToTileMap(string path)
        {
            //string dataDir = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            //string dataDir = AppDomain.CurrentDomain.BaseDirectory.ToString() + "/App_Data";
            string dataDir = Path.Combine(GameManager.Instance.webRootPath, "App_Data", path);
            string _mapData = System.IO.File.ReadAllText(dataDir);
            string[] _sizeAndTiles = _mapData.Split('#');

            string[] _mapSize = _sizeAndTiles[0].Split(':');
            int mapSizeX = int.Parse(_mapSize[0]);
            int mapSizeY = int.Parse(_mapSize[1]);

            string[] _mapTiles = _sizeAndTiles[1].Split(';');

            //Array Init
            tileMap = new Tile[mapSizeX][];
            for (int x = 0; x < mapSizeX; x++)
            {
                tileMap[x] = new Tile[mapSizeY];
            }

            foreach(string t in _mapTiles)
            {
                string[] _t = t.Split(',');
                int i = int.Parse(_t[0]);
                int j = int.Parse(_t[1]);
                switch (_t[2])
                {
                    case "N": tileMap[i][j] = new Tile(TileType.Normal, i, j, true); break;
                    case "W": tileMap[i][j] = new Tile(TileType.Water, i, j, true); break;
                    case "F": tileMap[i][j] = new Tile(TileType.Forest, i, j, true); break;
                    case "H": tileMap[i][j] = new Tile(TileType.Hills, i, j, true); break;
                    case "U": tileMap[i][j] = new Tile(TileType.Urban, i, j, true); break;
                    case "C": tileMap[i][j] = new Tile(TileType.Capital, i, j, true); break;
                    default: throw new Exception("tiletype did not match (Import->Convert Shorttype to Tiletype)");
                }
            }
            System.Diagnostics.Debug.WriteLine("Map Import Completed.");
        }
    }
}
