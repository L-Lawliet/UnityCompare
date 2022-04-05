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

        public override bool AllEqual()
        {
            return m_ComponentCompareType == ComponentCompareType.allEqual;
        }

        public ComponentCompareInfo(string name, int depth, int id) : base(name, depth, id)
        {

        }
    }
}
