using EmpireAttack2_ASP.Game.TileMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmpireAttack2_ASP.Game
{
    public class Game
    {
        MapBase map;

        private List<Faction> _faction;
        private Dictionary<Faction, int> _freepopulation;
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

            //setup free population of factions
            _freepopulation = new Dictionary<Faction, int>();
            foreach(Faction f in _faction)
            {
                _freepopulation.Add(f, 1);
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
                return true;
            }
            return false;
        }

        public Tile GetTileAtPosition(int x, int y)
        {
            return map.tileMap[x][y];
        }
    }
}
