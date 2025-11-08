using UnityEngine;

namespace VeilOfColours.General
{
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
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireCube(transform.position, size);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position, levelName);
#endif
        }
    }
}
