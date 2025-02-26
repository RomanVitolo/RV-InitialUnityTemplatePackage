#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;

namespace Unity_Template_Package.Editor
{
    public class TemplateToolWindow : EditorWindow
    {
        [MenuItem("RV - Template Tool/Open Template Tool")]
        public static void ShowWindow()
        {
            GetWindow<TemplateToolWindow>("RV - Template Tool");
        }
        
        private int selectedTab = 0;
        private readonly string[] tabNames = new string[] { "Template Tool", "Preview", "Documentation" };

        private TemplateConfiguration templateConfig = new TemplateConfiguration();
        private Vector2 toolScrollPosition;
        private Vector2 previewScrollPosition;
        private Vector2 docScrollPosition;
        private int selectedRootFileTypeIndex = 0;
        private readonly string[] fileTypeOptions = new string[] { ".cs", ".txt", ".md", ".unity", ".json", ".mat", ".asmdef" };

        private readonly Dictionary<string, int> directoryFileTypeSelections = new Dictionary<string, int>();
        private readonly Dictionary<string, int> subfolderFileTypeSelections = new Dictionary<string, int>();
        private Dictionary<string, bool> directoryFoldouts = new Dictionary<string, bool>();
        private Dictionary<string, bool> subfolderFoldouts = new Dictionary<string, bool>();
        private Dictionary<string, ReorderableList> subfoldersLists = new Dictionary<string, ReorderableList>();
        private Dictionary<string, ReorderableList> subfolderFilesLists = new Dictionary<string, ReorderableList>();
       
        private readonly ITemplateResourceCreator resourceCreator = new TemplateResourceCreator();
     
        private ReorderableList directoriesList;

        private void OnEnable()
        {
            InitializeUIComponents();
            
            directoryFileTypeSelections.Clear();
            
            foreach (var dir in templateConfig.directories)
            {
                directoryFileTypeSelections.TryAdd(dir, 0);

                if (templateConfig.subfolders.ContainsKey(dir))
                {
                    foreach (var sub in templateConfig.subfolders[dir])
                    {
                        string subKey = $"{dir}/{sub}";
                        if (!subfolderFileTypeSelections.ContainsKey(subKey))
                            subfolderFileTypeSelections[subKey] = 0; 
                    }
                }
            }
        }

        private void OnGUI()
        {
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames);

            if (selectedTab == 0)
                DrawToolTab();
            else if (selectedTab == 1)
                DrawPreviewTab();
            else if (selectedTab == 2)
                DrawDocumentationTab();
        }

