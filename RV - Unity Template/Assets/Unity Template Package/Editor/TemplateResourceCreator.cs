using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity_Template_Package.Editor
{
    public class TemplateResourceCreator : ITemplateResourceCreator
    {
        public void CreateResources(TemplateConfiguration config)
        {
            try
            {
                string mainPath = Path.Combine(Application.dataPath, config.mainRoot);
                if (!Directory.Exists(mainPath))
                {
                    Directory.CreateDirectory(mainPath);
                }
                AssetDatabase.Refresh();
              
                foreach (var dir in config.directories)
                {
                    string dirFullPath = Path.Combine(mainPath, dir);
                    if (!Directory.Exists(dirFullPath))
                    {
                        Directory.CreateDirectory(dirFullPath);
                    }
                    AssetDatabase.Refresh();
                  
                    if (config.subfolders.ContainsKey(dir))
                    {
                        foreach (var subfolder in new List<string>(config.subfolders[dir]))
                        {
                            string subfolderFullPath = Path.Combine(dirFullPath, subfolder);
                            if (!Directory.Exists(subfolderFullPath))
                            {
                                Directory.CreateDirectory(subfolderFullPath);
                            }
                            AssetDatabase.Refresh();
                           
                            if (config.subfolderFiles.TryGetValue(subfolder, out var subfolderFile))
                            {
                                foreach (var file in new List<string>(subfolderFile))
                                {
                                    string filePath = Path.Combine("Assets", config.mainRoot, dir, subfolder, file);
                                    if (!File.Exists(filePath))
                                    {
                                        CreateFile(filePath, file);
                                    }
                                }
                            }
                        }
                    }
                   
                    if (config.dirFiles.TryGetValue(dir, out var dirFile))
                    {
                        foreach (var file in new List<string>(dirFile))
                        {
                            string filePath = Path.Combine("Assets", config.mainRoot, dir, file);
                            if (!File.Exists(filePath))
                            {
                                CreateFile(filePath, file);
                            }
                        }
                    }
                }
                foreach (var file in config.files)
                {
                    string filePath = Path.Combine("Assets", config.mainRoot, file);
                    if (!File.Exists(filePath))
                    {
                        CreateFile(filePath, file);
                    }
                }
                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error Creating Resources", ex.Message, "OK");
            }
        }

        private void CreateFile(string filePath, string fileName)
        {
            try
            {
                if (fileName.EndsWith(".unity"))
                {
                    CreateTemplateScene(filePath);
                }
                else if (fileName.EndsWith(".cs"))
                {
                    string className = System.IO.Path.GetFileNameWithoutExtension(fileName);
                    string scriptContent =
$@"using UnityEngine;

public class {className} : MonoBehaviour
{{
    void Start() {{}}
    void Update() {{}}
}}";
                    File.WriteAllText(filePath, scriptContent);
                }
                else if (fileName.EndsWith(".mat"))
                {
                    Material newMat = new Material(Shader.Find("Standard"));
                    AssetDatabase.CreateAsset(newMat, filePath);
                }
                else
                {
                    File.WriteAllText(filePath, "");
                }
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error Creating File", $"Failed to create file '{fileName}': {ex.Message}", "OK");
            }
        }

        private void CreateTemplateScene(string scenePath)
        {
            try
            {
                Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

                GameObject camera = new GameObject("Main Camera");
                SceneManager.MoveGameObjectToScene(camera, newScene);
                Camera camComponent = camera.AddComponent<Camera>();
                camComponent.clearFlags = CameraClearFlags.Skybox;
                camera.transform.position = new Vector3(0, 1, -10);

                GameObject light = new GameObject("Directional Light");
                SceneManager.MoveGameObjectToScene(light, newScene);
                Light lightComponent = light.AddComponent<Light>();
                lightComponent.type = LightType.Directional;
                light.transform.rotation = Quaternion.Euler(50, -30, 0);

                GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                SceneManager.MoveGameObjectToScene(ground, newScene);
                ground.name = "Ground";
                ground.transform.localScale = new Vector3(10, 1, 10);

                EditorSceneManager.SaveScene(newScene, scenePath);
                EditorSceneManager.CloseScene(newScene, true);
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error Creating Scene", ex.Message, "OK");
            }
        }
    }
}
