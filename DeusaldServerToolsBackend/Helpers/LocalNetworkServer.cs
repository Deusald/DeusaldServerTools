using System.Net;
using System.Net.Sockets;
using NLog;

namespace DeusaldServerToolsBackend;

public static class LocalNetworkServer
{
    public static async Task<IPAddress> FindLocalExternalIpAddress(Logger logger)
    {
        logger.Info("Looking for own external address");
        using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        await socket.ConnectAsync("8.8.8.8", 65530);
        IPEndPoint endPoint = (socket.LocalEndPoint as IPEndPoint)!;
        logger.Info($"Found my external address http://{endPoint.Address}:50000");
        return endPoint.Address;
    }
}