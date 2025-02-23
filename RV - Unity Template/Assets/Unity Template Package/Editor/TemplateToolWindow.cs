using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace Unity_Template_Package.Editor
{
    public class TemplateToolWindow : EditorWindow
    {
        private int selectedTab = 0;
        private readonly string[] tabNames = new string[] { "Template Tool", "Documentation" };

        private TemplateConfiguration templateConfig = new TemplateConfiguration();
        private Vector2 toolScrollPosition;
        private Vector2 docScrollPosition;
       
        private int selectedFileTypeIndex = 0;
        
        private int selectedRootFileTypeIndex = 0;
        private readonly string[] fileTypeOptions = new string[] { ".cs", ".txt", ".md", ".unity", ".json", ".mat" };
        
        private readonly ITemplateResourceCreator resourceCreator = new TemplateResourceCreator();
        
        private ReorderableList directoriesList;

        [MenuItem("RV - Template Tool/Open Template Tool")]
        public static void ShowWindow()
        {
            GetWindow<TemplateToolWindow>("RV - Template Tool");
        }

        private void OnEnable()
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
                    onRemoveCallback = (ReorderableList list) =>
                    {
                        templateConfig.directories.RemoveAt(list.index);
                    }
                };
        }

        private void OnGUI()
        {
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames);

            if (selectedTab == 0)
                DrawToolTab();
            else if (selectedTab == 1)
                DrawDocumentationTab();
        }

        private void DrawToolTab()
        {
            toolScrollPosition = EditorGUILayout.BeginScrollView(toolScrollPosition);
            GUILayout.Label("Template Folder Setup", EditorStyles.boldLabel);

            templateConfig.mainRoot = EditorGUILayout.TextField("Main Root Name", templateConfig.mainRoot);

            // Use the ReorderableList to allow drag-and-drop reordering of directories.
            directoriesList.DoLayoutList();

            // Now draw the details for each directory (subfolders, files, etc.)
            foreach (var t in templateConfig.directories)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Label("Directory: " + t, EditorStyles.boldLabel);

                // Ensure subfolders and directory files lists exist.
                if (!templateConfig.subfolders.ContainsKey(t))
                    templateConfig.subfolders[t] = new List<string>();
                if (!templateConfig.dirFiles.ContainsKey(t))
                    templateConfig.dirFiles[t] = new List<string>();

                GUILayout.Label("Subfolders:");
                List<string> subfolderCopy = new List<string>(templateConfig.subfolders[t]);
                string subfolderToRemove = null;
                for (int j = 0; j < subfolderCopy.Count; j++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    templateConfig.subfolders[t][j] = EditorGUILayout.TextField(templateConfig.subfolders[t][j]);
                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                        subfolderToRemove = subfolderCopy[j];
                    GUI.backgroundColor = Color.white;
                    GUILayout.EndHorizontal();

                    if (!templateConfig.subfolderFiles.ContainsKey(templateConfig.subfolders[t][j]))
                        templateConfig.subfolderFiles[templateConfig.subfolders[t][j]] = new List<string>();

                    GUILayout.Label("    SubFolder Files:");
                    List<string> subfolderFileCopy = new List<string>(templateConfig.subfolderFiles[templateConfig.subfolders[t][j]]);
                    string subfolderFileToRemove = null;
                    for (int k = 0; k < subfolderFileCopy.Count; k++)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(40);
                        templateConfig.subfolderFiles[templateConfig.subfolders[t][j]][k] = EditorGUILayout.TextField(templateConfig.subfolderFiles[templateConfig.subfolders[t][j]][k]);
                        GUI.backgroundColor = Color.red;
                        if (GUILayout.Button("X", GUILayout.Width(20)))
                            subfolderFileToRemove = subfolderFileCopy[k];
                        GUI.backgroundColor = Color.white;
                        GUILayout.EndHorizontal();
                    }
                    if (subfolderFileToRemove != null)
                        templateConfig.subfolderFiles[templateConfig.subfolders[t][j]].Remove(subfolderFileToRemove);

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(40);
                    GUI.backgroundColor = Color.yellow;
                    if (GUILayout.Button("Create File", GUILayout.Width(100)))
                    {
                        string newFileName = "NewFile" + fileTypeOptions[selectedFileTypeIndex];
                        templateConfig.subfolderFiles[templateConfig.subfolders[t][j]].Add(newFileName);
                    }
                    GUI.backgroundColor = Color.white;
                    selectedFileTypeIndex = EditorGUILayout.Popup(selectedFileTypeIndex, fileTypeOptions, GUILayout.Width(65));
                    GUILayout.EndHorizontal();
                }
                if (subfolderToRemove != null)
                {
                    templateConfig.subfolders[t].Remove(subfolderToRemove);
                    templateConfig.subfolderFiles.Remove(subfolderToRemove);
                }
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUI.backgroundColor = Color.cyan;
                if (GUILayout.Button("+ SubFolder", GUILayout.Width(100)))
                    templateConfig.subfolders[t].Add("NewSubfolder");
                GUI.backgroundColor = Color.white;
                GUILayout.EndHorizontal();

                GUILayout.Label("Directory Files:");
                List<string> fileCopy = new List<string>(templateConfig.dirFiles[t]);
                string fileToRemove = null;
                for (int j = 0; j < fileCopy.Count; j++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    templateConfig.dirFiles[t][j] = EditorGUILayout.TextField(templateConfig.dirFiles[t][j]);
                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                        fileToRemove = fileCopy[j];
                    GUI.backgroundColor = Color.white;
                    GUILayout.EndHorizontal();
                }
                if (fileToRemove != null)
                    templateConfig.dirFiles[t].Remove(fileToRemove);

                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUI.backgroundColor = Color.yellow;
                if (GUILayout.Button("Create File", GUILayout.Width(100)))
                {
                    string newFileName = "NewFile" + fileTypeOptions[selectedFileTypeIndex];
                    templateConfig.dirFiles[t].Add(newFileName);
                }
                GUI.backgroundColor = Color.white;
                selectedFileTypeIndex = EditorGUILayout.Popup(selectedFileTypeIndex, fileTypeOptions, GUILayout.Width(65));
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();

                GUILayout.Space(10);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                GUILayout.Space(10);
            }

            GUILayout.BeginVertical("box");
            GUILayout.Label("Root Files", EditorStyles.boldLabel);
            List<string> rootFileCopy = new List<string>(templateConfig.files);
            string rootFileToRemove = null;
            for (int i = 0; i < rootFileCopy.Count; i++)
            {
                GUILayout.BeginHorizontal();
                templateConfig.files[i] = EditorGUILayout.TextField(templateConfig.files[i]);
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("X", GUILayout.Width(20)))
                    rootFileToRemove = rootFileCopy[i];
                GUI.backgroundColor = Color.white;
                GUILayout.EndHorizontal();
            }
            if (rootFileToRemove != null)
                templateConfig.files.Remove(rootFileToRemove);
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("Create File", GUILayout.Width(100)))
            {
                string newFileName = "NewFile" + fileTypeOptions[selectedRootFileTypeIndex];
                templateConfig.files.Add(newFileName);
            }
            GUI.backgroundColor = Color.white;
            selectedRootFileTypeIndex = EditorGUILayout.Popup(selectedRootFileTypeIndex, fileTypeOptions, GUILayout.Width(100));
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(10);

            GUI.backgroundColor = Color.white;
            if (GUILayout.Button("Add Directory"))
                templateConfig.directories.Add("NewFolder");
            GUI.backgroundColor = Color.white;

            GUI.backgroundColor = Color.white;
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
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Configuration", GUILayout.Width(200)))
            {
                string path = EditorUtility.SaveFilePanel("Save Template Configuration", Application.dataPath, "TemplateConfig", "json");
                if (!string.IsNullOrEmpty(path))
                {
                    if (TemplateConfigurationSerializer.SaveConfiguration(templateConfig, path))
                        EditorUtility.DisplayDialog("Save Configuration", "Configuration saved successfully!", "OK");
                }
            }
            if (GUILayout.Button("Load Configuration", GUILayout.Width(200)))
            {
                string path = EditorUtility.OpenFilePanel("Load Template Configuration", Application.dataPath, "json");
                if (!string.IsNullOrEmpty(path))
                {
                    TemplateConfiguration loadedConfig = TemplateConfigurationSerializer.LoadConfiguration(path);
                    if (loadedConfig != null)
                    {
                        templateConfig = loadedConfig;
                        Repaint();
                        EditorUtility.DisplayDialog("Load Configuration", "Configuration loaded successfully!", "OK");
                    }
                }
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();
        }

        private void DrawDocumentationTab()
        {
            docScrollPosition = EditorGUILayout.BeginScrollView(docScrollPosition);
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Documentation", EditorStyles.boldLabel);
        
            string documentation =
            "This tool helps you create a predefined folder and file structure in your Unity project. Follow these steps:\n\n" +
            "1. Set the 'Main Root Name' which will be the parent folder for your template.\n\n" +
            "2. In the 'Directories' section, add and modify directory names. Each directory can include its own subfolders and files. " +
            "You can change the order by dragging them in the list above. Use the '+ SubFolder' button to add subfolders and the 'Create File' button (with the dropdown for file type) to add files.\n\n" +
            "3. In the 'Root Files' section, create files that will be placed directly under the Main Root. " +
            "Select the desired file type from the dropdown and click 'Create File'.\n\n" +
            "4. Click 'Create Template Resources' to generate the folders and files in your project.\n\n" +
            "5. Save your current configuration using 'Save Configuration' so you can reload it later with 'Load Configuration' if needed.\n\n" +
            "Note: New files are created with the default name 'NewFile' followed by the selected extension. You can rename them later in your project.\n\n" +
            "About the author:\n" +
            "Name: Roman Vitolo\n" +
            "Web page link: https://romanvitolo.com\n" +
            "Repository link: https://github.com/RomanVitolo\n\n" +
            "For any doubt, error, or feedback, please contact me via email (you can find it on my web page).";
        
            GUIStyle docStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                fontSize = Mathf.Clamp((int)(position.width / 50), 10, 20)
            };
        
            EditorGUILayout.SelectableLabel(documentation, docStyle, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }
    }
}
