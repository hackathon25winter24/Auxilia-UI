using System;
using UnityEditor;
using UnityEngine;

public static class AuxiliaServerValidation
{
    [MenuItem("Tools/Auxilia/Validate server configuration")]
    public static void Run()
    {
        string originalEnvironment = Environment.GetEnvironmentVariable("AUXILIA_SERVER_URL");
        bool hadPreference = EditorPrefs.HasKey(NetworkClientCore.EditorServerUrlKey);
        string originalPreference = EditorPrefs.GetString(NetworkClientCore.EditorServerUrlKey);
        try
        {
            Environment.SetEnvironmentVariable("AUXILIA_SERVER_URL", null);
            EditorPrefs.DeleteKey(NetworkClientCore.EditorServerUrlKey);
            Require(NetworkClientCore.ResolveServerUrl() == NetworkClientCore.LocalServerUrl);
            EditorPrefs.SetString(NetworkClientCore.EditorServerUrlKey, NetworkClientCore.ProductionServerUrl);
            Require(NetworkClientCore.ResolveServerUrl() == NetworkClientCore.ProductionServerUrl);
            Environment.SetEnvironmentVariable("AUXILIA_SERVER_URL", "http://localhost:18082");
            Require(NetworkClientCore.ResolveServerUrl() == "http://localhost:18082/");
            foreach (string invalid in new[] { "not-a-url", "file:///tmp/test", "https://user:pass@example.com", "https://example.com/?token=test" })
            {
                Environment.SetEnvironmentVariable("AUXILIA_SERVER_URL", invalid);
                bool rejected = false;
                try { NetworkClientCore.ResolveServerUrl(); } catch (InvalidOperationException) { rejected = true; }
                Require(rejected);
            }
            Debug.Log("AUXILIA_SERVER_CONFIG_VALIDATION_PASSED");
        }
        finally
        {
            Environment.SetEnvironmentVariable("AUXILIA_SERVER_URL", originalEnvironment);
            if (hadPreference) EditorPrefs.SetString(NetworkClientCore.EditorServerUrlKey, originalPreference);
            else EditorPrefs.DeleteKey(NetworkClientCore.EditorServerUrlKey);
        }
    }
    private static void Require(bool value) { if (!value) throw new Exception("Server URL validation failed"); }
}
