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
            string mapPath = Path.Combine("Maps", "map2_serialized.txt");
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

        public void AddFreePopulationToForumla(int amount)
        {
            lock (_freepopulation)
            {
                foreach (Faction f in _freepopulation.Keys.ToList())
                {
                    _freepopulation[f] += (amount + (int)Math.Floor(0.1 * ((_freepopulation[f] / 60) ^ 2)));
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

        /// <summary>
        /// Returns a multiplier for attacking enemy tiles, based on neighboring tiles for that tile. The Multiplier is 2 ^ (noOfNeighbors - 1)
        /// </summary>
        /// <param name="x">X Coordinate of Tile to attack</param>
        /// <param name="y">y Coordinate of Tile to attack</param>
        /// <param name="faction">Faction of the attacking player</param>
        /// <returns></returns>
        public int GetAttackMultiplier(int x, int y, Faction faction)
        {
            Tile[] neighbors = map.GetNeighborTiles(x, y);
            int pow = 0;
            foreach(Tile n in neighbors)
            {
                if (n.Faction.Equals(faction))
                {
                    pow++;
                }
            }
            return (int)Math.Pow((double)2, (double)(pow - 1));
        }

        public Tile[] AttackTile(int x, int y, bool halfPopulation, Faction faction)
        {
            //Clicked on tile that is not a neighbor to a tile we own
            if (!map.IsNeighbor(faction, x, y))
            {
                return null;
            }

            //Prepare Attackforce and multiplier
            int attackingPopulation = GetAttackingForce(halfPopulation, faction);
            int neighborMultiplier = GetAttackMultiplier(x, y, faction);

            //Tile we try to Attack is a Capital
            if(GetTileAtPosition(x, y).Type.Equals(TileType.Capital))
            {
                //Can overtake? Capture all enemy Tiles and remove faction from availables
                if (map.CanOccupyTile(faction, attackingPopulation * neighborMultiplier, x, y))
                {
                    Faction removedFaction = GetTileAtPosition(x, y).Faction;
                    List<Tile> updatedTiles = map.OvertakeEnemyTiles(faction, removedFaction);
                    _faction.Remove(removedFaction);
                    _freepopulation[faction] = _freepopulation[faction] - attackingPopulation;
                    GameManager.Instance.GameEndedForFaction(removedFaction);
                    return updatedTiles.ToArray();
                }
                //Can Attack? Just update the captital tile
                if (map.CanAttackTile(x, y, faction))
                {
                    map.AttackTile(x, y, attackingPopulation * neighborMultiplier);
                    _freepopulation[faction] = _freepopulation[faction] - attackingPopulation;
                    Tile t = GetTileAtPosition(x, y);
                    return new Tile[]{ t };
                }
                //Do we own the tile? Add Troops to the tile
                if (map.tileMap[x][y].Faction.Equals(faction))
                {
                    map.AddPopulation(x, y, attackingPopulation * neighborMultiplier);
                    _freepopulation[faction] = _freepopulation[faction] - attackingPopulation;
                    Tile t = GetTileAtPosition(x, y);
                    return new Tile[] { t };
                }
            }

            //Tile has a Coin on it
            if(!GetTileAtPosition(x, y).Coin.Equals(Coin.None))
            {
                //Not enough Troops to capture tile
                if(!map.CanOccupyTile(faction, attackingPopulation  * neighborMultiplier, x, y))
                {
                    map.AttackTile(x, y, attackingPopulation * neighborMultiplier);
                    _freepopulation[faction] = _freepopulation[faction] - attackingPopulation;
                    Tile t = GetTileAtPosition(x, y);
                    return new Tile[] { t };
                }

                //Overtake Coin tile and set population on the surrounding tiles
                int populationToSet = (attackingPopulation * neighborMultiplier) - GetTileAtPosition(x, y).Population;

                List<Tile> updatedTiles = new List<Tile>();
                Queue<Tile[]> tileToProcess = new Queue<Tile[]>();
                tileToProcess.Enqueue(map.GetTilesFromCoin(x, y));
                map.SetCoinOnTile(x, y, Coin.None);

                while (tileToProcess.Any())
                {
                    foreach (Tile t in tileToProcess.Dequeue())
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
                        map.SetPopulationOfTile(t.Coordinates.x, t.Coordinates.y, populationToSet);
                        updatedTiles.Add(t);
                    }
                }
                _freepopulation[faction] = _freepopulation[faction] - attackingPopulation;
                return updatedTiles.ToArray();
            }

            //Normal Tile
            if (map.CanOccupyTile(faction, attackingPopulation * neighborMultiplier, x, y))
            {
                map.OccupyTile(faction, attackingPopulation * neighborMultiplier, x, y);
                _freepopulation[faction] = _freepopulation[faction] - attackingPopulation;
                return new Tile[] { GetTileAtPosition(x, y) };
            }
            else if (map.CanAttackTile(x, y, faction))
            {
                map.AttackTile(x, y, attackingPopulation * neighborMultiplier);
                _freepopulation[faction] = _freepopulation[faction] - attackingPopulation;
                return new Tile[] { GetTileAtPosition(x, y) };
            }
            else if (map.tileMap[x][y].Faction.Equals(faction))
            {
                map.AddPopulation(x, y, attackingPopulation * neighborMultiplier);
                _freepopulation[faction] = _freepopulation[faction] - attackingPopulation;
                return new Tile[] { GetTileAtPosition(x, y) };
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
