using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 
/// author:罐子（Lawliet）
/// vindicator:对比工具
/// versions:0.0.1
/// introduce:主要对两个GameObject或者两个Component进行对比，并返回对应的差异树
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
    public class CompareUtility
    {
        /// <summary>
        /// ID计数器
        /// </summary>
        public static int idCounter;

        /// <summary>
        /// 忽略的路径数组
        /// 当Component中的属性在此数组范围内，忽略对比
        /// </summary>
        public static string[] ingorePath = new string[]
        {
            "m_PrefabAsset",
            "m_GameObject",
            "m_Father",
            "m_Children",
        };

        /// <summary>
        /// 组件数组（重复利用）
        /// </summary>
        public static readonly List<Component> m_LeftComponentList = new List<Component>();

        /// <summary>
        /// 组件数组（重复利用）
        /// </summary>
        public static readonly List<Component> m_RightComponentList = new List<Component>();

        /// <summary>
        /// 对比两个Prefab
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static GameObjectCompareInfo ComparePrefab(GameObject left, GameObject right)
        {
            if(left == null || right == null)
            {
                return new GameObjectCompareInfo("", 0, 0);
            }

            return CompareGameObject(left, right, "Root", 0, MissType.allExist);
        }

        /// <summary>
        /// 对比两个GameObject
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="name"></param>
        /// <param name="depth"></param>
        /// <param name="missType">（左右对象）丢失状态</param>
        /// <param name="fileID"></param>
        /// <returns></returns>
        public static GameObjectCompareInfo CompareGameObject(GameObject left, GameObject right, string name, int depth, MissType missType)
        {
            GameObjectCompareInfo info = new GameObjectCompareInfo(name, depth, ++idCounter);

            info.leftGameObject = left;
            info.rightGameObject = right;

            info.missType = missType;

            if (missType == MissType.allExist)
            {
                CompareGameObject(left, right, ref info);
            }

            return info;
        }

        /// <summary>
        /// 对比两个GameObject
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="info"></param>
        public static void CompareGameObject(GameObject left, GameObject right, ref GameObjectCompareInfo info)
        {
            if (left.activeSelf == right.activeSelf)
            {
                info.gameObjectCompareType |= GameObjectCompareType.activeEqual;
            }

            if (left.CompareTag(right.tag))
            {
                info.gameObjectCompareType |= GameObjectCompareType.tagEqual;
            }

            if (left.layer == right.layer)
            {
                info.gameObjectCompareType |= GameObjectCompareType.layerEqual;
            }

            #region child
            var leftChildCount = left.transform.childCount;
            var rightChildCount = right.transform.childCount;

            int leftIndex = 0;
            int rightIndex = 0;

            bool childCountEqual = true;
            bool childContentEqual = true;

            MissType missType;

            while (leftIndex < leftChildCount || rightIndex < rightChildCount)
            {
                string name;

                GameObject leftChildGO;
                GameObject rightChildGO;


                if (leftIndex >= leftChildCount)
                {
                    var rightChild = right.transform.GetChild(rightIndex);

                    name = rightChild.name;
                    leftChildGO = null;
                    rightChildGO = rightChild.gameObject;

                    missType = MissType.missLeft;

                    rightIndex++;
                }
                else if (rightIndex >= rightChildCount)
                {
                    var leftChild = left.transform.GetChild(leftIndex);

                    name = leftChild.name;
                    leftChildGO = leftChild.gameObject;
                    rightChildGO = null;

                    missType = MissType.missRight;

                    leftIndex++;
                }
                else
                {
                    var leftChild = left.transform.GetChild(leftIndex);
                    var rightChild = right.transform.GetChild(rightIndex);

                    if (leftChild.name == rightChild.name)
                    {
                        name = leftChild.name;
                        leftChildGO = leftChild.gameObject;
                        rightChildGO = rightChild.gameObject;

                        missType = MissType.allExist;

                        leftIndex++;
                        rightIndex++;
                    }
                    else
                    {
                        childCountEqual = false;

                        if (leftIndex <= rightIndex)
                        {
                            name = leftChild.name;
                            leftChildGO = leftChild.gameObject;
                            rightChildGO = null;

                            missType = MissType.missRight;

                            leftIndex++;
                        }
                        else
                        {
                            name = rightChild.name;
                            leftChildGO = null;
                            rightChildGO = rightChild.gameObject;

                            missType = MissType.missLeft;

                            rightIndex++;
                        }
                    }
                }

                var childInfo = CompareGameObject(leftChildGO, rightChildGO, name, info.depth + 1, missType);

                childInfo.parent = info;

                info.children.Add(childInfo);

                if (!childInfo.AllEqual())
                {
                    childContentEqual = false;
                }
            }

            if (childCountEqual)
            {
                info.gameObjectCompareType |= GameObjectCompareType.childCountEqual;
            }

            if (childContentEqual)
            {
                info.gameObjectCompareType |= GameObjectCompareType.childContentEqual;
            }
            #endregion

            #region component

            if (info.missType == MissType.allExist)
            {
                CompareComponent(left, right, ref info);
            }

            #endregion
        }

        public static void CompareComponent(GameObject left, GameObject right, ref GameObjectCompareInfo info)
        {
            left.GetComponents(m_LeftComponentList);
            right.GetComponents(m_RightComponentList);

            var leftCount = m_LeftComponentList.Count;
            var rightCount = m_RightComponentList.Count;

            int leftIndex = 0;
            int rightIndex = 0;

            bool componentCountEqual = true;
            bool componentContentEqual = true;

            MissType missType;

            while (leftIndex < leftCount || rightIndex < rightCount)
            {
                string name;

                Component leftComponent;
                Component rightComponent;

                if (leftIndex >= leftCount)
                {
                    leftComponent = null;
                    rightComponent = m_RightComponentList[rightIndex];

                    name = rightComponent.GetType().FullName;

                    missType = MissType.missLeft;

                    rightIndex++;
                }
                else if (rightIndex >= rightCount)
                {
                    leftComponent = m_LeftComponentList[leftIndex];
                    rightComponent = null;

                    name = leftComponent.GetType().FullName;

                    missType = MissType.missRight;

                    leftIndex++;
                }
                else
                {
                    leftComponent = m_LeftComponentList[leftIndex];
                    rightComponent = m_RightComponentList[rightIndex];

                    if (leftComponent.GetType() == rightComponent.GetType())
                    {
                        name = leftComponent.GetType().FullName;

                        missType = MissType.allExist;

                        leftIndex++;
                        rightIndex++;
                    }
                    else
                    {
                        componentCountEqual = false;

                        if (leftIndex <= rightIndex)
                        {
                            rightComponent = null;

                            name = leftComponent.GetType().FullName;

                            missType = MissType.missRight;

                            leftIndex++;
                        }
                        else
                        {
                            leftComponent = null;

                            name = rightComponent.GetType().FullName;

                            missType = MissType.missLeft;

                            rightIndex++;
                        }
                    }
                }

                var childInfo = CompareComponent(leftComponent, rightComponent, name, info.depth, missType, info.fileID);

                childInfo.parent = info;

                info.components.Add(childInfo);

                if (!childInfo.AllEqual())
                {
                    componentContentEqual = false;
                }
            }

            if (componentCountEqual)
            {
                info.gameObjectCompareType |= GameObjectCompareType.componentCountEqual;
            }

            if (componentContentEqual)
            {
                info.gameObjectCompareType |= GameObjectCompareType.componentContentEqual;
            }
        }

        public static ComponentCompareInfo CompareComponent(Component left, Component right, string name, int depth, MissType missType, int fileID)
        {
            ComponentCompareInfo info = new ComponentCompareInfo(name, depth, ++idCounter);

            info.leftComponent = left;
            info.rightComponent = right;

            info.missType = missType;

            bool contentEqual = true;

            if (missType == MissType.allExist)
            {
                SerializedObject leftSO = new SerializedObject(left);
                SerializedObject rightSO = new SerializedObject(right);

                var property = leftSO.GetIterator();

                if (property.Next(true)) //跳过base
                {
                    do
                    {
                        var path = property.propertyPath;

                        if (string.IsNullOrWhiteSpace(path) || IsIngorePath(path))
                        {
                            continue;
                        }

                        var rightProperty = rightSO.FindProperty(path);

                        if (!PropertyEqual(property, rightProperty))
                        {
                            contentEqual = false;

                            //Debug.LogFormat("name:{0} property:{1}", name, path);
                        }
                    }
                    while (property.Next(false));
                }

                leftSO.Dispose();
                rightSO.Dispose();
            }

            if (contentEqual)
            {
                info.componentCompareType |= ComponentCompareType.contentEqual;
            }

            return info;
        }

        private static bool PropertyEqual(SerializedProperty left, SerializedProperty right)
        {
            switch (left.propertyType)
            {
                case SerializedPropertyType.ObjectReference:

                    if (left.objectReferenceValue == null && right.objectReferenceValue == null)
                    {
                        return true;
                    }
                    else if (left.objectReferenceValue == null || right.objectReferenceValue == null)
                    {
                        return false;
                    }

                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(left.objectReferenceValue, out string leftGUID, out long leftID);
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(right.objectReferenceValue, out string rightGUID, out long rightID);

                    if (leftGUID == rightGUID)
                    {
                        return true;
                    }

                    if (leftGUID == "" && rightGUID == "")
                    {
                        return true;
                    }

                    var leftType = left.objectReferenceValue.GetType();
                    var rightType = right.objectReferenceValue.GetType();

                    if (leftType == rightType)
                    {
                        if (leftType == typeof(GameObject))
                        {
                            var leftPath = GetFullPath(left.objectReferenceValue as GameObject);
                            var rightPath = GetFullPath(right.objectReferenceValue as GameObject);

                            if (leftPath == rightPath)
                            {
                                return true;
                            }
                        }
                        else if (leftType == typeof(Component))
                        {
                            var leftPath = GetFullPath((left.objectReferenceValue as Component).gameObject);
                            var rightPath = GetFullPath((right.objectReferenceValue as GameObject).gameObject);

                            if (leftPath == rightPath)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                case SerializedPropertyType.Generic: //数组相关
                    if (left.isArray && right.isArray)
                    {
                        if (left.arraySize != right.arraySize)
                        {
                            return false;
                        }

                        for (int i = 0; i < left.arraySize; i++)
                        {
                            var leftElement = left.GetArrayElementAtIndex(i);
                            var rightElement = right.GetArrayElementAtIndex(i);

                            if (!PropertyEqual(leftElement, rightElement))
                            {
                                return false;
                            }
                        }

                        return true;
                    }
                    else
                    {
                        return SerializedProperty.DataEquals(left, right);
                    }
                default:
                    return SerializedProperty.DataEquals(left, right);
            }
            return true;
        }

        private static string GetFullPath(GameObject go)
        {
            string path = "/" + go.name;
            while (go.transform.parent != null)
            {
                go = go.transform.parent.gameObject;
                path = "/" + go.name + path;
            }
            return path;
        }

        private static bool IsIngorePath(string path)
        {
            for (int i = 0; i < ingorePath.Length; i++)
            {
                if (path.StartsWith(ingorePath[i]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
