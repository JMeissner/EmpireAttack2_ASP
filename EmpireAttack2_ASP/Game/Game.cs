using EmpireAttack2_ASP.Game.TileMap;
using System;
using System.Collections.Generic;
using System.IO;
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
            string mapPath = Path.Combine("Maps", "map_serialized.txt");
            map = new MapTextImport(mapPath);

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
            //Number of coins depends on mapsize and how many coins there should be per row/column
            int _NoOfCoins = (int)Math.Ceiling(0.0d + map.tileMap.Length / coindivider) * (int)Math.Ceiling(0.0d + map.tileMap[0].Length / coindivider);
            Random r = new Random();

            for (int i = 0; i <= _NoOfCoins; i++)
            {
                //Random x and y coordinates
                int x = r.Next(0, map.tileMap.Length);
                int y = r.Next(0, map.tileMap[0].Length);

                //Random coin which is determined by bounds
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

        public bool AttackTile(int x, int y, bool halfPopulation, Faction faction)
        {
            int attackingPopulation = GetAttackingForce(halfPopulation, faction);

            if(map.CanOccupyTile(faction, attackingPopulation, x, y))
            {
                map.OccupyTile(faction, attackingPopulation, x, y);
                _freepopulation[faction] = _freepopulation[faction] - attackingPopulation;
                return true;
            }else if (map.CanAttackTile(x, y, faction))
            {
                map.AttackTile(x, y, attackingPopulation);
                _freepopulation[faction] = _freepopulation[faction] - attackingPopulation;
                return true;
            }else if(map.tileMap[x][y].Faction.Equals(faction)){
                map.AddPopulation(x, y, attackingPopulation);
                _freepopulation[faction] = _freepopulation[faction] - attackingPopulation;
                return true;
            }
            return false;
        }

        public string AttackTileWithCoin(int x, int y, Faction faction)
        {
            //TODO: There is currently no check if the coin captures a captital
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

        public string AttackCapital(int x, int y, bool halfPopulation, Faction faction)
        {
            int attackPopulation = GetAttackingForce(halfPopulation, faction);
            //Can overtake? Capture all enemy Tiles and remove faction from availables
            if (CanOverTakeTile(x, y, halfPopulation, faction))
            {
                Faction removedFaction = GetTileAtPosition(x, y).Faction;
                List<Tile> updatedTiles = map.OvertakeEnemyTiles(faction, removedFaction);
                List<string> tiles = new List<string>();
                foreach(Tile t in updatedTiles)
                {
                    tiles.Add(t.Coordinates.x + "," + t.Coordinates.y + "," + t.Faction.ToString() + "," + t.Population + "," + t.Coin.ToString());
                }
                _faction.Remove(removedFaction);
                GameManager.Instance.GameEndedForFaction(removedFaction);
                return String.Join(";", tiles);
            }
            //Can Attack? Just update the captital tile => Inefficient?
            if(map.CanAttackTile(x, y, faction))
            {
                map.AttackTile(x, y, attackPopulation);
                Tile t = GetTileAtPosition(x, y);
                return t.Coordinates.x + "," + t.Coordinates.y + "," + t.Faction.ToString() + "," + t.Population + "," + t.Coin.ToString();
            }
            return null;
        }

        public Tile GetTileAtPosition(int x, int y)
        {
            return map.tileMap[x][y];
        }

        public bool CanOverTakeTile(int x, int y, bool halfPopulation, Faction faction)
        {
            int attackingPopulation = GetAttackingForce(halfPopulation, faction);

            return map.CanOccupyTile(faction, attackingPopulation, x, y);
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

        private int GetAttackingForce(bool halfPopulation, Faction faction)
        {
            int attackingPopulation;
            if (halfPopulation)
            {
                attackingPopulation = (int)Math.Floor(0.0d + _freepopulation[faction] / 2);
            }
            else
            {
                attackingPopulation = _freepopulation[faction];
            }
            return attackingPopulation;
        }

    }
}
