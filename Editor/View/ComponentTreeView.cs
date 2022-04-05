using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UnityCompare
{
    public class ComponentTreeView : TreeView
    {
        private static Texture2D TEST_INCONCLUSIVE_ICON = EditorGUIUtility.FindTexture("TestInconclusive");
        private static Texture2D TEST_PASS_ICON = EditorGUIUtility.FindTexture("TestPassed");
        private static Texture2D TEST_FAILED_ICON = EditorGUIUtility.FindTexture("TestFailed");

        private GameObjectCompareInfo m_Info;

        private bool m_IsLeft;

        public Action<int, bool> onClickItemCallback;

        public Action<int, bool, bool> onExpandedStateChanged;

        private HashSet<int> m_ExpandedSet = new HashSet<int>();

        private TreeViewItem m_Root;

        public ComponentTreeView(TreeViewState state, GameObjectCompareInfo info, bool isLeft) : base(state)
        {
            m_Info = info;
            m_IsLeft = isLeft;

            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            m_Root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };

            var allItems = new List<TreeViewItem>();

            if (m_Info != null)
            {
                AddComponentItem(allItems, m_Info); 
            }

            SetupParentsAndChildrenFromDepths(m_Root, allItems);

            return m_Root;
        }

        public void Reload(GameObjectCompareInfo info)
        {
            m_Info = info;

            Reload();
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item as CompareTreeViewItem<ComponentCompareInfo>;

            var info = item.info;

            Rect rect = args.rowRect;

            var iconSize = 20;

            Rect iconRect = new Rect(rect.x + GetContentIndent(item), rect.y, iconSize, rect.height);

            if (info.missType == MissType.allExist && !info.AllEqual())
            {
                GUI.DrawTexture(iconRect, TEST_FAILED_ICON, ScaleMode.ScaleToFit);
            }
            else if(info.missType == MissType.missRight && m_IsLeft)
            {
                GUI.DrawTexture(iconRect, TEST_INCONCLUSIVE_ICON, ScaleMode.ScaleToFit);
            }
            else if (info.missType == MissType.missLeft && !m_IsLeft)
            {
                GUI.DrawTexture(iconRect, TEST_INCONCLUSIVE_ICON, ScaleMode.ScaleToFit);
            }
            else if(!string.IsNullOrWhiteSpace(item.displayName))
            {
                GUI.DrawTexture(iconRect, TEST_PASS_ICON, ScaleMode.ScaleToFit);
            }

            rect.x += iconSize;
            rect.width -= iconSize;
            args.rowRect = rect;

            base.RowGUI(args);
        }

        protected override void SingleClickedItem(int id)
        {
            base.SingleClickedItem(id);

            if(onClickItemCallback != null)
            {
                onClickItemCallback.Invoke(id, m_IsLeft);
            }

            var item = FindItem(id, m_Root) as CompareTreeViewItem<ComponentCompareInfo>;

            CompareInspector.GetWindow(item.info.leftComponent, item.info.rightComponent);
        }

        protected override void ExpandedStateChanged()
        {
            base.ExpandedStateChanged();

            var list = GetExpanded();

            //TODO: 优化堆内存
            var tempSet = new HashSet<int>();

            var removeList = new List<int>();

            for (int i = 0; i < list.Count; i++)
            {
                var id = list[i];

                tempSet.Add(id);

                if (!m_ExpandedSet.Contains(id))
                {
                    m_ExpandedSet.Add(id);

                    if (onExpandedStateChanged != null)
                    {
                        onExpandedStateChanged.Invoke(id, m_IsLeft, true);
                    }
                }
            }

            foreach (var id in m_ExpandedSet)
            {
                if (!tempSet.Contains(id))
                {
                    removeList.Add(id);

                    if (onExpandedStateChanged != null)
                    {
                        onExpandedStateChanged.Invoke(id, m_IsLeft, false);
                    }
                }
            }

            for (int i = 0; i < removeList.Count; i++)
            {
                m_ExpandedSet.Remove(removeList[i]);
            }
        }

        public new void SetExpanded(int id, bool expanded)
        {
            if (expanded)
            {
                m_ExpandedSet.Add(id);
            }
            else
            {
                m_ExpandedSet.Remove(id);
            }

            base.SetExpanded(id, expanded);
        }

        private void AddComponentItem(List<TreeViewItem> items, GameObjectCompareInfo info)
        {
            if(info.components == null)
            {
                return;
            }

            for (int i = 0; i < info.components.Count; i++)
            {
                var component = info.components[i];

                if(component == null)
                {
                    continue;
                }

                string displayName;

                if (component.missType == MissType.missLeft && m_IsLeft)
                {
                    displayName = "";
                }
                else if (component.missType == MissType.missRight && !m_IsLeft)
                {
                    displayName = "";
                }
                else
                {
                    displayName = component.name;
                }

                var item = new CompareTreeViewItem<ComponentCompareInfo> { info = component, id = component.id, depth = 0, displayName = displayName };

                items.Add(item);
            }
        }
    }
}
