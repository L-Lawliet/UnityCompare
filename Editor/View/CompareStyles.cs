using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 
/// author:罐子（Lawliet）
/// vindicator:界面样式
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
    public class CompareStyles
    {
        public readonly GUIStyle styleToolBar = new GUIStyle("ToolBar");

        public readonly GUIStyle styleToolButton = new GUIStyle("ToolBarButton");

        public readonly Texture2D failImg = EditorGUIUtility.FindTexture("TestFailed");

        public readonly Texture2D ignoreImg = EditorGUIUtility.FindTexture("TestIgnored");

        public readonly Texture2D successImg = EditorGUIUtility.FindTexture("TestPassed");

        public readonly Texture2D unknownImg = EditorGUIUtility.FindTexture("TestNormal");

        public readonly Texture2D inconclusiveImg = EditorGUIUtility.FindTexture("TestInconclusive");

        public readonly GUIContent prevContent = new GUIContent("back", EditorGUIUtility.FindTexture("back"));


        #region 菜单

        public readonly GUIContent menuRemoveContent = new GUIContent("Remove");

        public readonly GUIContent menuCopyToTheLeftContent = new GUIContent("Copy to the Left");

        public readonly GUIContent menuCopyToTheRightContent = new GUIContent("Copy to the Right");

        public readonly GUIContent menuCopyValueToTheLeftContent = new GUIContent("Copy value to the Left");

        public readonly GUIContent menuCopyValueToTheRightContent = new GUIContent("Copy value to the Right");

        public readonly GUIContent menuInspectComponentsContent = new GUIContent("Inspect Components");

        #endregion 
    }
}
