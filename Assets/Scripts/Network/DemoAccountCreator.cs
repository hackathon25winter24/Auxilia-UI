using UnityEngine;
using UnityEngine.UI;
using Game.Network; // あなたのNamespace
using System.Threading.Tasks;

public class DemoAccountCreator : MonoBehaviour
{
    // 先ほど作った GameConnector をインスペクターからドラッグ＆ドロップします
    public GameConnector connector;

    // 実行結果を表示するためのUI（あれば便利）
    public Text resultText;

    // --- これをボタンの「OnClick」に設定します ---
    public async void OnClick_CreateDemoUser()
    {
        Debug.Log("デモユーザー作成リクエスト開始...");

        // 1. 送信するデモデータを用意
        string demoHash = "DemoUser_" + Random.Range(1000, 9999);
        int demoStory = 1;
        int demoRate = 1200;

        // 2. GameConnectorを通じてサーバーに送信
        // （GameConnectorに SignUp メソッドがある前提です）
        UserResponse response = await connector.SignUp(demoHash, demoStory, demoRate);

        if (response != null)
        {
            // 3. 成功したらIDを表示
            string msg = $"作成成功！\nID: {response.Id}\nHash: {response.Hash}";
            Debug.Log("<color=yellow>" + msg + "</color>");

            if (resultText != null) resultText.text = msg;

            // 重要：後で GetUser で使えるようにIDをログからメモするか、
            // PlayerPrefs（自動保存）を確認してください
        }
        else
        {
            Debug.LogError("作成に失敗しました。サーバーとの接続を確認してください。");
        }
    }
}