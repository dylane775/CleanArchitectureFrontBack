using Microsoft.AspNetCore.SignalR;

namespace Notification.API.Hubs
{
    public class NotificationHub : Hub
    {
        private static readonly Dictionary<string, HashSet<string>> _userConnections = new();
        private static readonly object _lock = new();

        public override async Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();

            if (!string.IsNullOrEmpty(userId))
            {
                lock (_lock)
                {
                    if (!_userConnections.ContainsKey(userId))
                    {
                        _userConnections[userId] = new HashSet<string>();
                    }
                    _userConnections[userId].Add(Context.ConnectionId);
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();

            if (!string.IsNullOrEmpty(userId))
            {
                lock (_lock)
                {
                    if (_userConnections.ContainsKey(userId))
                    {
                        _userConnections[userId].Remove(Context.ConnectionId);
                        if (_userConnections[userId].Count == 0)
                        {
                            _userConnections.Remove(userId);
                        }
                    }
                }

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        public async Task LeaveUserGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        public static bool IsUserConnected(string userId)
        {
            lock (_lock)
            {
                return _userConnections.ContainsKey(userId) && _userConnections[userId].Count > 0;
            }
        }
    }
}
