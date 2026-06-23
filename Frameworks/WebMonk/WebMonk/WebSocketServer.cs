using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebMonk;

public abstract class WebSocketServer
{
    #region Embedded Types
    protected enum OpCodeEnum
    {
        ContinuationFrame = 0,
        TextFrame = 1,
        BinaryFrame = 2,
        ConnectionClose = 8,
        Ping = 9,
        Pong = 10
    }
    #endregion

    #region Constructors
    protected WebSocketServer(int port) : this(IPAddress.Any, port) { }
    protected WebSocketServer(IPAddress ip, int port)
    {
        IP = ip;
        Port = port;
        TcpListener = new TcpListener(IP, Port);
    }
    #endregion

    #region Methods
    public virtual void Start()
    {
        Task.Run(ListenerLoop);
    }
    public virtual void Stop()
    {
        _stop = true;
    }

    public virtual void WriteBytesToClient(TcpClient client, byte[] payload)
    {
        var buffer = PrepareBufferToWrite(payload);

        try
        {
            if (client.Connected) client.GetStream().Write(buffer, 0, buffer.Length);
            else CloseConnection(client);
        }
        catch (Exception)
        {
            CloseConnection(client);
        }
    }
    public virtual async Task WriteBytesToClientAsync(TcpClient client, byte[] payload)
    {
        var buffer = PrepareBufferToWrite(payload);

        try
        {
            if (client.Connected) await client.GetStream().WriteAsync(buffer, 0, buffer.Length);
            else CloseConnection(client);
        }
        catch (Exception)
        {
            CloseConnection(client);
        }
    }
    
    public virtual void WriteBytesToAllClients(byte[] payload)
    {
        var buffer = PrepareBufferToWrite(payload);

        var clients2Delete = new List<TcpClient>();
        foreach (var client in Clients.ToArray())
        {
            try
            {
                if (client.Connected) client.GetStream().Write(buffer, 0, buffer.Length);
                else clients2Delete.Add(client);
            }
            catch (Exception)
            {
                clients2Delete.Add(client);
            }
        }
        foreach (var client2Delete in clients2Delete) CloseConnection(client2Delete);
    }
    public virtual async Task WriteBytesToAllClientsAsync(byte[] payload)
    {
        var buffer = PrepareBufferToWrite(payload);

        var clients2Delete = new List<TcpClient>();
        foreach (var client in Clients.ToArray())
        {
            try
            {
                if (client.Connected) await client.GetStream().WriteAsync(buffer, 0, buffer.Length);
                else clients2Delete.Add(client);
            }
            catch (Exception)
            {
                clients2Delete.Add(client);
            }
        }
        foreach (var client2Delete in clients2Delete) CloseConnection(client2Delete);
    }

    public virtual void WriteStringToClient(TcpClient client, string message)
    {
        WriteBytesToClient(client, Encoding.UTF8.GetBytes(message));
    }
    public virtual Task WriteStringToClientAsync(TcpClient client, string message)
    {
        return WriteBytesToClientAsync(client, Encoding.UTF8.GetBytes(message));
    }

    public virtual void WriteStringToAllClients(string message)
    {
        WriteBytesToAllClients(Encoding.UTF8.GetBytes(message));
    }
    public virtual Task WriteStringToAllClientsAsync(string message)
    {
        return WriteBytesToAllClientsAsync(Encoding.UTF8.GetBytes(message));
    }

    protected abstract void ProcessTextRequest(string textRequest);
    protected abstract void ProcessBytesRequest(byte[] byteRequest);
    protected abstract bool ValidateNewConnection(TcpClient client, string requestData);
    protected abstract void ConnectionClosed(TcpClient client);

