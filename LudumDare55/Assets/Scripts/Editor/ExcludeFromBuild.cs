using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Assertions;

// from https://forum.unity.com/threads/excluding-folders-from-build.408375/page-2
public class ExcludeFromBuild : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    /*
     * Project relative path :
     * --> "Assets\Prefabs\..."
     */
    public List<string> paths = new()
        {
        };

    public static string tempDirectory = @"Assets\TempAssets~";
    public int callbackOrder => default;
    public static string GetTempFilename(string path)
    {
        return Path.Combine(tempDirectory, Path.GetFileName(path) + "~");
    }
    public static string GetExtension(BuildTarget target)
    {
        string videoExtension = null;
        switch (target)
        {
            case BuildTarget.iOS:
            case BuildTarget.Android:
            case BuildTarget.WebGL:
                videoExtension = ".webm";
                break;
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
            case BuildTarget.StandaloneOSX:
                videoExtension = ".mp4";
                break;
            case BuildTarget.StandaloneLinux64:
                videoExtension = ".vp8";
                break;
            default:
                break;
        }
        return videoExtension;
    }
    public void OnPreprocessBuild(BuildReport report)
    {
        string videoExtension = GetExtension(report.summary.platform);
        for (int i = 0; i < paths.Count; i++)
        {
            //Since we "create" an hidden folder or file, Unity will remove the META files associated with them
            if (!(Path.GetExtension(paths[i]).Equals(videoExtension)))
            {
                if (File.Exists(paths[i]))
                {
                    File.Move(paths[i], GetTempFilename(paths[i]));
                }
            }
        }
        AssetDatabase.Refresh();
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        string videoExtension = GetExtension(report.summary.platform);
        for (int i = 0; i < paths.Count; i++)
        {
            //No more META files to consider, so we can use File.Move()
            if (!(Path.GetExtension(paths[i]).Equals(videoExtension)))
            {
                if (File.Exists(GetTempFilename(paths[i])))
                {
                    File.Move(GetTempFilename(paths[i]), paths[i]);
                }
            }
        }
        AssetDatabase.Refresh();
        CleanupDoNotShipData(report);
    }

    // from https://forum.unity.com/threads/burstdebuginformation_donotship-in-builds.1172273/#post-8274465
    public void CleanupDoNotShipData(BuildReport report)
    {
        string outputPath = report.summary.outputPath;
        try
        {
            string applicationName = Path.GetFileNameWithoutExtension(outputPath);
            string outputFolder = Path.GetDirectoryName(outputPath);
            Assert.IsNotNull(outputFolder);
            outputFolder = Path.GetFullPath(outputFolder);

            //Delete Burst Debug Folder
            string burstDebugInformationDirectoryPath = Path.Combine(outputFolder, $"{applicationName}_BurstDebugInformation_DoNotShip");

            if (Directory.Exists(burstDebugInformationDirectoryPath))
            {
                Debug.Log($"Deleting Burst debug information folder at path '{burstDebugInformationDirectoryPath}'...");

                Directory.Delete(burstDebugInformationDirectoryPath, true);
            }

            //Delete il2cpp Debug Folder
            string il2cppDebugInformationDirectoryPath = Path.Combine(outputFolder, $"{applicationName}_BackUpThisFolder_ButDontShipItWithYourGame");

            if (Directory.Exists(il2cppDebugInformationDirectoryPath))
            {
                Debug.Log($"Deleting Burst debug information folder at path '{il2cppDebugInformationDirectoryPath}'...");

                Directory.Delete(il2cppDebugInformationDirectoryPath, true);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"An unexpected exception occurred while performing build cleanup: {e}");
        }
    }
}

