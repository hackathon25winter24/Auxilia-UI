using System;
using UnityEngine;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Grpc.Core;

public class NetworkClientCore : MonoBehaviour
{
    public const string ProductionServerUrl = "https://auxilia.trap.show/";
    public const string LocalServerUrl = "http://localhost:18080/";
    public const string EditorServerUrlKey = "Auxilia.ServerUrl";
    public string ServerUrl { get; private set; }

    public static string ResolveServerUrl()
    {
        // Environment override is useful for standalone clients and custom local ports.
        string value = Environment.GetEnvironmentVariable("AUXILIA_SERVER_URL");
        if (string.IsNullOrWhiteSpace(value))
        {
#if UNITY_EDITOR
            value = UnityEditor.EditorPrefs.GetString(EditorServerUrlKey, LocalServerUrl);
#else
            value = ProductionServerUrl;
#endif
        }
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
            !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
            throw new InvalidOperationException("AUXILIA_SERVER_URL must be an absolute HTTP(S) URL without credentials, query or fragment.");
        return uri.AbsoluteUri;
    }
    
    private GrpcChannel _channel;
    private GrpcChannel _streamChannel;

    public GrpcChannel Channel => _channel;
    public GrpcChannel StreamChannel => _streamChannel;
    private string sessionToken;
    public string PlayerId { get; private set; }
    public Metadata SessionHeaders => string.IsNullOrEmpty(sessionToken)
        ? new Metadata() : new Metadata { { "authorization", "Bearer " + sessionToken } };
    public void SetSession(string token, string playerId)
    {
        sessionToken = token;
        PlayerId = playerId;
    }

    // 通信エラーやサーバーからのメッセージを UI に渡すための共通イベント
    public event Action<string> OnErrorMessage;

    public void Initialize()
    {
        ServerUrl = ResolveServerUrl();
        //  通常リクエスト用チャンネルの初期化
        var handler = new GrpcWebHandler(new System.Net.Http.HttpClientHandler());
        _channel = GrpcChannel.ForAddress(ServerUrl, new GrpcChannelOptions
        {
            HttpHandler = handler
        });

        // ストリーム専用の独立チャンネルを作成
        // GrpcWebMode.GrpcWebText または GrpcWeb を明示し、ストリーミングを有効化
        var streamHandler = new GrpcWebHandler(GrpcWebMode.GrpcWebText, new System.Net.Http.HttpClientHandler());
        
        _streamChannel = GrpcChannel.ForAddress(ServerUrl, new GrpcChannelOptions
        {
            HttpHandler = streamHandler
        });

        Debug.Log($"[NetworkClientCore] Connecting to {ServerUrl} (gRPC-Web).");
    }

    public void ShowErrorMessage(string message)
    {
        Debug.LogError($"[Network Error] {message}");
        OnErrorMessage?.Invoke(message);
    }

    private void OnDestroy()
    {
        _channel?.Dispose();
        _streamChannel?.Dispose();
    }
}
