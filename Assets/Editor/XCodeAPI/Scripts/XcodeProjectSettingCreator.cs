using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.Linq;
using System;

public class XcodeProjectSettingCreator
{
    [MenuItem("Assets/Create/XcodeProjectSetting")]
    public static void CreateAsset()
    {
        AssetDatabase.GenerateUniqueAssetPath(GetCurrentFilePath());
        XcodeProjectSetting setting = XcodeProjectSetting.Instance;
        setting.SaveConfig();
    }

    public static string GetCurrentFilePath()
    {
        return "Assets/Editor/XCodeAPI/Setting/XcodeProjectSetting.asset";
    }
}