using UnityEngine;
using System;

public class NetworkManager : MonoBehaviour
{
    [Header("Dependencies")]
    public BattleOnlineManager battleOnlineManager;
    public UserData userData;

    private NetworkClientCore _core;
    private AuthenticationConnector _auth;
    private MatchingConnector _matching;
    private BattleConnector _battle;

    // 各通信機能への安全なプロパティアクセス（外部ゲームロジックから呼び出す窓口）
    public AuthenticationConnector Auth => _auth;
    public MatchingConnector Matching => _matching;
    public BattleConnector Battle => _battle;
    public NetworkClientCore Core => _core;

    public static NetworkManager Instance { get; private set; }

    // 下位互換用：既存のUIスクリプトなどがエラーイベントをリッスンできるようにイベントをブリッジ
    public event Action<string> OnErrorMessage
    {
        add { if (_core != null) _core.OnErrorMessage += value; }
        remove { if (_core != null) _core.OnErrorMessage -= value; }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // コンポーネントを動的アタッチ、またはインスペクター上のアタッチから取得して初期化
        _core = GetComponent<NetworkClientCore>();
        if (_core == null) _core = gameObject.AddComponent<NetworkClientCore>();
        
        _auth = GetComponent<AuthenticationConnector>();
        if (_auth == null) _auth = gameObject.AddComponent<AuthenticationConnector>();

        _matching = GetComponent<MatchingConnector>();
        if (_matching == null) _matching = gameObject.AddComponent<MatchingConnector>();

        _battle = GetComponent<BattleConnector>();
        if (_battle == null) _battle = gameObject.AddComponent<BattleConnector>();

        // 通信の初期化
        _core.Initialize();
        _auth.Initialize(_core, userData);
        _matching.Initialize(_core);
        _battle.Initialize(_core, battleOnlineManager);
        //_battle.Initialize(_core);

        Debug.Log("[NetworkManager] All split connectors initialized and grouped under NetworkManager.");
    }
}

// 例
//await NetworkManager.Instance.Battle.SendAttack(roomId, playerId, attackerId, type, infos, ct);
//await NetworkManager.Instance.Matching.JoinRoom(roomId, userId);
//await NetworkManager.Instance.Auth.Login(user, pass);