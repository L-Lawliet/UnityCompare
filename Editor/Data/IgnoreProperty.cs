using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 
/// author:罐子（Lawliet）
/// vindicator:配置属性忽略的类
/// versions:0.1.2
/// introduce:当对比属性时，某些属性我们需要忽略掉，就需要配置
/// note:
/// 由于是单例，因此需要ISerializationCallbackReceiver进行序列化
/// 
/// 
/// list:
/// 
/// 
/// 
/// </summary>
namespace UnityCompare
{
    [Serializable]
    public class IgnoreProperty
    {
        /// <summary>
        /// 正则规则
        /// </summary>
        [SerializeField]
        private string m_Pattern;

        public string pattern
        {
            get
            {
                return m_Pattern;
            }
            set
            {
                if(m_Pattern == value)
                {
                    return;
                }

                m_Pattern = value;

                m_Regex = new Regex(m_Pattern);
            }
        }

        /// <summary>
        /// 是否开启这个忽略
        /// </summary>
        [SerializeField]
        public bool on;

        private Regex m_Regex;

        public Regex regex
        {
            get
            {
                if(m_Regex == null)
                {
                    m_Regex = new Regex(pattern);
                }

                return m_Regex;
            }
        }

        public IgnoreProperty(string pattern, bool on = true)
        {
            this.pattern = pattern;
            this.on = on;
        }
    }
}