        private void DrawToolTab()
        {
            toolScrollPosition = EditorGUILayout.BeginScrollView(toolScrollPosition);
        
            try
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                GUI.color = Color.green;
                GUILayout.Label("Template Folder Setup", EditorStyles.boldLabel);
                GUI.color = Color.white;
                templateConfig.mainRoot = EditorGUILayout.TextField("Main Root Name", templateConfig.mainRoot);
                EditorGUILayout.EndVertical();
        
                GUILayout.Space(10);
               
                EditorGUILayout.BeginVertical(GUI.skin.box);
                GUI.color = Color.cyan;
                GUILayout.Label("Directories", EditorStyles.boldLabel);
                GUI.color = Color.white;
                directoriesList.DoLayoutList();
                EditorGUILayout.EndVertical();
        
                GUILayout.Space(10);
                
                for (int i = 0; i < templateConfig.directories.Count; i++)
                {
                    string dirName = templateConfig.directories[i];
                    
                    directoryFileTypeSelections.TryAdd(dirName, 0); 
                    
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    GUI.color = Color.black;
                    GUILayout.Space(10);
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    GUI.color = Color.cyan;
                    GUILayout.Label("Directory: " + dirName, EditorStyles.boldLabel);
                    GUI.color = Color.white;
        
                    if (!templateConfig.subfolders.ContainsKey(dirName))
                        templateConfig.subfolders[dirName] = new List<string>();
                    if (!templateConfig.dirFiles.ContainsKey(dirName))
                        templateConfig.dirFiles[dirName] = new List<string>();
                    
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    GUI.color = Color.magenta;
                    GUILayout.Label("Subfolders", EditorStyles.boldLabel);
                    GUI.color = Color.white;
        
                    if (!subfoldersLists.ContainsKey(dirName))
                    {
                        subfoldersLists[dirName] = new ReorderableList(templateConfig.subfolders[dirName], typeof(string), true, true, true, true)
                        {
                            drawHeaderCallback = (Rect rect) =>
                            {
                                EditorGUI.LabelField(rect, "Subfolders (Drag to Reorder)");
                            },
                            drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                            {
                                if (index >= 0 && index < templateConfig.subfolders[dirName].Count)
                                {
                                    templateConfig.subfolders[dirName][index] = EditorGUI.TextField(rect, templateConfig.subfolders[dirName][index]);
                                }
                            },
                            onAddCallback = (ReorderableList list) =>
                            {
                                string newSubName = GetUniqueName(templateConfig.subfolders[dirName], "NewSubfolder");
                                templateConfig.subfolders[dirName].Add(newSubName);
                            }
                        };
                    }
        
                    subfoldersLists[dirName].DoLayoutList();
                    EditorGUILayout.EndVertical(); 
                    GUILayout.Space(5);
                  
                    if (templateConfig.subfolders.ContainsKey(dirName))
                    {
                        foreach (var subName in new List<string>(templateConfig.subfolders[dirName]))
                        {
                            string subKey = $"{dirName}/{subName}";
                            
                            EditorGUILayout.BeginVertical(GUI.skin.box);
                            GUILayout.Label("Subfolder: " + subName, EditorStyles.boldLabel);
        
                            EditorGUILayout.BeginHorizontal();
                            GUI.color = Color.yellow;
                            EditorGUILayout.LabelField("File Type:", GUILayout.Width(100));
                            GUI.color = Color.white;
                            
                            if (!subfolderFileTypeSelections.ContainsKey(subKey))
                                subfolderFileTypeSelections[subKey] = 0;

                            int newFileTypeIndex = EditorGUILayout.Popup(
                                subfolderFileTypeSelections.ContainsKey(subKey) ? subfolderFileTypeSelections[subKey] : 0, 
                                fileTypeOptions, GUILayout.Width(80)
                            );

                            if (!subfolderFileTypeSelections.ContainsKey(subKey) || newFileTypeIndex != subfolderFileTypeSelections[subKey])
                            {
                                subfolderFileTypeSelections[subKey] = newFileTypeIndex;
                                EditorUtility.SetDirty(this); 
                            }
                            EditorGUILayout.EndHorizontal();
                           
                            if (!templateConfig.subfolderFiles.ContainsKey(subKey))
                                templateConfig.subfolderFiles[subKey] = new List<string>();
        
                            if (!subfolderFilesLists.ContainsKey(subKey))
                            {
                                subfolderFilesLists[subKey] = new ReorderableList(templateConfig.subfolderFiles[subKey], typeof(string), true, true, true, true)
                                {
                                    drawHeaderCallback = (Rect rect) =>
                                    {
                                        EditorGUI.LabelField(rect, "Files in " + subName);
                                    },
                                    drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                                    {
                                        if (index >= 0 && index < templateConfig.subfolderFiles[subKey].Count)
                                        {
                                            templateConfig.subfolderFiles[subKey][index] = EditorGUI.TextField(rect, templateConfig.subfolderFiles[subKey][index]);
                                        }
                                    },
                                    onAddCallback = (ReorderableList list) =>
                                    {
                                        if (!subfolderFileTypeSelections.ContainsKey(subKey))
                                            subfolderFileTypeSelections[subKey] = 0; 

                                        int fileTypeIndex = subfolderFileTypeSelections[subKey]; 
                                        string extension = fileTypeIndex >= 0 && fileTypeIndex < fileTypeOptions.Length ? fileTypeOptions[fileTypeIndex] : ".cs";
                                        
                                        if (string.IsNullOrWhiteSpace(extension))
                                            extension = ".txt"; 

                                        string newFileName = GetUniqueFileName(templateConfig.subfolderFiles[subKey], "NewFile", extension);
                                        templateConfig.subfolderFiles[subKey].Add(newFileName);
                                    }
                                };
                            }
        
                            subfolderFilesLists[subKey].DoLayoutList();
                            EditorGUILayout.EndVertical(); 
                        }
                    }
        
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    GUI.color = Color.yellow;
                    GUILayout.Label("Files in Directory", EditorStyles.boldLabel);
                    GUI.color = Color.white;
                    
                    if (!templateConfig.dirFiles.ContainsKey(dirName))
                        templateConfig.dirFiles[dirName] = new List<string>();
                    
                    foreach (var dir in templateConfig.directories)
                    {
                        directoryFileTypeSelections.TryAdd(dir, 0);
                    }
                    
                    int currentDirIndex = i;
                    
                    if (!subfolderFilesLists.ContainsKey(dirName))
                    {
                        subfolderFilesLists[dirName] = new ReorderableList(templateConfig.dirFiles[dirName], 
                            typeof(string), true, true, true, true)
                    {   
                    drawHeaderCallback = (Rect rect) =>
                    {
                        EditorGUI.LabelField(rect, "Files in " + dirName);
                    },
                    drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                    {
                    if (index >= 0 && index < templateConfig.dirFiles[dirName].Count)
                    {
                        templateConfig.dirFiles[dirName][index] = EditorGUI.TextField(rect, templateConfig.dirFiles[dirName][index]);
                    }
                    },
                    onAddCallback = (ReorderableList list) =>
                    {
                        if (!directoryFileTypeSelections.ContainsKey(dirName))
                            directoryFileTypeSelections[dirName] = 0;

                        int fileTypeIndex = directoryFileTypeSelections[dirName]; 
                        string extension = fileTypeOptions[fileTypeIndex]; 
                        
                        if (string.IsNullOrWhiteSpace(extension))
                            extension = ".txt"; 

                        string newFileName = GetUniqueFileName(templateConfig.dirFiles[dirName], "NewFile", extension);
                        templateConfig.dirFiles[dirName].Add(newFileName);
                    }
                    };
                    }
        
                    subfolderFilesLists[dirName].DoLayoutList();
        
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Directory File Type:", GUILayout.Width(120));
                    directoryFileTypeSelections.TryAdd(dirName, 0); 
                    directoryFileTypeSelections[dirName] = EditorGUILayout.Popup(directoryFileTypeSelections[dirName], fileTypeOptions, GUILayout.Width(80));
                    
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical(); 
                    
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(10);
                }
              
                EditorGUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label("Root Files", EditorStyles.boldLabel);
        
                ReorderableList rootFilesList = new ReorderableList(templateConfig.files, typeof(string), true, true, true, true)
                {
                    drawHeaderCallback = (Rect rect) =>
                    {
                        EditorGUI.LabelField(rect, "Root Files");
                    },
                    drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                    {
                        if (index >= 0 && index < templateConfig.files.Count)
                        {
                            templateConfig.files[index] = EditorGUI.TextField(rect, templateConfig.files[index]);
                        }
                    },
                    onAddCallback = (ReorderableList list) =>
                    {
                        string newFileName = GetUniqueFileName(templateConfig.files, "NewFile", fileTypeOptions[selectedRootFileTypeIndex]);
                        templateConfig.files.Add(newFileName);
                    }
                };
        
                rootFilesList.DoLayoutList();
        
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Root File Type:", GUILayout.Width(120));
                selectedRootFileTypeIndex = EditorGUILayout.Popup(selectedRootFileTypeIndex, fileTypeOptions, GUILayout.Width(65));
                EditorGUILayout.EndHorizontal();
        
                EditorGUILayout.EndVertical(); 
                GUILayout.Space(10);
                
                if (GUILayout.Button("Select Custom Save Location"))
                {
                    string folder = EditorUtility.OpenFolderPanel("Select Save Location", Application.dataPath, "");
                    if (!string.IsNullOrEmpty(folder))
                    {
                        templateConfig.saveLocation = folder;
                        EditorUtility.DisplayDialog("Save Location Set", "MainRoot folder will be created in:\n" + folder, "OK");
                    }
                }
            
                if (GUILayout.Button("Save Configuration"))
                {
                    string path = EditorUtility.SaveFilePanel("Save Template Configuration", Application.dataPath, "TemplateConfig", "json");
                    if (!string.IsNullOrEmpty(path))
                    {
                        if (TemplateConfigurationSerializer.SaveConfiguration(templateConfig, path))
                            EditorUtility.DisplayDialog("Save Configuration", "Configuration saved successfully!", "OK");
                    }
                }
        
                if (GUILayout.Button("Load Configuration"))
                {
                    string path = EditorUtility.OpenFilePanel("Load Template Configuration", Application.dataPath, "json");
                    if (!string.IsNullOrEmpty(path))
                    {
                        TemplateConfiguration loadedConfig = TemplateConfigurationSerializer.LoadConfiguration(path);
                        if (loadedConfig != null)
                        {
                            templateConfig = loadedConfig;
                            InitializeUIComponents();
                            Repaint();
                            EditorUtility.DisplayDialog("Load Configuration", "Configuration loaded successfully!", "OK");
                        }
                    }
                }
        
                if (GUILayout.Button("Create Template Resources"))
                {
                    try
                    {
                        resourceCreator.CreateResources(templateConfig);
                    }
                    catch (System.Exception ex)
                    {
                        EditorUtility.DisplayDialog("Error", ex.Message, "OK");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error in DrawToolTab: " + ex);
            }
            finally
            {
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawPreviewTab()   
        {
            previewScrollPosition = EditorGUILayout.BeginScrollView(previewScrollPosition);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Preview", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Main Root: " + templateConfig.mainRoot);

            foreach (string dir in templateConfig.directories)
            {
                directoryFoldouts.TryAdd(dir, true);
                directoryFoldouts[dir] = EditorGUILayout.Foldout(directoryFoldouts[dir], "Directory: " + dir);
        
                if (directoryFoldouts[dir])
                {
                    EditorGUI.indentLevel++;
            
                if (templateConfig.dirFiles.TryGetValue(dir, out var dirFile))
                {
                    foreach (string file in dirFile)
                    {
                     EditorGUILayout.LabelField("File: " + file);
                    }
                }

                if (templateConfig.subfolders.TryGetValue(dir, out var subfolders))
                {
                    foreach (string sub in subfolders)
                    {
                        string subKey = dir + "/" + sub;
                        subfolderFoldouts.TryAdd(subKey, true);
                        subfolderFoldouts[subKey] = EditorGUILayout.Foldout(subfolderFoldouts[subKey], "Subfolder: " + sub);
                        
                        if (subfolderFoldouts[subKey])
                        {
                            EditorGUI.indentLevel++;
                            if (templateConfig.subfolderFiles.TryGetValue(subKey, out var filesInSubfolder))
                            {
                                foreach (string sfile in filesInSubfolder)
                                {
                                 EditorGUILayout.LabelField("File: " + sfile);
                                }
                            }
                            EditorGUI.indentLevel--;
                        }
                    }
                }

                EditorGUI.indentLevel--;
                }
            }

            if (templateConfig.files.Count > 0)
            {
                EditorGUILayout.LabelField("Root Files:");
                EditorGUI.indentLevel++;
                foreach (string file in templateConfig.files)
                {
                    EditorGUILayout.LabelField("File: " + file);
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }


        private void DrawDocumentationTab()
        {
            docScrollPosition = EditorGUILayout.BeginScrollView(docScrollPosition);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Documentation", EditorStyles.boldLabel);

            string documentation =
            "RV - Template Tool Documentation\n\n" +
            "Overview\n" +
            "This tool allows you to generate a structured folder and file setup for Unity projects. " +
            "It provides a graphical interface for managing directories, subfolders, and various file types.\n\n" +
            
            "How to Use the Tool:**\n\n" +
            "1. Set Up the Main Root\n" +
            "   - Enter a name in the 'Main Root Name' field.\n\n" +
            "2. Add & Configure Directories**\n" +
            "   - Click the '+' button to add new directories.\n" +
            "   - Select a **default file type** for each directory.\n" +
            "   - Add **files inside each directory** using the 'Files in Directory' list.\n\n" +
            "3. Manage Subfolders**\n" +
            "   - Each directory can contain **subfolders**.\n" +
            "   - Click the '+' button inside a directory's **Subfolders** section to create new subfolders.\n" +
            "   - Assign a **default file type** to each subfolder.\n" +
            "   - Add **files inside subfolders** using the 'Files in Subfolder' list.\n\n" +
            "4. Customize File Creation**\n" +
            "   - Select the file type before adding files to directories or subfolders.\n" +
            "   - Supported file types: `.cs`, `.txt`, `.md`, `.unity`, `.json`, `.mat`, `.asmdef`.\n\n" +
            "5. Save & Load Configurations**\n" +
            "   - Use the 'Save Configuration' button to export your folder structure to a JSON file.\n" +
            "   - Reload previous configurations using 'Load Configuration'.\n\n" +
            "6. Generate the Template Structure**\n" +
            "   - Click 'Create Template Resources' to generate the entire directory and file structure in Unity.\n" +
            "   - Unity will create `.mat` files correctly using `AssetDatabase.CreateAsset()`.\n\n" +
            "7. Preview the Folder Structure**\n" +
            "   - Switch to the 'Preview' tab to see the hierarchy of created folders and files.\n" +
            "   - All subfolders and files are now correctly displayed.\n\n" +
            
            "**Developer Info:**\n" +
            "Author: Roman Vitolo\n" +
            "Website: https://romanvitolo.com\n" +
            "GitHub Repository: https://github.com/RomanVitolo\n" +
            "For Issues & Feedback: You can contact me via email (available on my website).\n";

            EditorGUILayout.SelectableLabel(documentation, EditorStyles.wordWrappedLabel, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
}

        
        private string GetUniqueName(List<string> existingNames, string baseName)
        {
            if (!existingNames.Contains(baseName))
                return baseName;
            int counter = 1;
            string newName;
            do
            {
                newName = baseName + "_" + counter.ToString("00");
                counter++;
            } while (existingNames.Contains(newName));
            return newName;
        }
        
        private string GetUniqueFileName(List<string> existingFiles, string baseName, string extension)
        {
            if (string.IsNullOrWhiteSpace(extension) || !extension.StartsWith("."))
                extension = ".txt"; 

            string fullName = baseName + extension;
            if (!existingFiles.Contains(fullName))
                return fullName;
    
            int counter = 1;
            string newName;
            do
            {
                newName = baseName + "_" + counter.ToString("00") + extension;
                counter++;
            } while (existingFiles.Contains(newName));

            return newName;
        }

        private int GetSubfolderFileType(string subfolderName)
        {
            subfolderFileTypeSelections.TryAdd(subfolderName, 0);
            return subfolderFileTypeSelections[subfolderName];
        }

        private void SetSubfolderFileType(string subfolderName, int newValue)
        {
            subfolderFileTypeSelections[subfolderName] = newValue;
        }
        
        private void InitializeUIComponents()
        {
            directoriesList = new ReorderableList(templateConfig.directories, typeof(string), true, 
                true, true, true)
            { 
            drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Directories (Drag to Reorder)");
            },
            drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                templateConfig.directories[index] = EditorGUI.TextField(rect, templateConfig.directories[index]);
            },
            onAddCallback = (ReorderableList list) =>
            {
                string newName = GetUniqueName(templateConfig.directories, "NewFolder");
                templateConfig.directories.Add(newName);
                directoryFileTypeSelections.TryAdd(newName, 0);
            },
            onRemoveCallback = (ReorderableList list) =>
            {
                int index = list.index;
                if (index >= 0 && index < templateConfig.directories.Count)
                {
                    string directoryToRemove = templateConfig.directories[index];
                    templateConfig.directories.RemoveAt(index);
                    if (directoryFileTypeSelections.ContainsKey(directoryToRemove))
                        directoryFileTypeSelections.Remove(directoryToRemove);
                }
            } 
            };

            directoryFoldouts = new Dictionary<string, bool>();
            subfolderFoldouts = new Dictionary<string, bool>();
            subfoldersLists = new Dictionary<string, ReorderableList>();
            subfolderFilesLists = new Dictionary<string, ReorderableList>();

            foreach (var dir in templateConfig.directories)
            {
                directoryFoldouts.TryAdd(dir, true);
            
                if (templateConfig.subfolders.ContainsKey(dir))
                {
                    subfoldersLists[dir] = new ReorderableList(templateConfig.subfolders[dir], typeof(string),
                        true, true, true, true)
                    {
                        drawHeaderCallback = (Rect rect) =>
                        {
                            EditorGUI.LabelField(rect, "Subfolders (Drag to Reorder)");
                        },
                        drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                        {
                            templateConfig.subfolders[dir][index] = EditorGUI
                                .TextField(rect, templateConfig.subfolders[dir][index]);
                        },
                        onAddCallback = (ReorderableList list) =>
                        {
                            string newSubName = GetUniqueName(templateConfig.subfolders[dir], "NewSubfolder");
                            templateConfig.subfolders[dir].Add(newSubName);
                            subfolderFileTypeSelections.TryAdd($"{dir}/{newSubName}", 0);
                        }
                    };
            
                    foreach (var sub in templateConfig.subfolders[dir])
                    {
                        string subKey = $"{dir}/{sub}";
                        subfolderFoldouts.TryAdd(subKey, true);
            
                        if (templateConfig.subfolderFiles.ContainsKey(subKey))
                        {
                            subfolderFilesLists[subKey] = new ReorderableList(templateConfig.subfolderFiles[subKey],
                                typeof(string), true, true, true, true)
                            {
                                drawHeaderCallback = (Rect rect) =>
                                {
                                    EditorGUI.LabelField(rect, $"Files in {sub}");
                                },
                                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                                {
                                    templateConfig.subfolderFiles[subKey][index] = EditorGUI
                                        .TextField(rect, templateConfig.subfolderFiles[subKey][index]);
                                },
                                onAddCallback = (ReorderableList list) =>
                                {
                                    int fileTypeIndex = subfolderFileTypeSelections.ContainsKey(subKey) ? 
                                        subfolderFileTypeSelections[subKey] : 0;
                                    string extension = fileTypeOptions[fileTypeIndex];
                                    string newFileName = GetUniqueFileName(templateConfig.subfolderFiles[subKey], 
                                        "NewFile", extension);
                                    templateConfig.subfolderFiles[subKey].Add(newFileName);
                                }
                            };
                        }
                    }
                }
            }
        }
    }
}
#endif
