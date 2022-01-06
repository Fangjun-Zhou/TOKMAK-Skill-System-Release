using System.Collections.Generic;
using UnityEngine;

namespace FinTOKMAK.SkillSystem.RunTime
{
    [CreateAssetMenu(fileName = "Skill Event Name Config", menuName = "FinTOKMAK/Skill System/Create Skill Event Name Config", order = 0)]
    public class SkillEventNameConfig : ScriptableObject
    {
        /// <summary>
        /// All the skill names
        /// </summary>
        public List<string> eventNames;
    }
}