    protected virtual async Task ListenerLoop()
    {
        TcpListener.Start();
        while (!_stop)
        {
            var clientTask = TcpListener.AcceptTcpClientAsync();
            while (!clientTask.IsCompleted && !Clients.Any(x => x.Connected && x.GetStream().DataAvailable))
            {
                await Task.Delay(250);

                //Delete all disconnected clients
                var clients2Delete = new List<TcpClient>();
                foreach (var client in Clients.Where(x => !x.Connected).ToArray()) clients2Delete.Add(client);
                foreach (var client2Delete in clients2Delete) CloseConnection(client2Delete);
            }

            if (clientTask.IsCompleted)
            {
                var client = await clientTask;
                Clients.Add(client);
            }

            try
            {
                foreach (var clientsWithData in Clients.Where(x => x.Connected && x.GetStream().DataAvailable).ToArray())
                {
                    await ProcessTcpRequestAsync(clientsWithData); //we use await because we process TCP requests one at a time (we should not get too many)
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception) { } // we ignore errors if Clients list gets modified in the middle of a loop
        }
        // ReSharper disable once FunctionNeverReturns
        TcpListener.Stop();
    }
    #endregion

    #region Helper methods
    protected byte[] PrepareBufferToWrite(byte[] payload)
    {
        //Code derived from https://www.codeproject.com/Articles/1063910/WebSocket-Server-in-Csharp
        var opCode = OpCodeEnum.TextFrame;

        // best to write everything to a memory stream before we push it onto the wire
        // not really necessary, but I like it this way
        using (var memoryStream = new MemoryStream())
        {
            var finBitSetAsByte = (byte)0x80;
            var byte1 = (byte)(finBitSetAsByte | (byte)opCode);
            memoryStream.WriteByte(byte1);

            // depending on the size of the length we want to write it as a byte, ushort or ulong
            if (payload.Length < 126)
            {
                var byte2 = (byte)payload.Length;
                memoryStream.WriteByte(byte2);
            }
            else if (payload.Length <= ushort.MaxValue)
            {
                var byte2 = (byte)126;
                memoryStream.WriteByte(byte2);
                WriteUShort((ushort)payload.Length, memoryStream);
            }
            else
            {
                var byte2 = (byte)127;
                memoryStream.WriteByte(byte2);
                WriteULong((ulong)payload.Length, memoryStream);
            }

            memoryStream.Write(payload, 0, payload.Length);
            var buffer = memoryStream.ToArray();

            return buffer;
        }
    }
    
    protected virtual async Task ProcessTcpRequestAsync(TcpClient client)
    {
        //wait for 2 seconds and, if no data, ignore the request
        var iteration = 0;
        while (client.Available < 3)
        {
            await Task.Delay(100);
            iteration++;
            if (iteration > 20) return;
        }

        var request = new byte[client.Available];
        var stream = client.GetStream();
        // ReSharper disable once MustUseReturnValue
        stream.Read(request, 0, request.Length);
        var data = Encoding.UTF8.GetString(request);

        if (Regex.IsMatch(data, "^GET"))
        {
            var response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + EOL
                + "Connection: Upgrade" + EOL
                + "Upgrade: websocket" + EOL
                + "Sec-WebSocket-Accept: " + Convert.ToBase64String(System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(new Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"))) + EOL
                + EOL);

            try
            {
                if (ValidateNewConnection(client, data)) stream.Write(response, 0, response.Length);
                else CloseConnection(client);
            }
            catch (Exception)
            {
                CloseConnection(client);
            }
        }
        else
        {
            //process byte 0
            const byte finBitFlag = 0x80;
            const byte opCodeFlag = 0x0F;
            var isFinBitSet = (request[0] & finBitFlag) == finBitFlag;
            var opCode = (OpCodeEnum)(request[0] & opCodeFlag);

            //process byte 1
            const byte maskFlag = 0x80;
            var isMaskBitSet = (request[1] & maskFlag) == maskFlag;

            //we only process one piece text messages with masked but set
            if (!isFinBitSet || !isMaskBitSet)
            {
                CloseConnection(client);
                return;
            }

            var len = ReadLength(request, out var lenOffset);
            var maskKey = new[] { request[2 + lenOffset], request[3 + lenOffset], request[4 + lenOffset], request[5 + lenOffset] };

            var payload = new byte[len];
            for (var i = 0; i < len; i++) payload[i] = (byte)(request[i + 6 + lenOffset] ^ maskKey[i % 4]);

            //if we get a Pong, we just ignore it, per Web Socket spec
            if (opCode == OpCodeEnum.Pong) return;

            //handle a ping by sending back a pong with the same payload
            if (opCode == OpCodeEnum.Ping)
            {
                WriteBytesToClient(client, payload);
                return;
            }

            //handle text or byte request
            if (opCode != OpCodeEnum.TextFrame)
            {
                ProcessBytesRequest(payload);
            }
            else
            {
                var stringPayload = Encoding.UTF8.GetString(payload);
                ProcessTextRequest(stringPayload);
            }
        }
    }
    
    protected void CloseConnection(TcpClient client)
    {
        client.Close();
        Clients.Remove(client);
        ConnectionClosed(client);
    }

    protected static uint ReadLength(byte[] request, out int lenOffset)
    {
        var payloadLenFlag = 0x7F;
        var len = (uint)(request[1] & payloadLenFlag);
        lenOffset = 0;

        // read a short length or a long length depending on the value of len
        if (len == 126)
        {
            //len = ReadUShort(fromStream, false, smallBuffer, cancellationToken);
            len = ReadUShort(request);
            lenOffset = 2;
        }
        if (len == 127)
        {
            //len = (uint)ReadULong(fromStream, false, smallBuffer, cancellationToken);
            len = (uint)ReadULong(request);
            lenOffset = 8;

            const uint maxLen = 2147483648; // 2GB - not part of the spec but just a precaution. Send large volumes of data in smaller frames.
            if (len > maxLen) throw new ArgumentOutOfRangeException($"Payload length out of range. Max 2GB. Actual {len:#,##0} bytes.");
        }

        return len;
    }

    protected static void WriteULong(ulong value, Stream stream, bool isLittleEndian = false)
    {
        var buffer = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian && !isLittleEndian) Array.Reverse(buffer);
        stream.Write(buffer, 0, buffer.Length);
    }
    protected static void WriteUShort(ushort value, Stream stream, bool isLittleEndian = false)
    {
        var buffer = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian && !isLittleEndian) Array.Reverse(buffer);
        stream.Write(buffer, 0, buffer.Length);
    }

    protected static ushort ReadUShort(byte[] request, bool isLittleEndian = false)
    {
        byte[] buffer;
        if (isLittleEndian) buffer = [request[2], request[3]];
        else buffer = [request[3], request[2]];
        return BitConverter.ToUInt16(buffer, 0);
    }
    protected static ulong ReadULong(byte[] request, bool isLittleEndian = false)
    {
        byte[] buffer;
        if (isLittleEndian) buffer = [request[2], request[4], request[5], request[6], request[7], request[8], request[9], request[10]
        ];
        else buffer = [request[8], request[8], request[7], request[6], request[5], request[4], request[3], request[2]];
        return BitConverter.ToUInt16(buffer, 0);
    }
    #endregion

    #region Properties
    const string EOL = "\r\n"; // HTTP/1.1 defines the sequence CR LF as the end-of-line marker

    protected TcpListener TcpListener { get; }

    public IPAddress IP { get; }
    public int Port { get; }

    public List<TcpClient> Clients { get; set; } = [];

    private bool _stop;
    #endregion
}
