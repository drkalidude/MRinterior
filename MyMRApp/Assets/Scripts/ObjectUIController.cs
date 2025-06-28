using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class ObjectUIController : MonoBehaviour
{
    [Header("VR Setup")]
    public XRRayInteractor rightRay;
    public InputActionReference selectAction;

    [Header("Main Canvas")]
    public Canvas objectToolsCanvas;
    public GameObject mainPanel;

    [Header("Panel System")]
    public GameObject menuPanel;
    public GameObject scalePanel;
    public GameObject rotationPanel;
    public GameObject positionPanel;
    public GameObject colorPanel;

    [Header("Menu Buttons")]
    public Button scaleMenuButton;
    public Button rotationMenuButton;
    public Button positionMenuButton;
    public Button colorMenuButton;
    public Button closeUIButton;

    [Header("Scale Panel Controls")]
    public Button scaleUpButton;
    public Button scaleDownButton;
    public Button resetScaleButton;
    public Button scaleBackButton;

    [Header("Rotation Panel Controls")]
    public Button rotateLeftButton;
    public Button rotateRightButton;
    public Button resetRotationButton;
    public Button rotationBackButton;

    [Header("Position Panel Controls")]
    public Button moveUpButton;
    public Button moveDownButton;
    public Button moveLeftButton;
    public Button moveRightButton;
    public Button positionBackButton;

    [Header("Color Panel Controls")]
    public Button redColorButton;
    public Button greenColorButton;
    public Button blueColorButton;
    public Button randomColorButton;
    public Button colorBackButton;

    [Header("Settings")]
    public float heightOffset = 1.5f;
    public float scaleStep = 0.2f;
    public float rotateStep = 45f;
    public float moveStep = 0.3f;

    private GameObject selectedObject;
    private bool isUIVisible = false;
    private Dictionary<string, GameObject> panels;
    private string currentPanelName = "menu";

    void Start()
    {
        InitializePanelSystem();
        SetupButtonListeners();
        HideUI();
    }

    void InitializePanelSystem()
    {
        // Создаем словарь панелей[1]
        panels = new Dictionary<string, GameObject>
        {
            {"menu", menuPanel},
            {"scale", scalePanel},
            {"rotation", rotationPanel},
            {"position", positionPanel},
            {"color", colorPanel}
        };

        // Скрываем все панели кроме меню
        ShowPanel("menu");
    }

    void SetupButtonListeners()
    {
        // Кнопки главного меню[1]
        scaleMenuButton.onClick.AddListener(() => ShowPanel("scale"));
        rotationMenuButton.onClick.AddListener(() => ShowPanel("rotation"));
        positionMenuButton.onClick.AddListener(() => ShowPanel("position"));
        colorMenuButton.onClick.AddListener(() => ShowPanel("color"));
        closeUIButton.onClick.AddListener(HideUI);

        // Кнопки панели масштаба
        scaleUpButton.onClick.AddListener(() => ScaleObject(1 + scaleStep));
        scaleDownButton.onClick.AddListener(() => ScaleObject(1 - scaleStep));
        resetScaleButton.onClick.AddListener(ResetScale);
        scaleBackButton.onClick.AddListener(() => ShowPanel("menu"));

        // Кнопки панели поворота
        rotateLeftButton.onClick.AddListener(() => RotateObject(-rotateStep));
        rotateRightButton.onClick.AddListener(() => RotateObject(rotateStep));
        resetRotationButton.onClick.AddListener(ResetRotation);
        rotationBackButton.onClick.AddListener(() => ShowPanel("menu"));

        // Кнопки панели позиционирования
        moveUpButton.onClick.AddListener(() => MoveObject(Vector3.up * moveStep));
        moveDownButton.onClick.AddListener(() => MoveObject(Vector3.down * moveStep));
        moveLeftButton.onClick.AddListener(() => MoveObject(Vector3.left * moveStep));
        moveRightButton.onClick.AddListener(() => MoveObject(Vector3.right * moveStep));
        positionBackButton.onClick.AddListener(() => ShowPanel("menu"));

        // Кнопки панели цветов
        redColorButton.onClick.AddListener(() => ChangeObjectColor(Color.red));
        greenColorButton.onClick.AddListener(() => ChangeObjectColor(Color.green));
        blueColorButton.onClick.AddListener(() => ChangeObjectColor(Color.blue));
        randomColorButton.onClick.AddListener(RandomObjectColor);
        colorBackButton.onClick.AddListener(() => ShowPanel("menu"));
    }

    void Update()
    {
        HandleObjectSelection();

        if (isUIVisible && selectedObject != null)
        {
            UpdateUIPosition();
            KeepUIFacingCamera();
        }
    }

    void ShowPanel(string panelName)
    {
        // Скрыть текущую панель[1]
        if (panels.ContainsKey(currentPanelName))
        {
            panels[currentPanelName].SetActive(false);
        }

        // Показать выбранную панель[1]
        if (panels.ContainsKey(panelName))
        {
            panels[panelName].SetActive(true);
            currentPanelName = panelName;
            Debug.Log($"[UI] Показана панель: {panelName}");
        }
    }

    void HandleObjectSelection()
    {
        if (selectAction.action.WasPressedThisFrame())
        {
            if (rightRay.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                GameObject hitObject = hit.collider.gameObject;

                if (IsValidObject(hitObject))
                {
                    if (selectedObject == hitObject && isUIVisible)
                    {
                        HideUI();
                    }
                    else
                    {
                        SelectObjectAndShowUI(hitObject);
                    }
                }
            }
            else if (isUIVisible)
            {
                HideUI();
            }
        }
    }

    bool IsValidObject(GameObject obj)
    {
        return !obj.CompareTag("UI") &&
               obj.GetComponent<Canvas>() == null &&
               obj.GetComponent<Renderer>() != null;
    }

    void SelectObjectAndShowUI(GameObject obj)
    {
        selectedObject = obj;
        UpdateUIPosition();

        mainPanel.SetActive(true);
        ShowPanel("menu"); // Всегда начинаем с главного меню
        isUIVisible = true;

        Debug.Log($"[UI] Выбран объект: {obj.name}");
    }

    void HideUI()
    {
        mainPanel.SetActive(false);
        isUIVisible = false;
        selectedObject = null;
        currentPanelName = "menu";

        Debug.Log("[UI] Панель скрыта");
    }

    void UpdateUIPosition()
    {
        if (selectedObject == null) return;

        Bounds bounds = GetObjectBounds(selectedObject);
        Vector3 uiPosition = bounds.center + Vector3.up * (bounds.extents.y + heightOffset);
        objectToolsCanvas.transform.position = uiPosition;
    }

    void KeepUIFacingCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            objectToolsCanvas.transform.LookAt(mainCamera.transform);
            objectToolsCanvas.transform.Rotate(0, 180, 0);
        }
    }

    Bounds GetObjectBounds(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        return renderer != null ? renderer.bounds : new Bounds(obj.transform.position, Vector3.one);
    }

    // === ФУНКЦИИ УПРАВЛЕНИЯ ОБЪЕКТОМ === //

    void ScaleObject(float scaleFactor)
    {
        if (selectedObject != null)
        {
            selectedObject.transform.localScale *= scaleFactor;
            Debug.Log($"[Scale] Новый масштаб: {selectedObject.transform.localScale}");
        }
    }

    void ResetScale()
    {
        if (selectedObject != null)
        {
            selectedObject.transform.localScale = Vector3.one;
            Debug.Log("[Scale] Масштаб сброшен");
        }
    }

    void RotateObject(float angle)
    {
        if (selectedObject != null)
        {
            selectedObject.transform.Rotate(0, angle, 0, Space.World);
            Debug.Log($"[Rotation] Поворот на: {angle}°");
        }
    }

    void ResetRotation()
    {
        if (selectedObject != null)
        {
            selectedObject.transform.rotation = Quaternion.identity;
            Debug.Log("[Rotation] Поворот сброшен");
        }
    }

    void MoveObject(Vector3 direction)
    {
        if (selectedObject != null)
        {
            selectedObject.transform.position += direction;
            Debug.Log($"[Position] Перемещение на: {direction}");
        }
    }

    void ChangeObjectColor(Color color)
    {
        if (selectedObject != null)
        {
            Renderer renderer = selectedObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
                Debug.Log($"[Color] Цвет изменен на: {color}");
            }
        }
    }

    void RandomObjectColor()
    {
        Color randomColor = new Color(Random.value, Random.value, Random.value);
        ChangeObjectColor(randomColor);
    }

    /*
    [Header("VR Setup")]
    public XRRayInteractor rightRay;
    public XRRayInteractor leftRay;
    public InputActionReference selectAction;

    [Header("UI Canvas")]
    public Canvas objectToolsCanvas;
    public GameObject uiPanel;

    [Header("UI Buttons")]
    public Button scaleUpButton;
    public Button scaleDownButton;
    public Button rotateButton;
    public Button colorButton;
    public Button deleteButton;

    [Header("Settings")]
    public float heightOffset = 1.5f;
    public float scaleStep = 0.2f;
    public float rotateStep = 45f;

    private GameObject selectedObject;
    private bool isUIVisible = false;

    void Start()
    {
        // Скрываем UI в начале
        uiPanel.SetActive(false);

        // Подключаем кнопки к функциям[2]
        scaleUpButton.onClick.AddListener(() => ScaleSelectedObject(1 + scaleStep));
        scaleDownButton.onClick.AddListener(() => ScaleSelectedObject(1 - scaleStep));
        rotateButton.onClick.AddListener(RotateSelectedObject);
        colorButton.onClick.AddListener(ChangeSelectedObjectColor);
        deleteButton.onClick.AddListener(DeleteSelectedObject);
    }

    void Update()
    {
        HandleObjectSelection();

        if (isUIVisible && selectedObject != null)
        {
            UpdateUIPosition();
            KeepUIFacingCamera();
        }
    }

    void HandleObjectSelection()
    {
        if (selectAction.action.WasPressedThisFrame())
        {
            GameObject hitObject = GetRaycastHit();

            if (hitObject != null && IsValidSelectable(hitObject))
            {
                if (selectedObject == hitObject && isUIVisible)
                {
                    // Повторный клик - скрываем UI
                    HideUI();
                }
                else
                {
                    // Новый объект - показываем UI
                    SelectObjectAndShowUI(hitObject);
                }
            }
            else if (isUIVisible)
            {
                // Клик в пустоту - скрываем UI
                HideUI();
            }
        }
    }

    GameObject GetRaycastHit()
    {
        // Проверяем правый луч[5]
        if (rightRay != null && rightRay.TryGetCurrent3DRaycastHit(out RaycastHit rightHit))
        {
            return rightHit.collider.gameObject;
        }

        // Проверяем левый луч[5]
        if (leftRay != null && leftRay.TryGetCurrent3DRaycastHit(out RaycastHit leftHit))
        {
            return leftHit.collider.gameObject;
        }

        return null;
    }

    bool IsValidSelectable(GameObject obj)
    {
        // Исключаем UI элементы и проверяем наличие Renderer[2]
        return !obj.CompareTag("UI") &&
               obj.GetComponent<Canvas>() == null &&
               obj.GetComponent<Renderer>() != null;
    }

    void SelectObjectAndShowUI(GameObject obj)
    {
        selectedObject = obj;

        // Позиционируем UI над объектом
        UpdateUIPosition();

        // Показываем UI панель
        uiPanel.SetActive(true);
        isUIVisible = true;

        Debug.Log($"[UI] Выбран объект: {obj.name}");
    }

    void HideUI()
    {
        uiPanel.SetActive(false);
        isUIVisible = false;
        selectedObject = null;

        Debug.Log("[UI] Панель скрыта");
    }

    void UpdateUIPosition()
    {
        if (selectedObject == null) return;

        // Получаем границы объекта
        Bounds bounds = GetObjectBounds(selectedObject);

        // Позиционируем UI над объектом[2]
        Vector3 uiPosition = bounds.center + Vector3.up * (bounds.extents.y + heightOffset);
        objectToolsCanvas.transform.position = uiPosition;
    }

    void KeepUIFacingCamera()
    {
        // UI всегда смотрит на камеру[2]
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            objectToolsCanvas.transform.LookAt(mainCamera.transform);
            objectToolsCanvas.transform.Rotate(0, 180, 0); // Исправляем поворот
        }
    }

    Bounds GetObjectBounds(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
            return renderer.bounds;

        return new Bounds(obj.transform.position, Vector3.one);
    }

    // === ФУНКЦИИ УПРАВЛЕНИЯ ОБЪЕКТОМ === //

    void ScaleSelectedObject(float scaleFactor)
    {
        if (selectedObject != null)
        {
            selectedObject.transform.localScale *= scaleFactor;
            Debug.Log($"[UI] Масштаб изменен: {scaleFactor}");
        }
    }

    void RotateSelectedObject()
    {
        if (selectedObject != null)
        {
            selectedObject.transform.Rotate(0, rotateStep, 0, Space.World);
            Debug.Log($"[UI] Поворот на: {rotateStep}°");
        }
    }

    void ChangeSelectedObjectColor()
    {
        if (selectedObject != null)
        {
            Renderer renderer = selectedObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color newColor = new Color(Random.value, Random.value, Random.value);
                renderer.material.color = newColor;
                Debug.Log("[UI] Цвет изменен");
            }
        }
    }

    void DeleteSelectedObject()
    {
        if (selectedObject != null)
        {
            Debug.Log($"[UI] Удален объект: {selectedObject.name}");
            Destroy(selectedObject);
            HideUI();
        }
    }*/
}

