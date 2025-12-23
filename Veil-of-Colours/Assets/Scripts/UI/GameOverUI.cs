using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VeilOfColours.General;

namespace VeilOfColours.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField]
        private Button restartButton;

        [SerializeField]
        private Button mainMenuButton;

        private void Awake()
        {
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        private void OnEnable()
        {
            SelectFirstButton();
        }

        private void SelectFirstButton()
        {
            if (restartButton != null)
            {
                EventSystem.current?.SetSelectedGameObject(restartButton.gameObject);
            }
        }

        private void OnRestartClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
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
            if (restartButton != null)
                restartButton.onClick.RemoveListener(OnRestartClicked);

            if (mainMenuButton != null)
                mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
        }
    }
}
