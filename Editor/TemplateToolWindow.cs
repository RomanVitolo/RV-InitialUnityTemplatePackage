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

        private readonly List<int> directoryFileTypeSelections = new List<int>();
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
            for (int i = 0; i < templateConfig.directories.Count; i++)
                directoryFileTypeSelections.Add(0);

            directoriesList = new ReorderableList(templateConfig.directories, typeof(string), true, true, true, true)
            {
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "Directories (Drag to Reorder)");
                },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    if (index >= 0 && index < templateConfig.directories.Count)
                        templateConfig.directories[index] = EditorGUI.TextField(rect, templateConfig.directories[index]);
                },
                onAddCallback = (ReorderableList list) =>
                {
                    string newName = GetUniqueName(templateConfig.directories, "NewFolder");
                    templateConfig.directories.Add(newName);
                    directoryFileTypeSelections.Add(0);
                },
                onRemoveCallback = (ReorderableList list) =>
                {
                    int index = list.index;
                    if (index >= 0 && index < templateConfig.directories.Count)
                    {
                        templateConfig.directories.RemoveAt(index);
                        directoryFileTypeSelections.RemoveAt(index);
                    }
                }
            };
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
                    if (i >= directoryFileTypeSelections.Count) 
                        directoryFileTypeSelections.Add(0);
        
                    string dirName = templateConfig.directories[i];
        
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
        
                    // Subfolders Section
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
        
                            // File Type Selection for Subfolder
                            EditorGUILayout.BeginHorizontal();
                            GUI.color = Color.yellow;
                            EditorGUILayout.LabelField("File Type:", GUILayout.Width(100));
                            GUI.color = Color.white;
                            int newSubType = EditorGUILayout.Popup(GetSubfolderFileType(subName), fileTypeOptions, GUILayout.Width(80));
                            SetSubfolderFileType(subName, newSubType);
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
                                        string newFileName = GetUniqueFileName(templateConfig.subfolderFiles[subKey], "NewFile", fileTypeOptions[newSubType]);
                                        templateConfig.subfolderFiles[subKey].Add(newFileName);
                                    }
                                };
                            }
        
                            subfolderFilesLists[subKey].DoLayoutList();
                            EditorGUILayout.EndVertical(); // End Subfolder Section
                        }
                    }
        
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    GUI.color = Color.yellow;
                    GUILayout.Label("Files in Directory", EditorStyles.boldLabel);
                    GUI.color = Color.white;
                    
                    if (!templateConfig.dirFiles.ContainsKey(dirName))
                        templateConfig.dirFiles[dirName] = new List<string>();
                    
                    while (directoryFileTypeSelections.Count < templateConfig.directories.Count)
                        directoryFileTypeSelections.Add(0);
                    while (directoryFileTypeSelections.Count > templateConfig.directories.Count)
                        directoryFileTypeSelections.RemoveAt(directoryFileTypeSelections.Count - 1);
                    
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
                        int fileTypeIndex = directoryFileTypeSelections[currentDirIndex];
                        string newFileName = GetUniqueFileName(templateConfig.dirFiles[dirName], "NewFile", fileTypeOptions[fileTypeIndex]);
                        templateConfig.dirFiles[dirName].Add(newFileName);
                    }
                    };
                    }
        
                    subfolderFilesLists[dirName].DoLayoutList();
        
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Directory File Type:", GUILayout.Width(120));
                    directoryFileTypeSelections[currentDirIndex] = EditorGUILayout.Popup(directoryFileTypeSelections[currentDirIndex], fileTypeOptions, GUILayout.Width(65));
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
                    if (templateConfig.dirFiles.TryGetValue(dir, value: out var dirFile))
                    {
                        foreach (string file in dirFile)
                        {
                            EditorGUILayout.LabelField("File: " + file);
                        }
                    }
                    if (templateConfig.subfolders.TryGetValue(dir, value: out var subfolder))
                    {
                        foreach (string sub in subfolder)
                        {
                            string subKey = dir + "_" + sub;
                            subfolderFoldouts.TryAdd(subKey, true);
                            subfolderFoldouts[subKey] = EditorGUILayout.Foldout(subfolderFoldouts[subKey], "Subfolder: " + sub);
                            if (subfolderFoldouts[subKey])
                            {
                                EditorGUI.indentLevel++;
                                if (templateConfig.subfolderFiles.TryGetValue(sub, value: out var file))
                                {
                                    foreach (string sfile in file)
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
                "This tool helps you create a predefined folder and file structure in your Unity project. Follow these steps:\n\n" +
                "1. Set the 'Main Root Name' which will be the parent folder for your template.\n\n" +
                "2. In the 'Directories' section, add and modify directory names. You can change the order by dragging them in the list above. " +
                "Each directory can include its own subfolders and files. Use the '+ SubFolder' button to add subfolders and the 'Create File' button (with the dropdown for file type) to add files.\n\n" +
                "3. In the 'Root Files' section, create files that will be placed directly under the Main Root. " +
                "Select the desired file type from the dropdown and click 'Create File'.\n\n" +
                "4. Click 'Create Template Resources' to generate the folders and files in your project.\n\n" +
                "5. Save your current configuration using the 'Save Configuration' button. This exports your settings as a JSON file, " +
                "allowing you to reload them later using the 'Load Configuration' button if changes are lost or you wish to reuse a configuration.\n\n" +
                "About the author:\n" +
                "Name: Roman Vitolo\n" +
                "Web page link: https://romanvitolo.com\n" +
                "Repository link: https://github.com/RomanVitolo\n\n" +
                "For any doubt, error, or feedback you can contact me via email (found on my web page).";
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
                directoriesList = new ReorderableList(templateConfig.directories, typeof(string), true, true, true, true)
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
                    directoryFileTypeSelections.Add(0);
                },
                onRemoveCallback = (ReorderableList list) =>
                {
                    int index = list.index;
                    if (index >= 0 && index < templateConfig.directories.Count)
                    {
                        templateConfig.directories.RemoveAt(index);
                        directoryFileTypeSelections.RemoveAt(index);
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
                    subfoldersLists[dir] = new ReorderableList(templateConfig.subfolders[dir], typeof(string), true, true, true, true)
                    {
                        drawHeaderCallback = (Rect rect) =>
                        {
                            EditorGUI.LabelField(rect, "Subfolders (Drag to Reorder)");
                        },
                        drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                        {
                            templateConfig.subfolders[dir][index] = EditorGUI.TextField(rect, templateConfig.subfolders[dir][index]);
                        },
                        onAddCallback = (ReorderableList list) =>
                        {
                            string newSubName = GetUniqueName(templateConfig.subfolders[dir], "NewSubfolder");
                            templateConfig.subfolders[dir].Add(newSubName);
                            subfolderFileTypeSelections.TryAdd(newSubName, 0);
                        }
                    };
                    
                    foreach (var sub in templateConfig.subfolders[dir])
                    {
                        string subKey = $"{dir}/{sub}";
                        subfolderFoldouts.TryAdd(subKey, true);
            
                        if (templateConfig.subfolderFiles.ContainsKey(subKey))
                        {
                            subfolderFilesLists[subKey] = new ReorderableList(templateConfig.subfolderFiles[subKey], typeof(string), true, true, true, true)
                            {
                                drawHeaderCallback = (Rect rect) =>
                                {
                                    EditorGUI.LabelField(rect, $"Files in {sub}");
                                },
                                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                                {
                                    templateConfig.subfolderFiles[subKey][index] = EditorGUI.TextField(rect, templateConfig.subfolderFiles[subKey][index]);
                                },
                                onAddCallback = (ReorderableList list) =>
                                {
                                    int fileTypeIndex = GetSubfolderFileType(sub);
                                    string newFileName = GetUniqueFileName(templateConfig.subfolderFiles[subKey], "NewFile", fileTypeOptions[fileTypeIndex]);
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
