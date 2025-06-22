using Microsoft.AspNetCore.Server.HttpSys;
using System.Net.Sockets;
using System.Text;

using var client = new TcpClient();

await client.ConnectAsync("127.0.0.1", 5000);

using var stream = client.GetStream();  

while (true)
{
    var input = Console.ReadLine();
    if(string.IsNullOrEmpty(input)) 
        { break; }

    var payload = Encoding.UTF8.GetBytes(input);
    var length = BitConverter.GetBytes(payload.Length).Reverse().ToArray();

    await stream.WriteAsync(length);
    await stream.WriteAsync(payload);

    var lengthBuffer = new byte[4];
    await ReadExactAsync(stream, lengthBuffer, 4);
    int responseLen = BitConverter.ToInt32(lengthBuffer.Reverse().ToArray(), 0);

    var responseBuffer = new byte[responseLen];
    await ReadExactAsync(stream, responseBuffer, responseLen);
    var response = Encoding.UTF8.GetString(responseBuffer);

    Console.WriteLine(response);
}

static async Task ReadExactAsync(NetworkStream stream, byte[] buffer, int size)
{
    int total = 0;
    while (total < size)
    {
        int read = await stream.ReadAsync(buffer, total, size - total);
        if (read == 0) throw new Exception("No data readed");
        total += read;
    }
}