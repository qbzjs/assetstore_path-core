using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Funly.SkyStudio
{
    [CustomEditor(typeof(SkyStudioSetupTool))]
    public class SkyStudioSetupToolEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GUIStyle largeBold = new GUIStyle(EditorStyles.label);
            largeBold.fontStyle = FontStyle.Bold;
            largeBold.richText = true;

            GUIStyle medium = new GUIStyle(EditorStyles.label);
            medium.richText = true;
            medium.wordWrap = true;
            
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Welcome to Sky Studio!", largeBold);
            EditorGUILayout.LabelField(
                "To finish setting up Sky Studio you <b>MUST select your rendering setup below</b>. This will configure Sky Studio and the install the demo scenes for you.",
                medium);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Built-in Pipeline", largeBold);
            EditorGUILayout.LabelField(
                "This is the standard default Unity rendering pipeline, and it's what most people want unless they know otherwise.",
                medium);
            if (GUILayout.Button("Setup for Built-In Pipeline"))
            {
                Debug.Log("Setting up for built-in rendering pipeline");
                RenderingPackageInstaller.Install(RenderingConfig.DetectedRenderingConfig.Builtin);
            }


            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("URP Pipeline", largeBold);
            EditorGUILayout.LabelField(
                "This is the Universal Rendering Pipeline. Sky Studio will create a new render pipeline asset (since we need to customize it), but you're free to configure it also for your own needs also.",
                medium);
            if (GUILayout.Button("Setup for URP Pipeline"))
            {
                Debug.Log("Setting up for built-in rendering pipeline");
                RenderingPackageInstaller.Install(RenderingConfig.DetectedRenderingConfig.URP);
            }


            // Check if they installed a version already.
            RenderingContentVersion installedVersion = RenderingPackageInstaller.GetInstalledVersionFile();
            if (installedVersion == null || installedVersion.version == null || installedVersion.version.Length <= 0)
            {
                return;
            }
            
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Currently Installed Pipeline & Version", largeBold);
            EditorGUILayout.LabelField(installedVersion.version, medium);

            var latestVersion = LatestVersion(installedVersion.version);
            if (latestVersion == null)
            {
                return;
            }

            if (latestVersion != installedVersion.version)
            {
                largeBold.normal.textColor = Color.red;
                EditorGUILayout.LabelField("Installed version is out of date, latest installable version is:", largeBold);
                EditorGUILayout.LabelField(latestVersion, medium);
            }
        }
        
        static string LatestVersion(string currentVersion) {
            // 
            if (currentVersion.Contains("Builtin"))
            {
                return RenderingContentPackages.builtinPackage;
            } else if (currentVersion.Contains("URP"))
            {
                return RenderingContentPackages.urpPackage;
            }

            return null;
        } 
    }
    
    
    
}
