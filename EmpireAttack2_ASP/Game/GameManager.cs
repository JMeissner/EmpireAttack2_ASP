using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using EmpireAttack2_ASP.Game.TileMap;
using EmpireAttack2_ASP.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace EmpireAttack2_ASP.Game
{
    public class GameManager
    {
        private static GameManager instance;
        private static readonly object padLock = new object();
        public Timer FastTick;
        public Timer SlowTick;
        public const int SlowTimerMultiplier = 2;

        public Gamestate gamestate;

        private Game game;

        private PlayerManager playerManager;

        public static GameManager Instance
        {
            get
            {
                lock (padLock)
                {
                    return instance ?? (instance = new GameManager());
                }
            }
        }

        public GameManager()
        {

        }

        public void Initilize()
        {
            FastTick = new Timer(FastUpdate, null, 0, 1000);
            SlowTick = new Timer(FastUpdate, null, 0, 1000 * SlowTimerMultiplier);
            gamestate = Gamestate.Lobby;
            game = new Game(2);
            playerManager = new PlayerManager();
        }

        private void FastUpdate(object state)
        {
            //Add free population to factions and send changes
            //GameHub.Current.Clients.All.SendAsync("ReceiveBeat");
        }

        private void SlowUpdate(object state)
        {
            //Do heavycomputing and send changed population on tiles
        }

        public List<Faction> GetFactions()
        {
            return game.GetAllFactions();
        }

        public string GetFactionsString()
        {
            return String.Join(':', game.GetAllFactions());
        }

        public void AddPlayer(string connectionId, string nickname, string faction)
        {
            playerManager.AddPlayer(connectionId, nickname, faction);
        }

        public void RemovePlayer(string connectionId)
        {
            playerManager.RemovePlayer(connectionId);
        }
        public string GetFactionFromPlayer(string connectionId)
        {
            return playerManager.GetFaction(connectionId).ToString();
        }

        public string GetSerializedMap()
        {
            return game.GetSerializedMap();
        }
    }
}
