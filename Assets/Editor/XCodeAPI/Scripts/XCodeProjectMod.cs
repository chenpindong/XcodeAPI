using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using ChillyRoom.UnityEditor.iOS.Xcode;
using System.Collections.Generic;
using System.IO;

public class XCodeProjectMod : MonoBehaviour
{
    [PostProcessBuild]
    private static void OnPostprocessBuild(BuildTarget buildTarget, string buildPath)
    {
        if (buildTarget != BuildTarget.iOS)
            return;
        PBXProject pbxProject = null;
        XcodeProjectSetting setting = null;
        string pbxProjPath = PBXProject.GetPBXProjectPath(buildPath);
        string targetGuid = null;
        Debug.Log("开始设置.XCodeProj");

        setting = XcodeProjectSetting.Instance;
        pbxProject = new PBXProject();
        pbxProject.ReadFromString(File.ReadAllText(pbxProjPath));
        targetGuid = pbxProject.TargetGuidByName(PBXProject.GetUnityTargetName());

        pbxProject.SetBuildProperty(targetGuid, XcodeProjectSetting.ENABLE_BITCODE_KEY, setting.EnableBitCode ? "YES" : "NO");
        if (!string.IsNullOrEmpty(setting.DevelopmentTeam))
        {
            pbxProject.SetBuildProperty(targetGuid, XcodeProjectSetting.DEVELOPMENT_TEAM, setting.DevelopmentTeam);
        }
        if (!string.IsNullOrEmpty(setting.ProvisioningProfile))
        {
            pbxProject.SetBuildProperty(targetGuid, XcodeProjectSetting.PROVISIONING_PROFILE, setting.ProvisioningProfile);
            pbxProject.SetBuildProperty(targetGuid, XcodeProjectSetting.PROVISIONING_PROFILE_SPECIFIER, setting.ProvisioningProfile);
        }
        if (!string.IsNullOrEmpty(setting.CodeSignIdentity))
        {
            pbxProject.SetBuildProperty(targetGuid, XcodeProjectSetting.CODE_SIGN_IDENTITY, setting.CodeSignIdentity);
            pbxProject.SetBuildProperty(targetGuid, XcodeProjectSetting.CODE_SIGN_IDENTITY_OTHER, setting.CodeSignIdentity);
        }
        pbxProject.SetBuildProperty(targetGuid, XcodeProjectSetting.GCC_ENABLE_CPP_EXCEPTIONS, setting.EnableCppEcceptions ? "YES" : "NO");
        pbxProject.SetBuildProperty(targetGuid, XcodeProjectSetting.GCC_ENABLE_CPP_RTTI, setting.EnableCppRtti ? "YES" : "NO");
        pbxProject.SetBuildProperty(targetGuid, XcodeProjectSetting.GCC_ENABLE_OBJC_EXCEPTIONS, setting.EnableObjcExceptions ? "YES" : "NO");

        //添加Capability
        CapabilityProcessor capProcessor = new CapabilityProcessor(pbxProject, buildPath, pbxProjPath, setting.EntitlementFilePath, targetGuid);
        capProcessor.AddInAppPurchase();
        capProcessor.AddPushNotifications(true);
        capProcessor.AddKeychainSharing();
        if (setting.EnableGameCenter)
        {
            capProcessor.AddGameCenter();
        }
        capProcessor.WriteToFile();

        if (!string.IsNullOrEmpty(setting.XcodeAPIDirectoryPath))
        {
            string modsPath = Path.Combine(setting.XcodeAPIDirectoryPath, XcodeProjectSetting.MODS_PATH);
            DirectoryProcessor.CopyAndAddBuildToXcode(pbxProject, targetGuid, modsPath, buildPath, "", setting.EmbedFrameworkList);
            if (setting.EmbedFrameworkList.Count > 0)
            {
                pbxProject.SetBuildProperty(targetGuid, "LD_RUNPATH_SEARCH_PATHS", "$(inherited) @executable_path/Frameworks");
            }
        }

        //编译器标记（Compiler flags）
        foreach (XcodeProjectSetting.CompilerFlagsSet compilerFlagsSet in setting.CompilerFlagsSetList)
        {
            foreach (string targetPath in compilerFlagsSet.TargetPathList)
            {
                if (!pbxProject.ContainsFileByProjectPath(targetPath))
                    continue;
                string fileGuid = pbxProject.FindFileGuidByProjectPath(targetPath);
                List<string> flagsList = pbxProject.GetCompileFlagsForFile(targetGuid, fileGuid);
                flagsList.Add(compilerFlagsSet.Flags);
                pbxProject.SetCompileFlagsForFile(targetGuid, fileGuid, flagsList);
            }
        }

        //引用内部框架
        foreach (string framework in setting.FrameworkList)
        {
            string libStr = framework;
            bool weak = false;
            if (framework.Contains(":"))
            {
                string[] ss = framework.Split(':');
                if (ss.Length > 1)
                {
                    libStr = ss[0];
                    weak = ss[1] == "weak";
                }
            }
            pbxProject.AddFrameworkToProject(targetGuid, libStr, weak);
        }

        //引用.tbd文件
        foreach (string lib in setting.LibList)
        {
            string libStr = lib;
            bool weak = false;
            if (lib.Contains(":"))
            {
                string[] ss = lib.Split(':');
                if (ss.Length > 1)
                {
                    libStr = ss[0];
                    weak = ss[1] == "weak";
                }
            }
            string fileGuid = pbxProject.AddFile("usr/lib/" + libStr, "Frameworks/" + libStr, PBXSourceTree.Sdk);
            pbxProject.AddFileToBuild(targetGuid, fileGuid, weak);
        }

        //设置OTHER_LDFLAGS
        pbxProject.UpdateBuildProperty(targetGuid, XcodeProjectSetting.LINKER_FLAG_KEY, setting.LinkerFlagArray, null);
        //设置Framework Search Paths
        pbxProject.UpdateBuildProperty(targetGuid, XcodeProjectSetting.FRAMEWORK_SEARCH_PATHS_KEY, setting.FrameworkSearchPathArray, null);
        File.WriteAllText(pbxProjPath, pbxProject.WriteToString());

        //已经存在的文件，拷贝替换
        foreach (XcodeProjectSetting.CopyFiles file in setting.CopyFilesList)
        {
            string sourcePath = Path.Combine(Application.dataPath, setting.XcodeAPIDirectoryPath.Replace("Assets/", ""), XcodeProjectSetting.COPY_PATH, file.sourcePath);
            string copyPath = buildPath + file.copyPath;
            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, copyPath, true);
            }
            else
            {
                DirectoryProcessor.CopyAndReplace(sourcePath, copyPath);
            }
        }

        //设置Plist
        InfoPlistProcessor.SetInfoPlist(buildPath, setting);
    }
}