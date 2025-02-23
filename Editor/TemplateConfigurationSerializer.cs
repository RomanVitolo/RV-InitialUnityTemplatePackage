using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Unity_Template_Package.Editor
{
    public static class TemplateConfigurationSerializer
    {
        public static bool SaveConfiguration(TemplateConfiguration config, string path)
        {
            try
            {
                config.PrepareForSerialization();
                string json = JsonUtility.ToJson(config, true);
                File.WriteAllText(path, json);
                return true;
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error Saving Configuration", ex.Message, "OK");
                return false;
            }
        }

        public static TemplateConfiguration LoadConfiguration(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                TemplateConfiguration config = JsonUtility.FromJson<TemplateConfiguration>(json);
                config.RestoreFromSerialization();
                return config;
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error Loading Configuration", ex.Message, "OK");
                return null;
            }
        }
    }
}