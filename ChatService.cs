using Microsoft.Extensions.Logging;

namespace WebSocketServer
{
    public class ChatService
    {
        private readonly RedisPubSubService _redisPubSubService;
        private Action<string>? _onRecvMsg = null;
        private ConcurrentDictionary<string, HashSet<ulong>> _subscribeChannelUserDict = new ConcurrentDictionary<string, HashSet<ulong>>();

        public ChatService(RedisPubSubService redisPubSubService)
        {
            _redisPubSubService = redisPubSubService;
        }

        private void Log(LogLevel level, ulong userId, string msg)
        {
            Console.WriteLine($"[{level.ToString()}]{userId}: {msg}");
        }

        public void Init(Action<string> onRecvMsg)
        {
            _onRecvMsg = onRecvMsg;
        }

        public async Task ProcessMessageAsync(ulong userId, string originMessage)
        {
            Log(LogLevel.Debug, userId, $"ProcessMessageStart ({originMessage})");
            var splitArr = originMessage.Split(':');
            var command = splitArr[0];
            if (originMessage.Count() == command.Count())
            {
                return;
            }

            var msg = originMessage.Substring(command.Count() + 1);

            switch (command)
            {
                case "subscribe":
                    {
                        var chanel = splitArr[1];

                    }
            }
        }

        private void Subcribe(ulong userId, string channel)
        {
            _subscribeChannelUserDict.TryAdd(channel, new HashSet<ulong>());
            var userIdSet = _subscribeChannelUserDict[channel];

            if (userIdSet.Contains(userId))
            {
                Log(LogLevel.Warning, userId, $"ALREADY_SUBSCRIBE Channel({channel})");
                return;
            }
        } 
    }
}
