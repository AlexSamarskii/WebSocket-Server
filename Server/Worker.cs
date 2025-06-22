namespace WebSocketServer.Server
{
    public class Worker : BackgroundService
    {
        private readonly ConcurrentQueue<string> _messageQueue = new();
        private readonly ChatService _chatService;
        private readonly ILogger _logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var listener = new TcpListener(IPAddress.Any, 5000);
            listener.Start();

            while (!stoppingToken.IsCancellationRequested)
            {
                var client = await listener.AcceptTcpClientAsync();
                _ = HandleClientAsync(client, stoppingToken);
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            using var stream = client.GetStream();

            var messageBuffer = new MemoryStream();

            while (!token.IsCancellationRequested)
            {
                var lenghtBuffer = new byte[4];

                int read = await ReadExactAsync(stream, lenghtBuffer, 4, token);
                if (read == 0)
                {
                    break;
                }

                int readMessageCnt = BitConverter.ToInt32(lenghtBuffer.Reverse().ToArray(), 0);
                if (readMessageCnt <= 0)
                {
                    break;  
                }

                var massageBytes = new byte[readMessageCnt];
                read = await ReadExactAsync(stream, massageBytes, readMessageCnt, token);
                if (read == 0)
                {
                    break;
                }

                var massage = Encoding.UTF8.GetString(massageBytes);
                Console.WriteLine(massage);

                var response = Encoding.UTF8.GetBytes(massage);
                var responswLength = BitConverter.GetBytes(response.Length).Reverse().ToArray();

                await stream.WriteAsync(responswLength, 0, 4, token);
                await stream.WriteAsync(response, 0, response.Length, token);
            }
        }

        private async Task<int> ReadExactAsync(Stream stream, byte[] buffer, int length, CancellationToken token)
        {
            int total = 0;
            while (total < length)
            {
                int read = await stream.ReadAsync(buffer, total, length - total, token);
                if (read == 0)
                {
                    return 0;
                }
                total += read;
            }
            return total;
        }
    }
}
