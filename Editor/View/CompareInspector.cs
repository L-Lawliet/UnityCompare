using UnityEditor;
using UnityEngine;

namespace UnityCompare
{
    public class CompareInspector : EditorWindow
    {
        [MenuItem("Tools/Compare/CompareInspector")]
        public static CompareInspector GetWindow(Object left, Object right)
        {
            var window = GetWindow<CompareInspector>();
            window.titleContent = new GUIContent("Compare Inspector");
            window.Focus();
            window.Repaint();

            window.SetObject(left, right);
            return window;
        }

        [SerializeField]
        private Object m_Left;

        [SerializeField]
        private Object m_Right;

        [SerializeField]
        private Editor m_LeftEditor;

        [SerializeField]
        private Editor m_RightEditor;

        [SerializeField]
        private Vector2 m_ScrollPosition;

        private void SetObject(Object left, Object right)
        {
            if(m_Left != left)
            {
                m_Left = left;

                if(m_Left != null)
                {
                    m_LeftEditor = Editor.CreateEditor(m_Left);
                }
                else
                {
                    m_LeftEditor = null;
                }
                
            }

            if (m_Right != right)
            {
                m_Right = right;

                if(m_Right != null)
                {
                    m_RightEditor = Editor.CreateEditor(m_Right);
                }
                else
                {
                    m_RightEditor = null;
                }
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();

            OnEditor(m_LeftEditor);

            EditorGUILayout.Separator();

            OnEditor(m_RightEditor);

            GUILayout.EndHorizontal();
        }

        private void OnEditor(Editor editor)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(this.position.width / 2 - 3));

            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

            if (editor!= null)
            {
                editor.DrawHeader();
                editor.OnInspectorGUI();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }
    }
}
