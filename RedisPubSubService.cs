namespace WebSocketServer
{
    class RedisConnection
    {
        private readonly string _id = string.Empty;
        private readonly string _connectionString = string.Empty;
        private ConnectionMultiplexer _multiplexer;
        private ISubscriber _subscriber;

        public RedisConnection(string id, string connectionString)
        {
            _id = id;
            _connectionString = connectionString;
            _multiplexer = ConnectionMultiplexer.Connect(_connectionString);
            _subscriber = _multiplexer.GetSubscriber();
        }

        public void Subscribe(string inChannel, Action<string?, string?> handler)
        {
            _subscriber.Subscribe(new RedisChannel(inChannel, RedisChannel.PatternMode.Literal), (channel, msg) =>
            {
                if (!msg.HasValue)
                {
                    return;
                }
                handler(channel, msg);
            });
            Console.WriteLine($"RedisId:{this._id} Subscribed to {inChannel}");
        }

        public void Unsubscribe(string channel)
        {
            _subscriber.Unsubscribe(new RedisChannel(channel, RedisChannel.PatternMode.Literal));
            Console.WriteLine($"RedisId:{this._id} Unsubscribed to {channel}");
        }

        public Task PublishAsync(string channel, string message)
        {
            return _subscriber.PublishAsync(new RedisChannel(channel, RedisChannel.PatternMode.Literal), message);
        }

        public void Dispose()
        {
            _multiplexer.Close();
            _multiplexer.Dispose();
        }
    }

    public class RedisPubSubService
    {
        private readonly List<RedisConnection> _connections = new();
        private readonly Dictionary<string, Action<string, string>> _channelActionDict = new Dictionary<string, Action<string, string>>();

        public RedisPubSubService() { }

        public void Connect(List<string> connectionStrList)
        {
            for (string i = 0; i < connectionStrList.Count; i++)
            {
                var connectionStr = connectionStrList[i];
                var redisConnection = new RedisConnection(i.ToString(), connectionStr);
                _connections.Add(redisConnection);
            }
        }
    }
}

