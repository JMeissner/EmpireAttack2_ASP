using EmpireAttack2_ASP.Game;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmpireAttack2_ASP.Hubs
{
    public class GameHub : Hub
    {
        public static IHubContext<GameHub> Current { get; set; }

        public async Task GetFactions()
        {
            await Clients.Caller.SendAsync("ReceiveFactions", GameManager.Instance.GetFactionsString());
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task Login(string username, string password, string faction)
        {
            bool answer = true;
            //Add to PlayerManager
            GameManager.Instance.AddPlayer(Context.ConnectionId, username, faction);
            await Groups.AddToGroupAsync(Context.ConnectionId, faction);
            await Clients.Caller.SendAsync("LoginAnswer", answer);
        }

        public async Task SendMap()
        {
            await Clients.Caller.SendAsync("DownloadMap", GameManager.Instance.GetSerializedMap());
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            GameManager.Instance.RemovePlayer(Context.ConnectionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GameManager.Instance.GetFactionFromPlayer(Context.ConnectionId));
            await base.OnDisconnectedAsync(exception);
        }
    }
}
