using UnityEngine;
using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class BuildVersionProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    private const string initalVersion = "0.1.0";
    
    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.Log("PREPROCESS BUILD CALL");
        string currentVersion = FindCurrentVersion();
        UpdateVersion(currentVersion);
    }

    private string FindCurrentVersion()
    {
        string[] currentVersions = PlayerSettings.bundleVersion.Split('-');
        string[] versionNumbers = currentVersions[0].Split(".");
        
        return versionNumbers.Length == 2 ? initalVersion : versionNumbers[2];
    }

    private void UpdateVersion(string version)
    {
        Debug.Log(version);
        if (float.TryParse(version, out float versionNumber))
        {
            float newVersion = versionNumber + 1f;
            string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            PlayerSettings.bundleVersion = string.Concat("0.1.", newVersion.ToString(), " - ", date);
        }
        else
        {
            Debug.LogError("COULD NOT PARSE VERSION NUMBER");
        }
    }
}
