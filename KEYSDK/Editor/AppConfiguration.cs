using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class AppConfiguration : MonoBehaviour {

    [MenuItem("KeySDK/Configuration/Apply KEYSDK Settings", false, 1)]
    public static void ApplyProjectSettings()
    {
        EditorUserBuildSettings.wsaSubtarget = WSASubtarget.HoloLens;
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WSA, BuildTarget.WSAPlayer);
        EditorUserBuildSettings.wsaUWPBuildType = WSAUWPBuildType.D3D;

        PlayerSettings.WSA.SetCapability(PlayerSettings.WSACapability.InternetClientServer, true);
        PlayerSettings.WSA.SetCapability(PlayerSettings.WSACapability.SpatialPerception, true);
        PlayerSettings.WSA.SetCapability(PlayerSettings.WSACapability.PicturesLibrary, true);
        PlayerSettings.WSA.SetCapability(PlayerSettings.WSACapability.PrivateNetworkClientServer, true);
        PlayerSettings.WSA.SetCapability(PlayerSettings.WSACapability.Microphone, true);
        PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.WSA, ApiCompatibilityLevel.NET_4_6);
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.WSA, ScriptingImplementation.WinRTDotNET);
        PlayerSettings.scriptingRuntimeVersion = ScriptingRuntimeVersion.Latest;
        RestartEditor();
    }

    [MenuItem("KeySDK/Configuration/Reimport KeySDK", false, 3)]
    static void ImportFolder()
    {
        string reImportFile = "Assets/KeySDK/Plugins/PracticalManagedDLL.dll";
        if (File.Exists(reImportFile))
        {
            AssetDatabase.ImportAsset(reImportFile);
			AssetDatabase.Refresh();
        }
    }

    static void RestartEditor()
    {
        string projectPath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        EditorApplication.OpenProject(projectPath);
    }
}
