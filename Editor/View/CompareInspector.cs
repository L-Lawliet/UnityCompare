using System;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 
/// author:罐子（Lawliet）
/// vindicator:对比的Inspector界面
/// versions:0.0.1
/// introduce:
/// note:
/// 
/// 
/// list:
/// 
/// 
/// 
/// </summary>
namespace UnityCompare
{
    public class CompareInspector : EditorWindow
    {
        public static CompareInspector GetWindow(CompareInfo info, UnityEngine.Object left, UnityEngine.Object right)
        {
            var window = GetWindow<CompareInspector>();
            window.titleContent = new GUIContent("Compare Inspector");
            window.Focus();
            window.Repaint();

            window.SetInfo(info);
            window.SetObject(left, right);
            return window;
        }

        public static void ClearWindow()
        {
            if(HasOpenInstances<CompareInspector>())
            {
                var window = GetWindow<CompareInspector>();
                window.titleContent = new GUIContent("Compare Inspector");

                window.Clear();
            }
        }

        /// <summary>
        /// 左边对象
        /// </summary>
        [SerializeField]
        private UnityEngine.Object m_Left;

        /// <summary>
        /// 右边对象
        /// </summary>
        [SerializeField]
        private UnityEngine.Object m_Right;

        /// <summary>
        /// 左边对象的Editor
        /// </summary>
        private Editor m_LeftEditor;

        /// <summary>
        /// 右边对象的Editor
        /// </summary>
        private Editor m_RightEditor;

        /// <summary>
        /// 滚动进度
        /// </summary>
        [SerializeField]
        private Vector2 m_ScrollPosition;

        /// <summary>
        /// 不相等的信息
        /// </summary>
        [SerializeField]
        private string m_UnequalMessage;

        /// <summary>
        /// 忽略对比的属性信息
        /// </summary>
        [SerializeField]
        private string m_IgnoreMessage;

        [SerializeField]
        private Vector2 m_UnequalBoxScroll;

        [SerializeField]
        private Vector2 m_IgnoreBoxScroll;

        /// <summary>
        /// 设置对比的对象
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        private void SetObject(UnityEngine.Object left, UnityEngine.Object right)
        {
            if(m_Left != left)
            {
                m_Left = left;

                if(m_LeftEditor != null)
                {
                    DestroyImmediate(m_LeftEditor);
                    m_LeftEditor = null;
                }

                if(m_Left != null)
                {
                    m_LeftEditor = Editor.CreateEditor(m_Left);
                }
            }

            if (m_Right != right)
            {
                m_Right = right;

                if (m_RightEditor != null)
                {
                    DestroyImmediate(m_RightEditor);
                    m_RightEditor = null;
                }

                if (m_Right != null)
                {
                    m_RightEditor = Editor.CreateEditor(m_Right);
                }
            }
        }

        /// <summary>
        /// 设置对比信息
        /// </summary>
        /// <param name="info"></param>
        private void SetInfo(CompareInfo info)
        {
            StringBuilder builder = new StringBuilder();

            string unequalMessage = info.GetUnequalMessage();

            if (string.IsNullOrWhiteSpace(unequalMessage))
            {
                m_UnequalMessage = "";
            }
            else
            {
                builder.Clear();

                builder.AppendLine("no equal item:");

                builder.AppendLine(unequalMessage);

                m_UnequalMessage = builder.ToString();
            }

            string ignoreMessage = "";

            if(info is ComponentCompareInfo)
            {
                ignoreMessage = (info as ComponentCompareInfo).GetIgnoreMessage();
            }

            if (string.IsNullOrWhiteSpace(ignoreMessage))
            {
                m_IgnoreMessage = "";
            }
            else
            {
                builder.Clear();

                builder.AppendLine("ignore item:");

                builder.AppendLine(ignoreMessage);

                m_IgnoreMessage = builder.ToString();
            }
        }

        private void OnEnable()
        {
            if(m_Left != null)
            {
                m_LeftEditor = Editor.CreateEditor(m_Left);
            }

            if (m_Right != null)
            {
                m_RightEditor = Editor.CreateEditor(m_Right);
            }
        }

        private void OnDisable()
        {
            if (m_LeftEditor != null)
            {
                DestroyImmediate(m_LeftEditor);
                m_LeftEditor = null;
            }

            if (m_RightEditor != null)
            {
                DestroyImmediate(m_RightEditor);
                m_RightEditor = null;
            }
        }

        private void Clear()
        {
            if(m_LeftEditor != null)
            {
                DestroyImmediate(m_LeftEditor);
                m_LeftEditor = null;
            }

            if (m_RightEditor != null)
            {
                DestroyImmediate(m_RightEditor);
                m_RightEditor = null;
            }

            m_Left = null;
            m_Right = null;

            m_UnequalMessage = "";
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();

            OnEditor(m_Left, m_LeftEditor);
            //OnField(m_Left);

            EditorGUILayout.Separator();

            OnEditor(m_Right, m_RightEditor);
            //OnField(m_Right);

            GUILayout.EndHorizontal();

            /*if (GUILayout.Button("Print"))
            {
                Debug.Log(PrefabUtility.IsAnyPrefabInstanceRoot(m_Left as GameObject));
                //CompareUtility.PrintProperty(m_LeftEditor.serializedObject, m_RightEditor.serializedObject);
            }*/

            OnMessage();
        }

        /// <summary>
        /// Editor的绘制
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="editor"></param>
        private void OnEditor(UnityEngine.Object obj, Editor editor)
        {
            EditorGUIUtility.wideMode = true;

            var width = this.position.width / 2 - 3;

            EditorGUILayout.BeginVertical(GUILayout.Width(width));

            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, GUILayout.ExpandWidth(true));

            if (editor!= null)
            {
                if(obj is GameObject)
                {
                    editor.DrawHeader();
                }
                else
                {
                    EditorGUIUtility.hierarchyMode = true;
                    EditorGUILayout.InspectorTitlebar(false, editor);
                    //EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins, GUILayout.Width(width - 10));
                    editor.OnInspectorGUI();
                    //EditorGUILayout.EndVertical();
                }  
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();

            EditorGUIUtility.hierarchyMode = false;
            EditorGUIUtility.wideMode = false;
        }

        /// <summary>
        /// 提示信息的绘制
        /// </summary>
        private void OnMessage()
        {
            EditorGUILayout.BeginHorizontal();

            if (!string.IsNullOrWhiteSpace(m_UnequalMessage))
            {
                m_UnequalBoxScroll = EditorGUILayout.BeginScrollView(m_UnequalBoxScroll, GUILayout.MaxHeight(150));
                EditorGUILayout.HelpBox(m_UnequalMessage, MessageType.Error);
                EditorGUILayout.EndScrollView();
            }

            if (!string.IsNullOrWhiteSpace(m_IgnoreMessage))
            {
                m_IgnoreBoxScroll = EditorGUILayout.BeginScrollView(m_IgnoreBoxScroll, GUILayout.MaxHeight(150));
                EditorGUILayout.HelpBox(m_IgnoreMessage, MessageType.Warning);
                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
