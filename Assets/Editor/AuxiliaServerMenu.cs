using UnityEditor;
using UnityEngine;

public static class AuxiliaServerMenu
{
    [MenuItem("Tools/Auxilia/Server/Local (localhost 18080)")]
    public static void Local() => Select(NetworkClientCore.LocalServerUrl);

    [MenuItem("Tools/Auxilia/Server/Production")]
    public static void Production() => Select(NetworkClientCore.ProductionServerUrl);

    private static void Select(string url)
    {
        EditorPrefs.SetString(NetworkClientCore.EditorServerUrlKey, url);
        Debug.Log($"Auxilia server: {url}. Restart Play Mode to apply. AUXILIA_SERVER_URL, if set, takes precedence.");
    }
}
