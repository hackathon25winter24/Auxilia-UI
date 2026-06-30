using System;
using UnityEngine;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;

public class NetworkClientCore : MonoBehaviour
{
    private const string ServerUrl = "https://auxilia.trap.show/";
    
    private GrpcChannel _channel;
    private GrpcChannel _streamChannel;

    public GrpcChannel Channel => _channel;
    public GrpcChannel StreamChannel => _streamChannel;

    // 通信エラーやサーバーからのメッセージを UI に渡すための共通イベント
    public event Action<string> OnErrorMessage;

    public void Initialize()
    {
        // 通常リクエスト用チャンネルの初期化
        var handler = new GrpcWebHandler(new System.Net.Http.HttpClientHandler());
        _channel = GrpcChannel.ForAddress(ServerUrl, new GrpcChannelOptions
        {
            HttpHandler = handler
        });

        // ストリーム専用の独立チャンネルを作成
        var streamHandler = new GrpcWebHandler(new System.Net.Http.HttpClientHandler());
        _streamChannel = GrpcChannel.ForAddress(ServerUrl, new GrpcChannelOptions
        {
            HttpHandler = streamHandler
        });

        Debug.Log("[NetworkClientCore] Channels initialized successfully.");
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