using EmpireAttack2_ASP.Game.TileMap;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmpireAttack2_ASP.Game
{
    public class PlayerManager
    {
        private ConcurrentDictionary<string, string> _players = new ConcurrentDictionary<string, string>();
        private ConcurrentDictionary<string, Faction> _factions = new ConcurrentDictionary<string, Faction>();

        public PlayerManager()
        {

        }

        public void AddPlayer(string connectionId, string nickname, string faction)
        {
            _players.TryAdd(connectionId, nickname);
            Enum.TryParse(faction, out Faction pfaction);
            _factions.TryAdd(connectionId, pfaction);
        }

        public void RemovePlayer(string connectionId)
        {
            _players.TryRemove(connectionId, out var value);
            _factions.TryRemove(connectionId, out var value2);
        }

        public string GetConnectionId(string nickname)
        {
            return _players.FirstOrDefault(x => x.Value == nickname).Key;
        }

        public string GetNickname(string connectionId)
        {
            return _players.FirstOrDefault(x => x.Key == connectionId).Value;
        }

        public Faction GetFaction(string connectionId)
        {
            return _factions.FirstOrDefault(x => x.Key == connectionId).Value;
        }

        public Dictionary<string, Faction> GetPlayersInFaction(Faction faction)
        {
            return _factions.Where(x => x.Value == faction).ToDictionary(dict => dict.Key, dict => dict.Value);
        }

        public ConcurrentDictionary<string, string> GetPlayers()
        {
            return _players;
        }

    }
}
