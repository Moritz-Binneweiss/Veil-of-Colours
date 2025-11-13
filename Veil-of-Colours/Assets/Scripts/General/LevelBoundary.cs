using UnityEngine;

namespace VeilOfColours.General
{
    /// <summary>
    /// Visual boundary marker for level areas in the editor.
    /// </summary>
    public class LevelBoundary : MonoBehaviour
    {
        [Header("Level Settings")]
        [SerializeField]
        private string levelName = "Level A";

        [SerializeField]
        private Color gizmoColor = Color.cyan;

        [Header("Boundary Size")]
        [SerializeField]
        private Vector2 size = new Vector2(20f, 15f);

        private void OnDrawGizmos()
        {
            DrawBoundary();
            DrawLabel();
        }

        private void DrawBoundary()
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireCube(transform.position, size);
        }

        private void DrawLabel()
        {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(levelName))
            {
                var style = new UnityEngine.GUIStyle
                {
                    normal = { textColor = gizmoColor },
                    alignment = UnityEngine.TextAnchor.MiddleCenter,
                    fontSize = 14,
                    fontStyle = UnityEngine.FontStyle.Bold,
                };
                UnityEditor.Handles.Label(transform.position, levelName, style);
            }
#endif
        }
    }
}
