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

    private static void SetBundleName(PlistDocument plist)
    {
        PlistElementDict rootDict = plist.root;
        rootDict.SetString(BundleNameInfo.Key, PlayerSettings.productName);
    }

    private static void SetTransportSecurity(PlistDocument plist)
    {
        PlistElementDict transportSecurity;
        if (plist.root.values.ContainsKey(TransportSecurityInfo.Key))
            transportSecurity = plist.root[TransportSecurityInfo.Key].AsDict();
        else
            transportSecurity = plist.root.CreateDict(TransportSecurityInfo.Key);

        foreach (var item in TransportSecurityInfo.Value)
        {
            if (item.Value.GetType() == typeof(bool))
            {
                transportSecurity.SetBoolean(item.Key, (bool)item.Value);
            }
            else if (item.Value.GetType() == typeof(string))
            {
                transportSecurity.SetString(item.Key, (string)item.Value);
            }
            else if (item.Value.GetType() == typeof(int))
            {
                transportSecurity.SetInteger(item.Key, (int)item.Value);
            }
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
        SetTransportSecurity(plist);

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

    //允许http请求
    internal class TransportSecurityInfo
    {
        internal static readonly string Key = "NSAppTransportSecurity";
        internal static readonly Dictionary<string, object> Value = new Dictionary<string, object>() { { "NSAllowsArbitraryLoads", true } };
    }
}