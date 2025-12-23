using Unity.Netcode;
using UnityEngine;
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
