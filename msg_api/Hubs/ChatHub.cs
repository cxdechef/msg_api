using Microsoft.AspNetCore.SignalR;
using msg_api.Models;

namespace msg_api.Hubs
{


    public class ChatHub : Hub
    {
        private readonly IDictionary<string, UserRoomConnection> _coneection;
        public ChatHub(IDictionary<string, UserRoomConnection> coneection)
        {
            _coneection = coneection;
        }
        public async Task JoinRoom(UserRoomConnection userConnection)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.Room!);
            _coneection[Context.ConnectionId] = userConnection;
            await Clients.Group(userConnection.Room!)
            .SendAsync("ReceiveMessage", "MSG Bot", $"{userConnection.User} has joined the group", DateTime.Now);
            await SendConnectedUser(userConnection.Room!);
        }



        public async Task SendMessage(string message)
        {
            if (_coneection.TryGetValue(Context.ConnectionId, out UserRoomConnection userRoomConnection))
            {
                await Clients.Group(userRoomConnection.Room!)
                .SendAsync("ReceiveMessage", userRoomConnection.User, message);
            }
        }


        public override Task OnDisconnectedAsync(Exception? exp)
        {
            if (!_coneection.TryGetValue(Context.ConnectionId, out UserRoomConnection roomConnection))
            {
                return base.OnDisconnectedAsync(exp);
            }
            _coneection.Remove(Context.ConnectionId);
            Clients.Group(roomConnection.Room!)
            .SendAsync("ReceiveMessage", "MSG Bot", $"{roomConnection.User} has left the group", DateTime.Now);
            SendConnectedUser(roomConnection.Room!);
            return base.OnDisconnectedAsync(exp);
        }


        public Task SendConnectedUser(string room)
        {
            var users = _coneection.Values.Where(u => u.Room == room).Select(s => s.User);
            return Clients.Group(room).SendAsync("ConnectedUser", users);
        }
    }
}