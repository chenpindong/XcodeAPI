using ChillyRoom.UnityEditor.iOS.Xcode;
using ChillyRoom.UnityEditor.iOS.Xcode.Extensions;
using System.Collections.Generic;
using System.IO;

public static class DirectoryProcessor
{
    //拷贝并增加到项目
    public static void CopyAndAddBuildToXcode(PBXProject pbxProject, string targetGuid, string copyDirectoryPath, string buildPath, string currentDirectoryPath, List<string> embedFrameworks, bool needToAddBuild = true)
    {
        string unityDirectoryPath = copyDirectoryPath;
        string xcodeDirectoryPath = buildPath;

        if (!string.IsNullOrEmpty(currentDirectoryPath))
        {
            unityDirectoryPath = Path.Combine(unityDirectoryPath, currentDirectoryPath);
            xcodeDirectoryPath = Path.Combine(xcodeDirectoryPath, currentDirectoryPath);
            Delete(xcodeDirectoryPath);
            Directory.CreateDirectory(xcodeDirectoryPath);
        }

        foreach (string filePath in Directory.GetFiles(unityDirectoryPath))
        {
            //过滤.meta文件
            string extension = Path.GetExtension(filePath);
            if (extension == ExtensionName.META)
                continue;
            //
            if (extension == ExtensionName.ARCHIVE)
            {
                pbxProject.AddBuildProperty(targetGuid, XcodeProjectSetting.LIBRARY_SEARCH_PATHS_KEY, XcodeProjectSetting.PROJECT_ROOT + currentDirectoryPath);
            }

            string fileName = Path.GetFileName(filePath);
            string copyPath = Path.Combine(xcodeDirectoryPath, fileName);

            //有可能是.DS_Store文件，直接过滤
            if (fileName[0] == '.')
                continue;
            File.Delete(copyPath);
            File.Copy(filePath, copyPath);

            if (needToAddBuild)
            {
                string relativePath = Path.Combine(currentDirectoryPath, fileName);
                //特殊化处理，XMain目录下文件添加flags：-fno-objc-arc
                if (relativePath.Contains(ExtensionName.XMain))
                {
                    pbxProject.AddFileToBuildWithFlags(targetGuid, pbxProject.AddFile(relativePath, relativePath, PBXSourceTree.Source), "-fno-objc-arc");
                }
                else
                {
                    pbxProject.AddFileToBuild(targetGuid, pbxProject.AddFile(relativePath, relativePath, PBXSourceTree.Source));
                }
            }
        }

        foreach (string directoryPath in Directory.GetDirectories(unityDirectoryPath))
        {
            string directoryName = Path.GetFileName(directoryPath);
            if (directoryName.Contains(ExtensionName.LANGUAGE) && needToAddBuild)
            {
                //特殊化处理本地语言，暂时官方PBXProject不支持AddLocalization方法，如果需要，则必须自己扩充
                string relativePath = Path.Combine(currentDirectoryPath, directoryName);
                CopyAndAddBuildToXcode(pbxProject, targetGuid, copyDirectoryPath, buildPath, relativePath, embedFrameworks, false);
                string[] dirs = Directory.GetDirectories(Path.Combine(xcodeDirectoryPath, directoryName));
                if (dirs.Length > 0)
                {
                    string fileName = Path.GetFileName(Directory.GetFiles(dirs[0], "*.strings")[0]);
                    AddLocalizedStrings(pbxProject, buildPath, fileName, directoryPath, directoryName);
                }
            }
            else
            {
                bool nextNeedToAddBuild = needToAddBuild;
                if (directoryName.Contains(ExtensionName.FRAMEWORK) || directoryName.Contains(ExtensionName.BUNDLE) || directoryName == XcodeProjectSetting.IMAGE_XCASSETS_DIRECTORY_NAME)
                {
                    nextNeedToAddBuild = false;
                }
                CopyAndAddBuildToXcode(pbxProject, targetGuid, copyDirectoryPath, buildPath, Path.Combine(currentDirectoryPath, directoryName), embedFrameworks, nextNeedToAddBuild);
                if (directoryName.Contains(ExtensionName.FRAMEWORK))
                {
                    string relativePath = Path.Combine(currentDirectoryPath, directoryName);
                    string fileGuid = pbxProject.AddFile(relativePath, relativePath, PBXSourceTree.Source);
                    pbxProject.AddFileToBuild(targetGuid, fileGuid);
                    pbxProject.AddBuildProperty(targetGuid, XcodeProjectSetting.FRAMEWORK_SEARCH_PATHS_KEY, XcodeProjectSetting.PROJECT_ROOT + currentDirectoryPath);

                    if (embedFrameworks.Contains(directoryName))
                    {
                        PBXProjectExtensions.AddFileToEmbedFrameworks(pbxProject, targetGuid, fileGuid);
                    }
                }
                else if (directoryName.Contains(ExtensionName.BUNDLE) && needToAddBuild)
                {
                    string relativePath = Path.Combine(currentDirectoryPath, directoryName);
                    string fileGuid = pbxProject.AddFile(relativePath, relativePath, PBXSourceTree.Source);
                    pbxProject.AddFileToBuild(targetGuid, fileGuid);
                    pbxProject.AddBuildProperty(targetGuid, XcodeProjectSetting.FRAMEWORK_SEARCH_PATHS_KEY, XcodeProjectSetting.PROJECT_ROOT + currentDirectoryPath);
                }
            }
        }
    }

