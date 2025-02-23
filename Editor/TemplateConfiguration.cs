using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity_Template_Package.Editor
{
    [Serializable]
    public class TemplateConfiguration
    {
        public string mainRoot = "CorePackage";
        public List<string> directories = new List<string> { "Documentation", "Editor", "Runtime", "Samples", "Tests" };
        public List<string> files = new List<string> { "README.md", "config.json", "notes.txt" };
       
        [NonSerialized]
        public Dictionary<string, List<string>> subfolders = new Dictionary<string, List<string>>();
        [NonSerialized]
        public Dictionary<string, List<string>> dirFiles = new Dictionary<string, List<string>>();
        [NonSerialized]
        public Dictionary<string, List<string>> subfolderFiles = new Dictionary<string, List<string>>();
       
        public List<SerializableDictionaryEntry> subfoldersEntries = new List<SerializableDictionaryEntry>();
        public List<SerializableDictionaryEntry> dirFilesEntries = new List<SerializableDictionaryEntry>();
        public List<SerializableDictionaryEntry> subfolderFilesEntries = new List<SerializableDictionaryEntry>();

        public void PrepareForSerialization()
        {
            subfoldersEntries.Clear();
            foreach (var pair in subfolders)
            {
                subfoldersEntries.Add(new SerializableDictionaryEntry(pair.Key, pair.Value));
            }
            dirFilesEntries.Clear();
            foreach (var pair in dirFiles)
            {
                dirFilesEntries.Add(new SerializableDictionaryEntry(pair.Key, pair.Value));
            }
            subfolderFilesEntries.Clear();
            foreach (var pair in subfolderFiles)
            {
                subfolderFilesEntries.Add(new SerializableDictionaryEntry(pair.Key, pair.Value));
            }
        }

        public void RestoreFromSerialization()
        {
            subfolders = new Dictionary<string, List<string>>();
            foreach (var entry in subfoldersEntries)
            {
                subfolders[entry.key] = entry.value;
            }
            dirFiles = new Dictionary<string, List<string>>();
            foreach (var entry in dirFilesEntries)
            {
                dirFiles[entry.key] = entry.value;
            }
            subfolderFiles = new Dictionary<string, List<string>>();
            foreach (var entry in subfolderFilesEntries)
            {
                subfolderFiles[entry.key] = entry.value;
            }
        }
    }

    [Serializable]
    public class SerializableDictionaryEntry
    {
        public string key;
        public List<string> value;

        public SerializableDictionaryEntry() { }

        public SerializableDictionaryEntry(string key, List<string> value)
        {
            this.key = key;
            this.value = value;
        }
    }
}
