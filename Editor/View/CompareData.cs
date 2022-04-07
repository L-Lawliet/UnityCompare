
using System;
using UnityEngine;

namespace UnityCompare
{
    [Serializable]
    public class CompareData : ISerializationCallbackReceiver
    {
        private static CompareData m_Instance;

        [SerializeField]
        private bool m_ShowEqual;

        public static bool showEqual
        {
            get { return m_Instance.m_ShowEqual; }
            set { m_Instance.m_ShowEqual = value; }
        }

        [SerializeField]
        private bool m_ShowMiss;

        public static bool showMiss
        {
            get { return m_Instance.m_ShowMiss; }
            set { m_Instance.m_ShowMiss = value; }
        }

        private Action m_OnShowStateChange;

        public static Action onShowStateChange
        {
            get { return m_Instance.m_OnShowStateChange; }
            set { m_Instance.m_OnShowStateChange = value;  }
        }

        public static void InitInstance()
        {
            m_Instance = new CompareData();
        }

        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            m_Instance = this;
        }
    }
}
