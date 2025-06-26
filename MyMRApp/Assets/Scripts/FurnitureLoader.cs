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
    public string loadPath;

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

        loadPath = $"Furniture/Prefab/{categoryName}";
        var prefabs = Resources.LoadAll<GameObject>(loadPath);

        for (int i = 0; i < prefabs.Length; i++)
        {
            var prefab = prefabs[i];
            currentLoadedPrefabs.Add(prefab);

            var btn = Instantiate(furnitureButtonPrefab, furnitureButtonsParent, false);
            var txt = btn.GetComponentInChildren<Text>();

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
