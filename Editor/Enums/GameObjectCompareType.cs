using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityCompare
{
    [Flags]
    public enum GameObjectCompareType
    {
        none = 0,
        activeEqual = 1 << 0,
        tagEqual = 1 << 1,
        layerEqual = 1 << 2,
        childCountEqual = 1 << 3,
        childContentEqual = 1 << 4,
        componentCountEqual = 1 << 5,
        componentContentEqual = 1 << 6,

        allEqual = activeEqual + tagEqual + layerEqual + childCountEqual + childContentEqual + componentCountEqual + componentContentEqual,
    }
}
