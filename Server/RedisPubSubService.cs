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
            for (int i = 0; i < connectionStrList.Count; i++)
            {
                var connectionStr = connectionStrList[i];
                var redisConnection = new RedisConnection(i.ToString(), connectionStr);
                _connections.Add(redisConnection);
            }
        }

        public bool Subscribe(string channel, Action<string?, string?> handler)
        {
            var shardId = 0;
            if (shardId >= _connections.Count)
            { 
                return false;
            }

            if (_channelActionDict.ContainsKey(channel))
            {
                return false;
            }

            var connection = _connections[shardId];
            connection.Subscribe(channel, handler);
            _channelActionDict.Add(channel, handler);
            return true;
        }

        public bool Unsubscribe(string channel)
        {
            var shardId = 0;
            if (shardId >= _connections.Count)
            {
                return false;
            }

            if (!_channelActionDict.ContainsKey(channel))
            {
                return false;
            }

            var connection = _connections[shardId];
            connection.Unsubscribe(channel);
            _channelActionDict.Remove(channel);
            return true;
        }

        public async Task<bool> PublishAsync(string channel, string message)
        {
            var shardId = 0; 
            if (shardId >= _connections.Count)
            {
                return false;
            }

            var connection = _connections[shardId];
            await connection.PublishAsync(channel, message);
            return true;
        }

    }

}

