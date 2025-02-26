using System;
using System.Collections.Generic;

namespace Unity_Template_Package.Editor
{
    [Serializable]
    public class TemplateConfiguration
    {
        public string mainRoot = "CoreProject";
        public string saveLocation = "";
        public List<string> directories = new List<string> { };
        public List<string> files = new List<string> {  };
       
        [NonSerialized]
        public Dictionary<string, List<string>> subfolders = new Dictionary<string, List<string>>();
        [NonSerialized]
        public Dictionary<string, List<string>> dirFiles = new Dictionary<string, List<string>>();
        [NonSerialized]
        public Dictionary<string, List<string>> subfolderFiles = new Dictionary<string, List<string>>();
        
        public void EnsureSubfolderFiles(string dirName, string subName)
        {
            string subKey = $"{dirName}/{subName}";
            if (!subfolderFiles.ContainsKey(subKey))
                subfolderFiles[subKey] = new List<string>();
        }
       
        public List<SerializableDictionaryEntry> subfoldersEntries = new List<SerializableDictionaryEntry>();
        public List<SerializableDictionaryEntry> dirFilesEntries = new List<SerializableDictionaryEntry>();
        public List<SerializableDictionaryEntry> subfolderFilesEntries = new List<SerializableDictionaryEntry>();
        public Dictionary<string, List<string>> nestedSubfolders = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> nestedSubfolderFiles = new Dictionary<string, List<string>>();


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
