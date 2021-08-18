using System.Collections.Generic;
using UnityEngine;

namespace FinTOKMAK.SkillSystem
{
    [CreateAssetMenu(fileName = "Skill Event Name Config", menuName = "FinTOKMAK/Skill System/Create Skill Event Name Config", order = 0)]
    public class SkillEventNameConfig : ScriptableObject
    {
        /// <summary>
        /// 所有的技能事件名称
        /// </summary>
        public List<string> eventNames;
    }
}