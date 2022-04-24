using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

/// <summary>
/// 
/// author:罐子（Lawliet）
/// vindicator:设置界面
/// versions:0.1.2
/// introduce:在CompareWindow打开
/// note:
/// 1. 用于设置需要忽略的PropertyPath
/// 
/// list:
/// 
/// 
/// 
/// </summary>
namespace UnityCompare
{
    public class CompareSettingWindow : EditorWindow
    {
        public static CompareSettingWindow OpenWindow()
        {
            var window = GetWindow<CompareSettingWindow>();
            window.titleContent = new GUIContent("Compare Setting");
            window.Focus();
            window.Repaint();
            return window;
        }

        private CompareStyles m_Styles;

        public CompareStyles styles
        {
            get
            {
                if (m_Styles == null)
                {
                    m_Styles = new CompareStyles();
                }

                return m_Styles;
            }
        }

        /// <summary>
        /// 默认忽略列表的列表组件
        /// </summary>
        private ReorderableList m_DefaultReorderableList;

        /// <summary>
        /// 自定义忽略列表的列表组件
        /// </summary>
        private ReorderableList m_CustomReorderableList;

        private void OnEnable()
        {
            if (m_DefaultReorderableList == null)
            {
                m_DefaultReorderableList = new ReorderableList(CompareData.defaultIgnores, typeof(IgnoreProperty), false, false, false, false);

                m_DefaultReorderableList.elementHeight = 25;

                m_DefaultReorderableList.drawElementCallback += DrawDefaultElement;
            }

            if (m_CustomReorderableList == null)
            {
                m_CustomReorderableList = new ReorderableList(CompareData.customIgnores, typeof(IgnoreProperty), true, false, true, true);

                m_CustomReorderableList.elementHeight = 25;

                m_CustomReorderableList.drawElementCallback += DrawCustomElement;
                m_CustomReorderableList.onAddCallback += AddCustomElement;
            }
        }

        private void OnDisable()
        {
            if (m_DefaultReorderableList != null)
            {
                m_DefaultReorderableList.drawElementCallback -= DrawDefaultElement;
            }

            if (m_CustomReorderableList != null)
            {
                m_CustomReorderableList.drawElementCallback -= DrawCustomElement;
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField(styles.defaultIgnoreLableContent, EditorStyles.boldLabel);
            
            m_DefaultReorderableList.DoLayoutList();

            GUILayout.Space(10);

            EditorGUILayout.LabelField(styles.customIgnoreLableContent, EditorStyles.boldLabel);

            m_CustomReorderableList.DoLayoutList();
        }

        /// <summary>
        /// 绘制默认忽略列表的元素
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="index"></param>
        /// <param name="isActive"></param>
        /// <param name="isFocused"></param>
        private void DrawDefaultElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if(CompareData.defaultIgnores != null && index < CompareData.defaultIgnores.Count)
            {
                IgnoreProperty ignoreProperty = CompareData.defaultIgnores[index];

                int toggleWidth = 20;

                int textHeight = 20;

                Rect regexRect = new Rect(rect.x, rect.y + (rect.height - textHeight) / 2, rect.width - toggleWidth - 2, textHeight);

                Rect onRect = new Rect(rect.x + rect.width - toggleWidth, rect.y, toggleWidth, rect.height);

                using(new EditorGUI.DisabledGroupScope(true))
                {
                    ignoreProperty.pattern = EditorGUI.TextField(regexRect, ignoreProperty.pattern);
                }

                ignoreProperty.on = EditorGUI.Toggle(onRect, ignoreProperty.on);
            }
        }

        /// <summary>
        /// 绘制自定义忽略列表元素
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="index"></param>
        /// <param name="isActive"></param>
        /// <param name="isFocused"></param>
        private void DrawCustomElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (CompareData.customIgnores != null && index < CompareData.customIgnores.Count)
            {
                IgnoreProperty ignoreProperty = CompareData.customIgnores[index];

                int toggleWidth = 20;

                int textHeight = 20;

                Rect regexRect = new Rect(rect.x, rect.y + (rect.height - textHeight) / 2, rect.width - toggleWidth - 2, textHeight);

                Rect onRect = new Rect(rect.x + rect.width - toggleWidth, rect.y, toggleWidth, rect.height);

                ignoreProperty.pattern = EditorGUI.TextField(regexRect, ignoreProperty.pattern);

                ignoreProperty.on = EditorGUI.Toggle(onRect, ignoreProperty.on);
            }
        }

        /// <summary>
        /// 添加自定义的忽略元素
        /// </summary>
        /// <param name="list"></param>
        private void AddCustomElement(ReorderableList list)
        {
            if (CompareData.customIgnores != null)
            {
                CompareData.customIgnores.Add(new IgnoreProperty("", false));
            }
        }

    }
}
