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

            if (gameObjects.Length != 2)
            {
                EditorUtility.DisplayDialog("Error", "需要选中两个Prefab进行对比", "ok");
            }
            else
            {
                var left = gameObjects[0];
                var right = gameObjects[1];

                CompareWindow.ComparePrefab(left, right);
            }
        }

        [MenuItem("Assets/RemoveTest")]
        static void RemoveTest()
        {
            var select = Selection.activeGameObject;

            if(select != null)
            {
                var path = AssetDatabase.GetAssetPath(select);

                var content = PrefabUtility.LoadPrefabContents(path);

                var cube = content.transform.Find("Cube (3)");

                DestroyImmediate(cube.gameObject);

                PrefabUtility.SaveAsPrefabAsset(content, path);
                PrefabUtility.UnloadPrefabContents(content);
            }
        }
    }
}