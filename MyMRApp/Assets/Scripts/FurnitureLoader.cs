using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FurnitureLoader : MonoBehaviour
{
    [Header("UI")]
    public GameObject categoryPanel;
    public GameObject furniturePanel;
    public Transform furnitureButtonsParent;
    public GameObject furnitureButtonPrefab;

    [Header("Editor Reference")]
    public VRObjectEditor objectEditor;

    private List<GameObject> currentLoadedPrefabs = new List<GameObject>();

    void Start()
    {
        categoryPanel.SetActive(true);
        furniturePanel.SetActive(false);
        Debug.Log("FurnitureLoader initialized");
    }

    public void ShowFurnitureForCategory(string categoryName)
    {
        Debug.Log($"ShowFurnitureForCategory called with category: {categoryName}");

        // Проверяем, что панели назначены
        if (categoryPanel == null)
        {
            Debug.LogError("categoryPanel is null!");
            return;
        }

        if (furniturePanel == null)
        {
            Debug.LogError("furniturePanel is null!");
            return;
        }

        Debug.Log("Switching panels...");
        categoryPanel.SetActive(false);
        furniturePanel.SetActive(true);

        Debug.Log($"Category panel active: {categoryPanel.activeInHierarchy}");
        Debug.Log($"Furniture panel active: {furniturePanel.activeInHierarchy}");

        if (objectEditor != null)
        {
            objectEditor.ClearSelection();
        }

        // Очищаем старые кнопки
        foreach (Transform c in furnitureButtonsParent)
            Destroy(c.gameObject);
        currentLoadedPrefabs.Clear();

        // Загружаем префабы и миниатюры
        var prefabs = Resources.LoadAll<GameObject>($"Furniture/{categoryName}");
        var thumbnails = Resources.LoadAll<Sprite>($"Thumbnails/{categoryName}");

        Debug.Log($"Loaded {prefabs.Length} prefabs and {thumbnails.Length} thumbnails for {categoryName}");

        if (prefabs.Length == 0)
        {
            Debug.LogWarning($"No prefabs found in Resources/Furniture/{categoryName}");
            return;
        }

        // Создаем кнопки
        for (int i = 0; i < prefabs.Length; i++)
        {
            var prefab = prefabs[i];
            currentLoadedPrefabs.Add(prefab);

            var btn = Instantiate(furnitureButtonPrefab, furnitureButtonsParent, false);

            // Показываем имя префаба вместо удаления текста
            Text txt = btn.GetComponentInChildren<Text>();
            if (txt != null)
            {
                txt.text = prefab.name;
            }

            Image previewImage = btn.GetComponent<Image>();
            if (previewImage == null) previewImage = btn.AddComponent<Image>();

            if (i < thumbnails.Length)
            {
                previewImage.sprite = thumbnails[i];
            }

            int idx = i;
            Button buttonComponent = btn.GetComponent<Button>();
            if (buttonComponent != null)
            {
                buttonComponent.onClick.AddListener(() => {
                    Debug.Log($"Furniture button clicked for: {prefab.name}");
                    if (objectEditor != null)
                    {
                        objectEditor.SelectPrefabByIndex(idx, currentLoadedPrefabs);
                    }
                    else
                    {
                        Debug.LogError("objectEditor is null!");
                    }
                });
            }
            else
            {
                Debug.LogError("Button component not found on furniture button prefab!");
            }
        }

        Debug.Log($"Successfully created {prefabs.Length} furniture buttons");
    }

    public void ReturnToCategories()
    {
        Debug.Log("ReturnToCategories called");

        furniturePanel.SetActive(false);
        categoryPanel.SetActive(true);

        foreach (Transform c in furnitureButtonsParent)
            Destroy(c.gameObject);
        currentLoadedPrefabs.Clear();

        if (objectEditor != null)
        {
            objectEditor.ClearSelection();
        }
    }
}

/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FurnitureLoader : MonoBehaviour
{
    [Header("UI")]
    public GameObject categoryPanel;
    public GameObject furniturePanel;
    public Transform furnitureButtonsParent;
    public GameObject furnitureButtonPrefab;
    private string loadPath;

    [Header("Editor Reference")]
    public VRObjectEditor objectEditor;

    private List<GameObject> currentLoadedPrefabs = new List<GameObject>();

    void Start()
    {
        categoryPanel.SetActive(true);
        furniturePanel.SetActive(false);
    }

    public void ShowFurnitureForCategory(string categoryName)
    {
        categoryPanel.SetActive(false);
        furniturePanel.SetActive(true);
        objectEditor.ClearSelection();

        foreach (Transform c in furnitureButtonsParent)
            Destroy(c.gameObject);
        currentLoadedPrefabs.Clear();

        var prefabs = Resources.LoadAll<GameObject>($"Furniture/{categoryName}");

        var thumbnails = Resources.LoadAll<Sprite>($"Thumbnails/{categoryName}");

        for (int i = 0; i < prefabs.Length; i++)
        {
            var prefab = prefabs[i];
            currentLoadedPrefabs.Add(prefab);

            var btn = Instantiate(furnitureButtonPrefab, furnitureButtonsParent, false);

            Text txt = btn.GetComponentInChildren<Text>();
            if (txt != null) Destroy(txt.gameObject);

            Image previewImage = btn.GetComponent<Image>();
            if (previewImage == null) previewImage = btn.AddComponent<Image>();

            if (i < thumbnails.Length)
            {
                previewImage.sprite = thumbnails[i];
            }

            int idx = i;
            btn.GetComponent<Button>().onClick.AddListener(() =>
                objectEditor.SelectPrefabByIndex(idx, currentLoadedPrefabs));
        }
    }

    public void ReturnToCategories()
    {
        furniturePanel.SetActive(false);
        categoryPanel.SetActive(true);

        foreach (Transform c in furnitureButtonsParent)
            Destroy(c.gameObject);
        currentLoadedPrefabs.Clear();
        objectEditor.ClearSelection();
    }
}
*/