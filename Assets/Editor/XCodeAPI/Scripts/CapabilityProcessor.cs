using ChillyRoom.UnityEditor.iOS.Xcode;
using System.IO;
using UnityEditor;

public class CapabilityProcessor
{
    private readonly string m_BuildPath;
    private readonly string m_TargetGuid;
    private readonly string m_PBXProjectPath;
    private readonly string m_EntitlementFilePath;
    private PlistDocument m_Entitlements;
    private PBXProject m_Project;

    /// <summary>
    /// Creates a new instance of ProjectCapabilityManager. The returned 
    /// instance assumes ownership of the referenced pbxproj project file, 
    /// the entitlements file and project Info.plist files until the last 
    /// WriteToFile() call.
    /// </summary>
    /// <param name="pbxProjectPath">Path to the pbxproj file.</param>
    /// <param name="entitlementFilePath">Path to the entitlements file.</param>
    /// <param name="targetGuid">The name of the target to add entitlements for.</param>
    public CapabilityProcessor(PBXProject project, string buildPath, string pbxProjectPath, string entitlementFilePath, string targetGuid)
    {
        //m_BuildPath = Directory.GetParent(Path.GetDirectoryName(pbxProjectPath)).FullName;

        m_Project = project;
        m_BuildPath = buildPath;
        m_PBXProjectPath = pbxProjectPath;
        m_EntitlementFilePath = entitlementFilePath;
        m_TargetGuid = targetGuid;
    }

    /// <summary>
    /// Writes the modifications to the project file, entitlements file and 
    /// the ProjectCapabilityManager instance has been created and before
    /// the call to WriteToFile() will be overwritten.
    /// </summary>
    public void WriteToFile()
    {
        File.WriteAllText(m_PBXProjectPath, m_Project.WriteToString());
        if (m_Entitlements != null)
            m_Entitlements.WriteToFile(Path.Combine(m_BuildPath, m_EntitlementFilePath));
    }

    private PlistDocument GetOrCreateEntitlementDoc()
    {
        if (m_Entitlements == null)
        {
            m_Entitlements = new PlistDocument();
            string[] entitlementsFiles = Directory.GetFiles(m_BuildPath, m_EntitlementFilePath);
            if (entitlementsFiles.Length > 0)
            {
                m_Entitlements.ReadFromFile(entitlementsFiles[0]);
            }
            else
            {
                m_Entitlements.Create();
            }
        }

        return m_Entitlements;
    }

    /// <summary>
    /// Adds Game Center capability to the project
    /// </summary>
    public void AddGameCenter()
    {
        m_Project.AddCapability(m_TargetGuid, PBXCapabilityType.GameCenter);
    }

    public void AddKeychainSharing()
    {
        PlistElementArray temp = new PlistElementArray();
        temp.AddString("$(AppIdentifierPrefix)" + PlayerSettings.applicationIdentifier);
        GetOrCreateEntitlementDoc().root[KeychainEntitlements.Key] = temp;
        m_Project.AddCapability(m_TargetGuid, PBXCapabilityType.KeychainSharing);
    }

    /// <summary>
    /// Add Push (or remote) Notifications capability to the project
    /// </summary>
    /// <param name="development">Sets the development option if set to true</param>
    public void AddPushNotifications(bool development)
    {
        GetOrCreateEntitlementDoc().root[PushNotificationEntitlements.Key] = new PlistElementString(development ? PushNotificationEntitlements.DevelopmentValue : PushNotificationEntitlements.ProductionValue);
        m_Project.AddCapability(m_TargetGuid, PBXCapabilityType.PushNotifications, m_EntitlementFilePath);
    }

    /// <summary>
    /// Adds In App Purchase capability to the project.
    /// </summary>
    public void AddInAppPurchase()
    {
        m_Project.AddCapability(m_TargetGuid, PBXCapabilityType.InAppPurchase);
    }

    internal class PushNotificationEntitlements
    {
        internal static readonly string Key = "aps-environment";
        internal static readonly string DevelopmentValue = "development";
        internal static readonly string ProductionValue = "production";
    }

    internal class KeychainEntitlements
    {
        internal static readonly string Key = "keychain-access-groups";
    }
}
