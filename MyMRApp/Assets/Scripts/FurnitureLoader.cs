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
