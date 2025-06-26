using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;



public class VRObjectEditor : MonoBehaviour
{
    public GameObject[] objectPrefabs;
    public TMP_Text infoText;
    public TMP_Text selectedObjectText;

    public XRRayInteractor rightControllerRay;
    public XRRayInteractor leftControllerRay;

    public InputActionReference selectRightAction;
    public InputActionReference selectLeftAction;
    public InputActionReference rightHandJoystickAction;
    public InputActionReference leftHandJoystickAction;

    private GameObject selectedPrefab;
    private GameObject selectedObject;
    private int currentIndex = -1;

    private List<GameObject> currentCategoryPrefabs = new List<GameObject>();

    public enum EditModeAction { Position, ScaleAndRotate }
    public EditModeAction currentEditAction;

    void Update()
    {
        HandleEditMode();
    }

    public void SelectPrefabByIndex(int index, List<GameObject> categoryPrefabs)
    {
        currentCategoryPrefabs = categoryPrefabs;

        if (index >= 0 && index < currentCategoryPrefabs.Count)
        {
            currentIndex = index;
            selectedPrefab = currentCategoryPrefabs[index];
            infoText.text = $"Selected prefab: {selectedPrefab.name}";
        } else
        {
            Debug.LogWarning("Invalid prefab index selected from category.");
            infoText.text = "Invalid prefab index selected.";
        }
    }

    public void PlacePrefab()
    {
        if (IsPointerOverUI(rightControllerRay))
        {
            Debug.Log("Pointer is over UI. Skipping placement.");
            return;
        }

        if (selectedPrefab == null)
        {
            Debug.LogWarning("No prefab selected.");
            return;
        }

        if ((rightControllerRay != null && TryPlaceUsingVRRay(rightControllerRay)) ||
            (leftControllerRay != null && TryPlaceUsingVRRay(leftControllerRay)))
        {
            return;
        }

        Debug.LogWarning("No valid surface to place the prefab.");
    }

    private bool TryPlaceUsingVRRay(XRRayInteractor controllerRay)
    {
        if (controllerRay.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            Vector3 hitPosition = hit.point;
            Quaternion hitRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(Vector3.forward, hit.normal), hit.normal);
            Instantiate(selectedPrefab, hitPosition, hitRotation);
            return true;
        }
        return false;
    }

    public void HandleEditMode()
    {
        if (selectRightAction.action.WasPressedThisFrame() || selectLeftAction.action.WasPressedThisFrame())
        {
            SelectObject();
        }

        if (selectedObject == null) return;

        switch (currentEditAction)
        {
            case EditModeAction.ScaleAndRotate:
                HandleObjectScaleAndRotation();
                break;
            case EditModeAction.Position:
                HandleObjectPosition();
                break;
        }
    }

    private void SelectObject()
    {
        RaycastHit hit = default;
        bool hasHit = false;

        if (rightControllerRay != null && rightControllerRay.TryGetCurrent3DRaycastHit(out hit))
        {
            hasHit = true;
        }
        else if (leftControllerRay != null && leftControllerRay.TryGetCurrent3DRaycastHit(out hit))
        {
            hasHit = true;
        }

        if (!hasHit) return;

        GameObject hitObject = hit.collider.gameObject;

        if (hitObject.CompareTag("UI") || hitObject.GetComponent<ARPlane>() != null)
        {
            Debug.Log("Hit ignored: UI element or AR Plane.");
            return;
        }

        if (selectedObject != hitObject)
        {
            selectedObject = hitObject;
            selectedObjectText.text = $"Selected: {selectedObject.name}";
        }
    }


    private void HandleObjectScaleAndRotation()
    {
        Vector2 leftJoystickInput = leftHandJoystickAction.action.ReadValue<Vector2>();
        Vector2 rightJoystickInput = rightHandJoystickAction.action.ReadValue<Vector2>();

        if (leftJoystickInput.y != 0)
        {
            Vector3 currentScale = selectedObject.transform.localScale;
            float scaleChange = leftJoystickInput.y * Time.deltaTime;
            selectedObject.transform.localScale = new Vector3(
                Mathf.Max(0.1f, currentScale.x + scaleChange),
                Mathf.Max(0.1f, currentScale.y + scaleChange),
                Mathf.Max(0.1f, currentScale.z + scaleChange)
            );
        }

        if (rightJoystickInput.x != 0)
        {
            float rotationSpeed = 100f;
            selectedObject.transform.Rotate(0, rightJoystickInput.x * rotationSpeed * Time.deltaTime, 0, Space.World);
        }
    }

    private void HandleObjectPosition()
    {
        Vector2 joystickInput = rightHandJoystickAction.action.ReadValue<Vector2>();
        if (joystickInput != Vector2.zero)
        {
            float moveSpeed = 2f;
            Vector3 move = new Vector3(joystickInput.x, 0, joystickInput.y) * moveSpeed * Time.deltaTime;
            selectedObject.transform.position += move;
        }
    }

    public void DeleteSelectedObject()
    {
        if (selectedObject != null)
        {
            Destroy(selectedObject);
            selectedObject = null;
            selectedObjectText.text = "No object selected";
            selectedPrefab = null;
            infoText.text = "No prefab selected.";
        }
    }

    public void ClearSelection()
    {
        selectedPrefab = null;
        selectedObject = null;
        currentIndex = -1;
        currentCategoryPrefabs.Clear();
        infoText.text = "No prefab selected.";
        selectedObjectText.text = "No object selected";
    }

    public void ResetRotationToUpright()
    {
        if (selectedObject != null)
        {
            selectedObject.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            Debug.Log($"Rotation of {selectedObject.name} has been reset to upright.");
        }
        else
        {
            Debug.LogWarning("No object selected. Cannot reset rotation.");
        }
    }

    private bool IsPointerOverUI(XRRayInteractor controllerRay)
    {
        if (controllerRay == null) return false;

        if (controllerRay.TryGetCurrentUIRaycastResult(out RaycastResult uiRaycastResult))
            return uiRaycastResult.gameObject != null;

        return false;
    }
}
