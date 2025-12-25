using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace VeilOfColours.GameLogic
{
    public class ColorWheel : NetworkBehaviour
    {
        [Header("Color Wheel UI")]
        [SerializeField]
        private GameObject colorWheelUI;

        [SerializeField]
        private Image colorWheelImage;

        [SerializeField]
        private Transform selectorIndicator;

        [Header("Input")]
        [SerializeField]
        private InputActionReference colorWheelAction;

        [Header("Settings")]
        [SerializeField]
        private float activationThreshold = 0.3f;

        [SerializeField]
        private float selectionDeadzone = 0.2f;

        [SerializeField]
        private float selectorRadius = 130f;

        [Header("Color Assignments")]
        [SerializeField]
        private Color[] availableColors = new Color[]
        {
            Color.blue,
            Color.red,
            Color.green,
            Color.yellow,
        };

        private bool isWheelActive;
        private int currentColorIndex = -1;
        private Vector2 currentStickInput;

        private void OnEnable()
        {
            if (!IsOwner)
                return;

            if (colorWheelAction != null)
                colorWheelAction.action.Enable();
        }

        private void OnDisable()
        {
            if (!IsOwner)
                return;

            if (colorWheelAction != null)
                colorWheelAction.action.Disable();
        }

        private void Start()
        {
            if (colorWheelUI != null)
                colorWheelUI.SetActive(false);
        }

        private void Update()
        {
            if (!IsOwner)
                return;

            HandleColorWheelInput();
        }

        private void HandleColorWheelInput()
        {
            if (colorWheelAction == null)
                return;

            Vector2 inputVector = colorWheelAction.action.ReadValue<Vector2>();
            float magnitude = inputVector.magnitude;

            if (magnitude > activationThreshold && !isWheelActive)
            {
                OpenColorWheel();
            }
            else if (magnitude < selectionDeadzone && isWheelActive)
            {
                CloseColorWheel();
            }

            if (isWheelActive && magnitude > selectionDeadzone)
            {
                UpdateColorSelection(inputVector);
            }
        }

        private void OpenColorWheel()
        {
            isWheelActive = true;
            if (colorWheelUI != null)
                colorWheelUI.SetActive(true);
        }

        private void CloseColorWheel()
        {
            isWheelActive = false;
            if (colorWheelUI != null)
                colorWheelUI.SetActive(false);

            if (currentColorIndex >= 0)
            {
                ApplyColorSelection(currentColorIndex);
            }

            currentColorIndex = -1;
        }

        private void UpdateColorSelection(Vector2 stickInput)
        {
            float angle = Mathf.Atan2(stickInput.y, stickInput.x) * Mathf.Rad2Deg;

            if (angle < 0)
                angle += 360f;

            // New layout: Top=Red, Right=Green, Bottom=Yellow, Left=Blue
            int colorIndex;
            if (angle >= 45f && angle < 135f)
                colorIndex = 1; // Red (Top)
            else if (angle >= 135f && angle < 225f)
                colorIndex = 0; // Blue (Left)
            else if (angle >= 225f && angle < 315f)
                colorIndex = 3; // Yellow (Bottom)
            else
                colorIndex = 2; // Green (Right)

            if (colorIndex != currentColorIndex)
            {
                currentColorIndex = colorIndex;
            }

            UpdateSelectorVisual(stickInput);
        }

        private void UpdateSelectorVisual(Vector2 stickInput)
        {
            if (selectorIndicator == null)
                return;

            Vector2 selectorPosition = stickInput.normalized * selectorRadius;
            selectorIndicator.localPosition = new Vector3(
                selectorPosition.x,
                selectorPosition.y,
                0f
            );

            if (currentColorIndex >= 0 && currentColorIndex < availableColors.Length)
            {
                Image selectorImage = selectorIndicator.GetComponent<Image>();
                if (selectorImage != null)
                {
                    selectorImage.color = availableColors[currentColorIndex];
                }
            }
        }

        private void ApplyColorSelection(int colorIndex)
        {
            if (colorIndex < 0 || colorIndex >= availableColors.Length)
                return;

            if (ColorManager.Instance == null)
            {
                Debug.LogError("ColorManager not found in scene!");
                return;
            }

            if (ColorManager.Instance.IsSpawned)
            {
                ColorManager.Instance.RequestColorChange(colorIndex);
            }
        }

        public Color GetCurrentColor()
        {
            if (ColorManager.Instance != null)
            {
                int activeIndex = ColorManager.Instance.GetActiveColorIndex();
                if (activeIndex >= 0 && activeIndex < availableColors.Length)
                    return availableColors[activeIndex];
            }

            return Color.white;
        }

        public int GetCurrentColorIndex()
        {
            if (ColorManager.Instance != null)
            {
                return ColorManager.Instance.GetActiveColorIndex();
            }
            return 0;
        }
    }
}
