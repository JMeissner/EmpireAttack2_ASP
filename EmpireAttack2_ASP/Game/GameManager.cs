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
        public Timer GameTimer;
        public const int SlowTimerMultiplier = 2;
        public const int GameTimeInMin = 5;
        public int NoOfFactions;
        public string webRootPath;

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

        public void Initilize(int noOfFactions, string webContentPath)
        {
            //Setup
            gamestate = Gamestate.Lobby;
            webRootPath = webContentPath;
            game = new Game(noOfFactions);
            NoOfFactions = noOfFactions;
            playerManager = new PlayerManager();

            //Timers
            FastTick = new Timer(FastUpdate, null, Timeout.Infinite, Timeout.Infinite);
            SlowTick = new Timer(SlowUpdate, null, Timeout.Infinite, Timeout.Infinite);
            GameTimer = new Timer(GameTimerEnded, null, Timeout.Infinite, Timeout.Infinite);
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
            GameHub.Current.Clients.All.SendAsync("Cl_CompressedUpdate", GZIPCompress.Compress(game.UpdateTilePopulation()));
        }

        private void GameTimerEnded(object state)
        {
            //End Game because of Timer and restart a new one
            GameHub.Current.Clients.All.SendAsync("Cl_GameEnded", "Game Timer has run out. Better luck next time. Your Faction: ");
            EndGame();
        }

        private void StartGame()
        {
            //Start Timers
            FastTick.Change(0, 1000);
            SlowTick.Change(0, 1000 * SlowTimerMultiplier);
            GameTimer.Change(1000 * GameTimeInMin * 60, 1000 * GameTimeInMin * 60);
            //Set Gamestate
            gamestate = Gamestate.InGame;
        }

        private void CheckStartGame()
        {
            //TODO: CHANGE! TESTING ONLY
            if(playerManager.GetFactions().Values.Count == NoOfFactions)
            {
                StartGame();
            }
        }

        private void EndGame()
        {
            //Set Game to end and stop Timers
            gamestate = Gamestate.Ended;
            FastTick.Change(Timeout.Infinite, Timeout.Infinite);
            SlowTick.Change(Timeout.Infinite, Timeout.Infinite);

            //Start new Game
            Initilize(NoOfFactions, webRootPath);
        }

        //TODO: Check if capital and apply Overtake Enemy
        public async Task AttackTile(int x, int y, bool halfPopulation, string connectionID)
        {
            //Game is not running, should not accept input
            if (!gamestate.Equals(Gamestate.InGame))
            {
                return;
            }

            Faction playerFaction = playerManager.GetFaction(connectionID);

            if(game.GetTileAtPosition(x, y).GetShortType().Equals("C"))
            {
                //Capital Tile => Apply overtake Enemy modifier and end game if needed or disable Faction
                string updatedTiles = game.AttackCapital(x, y, halfPopulation, playerFaction);
                if (updatedTiles != null)
                {
                    await GameHub.Current.Clients.All.SendAsync("Cl_CompressedUpdate", GZIPCompress.Compress(updatedTiles));
                }
                //A Faction won, all others have been eliminated
                if(game.GetAllFactions().Count == 1)
                {
                    await GameHub.Current.Clients.Group(playerFaction.ToString()).SendAsync("Cl_GameEnded", "Your Faction won! You were part of: ");
                    EndGame();
                }
            }
            else if (game.AttackTile(x, y, halfPopulation, playerFaction) && game.GetTileAtPosition(x, y).Coin.Equals(Coin.None))
            {
                //Normal Attack
                Tile t = game.GetTileAtPosition(x, y);
                await GameHub.Current.Clients.All.SendAsync("Cl_TileUpdate", x, y, t.Faction.ToString(), t.Population, t.Coin.ToString());
                await GameHub.Current.Clients.Group(playerFaction.ToString()).SendAsync("Cl_FastTick", game.GetFreePopulationFromFaction(playerFaction));
            }
            else
            {
                //Tile has Coin on it -> Use Coin first
                string updatedTiles = game.AttackTileWithCoin(x, y, playerFaction);
                if(updatedTiles != null)
                {
                    await GameHub.Current.Clients.All.SendAsync("Cl_CompressedUpdate", GZIPCompress.Compress(updatedTiles));
                }
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

        public async Task GameEndedForFaction(Faction faction)
        {
            await GameHub.Current.Clients.Group(faction.ToString()).SendAsync("Cl_GameEnded", "Your Faction was eliminated. You were part of: ");
        }
    }
}
