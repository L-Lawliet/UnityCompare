﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UnityCompare
{
    [Serializable]
    public class CompareView
    {
        private static Texture2D PREV_ICON = EditorGUIUtility.FindTexture("back");

        private static GUIContent PREV_CONTENT = new GUIContent("back", PREV_ICON);

        private static readonly List<int> m_SelectIDs = new List<int>();

        [SerializeField]
        private GameObject m_GameObject;

        public GameObject gameObject
        {
            get { return m_GameObject; }
            set { m_GameObject = value; }
        }

        [SerializeField]
        private TreeViewState m_GOTreeState;

        private GameObjectTreeView m_GOTree;

        [SerializeField]
        private TreeViewState m_ComponentTreeState;

        private ComponentTreeView m_ComponentTree;

        [SerializeField]
        private bool m_ShowComponentView;
        
        [SerializeField]
        private GameObjectCompareInfo m_ShowComponentTarget;

        private GameObjectCompareInfo m_Info;

        public GameObjectCompareInfo info
        {
            set
            {
                m_Info = value;

                m_GOTree.Reload(m_Info);
            }
        }

        [SerializeField]
        private bool m_IsLeft;

        public Action gameObjectChangeCallback;

        public Action<int, bool> onGOTreeClickItemCallback
        {
            get { return m_GOTree.onClickItemCallback; }
            set { m_GOTree.onClickItemCallback = value; }
        }

        public Action<int, bool, bool> onGOTreeExpandedStateChanged
        {
            get{ return m_GOTree.onExpandedStateChanged; }
            set{ m_GOTree.onExpandedStateChanged = value; }
        }

        public Action<GameObjectCompareInfo> onDoubleClickItem
        {
            get{ return m_GOTree.onDoubleClickItem; }
            set{ m_GOTree.onDoubleClickItem = value; }
        }

        public Action<int, bool> onComponentTreeClickItemCallback
        {
            get { return m_ComponentTree.onClickItemCallback; }
            set { m_ComponentTree.onClickItemCallback = value; }
        }

        public Action onShowGameObjectView;

        public CompareView(bool isLeft)
        {
            m_IsLeft = isLeft;
        }

        public void Init()
        {
            if (m_GOTreeState == null)
            {
                m_GOTreeState = new TreeViewState();
            }

            m_GOTree = new GameObjectTreeView(m_GOTreeState, m_Info, m_IsLeft);

            if (m_ComponentTreeState == null)
            {
                m_ComponentTreeState = new TreeViewState();
            }

            m_ComponentTree = new ComponentTreeView(m_ComponentTreeState, m_ShowComponentTarget, m_IsLeft);
        }

        public void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            OnToolBar();

            OnTreeView();

            EditorGUILayout.EndVertical();
        }

        private void OnToolBar()
        {
            if (m_ShowComponentView)
            {
                if (m_ShowComponentTarget != null)
                {
                    PREV_CONTENT.text = m_ShowComponentTarget.name;
                }
                else
                {
                    PREV_CONTENT.text = "back";
                }

                if (GUILayout.Button(PREV_CONTENT, EditorStyles.boldLabel))
                {
                    if(onShowGameObjectView != null)
                    {
                        onShowGameObjectView.Invoke();
                    }
                }
            }
            else
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    m_GameObject = EditorGUILayout.ObjectField(m_GameObject, typeof(GameObject), false) as GameObject;

                    if (check.changed)
                    {
                        if (gameObjectChangeCallback != null)
                        {
                            gameObjectChangeCallback.Invoke();
                        }
                    }
                }
            }
        }

        private void OnTreeView()
        {
            Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);

            if (m_ShowComponentView)
            {
                m_ComponentTree.OnGUI(rect);
            }
            else
            {
                m_GOTree.OnGUI(rect);
            }
        }

        public void SetExpanded(int id, bool expanded)
        {
            m_GOTree.SetExpanded(id, expanded);
        }

        public void Select(int id, bool isComponentView)
        {
            m_SelectIDs.Clear();
            m_SelectIDs.Add(id);

            if (isComponentView)
            {
                m_ComponentTree.SetSelection(m_SelectIDs, TreeViewSelectionOptions.FireSelectionChanged);
            }
            else
            {
                m_GOTree.SetSelection(m_SelectIDs, TreeViewSelectionOptions.FireSelectionChanged);
            }

            m_SelectIDs.Clear();
        }

        public void ChangeTree(bool showComponent, GameObjectCompareInfo info = null)
        {
            m_ShowComponentView = showComponent;

            if (m_ShowComponentView)
            {
                m_ShowComponentTarget = info;
                m_ComponentTree.Reload(info);
            }
        }
    }
}
