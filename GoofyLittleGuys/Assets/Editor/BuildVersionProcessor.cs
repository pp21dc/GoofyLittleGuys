using UnityEngine;
using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class BuildVersionProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    private const string initalVersion = "0.0.0";
    
    public void OnPreprocessBuild(BuildReport report)
    {
        string currentVersion = FindCurrentVerion();
        UpdateVersion(currentVersion);
    }

    private string FindCurrentVerion()
    {
        string[] currentVersions = PlayerSettings.bundleVersion.Split('.');
        
        return currentVersions.Length == 1 ? initalVersion : currentVersions[1];
    }

    private void UpdateVersion(string version)
    {
        if (float.TryParse(version, out float versionNumber))
        {
            float newVersion = versionNumber + 0.01f;
            string date = DateTime.Now.ToString("yyyyMMddHHmmss");
            
            PlayerSettings.bundleVersion = string.Format("Version [{0}] - {1}", newVersion, date);
            Debug.Log(PlayerSettings.bundleVersion);
        }
    }
}
