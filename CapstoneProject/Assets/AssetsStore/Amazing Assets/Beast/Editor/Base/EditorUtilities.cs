using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEditor;


namespace AmazingAssets.BeastEditor
{
    static internal class EditorUtilities
    {
        static string thisAssetPath = string.Empty;

        static public char[] invalidFileNameCharachters = Path.GetInvalidFileNameChars();




        static internal string GetThisAssetsProjectPath()
        {
            if (string.IsNullOrEmpty(thisAssetPath) || File.Exists(thisAssetPath) == false)
            {

                string[] assets = AssetDatabase.FindAssets("EditorUtilities", null);
                if (assets != null && assets.Length > 0)
                {
                    for (int i = 0; i < assets.Length; i++)
                    {
                        if (string.IsNullOrEmpty(assets[i]) == false)
                        {
                            string filePath = AssetDatabase.GUIDToAssetPath(assets[i]);

                            if (filePath.Contains("Amazing Assets") &&
                                filePath.Contains("Beast") &&
                                filePath.Contains("Editor") &&
                                filePath.Contains("Base") &&
                                filePath.Contains("EditorUtilities.cs"))
                            {
                                thisAssetPath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(filePath)));
                            }
                        }
                    }
                }
            }

            return thisAssetPath;
        }


        static public bool ConvertFullPathToProjectRelative(string path, out string newPath)
        {
            if (string.IsNullOrEmpty(path))
            {
                newPath = string.Empty;
                return false;
            }


            if (path.IndexOf("Assets") == 0)
            {
                newPath = path;
                return true;
            }


            path = path.Replace("\\", "/").Replace("\"", "/");
            if (path.StartsWith(Application.dataPath))
            {
                newPath = "Assets" + path.Substring(Application.dataPath.Length);
                return true;
            }
            else
            {
                newPath = path;
                return false;
            }
        }
        static public bool IsPathProjectRelative(string path)
        {
            if (path.IndexOf("Assets") == 0)
            {
                return true;
            }
            else
            {
                string projectPath = Application.dataPath.Replace("\\", string.Empty).Replace("/", string.Empty);
                path = path.Replace("\\", string.Empty).Replace("/", string.Empty);

                return path.Contains(projectPath);
            }
        }


        static public string RemoveInvalidCharacters(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;
            else
            {
                if (name.IndexOfAny(invalidFileNameCharachters) == -1)
                    return name;
                else
                    return string.Concat(name.Split(invalidFileNameCharachters, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        static public bool ContainsInvalidFileNameCharacters(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            else
                return name.IndexOfAny(invalidFileNameCharachters) >= 0;
        }
    }
}