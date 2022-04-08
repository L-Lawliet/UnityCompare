using System;
using System.Collections.Generic;
using System.Text;
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
            "m_PrefabInstance",
        };

        private delegate bool PropertyComparer(SerializedProperty a, SerializedProperty b);

        private static Dictionary<SerializedPropertyType, PropertyComparer> CustomPropertyComparers = new Dictionary<SerializedPropertyType, PropertyComparer>()
        {
            { SerializedPropertyType.ObjectReference, ObjectReferenceComparer},
            { SerializedPropertyType.Generic, IgnoreComparer},
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

            CompareChild(left, right, info);

            #endregion

            #region component

            if (info.missType == MissType.allExist)
            {
                CompareComponent(left, right, ref info);
            }

            #endregion
        }

        private static void CompareChild(GameObject left, GameObject right, GameObjectCompareInfo info)
        {
            var leftChildCount = left.transform.childCount;
            var rightChildCount = right.transform.childCount;

            int leftIndex = 0;
            int rightIndex = 0;

            bool childCountEqual = true;
            bool childContentEqual = true;

            if(leftChildCount != rightChildCount)
            {
                childCountEqual = false;
            }

            while (leftIndex < leftChildCount || rightIndex < rightChildCount)
            {
                if (leftIndex >= leftChildCount)
                {
                    for (int i = rightIndex; i < rightChildCount; i++)
                    {
                        var rightChild = right.transform.GetChild(i);

                        var childInfo = AddChildInfo(info, null, rightChild.gameObject, rightChild.name, MissType.missLeft);

                        if (!childInfo.AllEqual())
                        {
                            childContentEqual = false;
                        }
                    }

                    break;
                }
                else if (rightIndex >= rightChildCount)
                {
                    for (int i = leftIndex; i < leftChildCount; i++)
                    {
                        var leftChild = left.transform.GetChild(i);

                        var childInfo = AddChildInfo(info, leftChild.gameObject, null, leftChild.name, MissType.missRight);

                        if (!childInfo.AllEqual())
                        {
                            childContentEqual = false;
                        }
                    }
                    
                    break;
                }
                else
                {
                    var leftChild = left.transform.GetChild(leftIndex);

                    var index = -1;

                    for (int i = rightIndex; i < rightChildCount; i++)
                    {
                        var rightChild = right.transform.GetChild(i);

                        if (leftChild.name == rightChild.name)
                        {
                            index = i;
                            break;
                        }
                    }

                    if(index == -1)
                    {
                        var childInfo = AddChildInfo(info, leftChild.gameObject, null, leftChild.name, MissType.missRight);

                        if (!childInfo.AllEqual())
                        {
                            childContentEqual = false;
                        }

                        leftIndex++;
                    }
                    else
                    {
                        Transform rightChild = null;
                        GameObjectCompareInfo childInfo = null;

                        for (int i = rightIndex; i < index; i++)
                        {
                            rightChild = right.transform.GetChild(i);

                            childInfo = AddChildInfo(info, null, rightChild.gameObject, rightChild.name, MissType.missLeft);

                            if (!childInfo.AllEqual())
                            {
                                childContentEqual = false;
                            }
                        }

                        rightChild = right.transform.GetChild(index);

                        childInfo = AddChildInfo(info, leftChild.gameObject, rightChild.gameObject, leftChild.name, MissType.allExist);

                        if (!childInfo.AllEqual())
                        {
                            childContentEqual = false;
                        }

                        leftIndex++;
                        rightIndex = index + 1;
                    }
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
        }

        private static GameObjectCompareInfo AddChildInfo(GameObjectCompareInfo parent, GameObject left, GameObject right, string name, MissType missType)
        {
            var childInfo = CompareGameObject(left, right, name, parent.depth + 1, missType);

            childInfo.parent = parent;

            parent.children.Add(childInfo);

            return childInfo;
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

            if (leftCount != rightCount)
            {
                componentCountEqual = false;
            }

            while (leftIndex < leftCount || rightIndex < rightCount)
            {
                if (leftIndex >= leftCount)
                {
                    for (int i = rightIndex; i < rightCount; i++)
                    {
                        var rightComponent = m_RightComponentList[rightIndex];

                        var childInfo = AddComponentInfo(info, null, rightComponent, rightComponent.GetType().FullName, MissType.missLeft);

                        if (!childInfo.AllEqual())
                        {
                            componentContentEqual = false;
                        }
                    }

                    break;
                }
                else if (rightIndex >= rightCount)
                {
                    for (int i = leftIndex; i < leftCount; i++)
                    {
                        var leftComponent = m_LeftComponentList[leftIndex];

                        var childInfo = AddComponentInfo(info, leftComponent, null, leftComponent.GetType().FullName, MissType.missRight);

                        if (!childInfo.AllEqual())
                        {
                            componentContentEqual = false;
                        }
                    }

                    break;
                }
                else
                {
                    var leftComponent = m_LeftComponentList[leftIndex];

                    var index = -1;

                    for (int i = rightIndex; i < rightCount; i++)
                    {
                        var rightComponent = m_RightComponentList[leftIndex];

                        if (leftComponent.GetType() == rightComponent.GetType())
                        {
                            index = i;
                            break;
                        }
                    }

                    if (index == -1)
                    {
                        var childInfo = AddComponentInfo(info, leftComponent, null, leftComponent.GetType().FullName, MissType.missRight);

                        if (!childInfo.AllEqual())
                        {
                            componentContentEqual = false;
                        }

                        leftIndex++;
                    }
                    else
                    {
                        Component rightComponent = null;
                        ComponentCompareInfo childInfo = null;

                        for (int i = rightIndex; i < index; i++)
                        {
                            rightComponent = m_RightComponentList[rightIndex];

                            childInfo = AddComponentInfo(info, null, rightComponent, rightComponent.GetType().FullName, MissType.missLeft);

                            if (!childInfo.AllEqual())
                            {
                                componentContentEqual = false;
                            }
                        }

                        rightComponent = m_RightComponentList[index];

                        childInfo = AddComponentInfo(info, leftComponent, rightComponent, leftComponent.GetType().FullName, MissType.allExist);

                        if (!childInfo.AllEqual())
                        {
                            componentContentEqual = false;
                        }

                        leftIndex++;
                        rightIndex = index + 1;
                    }
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

        private static ComponentCompareInfo AddComponentInfo(GameObjectCompareInfo parent, Component left, Component right, string name, MissType missType)
        {
            var componentInfo = CompareComponent(left, right, name, parent.depth, missType);

            componentInfo.parent = parent;

            parent.components.Add(componentInfo);

            return componentInfo;
        }

        public static ComponentCompareInfo CompareComponent(Component left, Component right, string name, int depth, MissType missType)
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

                bool enterChildren = true;

                if (property.Next(true)) //跳过base
                {
                    do
                    {
                        enterChildren = true;

                        var path = property.propertyPath;

                        if (string.IsNullOrWhiteSpace(path) || IsIngorePath(path))
                        {
                            continue;
                        }

                        var rightProperty = rightSO.FindProperty(path);

                        PropertyComparer comparer;

                        if (!CustomPropertyComparers.TryGetValue(property.propertyType, out comparer))
                        {
                            comparer = SerializedProperty.DataEquals;
                        }

                        if (!comparer(property, rightProperty))
                        {
                            contentEqual = false;

                            info.unequalPaths.Add(path);
                            //Debug.LogFormat("name:{0} property:{1}", name, path);
                        }

                        if(property.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            enterChildren = false;
                        }
                    }
                    while (property.Next(enterChildren));
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

        private static bool IgnoreComparer(SerializedProperty left, SerializedProperty right)
        {
            return true;
        }

        private static bool ObjectReferenceComparer(SerializedProperty left, SerializedProperty right)
        {
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
                    var leftPath = GetFullPath(left.objectReferenceValue as GameObject, true);
                    var rightPath = GetFullPath(right.objectReferenceValue as GameObject, true);

                    if (leftPath == rightPath)
                    {
                        return true;
                    }
                }
                else if (typeof(Component).IsAssignableFrom(leftType))
                {
                    var leftPath = GetFullPath((left.objectReferenceValue as Component).gameObject, true);
                    var rightPath = GetFullPath((right.objectReferenceValue as Component).gameObject, true);

                    if (leftPath == rightPath)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static void PrintProperty(SerializedObject left, SerializedObject right)
        {
            StringBuilder builder = new StringBuilder();

            var property = left.GetIterator();

            while (property.Next(true))
            {
                var rightProperty = right.FindProperty(property.propertyPath);

                builder.AppendFormat("path:{0}\ttype:{1}\tdataEquals:{2}\n", property.propertyPath, property.propertyType, SerializedProperty.DataEquals(property, rightProperty));
            }

            Debug.Log(builder.ToString());
        }

        private static string GetFullPath(GameObject go, bool ignoreRoot = false)
        {
            string path = "/" + go.name;
            while (go.transform.parent != null)
            {
                if(ignoreRoot && go.transform.parent.parent == null)
                {
                    break;
                }

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
