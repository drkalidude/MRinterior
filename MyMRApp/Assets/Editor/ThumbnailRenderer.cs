#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class ThumbnailGenerator : EditorWindow
{
    private Camera thumbnailCamera;
    private Light thumbnailLight;
    private RenderTexture renderTexture;
    private string exportPath = "Assets/Resources/Thumbnails";
    private int imageSize = 512;
    private Vector3 cameraOffset = new Vector3(0, 0, -3);
    private Vector3 lightRotation = new Vector3(50f, -30f, 0);

    [MenuItem("Tools/Furniture Thumbnail Generator")]
    static void ShowWindow()
    {
        GetWindow<ThumbnailGenerator>("Thumbnail Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Furniture Thumbnail Generator", EditorStyles.boldLabel);

        imageSize = EditorGUILayout.IntField("Image Size", imageSize);
        cameraOffset = EditorGUILayout.Vector3Field("Camera Offset", cameraOffset);
        lightRotation = EditorGUILayout.Vector3Field("Light Rotation", lightRotation);

        if (GUILayout.Button("Setup Scene"))
        {
            SetupThumbnailScene();
        }

        if (GUILayout.Button("Generate All Thumbnails"))
        {
            GenerateAllThumbnails();
        }

        if (GUILayout.Button("Clear Thumbnails"))
        {
            ClearThumbnails();
        }
    }

    void SetupThumbnailScene()
    {
        // Создаем камеру
        GameObject cameraGO = new GameObject("Thumbnail Camera");
        thumbnailCamera = cameraGO.AddComponent<Camera>();
        thumbnailCamera.clearFlags = CameraClearFlags.SolidColor;
        thumbnailCamera.backgroundColor = Color.clear;
        thumbnailCamera.orthographic = false;
        thumbnailCamera.fieldOfView = 60;
        thumbnailCamera.transform.position = cameraOffset;
        thumbnailCamera.transform.LookAt(Vector3.zero);

        // Создаем освещение
        GameObject lightGO = new GameObject("Thumbnail Light");
        thumbnailLight = lightGO.AddComponent<Light>();
        thumbnailLight.type = LightType.Directional;
        thumbnailLight.intensity = 1.5f;
        thumbnailLight.transform.rotation = Quaternion.Euler(lightRotation);

        // Создаем RenderTexture
        renderTexture = new RenderTexture(imageSize, imageSize, 24, RenderTextureFormat.ARGB32);
        thumbnailCamera.targetTexture = renderTexture;

        Debug.Log("Thumbnail scene setup complete!");
    }

    void GenerateAllThumbnails()
    {
        if (thumbnailCamera == null)
        {
            EditorUtility.DisplayDialog("Error", "Please setup scene first!", "OK");
            return;
        }

        string furniturePath = "Assets/Resources/Furniture";

        // Проверяем существование папки
        if (!Directory.Exists(furniturePath))
        {
            EditorUtility.DisplayDialog("Error",
                $"Folder not found: {furniturePath}\nPlease create the folder first!", "OK");
            return;
        }

        // Автоматически находим все папки (категории) в Furniture[1][2]
        string[] categoryPaths = Directory.GetDirectories(furniturePath);

        if (categoryPaths.Length == 0)
        {
            EditorUtility.DisplayDialog("Warning",
                "No categories found in Furniture folder!", "OK");
            return;
        }

        // Извлекаем только имена папок (без полного пути)[2]
        string[] categories = new string[categoryPaths.Length];
        for (int i = 0; i < categoryPaths.Length; i++)
        {
            categories[i] = Path.GetFileName(categoryPaths[i]);
        }

        // Показываем найденные категории в логе
        Debug.Log($"Found {categories.Length} categories: {string.Join(", ", categories)}");

        // Генерируем превью для каждой найденной категории
        foreach (string category in categories)
        {
            GenerateThumbnailsForCategory(category);
        }

        EditorUtility.DisplayDialog("Complete",
            $"Thumbnails generated for {categories.Length} categories:\n{string.Join(", ", categories)}", "OK");
    }

    /*
    void GenerateAllThumbnails()
    {
        if (thumbnailCamera == null)
        {
            EditorUtility.DisplayDialog("Error", "Please setup scene first!", "OK");
            return;
        }

        // Находим все префабы в папке Furniture
        string[] categories = { "Chairs", "Tables", "Sofas", "Lamps", "Decorations", "Rofl" };

        foreach (string category in categories)
        {
            GenerateThumbnailsForCategory(category);
        }

        EditorUtility.DisplayDialog("Complete", "All thumbnails generated successfully!", "OK");
    }*/

    void GenerateThumbnailsForCategory(string categoryName)
    {
        string prefabPath = $"Assets/Resources/Furniture/{categoryName}";

        if (!Directory.Exists(prefabPath))
        {
            Debug.LogWarning($"Category folder not found: {prefabPath}");
            return;
        }

        // Создаем папку для превью
        string thumbnailCategoryPath = Path.Combine(exportPath, categoryName);
        Directory.CreateDirectory(thumbnailCategoryPath);

        // Находим все префабы в категории
        string[] prefabGuids = AssetDatabase.FindAssets("t:GameObject", new[] { prefabPath });

        EditorUtility.DisplayProgressBar("Generating Thumbnails", $"Processing {categoryName}...", 0);

        for (int i = 0; i < prefabGuids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (prefab != null)
            {
                GenerateThumbnailForPrefab(prefab, thumbnailCategoryPath);

                float progress = (float)(i + 1) / prefabGuids.Length;
                EditorUtility.DisplayProgressBar("Generating Thumbnails",
                    $"Processing {categoryName}: {prefab.name}", progress);
            }
        }

        EditorUtility.ClearProgressBar();
        Debug.Log($"Generated thumbnails for category: {categoryName}");
    }

    void GenerateThumbnailForPrefab(GameObject prefab, string savePath)
    {
        // Создаем экземпляр объекта
        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

        // Позиционируем объект
        PositionObject(instance);

        // Рендерим
        thumbnailCamera.Render();

        // Сохраняем как PNG
        SaveRenderTexture(renderTexture, Path.Combine(savePath, prefab.name + ".png"));

        // Удаляем экземпляр
        DestroyImmediate(instance);
    }

    void PositionObject(GameObject obj)
    {
        // Центрируем объект
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        // Перемещаем объект в центр
        obj.transform.position = -bounds.center;

        // Настраиваем камеру под размер объекта
        float distance = bounds.size.magnitude * 1.2f;
        thumbnailCamera.transform.position = cameraOffset.normalized * distance;
        thumbnailCamera.transform.LookAt(Vector3.zero);
    }

    void SaveRenderTexture(RenderTexture rt, string path)
    {
        RenderTexture.active = rt;
        Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();

        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(path, bytes);

        DestroyImmediate(texture);
        RenderTexture.active = null;
    }

    void ClearThumbnails()
    {
        if (Directory.Exists(exportPath))
        {
            Directory.Delete(exportPath, true);
            AssetDatabase.Refresh();
            Debug.Log("All thumbnails cleared!");
        }
    }

    void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            DestroyImmediate(renderTexture);
        }
    }
}
#endif
