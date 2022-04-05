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
    public class GameObjectTreeView : TreeView
    {
        private static Texture2D TEST_INCONCLUSIVE_ICON = EditorGUIUtility.FindTexture("TestInconclusive");
        private static Texture2D TEST_PASS_ICON = EditorGUIUtility.FindTexture("TestPassed");
        private static Texture2D TEST_FAILED_ICON = EditorGUIUtility.FindTexture("TestFailed");

        private static Texture GAME_OBJECT_ICON = EditorGUIUtility.FindTexture("GameObject Icon");

        private static Texture PREFAB_ICON = EditorGUIUtility.FindTexture("Prefab Icon");

        private GameObjectCompareInfo m_Info;

        private bool m_IsLeft;

        public Action<int, bool> onClickItemCallback;

        public Action<int, bool, bool> onExpandedStateChanged;

        public Action<GameObjectCompareInfo> onDoubleClickItem;

        private HashSet<int> m_ExpandedSet = new HashSet<int>();

        private TreeViewItem m_Root;

        public GameObjectTreeView(TreeViewState state, GameObjectCompareInfo info, bool isLeft) : base(state)
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
                var item = new CompareTreeViewItem<GameObjectCompareInfo> { info = m_Info, id = m_Info.id, depth = m_Info.depth, displayName = m_Info.name };
                allItems.Add(item);

                AddChildItem(allItems, m_Info); 
            }

            SetupParentsAndChildrenFromDepths(m_Root, allItems);

            /*var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
            var allItems = new List<TreeViewItem>
            {
                new TreeViewItem {id = 1, depth = 0, displayName = "Animals"},
                new TreeViewItem {id = 2, depth = 1, displayName = "Mammals"},
                new TreeViewItem {id = 3, depth = 2, displayName = "Tiger"},
                new TreeViewItem {id = 4, depth = 2, displayName = "Elephant"},
                new TreeViewItem {id = 5, depth = 2, displayName = "Okapi"},
                new TreeViewItem {id = 6, depth = 2, displayName = "Armadillo"},
                new TreeViewItem {id = 7, depth = 1, displayName = "Reptiles"},
                new TreeViewItem {id = 8, depth = 2, displayName = "Crocodile"},
                new TreeViewItem {id = 9, depth = 2, displayName = "Lizard"},
            };

            SetupParentsAndChildrenFromDepths(root, allItems);*/

            return m_Root;
        }

        public void Reload(GameObjectCompareInfo info)
        {
            m_Info = info;

            Reload();
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item as CompareTreeViewItem<GameObjectCompareInfo>;

            var info = item.info;

            Rect rect = args.rowRect;

            var iconSize = 20;

            Rect iconRect = new Rect(rect.x + GetContentIndent(item), rect.y, rect.height, rect.height);

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

            rect.x += rect.height;
            rect.width -= rect.height;
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

            var item = FindItem(id, m_Root) as CompareTreeViewItem<GameObjectCompareInfo>;

            CompareInspector.GetWindow(item.info, item.info.leftGameObject, item.info.rightGameObject);
        }

        protected override void DoubleClickedItem(int id)
        {
            base.DoubleClickedItem(id);

            if(onDoubleClickItem != null)
            {
                var item = FindItem(id, m_Root) as CompareTreeViewItem<GameObjectCompareInfo>;

                onDoubleClickItem.Invoke(item.info);
            }
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

        private void AddChildItem(List<TreeViewItem> items, GameObjectCompareInfo info)
        {
            if(info.children == null)
            {
                return;
            }

            for (int i = 0; i < info.children.Count; i++)
            {
                var child = info.children[i];

                if(child == null)
                {
                    continue;
                }

                string displayName;

                if (child.missType == MissType.missLeft && m_IsLeft)
                {
                    displayName = "";
                }
                else if (child.missType == MissType.missRight && !m_IsLeft)
                {
                    displayName = "";
                }
                else
                {
                    displayName = child.name;
                }

                var item = new CompareTreeViewItem<GameObjectCompareInfo> { info = child, id = child.id, depth = child.depth, displayName = displayName };

                items.Add(item);

                AddChildItem(items, child);
            }
        }
    }
}
