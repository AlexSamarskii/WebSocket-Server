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
            _redisPubSubService.Connect(new List<string> { "127.0.0.1:6379", "127.0.0.1:6379" });
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
                        Subcribe(userId, chanel);
                    }
                    break;
                case "unsubscribe":
                    {
                        var chanel = splitArr[1];
                        Unsubcribe(userId, chanel);
                    }
                    break;
                case "publish":
                    {
                        var chanel = splitArr[1];
                        var sendingMsg = msg.Substring(chanel.Count() + 1);
                        await PublishMsgAsync(userId, chanel, sendingMsg);
                    }
                    break;
                default:
                    break;
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

        private void Unsubcribe(ulong userId, string channel)
        {
            if (!_subscribeChannelUserDict.TryGetValue(channel, out var userIdSet))
            {
                Log(LogLevel.Warning, userId, $"NOT_UNSUBSCRIBE Channel({channel})");
                return;
            }

            if (!userIdSet.Contains(userId))
            {
                Log(LogLevel.Warning, userId, $"NOT_UNSUBSCRIBE Channel({channel})");
                return;
            }

            userIdSet.Remove(userId);
            Log(LogLevel.Information, userId, $"UnsubscribeToRedis Channel({channel})");

            if (userIdSet.Count == 0)
            {
                _redisPubSubService.Unsubscribe(channel);
            }
        }

        private async Task PublishMsgAsync(ulong userId, string channel, string msg)
        {
            var sendMsg = $"FROM({channel}) UserId({userId}) : {msg}";
            await _redisPubSubService.PublishAsync(channel, sendMsg);
            Log(LogLevel.Information, userId, $"PublishToRedis Channel({channel})");
        }
    }
}
