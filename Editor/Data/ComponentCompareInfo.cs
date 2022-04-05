using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityCompare
{
    [Serializable]
    public class ComponentCompareInfo : CompareInfo
    {
        [SerializeField]
        private ComponentCompareType m_ComponentCompareType;

        public ComponentCompareType componentCompareType
        {
            get { return m_ComponentCompareType; }
            set { m_ComponentCompareType = value; }
        }

        [NonSerialized]
        private Component m_LeftComponent;

        public Component leftComponent
        {
            get { return m_LeftComponent; }
            set { m_LeftComponent = value; }
        }

        [NonSerialized]
        private Component m_RightComponent;

        public Component rightComponent
        {
            get { return m_RightComponent; }
            set { m_RightComponent = value; }
        }

        private List<string> m_UnequalPaths = new List<string>();

        public List<string> unequalPaths
        {
            get { return m_UnequalPaths; }
            set { m_UnequalPaths = value; }
        }

        public override bool AllEqual()
        {
            return m_ComponentCompareType == ComponentCompareType.allEqual;
        }

        public override string GetUnequalMessage()
        {
            BUILDER_BUFFER.Clear();

            if (missType == MissType.allExist)
            {
                for (int i = 0; i < m_UnequalPaths.Count; i++)
                {
                    BUILDER_BUFFER.Append("\t");
                    BUILDER_BUFFER.AppendLine(m_UnequalPaths[i]);
                }
            }
            else
            {
                BUILDER_BUFFER.Append("\t");
                BUILDER_BUFFER.AppendLine(missType.ToString());
            }

            string message = BUILDER_BUFFER.ToString();

            BUILDER_BUFFER.Clear();

            return message;
        }

        public ComponentCompareInfo(string name, int depth, int id) : base(name, depth, id)
        {

        }
    }
}
