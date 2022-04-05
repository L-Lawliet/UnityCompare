using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityCompare
{
    [Serializable]
    public abstract class CompareInfo
    {
        protected static readonly StringBuilder BUILDER_BUFFER = new StringBuilder();

        [SerializeField]
        private int m_ID;

        public int id
        {
            get { return m_ID; }
            set { m_ID = value; }
        }

        [SerializeField]
        private string m_Name;

        public string name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        [SerializeField]
        private int m_Depth;

        public int depth
        {
            get { return m_Depth; }
            set { m_Depth = value; }
        }

        [NonSerialized]
        private CompareInfo m_Parent;

        public CompareInfo parent
        {
            get { return m_Parent; }
            set { m_Parent = value; }
        }

        [SerializeField]
        private MissType m_MissType;

        public MissType missType
        {
            get { return m_MissType; }
            set { m_MissType = value; }
        }

        [SerializeField]
        private int m_FileID;

        public int fileID
        {
            get { return m_FileID; }
            set { m_FileID = value; }
        }

        public abstract bool AllEqual();

        public abstract string GetUnequalMessage();

        public CompareInfo()
        {

        }

        public CompareInfo(string name, int depth, int id)
        {
            m_Name = name;
            m_ID = id;
            m_Depth = depth;
        }
    }
}
