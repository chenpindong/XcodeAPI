using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
#endif
/// <summary>
/// Xcode项目的一些设定值
/// </summary>
[System.Serializable]
public class XcodeProjectSetting : ScriptableObject
{
    private static XcodeProjectSetting instance;

    public static XcodeProjectSetting Instance
    {
        get
        {
            string SETTING_DATA_PATH = XcodeProjectSettingCreator.GetCurrentFilePath();
            instance = AssetDatabase.LoadAssetAtPath<XcodeProjectSetting>(SETTING_DATA_PATH);
            if (instance == null)
            {
                instance = CreateInstance<XcodeProjectSetting>();
#if UNITY_EDITOR
                AssetDatabase.CreateAsset(instance, SETTING_DATA_PATH);
#endif
            }
            return instance;
        }
    }

    public void SaveConfig()
    {
#if !UNITY_WEBPLAYER

#if UNITY_EDITOR
        EditorUtility.SetDirty(Instance);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
#endif

#endif
    }

    public const string PROJECT_ROOT = "$(PROJECT_DIR)/";
    public const string IMAGE_XCASSETS_DIRECTORY_NAME = "Unity-iPhone";
    public const string LINKER_FLAG_KEY = "OTHER_LDFLAGS";
    public const string FRAMEWORK_SEARCH_PATHS_KEY = "FRAMEWORK_SEARCH_PATHS";
    public const string LIBRARY_SEARCH_PATHS_KEY = "LIBRARY_SEARCH_PATHS";
    public const string ENABLE_BITCODE_KEY = "ENABLE_BITCODE";
    public const string DEVELOPMENT_TEAM = "DEVELOPMENT_TEAM";
    public const string PROVISIONING_PROFILE = "PROVISIONING_PROFILE";
    public const string PROVISIONING_PROFILE_SPECIFIER = "PROVISIONING_PROFILE_SPECIFIER";
    public const string CODE_SIGN_IDENTITY = "CODE_SIGN_IDENTITY";
    public const string CODE_SIGN_IDENTITY_OTHER = "CODE_SIGN_IDENTITY[sdk=iphoneos*]";
    public const string GCC_ENABLE_CPP_EXCEPTIONS = "GCC_ENABLE_CPP_EXCEPTIONS";
    public const string GCC_ENABLE_CPP_RTTI = "GCC_ENABLE_CPP_RTTI";
    public const string GCC_ENABLE_OBJC_EXCEPTIONS = "GCC_ENABLE_OBJC_EXCEPTIONS";
    public const string INFO_PLIST_NAME = "Info.plist";

    public const string URL_TYPES_KEY = "CFBundleURLTypes";
    public const string URL_TYPE_ROLE_KEY = "CFBundleTypeRole";
    public const string URL_IDENTIFIER_KEY = "CFBundleURLName";
    public const string URL_SCHEMES_KEY = "CFBundleURLSchemes";
    public const string APPLICATION_QUERIES_SCHEMES_KEY = "LSApplicationQueriesSchemes";

    public const string COPY_PATH = "Copy";
    public const string MODS_PATH = "Mods";

    #region XCodeproj
    [SerializeField]
    public bool EnableBitCode = false;
    [SerializeField]
    public bool EnableCppEcceptions = true;
    [SerializeField]
    public bool EnableCppRtti = true;
    [SerializeField]
    public bool EnableObjcExceptions = true;
    [SerializeField]
    public bool EnableGameCenter = false;

    //Entitlement文件路径
    [SerializeField]
    public string EntitlementFilePath = "hw.entitlements";
    //XCodeAPI文件的路径
    [SerializeField]
    public string XcodeAPIDirectoryPath = "Assets/Editor/XCodeAPI";
    //AppleDevelopment内TeamID
    [SerializeField]
    public string DevelopmentTeam = "K3G2K7L99P";
    //证书描述文件
    [SerializeField]
    public string ProvisioningProfile = "kp_product_20180716";
    //签名ID
    [SerializeField]
    public string CodeSignIdentity = "iPhone Distribution: Jiu Wanli network technology (Shanghai) Co., Ltd.";
    #region 引用的内部Framework
    [System.Serializable]
    public struct Framework
    {
        [SerializeField]
        public string name;
        [SerializeField]
        public bool weak;

        public Framework(string name, bool weak)
        {
            this.name = name;
            this.weak = weak;
        }
    }

    [SerializeField]
    public List<Framework> FrameworkList = new List<Framework>() { };
    #endregion

    // Embed Frameworks
    [SerializeField]
    public List<string> EmbedFrameworkList = new List<string>() { };

    //引用的内部.tbd
    [SerializeField]
    public List<string> TbdList = new List<string>() { };
    //设置OtherLinkerFlag
    [SerializeField]
    public string[] LinkerFlagArray = new string[] { };
    //设置FrameworkSearchPath
    [SerializeField]
    public string[] FrameworkSearchPathArray = new string[] { "$(inherited)", "$(PROJECT_DIR)/Frameworks" };

    #region 针对单个文件进行flag标记
    [System.Serializable]
    public struct CompilerFlagsSet
    {
        [SerializeField]
        public string Flags;
        [SerializeField]
        public List<string> TargetPathList;

        public CompilerFlagsSet(string flags, List<string> targetPathList)
        {
            Flags = flags;
            TargetPathList = targetPathList;
        }
    }

    [SerializeField]
    public List<CompilerFlagsSet> CompilerFlagsSetList = new List<CompilerFlagsSet>()
    {
        /*new CompilerFlagsSet ("-fno-objc-arc", new List<string> () {"Plugin/Plugin.mm"})*/
        //实例，请勿删除
    };
    #endregion

    #endregion

    #region 拷贝文件
    [System.Serializable]
    public struct CopyFiles
    {
        [SerializeField]
        public string sourcePath;
        [SerializeField]
        public string copyPath;

        public CopyFiles(string sourcePath, string copyPath)
        {
            this.sourcePath = sourcePath;
            this.copyPath = copyPath;
        }
    }

    [SerializeField]
    public List<CopyFiles> CopyFilesList = new List<CopyFiles>() { };
    #endregion

    #region info.Plist
    //白名单
    [SerializeField]
    public List<string> ApplicationQueriesSchemes = new List<string>() { };

    #region iOS10新的特性
    public enum NValueType
    {
        String,
        Int,
        Bool,
    }

    [System.Serializable]
    public struct PrivacySensiticeData
    {
        [SerializeField]
        public string key;
        [SerializeField]
        public string value;
        [SerializeField]
        public NValueType type;

        public PrivacySensiticeData(string key, string value, NValueType type)
        {
            this.key = key;
            this.value = value;
            this.type = type;
        }
    }

    [SerializeField]
    public List<PrivacySensiticeData> privacySensiticeData = new List<PrivacySensiticeData>() { };
    #endregion

    #region 第三方平台URL Scheme
    [System.Serializable]
    public struct BundleUrlType
    {
        [SerializeField]
        public string identifier;
        [SerializeField]
        public List<string> bundleSchmes;

        public BundleUrlType(string identifier, List<string> bundleSchmes)
        {
            this.identifier = identifier;
            this.bundleSchmes = bundleSchmes;
        }
    }

    [SerializeField]
    public List<BundleUrlType> BundleUrlTypeList = new List<BundleUrlType>() { };
    #endregion

    //放置后台需要开启的功能
    [SerializeField]
    public List<string> BackgroundModes = new List<string>() { };
    #endregion
}