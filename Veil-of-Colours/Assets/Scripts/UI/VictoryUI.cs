using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VeilOfColours.General;

namespace VeilOfColours.UI
{
    public class VictoryUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField]
        private Button mainMenuButton;

        private void Awake()
        {
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        private void OnEnable()
        {
            SelectFirstButton();
        }

        private void SelectFirstButton()
        {
            if (mainMenuButton != null)
            {
                EventSystem.current?.SetSelectedGameObject(mainMenuButton.gameObject);
            }
        }

        private void OnMainMenuClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitToMainMenu();
            }
        }

        private void OnDestroy()
        {
            if (mainMenuButton != null)
                mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
        }
    }
}
