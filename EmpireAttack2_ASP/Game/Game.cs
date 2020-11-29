using EmpireAttack2_ASP.Game.TileMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmpireAttack2_ASP.Game
{
    public class Game
    {
        readonly MapBase map;

        private readonly List<Faction> _faction;
        private readonly Dictionary<Faction, int> _freepopulation;
        public Game(int noOfFactions)
        {
            //Adds the number of specified factions
            _faction = new List<Faction>();
            for(int i = 1; i <= noOfFactions; i++)
            {
                _faction.Add((Faction) i);
            }

            //Load Map
            map = new MapTextImport("/Maps/map_serialized.txt");

            //set capitals of factions
            int[] capitals = map.GetCapitals();
            int counter = 0;
            foreach(Faction f in _faction)
            {
                map.SetTileToFaction(f, capitals[counter], capitals[counter + 1]);
                counter = counter + 2;
            }

            //Generate Coins on Tilemap
            GenerateCoins(5, 70, 90);

            //setup free population of factions
            _freepopulation = new Dictionary<Faction, int>();
            foreach(Faction f in _faction)
            {
                _freepopulation.Add(f, 1);
            }
        }

        public void GenerateCoins(int coindivider, int lowerBound, int upperBound)
        {
            int _NoOfCoins = (int)Math.Ceiling(0.0d + map.tileMap.Length / coindivider) * (int)Math.Ceiling(0.0d + map.tileMap[0].Length / coindivider);
            Random r = new Random();

            for (int i = 0; i <= _NoOfCoins; i++)
            {
                int x = r.Next(0, map.tileMap.Length);
                int y = r.Next(0, map.tileMap[0].Length);

                int c = r.Next(0, 100);

                if(c < lowerBound)
                {
                    //Bronze Coin
                    map.SetCoinOnTile(x, y, Coin.Bronze);
                }
                else if(c < upperBound)
                {
                    //Silver Coin
                    map.SetCoinOnTile(x, y, Coin.Silver);
                }
                else
                {
                    //Gold Coin
                    map.SetCoinOnTile(x, y, Coin.Gold);
                }
            }
        }

        public void AddFreePopulationToAll(int amount)
        {
            lock (_freepopulation)
            {
                foreach(Faction f in _freepopulation.Keys.ToList())
                {
                    _freepopulation[f] += amount;
                }
            }
        }

        public int GetFreePopulationFromFaction(Faction f)
        {
            lock (_freepopulation)
            {
                return _freepopulation[f];
            }
        }

        public List<Faction> GetAllFactions()
        {
            return _faction;
        }

        public string GetSerializedMap()
        {
            return map.GetSerializedMap();
        }

        public bool AttackTile(int x, int y, Faction faction)
        {
            if(map.CanOccupyTile(faction, _freepopulation[faction], x, y))
            {
                map.OccupyTile(faction, _freepopulation[faction], x, y);
                _freepopulation[faction] = 0;
                return true;
            }else if (map.CanAttackTile(x, y, faction))
            {
                map.AttackTile(x, y, _freepopulation[faction]);
                _freepopulation[faction] = 0;
                return true;
            }else if(map.tileMap[x][y].Faction.Equals(faction)){
                map.AddPopulation(x, y, _freepopulation[faction]);
                _freepopulation[faction] = 0;
                return true;
            }
            return false;
        }

        public string AttackTileWithCoin(int x, int y, Faction faction)
        {
            if(!map.IsNeighbor(faction, x, y))
            {
                return null;
            }

            List<string> tiles = new List<string>();
            List<Tile> updatedTiles = new List<Tile>();
            Queue<Tile[]> tileToProcess = new Queue<Tile[]>();
            tileToProcess.Enqueue(map.GetTilesFromCoin(x, y));
            map.SetCoinOnTile(x, y, Coin.None);

            while (tileToProcess.Any())
            {
                foreach(Tile t in tileToProcess.Dequeue())
                {
                    //Tile has a coin on it
                    if (!t.Coin.Equals(Coin.None))
                    {
                        tileToProcess.Enqueue(map.GetTilesFromCoin(t.Coordinates.x, t.Coordinates.y));
                    }
                    if (updatedTiles.Contains(t))
                    {
                        continue;
                    }
                    //Update Tile and add to list
                    map.SetTileToFaction(faction, t.Coordinates.x, t.Coordinates.y);
                    map.SetCoinOnTile(t.Coordinates.x, t.Coordinates.y, Coin.None);
                    updatedTiles.Add(t);
                    tiles.Add(t.Coordinates.x + "," + t.Coordinates.y + "," + t.Faction.ToString() + "," + t.Population + "," + t.Coin.ToString());
                }
            }

            return String.Join(";", tiles);
        }

        public Tile GetTileAtPosition(int x, int y)
        {
            return map.tileMap[x][y];
        }

        public string UpdateTilePopulation()
        {
            int[] capCoords = map.GetCapitals();
            List<string> tiles = new List<string>();
            for(int i = 0; i < capCoords.Length; i += 2)
            {
                tiles.Add(map.UpdateMapPopulation(capCoords[i], capCoords[i + 1]));
            }
            return String.Join(";", tiles);
        }

    }
}
