using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using EmpireAttack2_ASP.Game.TileMap;
using EmpireAttack2_ASP.Hubs;
using Microsoft.AspNetCore.SignalR;
using EmpireAttack2_ASP.Utils;

namespace EmpireAttack2_ASP.Game
{
    public class GameManager
    {
        private static GameManager instance;
        private static readonly object padLock = new object();
        public Timer FastTick;
        public Timer SlowTick;
        public const int SlowTimerMultiplier = 2;
        public int NoOfPlayers;

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

        public void Initilize(int noOfPlayers)
        {
            //Setup
            gamestate = Gamestate.Lobby;
            game = new Game(noOfPlayers);
            NoOfPlayers = noOfPlayers;
            playerManager = new PlayerManager();

            //Timers
            FastTick = new Timer(FastUpdate, null, Timeout.Infinite, Timeout.Infinite);
            SlowTick = new Timer(SlowUpdate, null, Timeout.Infinite, Timeout.Infinite);
        }

        private void FastUpdate(object state)
        {
            //Add free population to factions and send changes
            //GameHub.Current.Clients.All.SendAsync("ReceiveBeat");
            game.AddFreePopulationToAll(1);
            foreach(Faction f in game.GetAllFactions())
            {
                GameHub.Current.Clients.Group(f.ToString()).SendAsync("Cl_FastTick", game.GetFreePopulationFromFaction(f));
            }
        }

        private void SlowUpdate(object state)
        {
            //Do heavycomputing and send changed population on tiles
            GameHub.Current.Clients.All.SendAsync("Cl_MapCompressedUpdate", GZIPCompress.Compress(game.UpdateTilePopulation()));
        }

        private void StartGame()
        {
            //Start Timers
            FastTick.Change(0, 1000);
            SlowTick.Change(0, 1000 * SlowTimerMultiplier);
            //Set Gamestate
            gamestate = Gamestate.InGame;
        }

        private void CheckStartGame()
        {
            //TODO: CHANGE! TESTING ONLY
            if(playerManager.GetPlayers().Keys.Count == NoOfPlayers - 1)
            {
                StartGame();
            }
        }

        private void EndGame()
        {
            FastTick.Change(Timeout.Infinite, Timeout.Infinite);
            SlowTick.Change(Timeout.Infinite, Timeout.Infinite);
        }

        //TODO: Check if capital and apply Overtake Enemy
        public async Task AttackTile(int x, int y, string connectionID)
        {
            Faction playerFaction = playerManager.GetFaction(connectionID);
            if (game.AttackTile(x, y, playerFaction)){
                Tile t = game.GetTileAtPosition(x, y);
                await GameHub.Current.Clients.All.SendAsync("Cl_TileUpdate", x, y, t.Faction.ToString(), t.Population);
                await GameHub.Current.Clients.Group(playerFaction.ToString()).SendAsync("Cl_FastTick", game.GetFreePopulationFromFaction(playerFaction));
            }
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
            CheckStartGame();
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