    //添加本地化strings文件
    public static void AddLocalizedStrings(PBXProject pbxProject, string buildPath, string fileName, string localizedDirectoryPath, string directoryName)
    {
        DirectoryInfo dir = new DirectoryInfo(localizedDirectoryPath);
        if (!dir.Exists)
            return;

        List<string> locales = new List<string>();
        var localeDirs = dir.GetDirectories("*.lproj", SearchOption.TopDirectoryOnly);

        foreach (var sub in localeDirs)
            locales.Add(Path.GetFileNameWithoutExtension(sub.Name));

        AddLocalizedStrings(pbxProject, buildPath, directoryName, fileName, localizedDirectoryPath, locales);
    }

    //添加本地化strings文件
    public static void AddLocalizedStrings(PBXProject pbxProject, string buildPath, string directoryName, string fileName, string localizedDirectoryPath, List<string> validLocales)
    {
        foreach (var locale in validLocales)
        {
            string fileRelatvePath = string.Format("{0}/{1}.lproj/{2}", directoryName, locale, fileName);
            pbxProject.AddLocalization(fileName, locale, fileRelatvePath);
        }
    }

    //拷贝文件夹或者文件
    public static void CopyAndReplace(string sourcePath, string copyPath)
    {
        Delete(copyPath);
        Directory.CreateDirectory(copyPath);
        foreach (var file in Directory.GetFiles(sourcePath))
        {
            if (Path.GetExtension(file) == ExtensionName.META)
                continue;
            File.Copy(file, Path.Combine(copyPath, Path.GetFileName(file)));
        }
        foreach (var dir in Directory.GetDirectories(sourcePath))
        {
            CopyAndReplace(dir, Path.Combine(copyPath, Path.GetFileName(dir)));
        }
    }

    //删除目标文件夹以及文件夹内的所有文件
    public static void Delete(string targetDirectoryPath)
    {
        if (!Directory.Exists(targetDirectoryPath))
            return;
        string[] filePaths = Directory.GetFiles(targetDirectoryPath);
        foreach (string filePath in filePaths)
        {
            File.SetAttributes(filePath, FileAttributes.Normal);
            File.Delete(filePath);
        }
        string[] directoryPaths = Directory.GetDirectories(targetDirectoryPath);
        foreach (string directoryPath in directoryPaths)
        {
            Delete(directoryPath);
        }
        Directory.Delete(targetDirectoryPath, false);
    }
}