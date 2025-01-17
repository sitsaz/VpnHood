﻿using System;
using System.Net;
using System.Threading.Tasks;
using VpnHood.Common.Messaging;
using VpnHood.Server;
using VpnHood.Server.Messaging;
using VpnHood.Server.Providers.HttpAccessServerProvider;

#nullable enable
namespace VpnHood.Test;

public class TestAccessServer : IAccessServer
{
    private readonly HttpAccessServer _httpAccessServer;

    public TestAccessServer(IAccessServer baseAccessServer)
    {
        BaseAccessServer = baseAccessServer;
        EmbedIoAccessServer = new TestEmbedIoAccessServer(baseAccessServer);
        _httpAccessServer = HttpAccessServer.Create(new HttpAccessServerOptions(EmbedIoAccessServer.BaseUri, "Bearer"));
    }

    public DateTime? LastConfigureTime { get; private set; }
    public ServerInfo? LastServerInfo { get; private set; }
    public ServerStatus? LastServerStatus { get; private set; }

    public TestEmbedIoAccessServer EmbedIoAccessServer { get; }
    public IAccessServer BaseAccessServer { get; }

    public bool IsMaintenanceMode => _httpAccessServer.IsMaintenanceMode;

    public async Task<ServerCommand> Server_UpdateStatus(ServerStatus serverStatus)
    {
        var ret = await  _httpAccessServer.Server_UpdateStatus(serverStatus);
        LastServerStatus = serverStatus;
        return ret;
    }

    public Task<ServerConfig> Server_Configure(ServerInfo serverInfo)
    {
        LastConfigureTime = DateTime.Now;
        LastServerInfo = serverInfo;
        LastServerStatus = serverInfo.Status;
        return _httpAccessServer.Server_Configure(serverInfo);
    }

    public Task<SessionResponseEx> Session_Get(uint sessionId, IPEndPoint hostEndPoint, IPAddress? clientIp)
    {
        return _httpAccessServer.Session_Get(sessionId, hostEndPoint, clientIp);
    }

    public Task<SessionResponseEx> Session_Create(SessionRequestEx sessionRequestEx)
    {
        return _httpAccessServer.Session_Create(sessionRequestEx);
    }

    public Task<ResponseBase> Session_AddUsage(uint sessionId, UsageInfo usageInfo)
    {
        return _httpAccessServer.Session_AddUsage(sessionId, usageInfo);
    }
    public Task<ResponseBase> Session_Close(uint sessionId, UsageInfo usageInfo)
    {
        return _httpAccessServer.Session_Close(sessionId, usageInfo);
    }

    public Task<byte[]> GetSslCertificateData(IPEndPoint hostEndPoint)
    {
        return _httpAccessServer.GetSslCertificateData(hostEndPoint);
    }

    public void Dispose()
    {
        _httpAccessServer.Dispose();
        EmbedIoAccessServer.Dispose();
        BaseAccessServer.Dispose();
        GC.SuppressFinalize(this);
    }
}