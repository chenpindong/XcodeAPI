using System.Collections.Generic;
using ChillyRoom.UnityEditor.iOS.Xcode;
using System.IO;
using UnityEditor;

//Info.Plist修改设置
public static class InfoPlistProcessor
{
    private static string GetInfoPlistPath(string buildPath)
    {
        return Path.Combine(buildPath, XcodeProjectSetting.INFO_PLIST_NAME);
    }

    private static PlistDocument GetInfoPlist(string buildPath)
    {
        string plistPath = GetInfoPlistPath(buildPath);
        PlistDocument plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        return plist;
    }

    /// <summary>
    /// 设置私有数据
    /// </summary>
    /// <param name="plist"></param>
    /// <param name="privacySensiticeDataList"></param>
    private static void SetPrivacySensiticeData(PlistDocument plist, List<XcodeProjectSetting.PrivacySensiticeData> privacySensiticeDataList)
    {
        PlistElementDict rootDict = plist.root;
        int count = privacySensiticeDataList.Count;
        for (int i = 0; i < count; i++)
        {
            XcodeProjectSetting.PrivacySensiticeData data = privacySensiticeDataList[i];
            switch (data.type)
            {
                case XcodeProjectSetting.NValueType.String:
                    rootDict.SetString(data.key, data.value);
                    break;
                case XcodeProjectSetting.NValueType.Int:
                    rootDict.SetInteger(data.key, int.Parse(data.value));
                    break;
                case XcodeProjectSetting.NValueType.Bool:
                    rootDict.SetBoolean(data.key, bool.Parse(data.value));
                    break;
                default:
                    rootDict.SetString(data.key, data.value);
                    break;
            }
        }
    }

    /// <summary>
    /// 设置白名单
    /// </summary>
    /// <param name="plist"></param>
    /// <param name="_applicationQueriesSchemes"></param>
    private static void SetApplicationQueriesSchemes(PlistDocument plist, List<string> _applicationQueriesSchemes)
    {
        PlistElementArray queriesSchemes;
        int count = _applicationQueriesSchemes.Count;
        string queriesScheme = null;

        if (plist.root.values.ContainsKey(XcodeProjectSetting.APPLICATION_QUERIES_SCHEMES_KEY))
            queriesSchemes = plist.root[XcodeProjectSetting.APPLICATION_QUERIES_SCHEMES_KEY].AsArray();
        else
            queriesSchemes = plist.root.CreateArray(XcodeProjectSetting.APPLICATION_QUERIES_SCHEMES_KEY);

        for (int i = 0; i < count; i++)
        {
            queriesScheme = _applicationQueriesSchemes[i];
            if (!queriesSchemes.values.Contains(new PlistElementString(queriesScheme)))
                queriesSchemes.AddString(queriesScheme);
        }
    }

    /// <summary>
    /// 设置后台模式
    /// </summary>
    /// <param name="plist"></param>
    /// <param name="modes"></param>
    private static void SetBackgroundModes(PlistDocument plist, List<string> modes)
    {
        int count = modes.Count;
        if (count > 0)
        {
            PlistElementDict rootDict = plist.root;
            PlistElementArray bgModes = rootDict.CreateArray("UIBackgroundModes");
            for (int i = 0; i < count; i++)
            {
                bgModes.AddString(modes[i]);
            }
        }
    }

    /// <summary>
    /// 设置url schemes地址
    /// </summary>
    /// <param name="plist"></param>
    /// <param name="urlList"></param>
    private static void SetURLSchemes(PlistDocument plist, List<XcodeProjectSetting.BundleUrlType> urlList)
    {
        PlistElementArray urlTypes;
        PlistElementDict itmeDict;
        if (plist.root.values.ContainsKey(XcodeProjectSetting.URL_TYPES_KEY))
            urlTypes = plist.root[XcodeProjectSetting.URL_TYPES_KEY].AsArray();
        else
            urlTypes = plist.root.CreateArray(XcodeProjectSetting.URL_TYPES_KEY);

        for (int i = 0; i < urlList.Count; i++)
        {
            itmeDict = urlTypes.AddDict();
            itmeDict.SetString(XcodeProjectSetting.URL_TYPE_ROLE_KEY, "Editor");
            itmeDict.SetString(XcodeProjectSetting.URL_IDENTIFIER_KEY, urlList[i].identifier);
            PlistElementArray schemesArray = itmeDict.CreateArray(XcodeProjectSetting.URL_SCHEMES_KEY);
            if (itmeDict.values.ContainsKey(XcodeProjectSetting.URL_SCHEMES_KEY))
                schemesArray = itmeDict[XcodeProjectSetting.URL_SCHEMES_KEY].AsArray();
            else
                schemesArray = itmeDict.CreateArray(XcodeProjectSetting.URL_SCHEMES_KEY);
            //TODO:按理说要排除已经存在的，但由于我们是新生成，所以不做排除
            for (int j = 0; j < urlList[i].bundleSchmes.Count; j++)
            {
                schemesArray.AddString(urlList[i].bundleSchmes[j]);
            }
        }
    }

