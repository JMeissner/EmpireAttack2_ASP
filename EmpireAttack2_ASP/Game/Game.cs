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
        }

        public List<Faction> GetAllFactions()
        {
            return _faction;
        }

        public string GetSerializedMap()
        {
            return map.GetSerializedMap();
        }
    }
}
