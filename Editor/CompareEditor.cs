using UnityEngine;
using UnityEditor;

/// <summary>
/// 
/// author:罐子（Lawliet）
/// vindicator:对比的菜单
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
    public class CompareEditor : ScriptableObject
    {
        [MenuItem("Assets/Compare")]
        static void Compare()
        {
            var gameObjects = Selection.gameObjects;

            if (gameObjects.Length >= 2)
            {
                var left = gameObjects[0];
                var right = gameObjects[1];

                var info = CompareUtility.ComparePrefab(left, right);

                ShowGameObjectInfo(info);
            }
        }

        private static void ShowGameObjectInfo(GameObjectCompareInfo info)
        {
            if (!info.AllEqual())
            {
                Debug.LogErrorFormat("go {0} no equal, missType:{1}   type:{2}", info.name, info.missType, info.gameObjectCompareType);

                for (int i = 0; i < info.children.Count; i++)
                {
                    ShowGameObjectInfo(info.children[i]);
                }

                for (int i = 0; i < info.components.Count; i++)
                {
                    ShowComponentInfo(info.components[i]);
                }
            }
        }

        private static void ShowComponentInfo(ComponentCompareInfo info)
        {
            if (!info.AllEqual())
            {
                Debug.LogErrorFormat("component {0} no equal, missType:{1}   type:{2}", info.name, info.missType, info.componentCompareType);
            }
        }
    }
}