    /// <summary>
    /// 设置GameCenter
    /// </summary>
    /// <param name="plist"></param>
    private static void SetGameCenter(PlistDocument plist)
    {
        PlistElementArray gameCenter;
        if (plist.root.values.ContainsKey(GameCenterInfo.Key))
            gameCenter = plist.root[GameCenterInfo.Key].AsArray();
        else
            gameCenter = plist.root.CreateArray(GameCenterInfo.Key);

        foreach (var item in GameCenterInfo.Value)
        {
            gameCenter.AddString(item);
        }
    }

    /// <summary>
    /// 设置BundleName
    /// </summary>
    /// <param name="plist"></param>
    private static void SetBundleName(PlistDocument plist)
    {
        PlistElementDict rootDict = plist.root;
        rootDict.SetString(BundleNameInfo.Key, PlayerSettings.productName);
    }

    /// <summary>
    /// 设置ATS（是否允许http）
    /// </summary>
    /// <param name="plist"></param>
    /// <param name="enableATS"></param>
    private static void SetATS(PlistDocument plist, bool enableATS)
    {
        PlistElementDict atsDict;
        if (plist.root.values.ContainsKey(XcodeProjectSetting.ATS_KEY))
            atsDict = plist.root[XcodeProjectSetting.ATS_KEY].AsDict();
        else
            atsDict = plist.root.CreateDict(XcodeProjectSetting.ATS_KEY);

        atsDict.SetBoolean(XcodeProjectSetting.ALLOWS_ARBITRARY_LOADS_KEY, enableATS);
    }

    /// <summary>
    /// 设置状态栏
    /// </summary>
    /// <param name="plist"></param>
    /// <param name="enable"></param>
	public static void SetStatusBar(PlistDocument plist, bool enable)
    {
        plist.root.SetBoolean(XcodeProjectSetting.STATUS_HIDDEN_KEY, !enable);
        plist.root.SetBoolean(XcodeProjectSetting.STATUS_BAR_APPEARANCE_KEY, enable);
    }

    /// <summary>
    /// 设置全屏
    /// </summary>
    /// <param name="plist"></param>
    /// <param name="enable"></param>
    public static void SetFullScreen(PlistDocument plist, bool enable)
    {
        plist.root.SetBoolean(XcodeProjectSetting.UI_REQUIRES_FULL_SCREEN, enable);
    }

    /// <summary>
    /// 删除开场动画
    /// </summary>
    /// <param name="plist"></param>
    public static void DeleteLaunchiImagesKey(PlistDocument plist)
    {
        if (plist.root.values.ContainsKey(XcodeProjectSetting.UI_LAUNCHI_IMAGES_KEY))
        {
            plist.root.values.Remove(XcodeProjectSetting.UI_LAUNCHI_IMAGES_KEY);
        }
        if (plist.root.values.ContainsKey(XcodeProjectSetting.UI_LAUNCHI_STORYBOARD_NAME_KEY))
        {
            plist.root.values.Remove(XcodeProjectSetting.UI_LAUNCHI_STORYBOARD_NAME_KEY);
        }
    }

    public static void SetInfoPlist(string buildPath, XcodeProjectSetting setting)
    {
        PlistDocument plist = GetInfoPlist(buildPath);
        SetPrivacySensiticeData(plist, setting.privacySensiticeData);
        SetApplicationQueriesSchemes(plist, setting.ApplicationQueriesSchemes);
        SetBackgroundModes(plist, setting.BackgroundModes);
        SetURLSchemes(plist, setting.BundleUrlTypeList);
        SetBundleName(plist);
        SetATS(plist, setting.EnableATS);
        SetStatusBar(plist, setting.EnableStatusBar);
        SetFullScreen(plist, setting.EnableFullScreen);
        if (setting.NeedToDeleteLaunchiImagesKey)
        {
            DeleteLaunchiImagesKey(plist);
        }

        if (setting.EnableGameCenter)
        {
            SetGameCenter(plist);
        }

        plist.WriteToFile(GetInfoPlistPath(buildPath));
    }


    internal class GameCenterInfo
    {
        internal static readonly string Key = "UIRequiredDeviceCapabilities";
        internal static readonly List<string> Value = new List<string> { "armv7", "gamekit" };
    }

    internal class BundleNameInfo
    {
        internal static readonly string Key = "CFBundleName";
    }
}