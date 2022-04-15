using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

/// <summary>
/// 
/// author:罐子（Lawliet）
/// vindicator:对比视图
/// versions:0.0.1
/// introduce:CompareWindow中左右两列的显示视图
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
    [Serializable]
    public class CompareView
    {
        /// <summary>
        /// 选中的ID列表
        /// </summary>
        private static readonly List<int> m_SelectIDs = new List<int>();

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
        /// 需要对比的GameObject
        /// </summary>
        [SerializeField]
        private GameObject m_GameObject;

        public GameObject gameObject
        {
            get { return m_GameObject; }
            set { m_GameObject = value; }
        }

        /// <summary>
        /// GameObject树视图的状态
        /// </summary>
        [SerializeField]
        private TreeViewState m_GOTreeState;

        /// <summary>
        /// GameObject树视图
        /// </summary>
        private GameObjectTreeView m_GOTree;

        /// <summary>
        /// Component树视图的状态
        /// </summary>
        [SerializeField]
        private TreeViewState m_ComponentTreeState;

        /// <summary>
        /// Component树视图
        /// </summary>
        private ComponentTreeView m_ComponentTree;

        /// <summary>
        /// 显示GameObject视图还是Component视图
        /// </summary>
        [SerializeField]
        private bool m_ShowComponentView;

        /// <summary>
        /// 左边还是右边的视图
        /// </summary>
        [SerializeField]
        private bool m_IsLeft;

        /// <summary>
        /// GameObject变更回调
        /// </summary>
        public Action gameObjectChangeCallback;

        /// <summary>
        /// 在GameObject树结构选中GameObject的回调
        /// </summary>
        public Action<int, bool> onGOTreeClickItemCallback
        {
            get { return m_GOTree.onClickItemCallback; }
            set { m_GOTree.onClickItemCallback = value; }
        }

        /// <summary>
        /// GameObject树结构展开状态变更回调
        /// </summary>
        public Action<int, bool, bool> onGOTreeExpandedStateChanged
        {
            get{ return m_GOTree.onExpandedStateChanged; }
            set{ m_GOTree.onExpandedStateChanged = value; }
        }

        /// <summary>
        /// 双击GameObject树回调
        /// </summary>
        public Action<GameObjectCompareInfo> onDoubleClickItem
        {
            get{ return m_GOTree.onDoubleClickItem; }
            set{ m_GOTree.onDoubleClickItem = value; }
        }

        /// <summary>
        /// 在Component树结构选中Component的回调
        /// </summary>
        public Action<int, bool> onComponentTreeClickItemCallback
        {
            get { return m_ComponentTree.onClickItemCallback; }
            set { m_ComponentTree.onClickItemCallback = value; }
        }

        /// <summary>
        /// 显示GameObject视图的回调
        /// </summary>
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

            m_GOTree = new GameObjectTreeView(m_GOTreeState, CompareData.rootInfo, m_IsLeft);

            if (m_ComponentTreeState == null)
            {
                m_ComponentTreeState = new TreeViewState();
            }

            m_ComponentTree = new ComponentTreeView(m_ComponentTreeState, CompareData.showComponentTarget, m_IsLeft);

            CompareData.onShowStateChange += OnShowStateChange;
        }

        public void Destory()
        {
            CompareData.onShowStateChange -= OnShowStateChange;
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
                if (CompareData.showComponentTarget != null)
                {
                    styles.prevContent.text = string.Format("[{0}]\t{1}", m_GameObject.name, CompareData.showComponentTarget.name);
                }
                else
                {
                    styles.prevContent.text = "back";
                }

                if (GUILayout.Button(styles.prevContent, EditorStyles.boldLabel))
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
            if (m_ShowComponentView)
            {
                Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
                m_ComponentTree.OnGUI(rect);
            }
            else
            {
                if(m_GOTreeState.scrollPos != CompareData.gameObjectTreeScroll)
                {
                    m_GOTreeState.scrollPos = CompareData.gameObjectTreeScroll;
                }

                Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
                m_GOTree.OnGUI(rect);

                if (m_GOTreeState.scrollPos != CompareData.gameObjectTreeScroll)
                {
                    CompareData.gameObjectTreeScroll = m_GOTreeState.scrollPos;
                }
            }
        }

        /// <summary>
        /// 展开对应ID的节点
        /// </summary>
        /// <param name="id"></param>
        /// <param name="expanded"></param>
        public void SetExpanded(int id, bool expanded)
        {
            m_GOTree.SetExpanded(id, expanded);
        }

        /// <summary>
        /// 选中对应ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isComponentView"></param>
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

        /// <summary>
        /// 改变树显示内容
        /// </summary>
        /// <param name="showComponent"></param>
        /// <param name="info"></param>
        public void ChangeTree(bool showComponent, GameObjectCompareInfo info = null)
        {
            m_ShowComponentView = showComponent;

            if (m_ShowComponentView)
            {
                CompareData.showComponentTarget = info;
                m_ComponentTree.Reload(CompareData.showComponentTarget);
            }
        }

        /// <summary>
        /// 重刷
        /// </summary>
        public void Reload()
        {
            m_GOTree.Reload(CompareData.rootInfo);
        }

        /// <summary>
        /// 显示类型状态改变
        /// </summary>
        private void OnShowStateChange()
        {
            m_GOTree.Reload(CompareData.rootInfo);
            m_ComponentTree.Reload(CompareData.showComponentTarget);
        }

    }
}
