using System.Collections.Generic;
using UnityEngine;

namespace FinTOKMAK.SkillSystem
{
    [CreateAssetMenu(fileName = "Skill Info Config", menuName = "FinTOKMAK/SkillSystem/Create Skill Info Config",
        order = 0)]
    public class SkillInfo : ScriptableObject
    {
        /// <summary>
        /// 技能的唯一ID
        /// </summary>
        public string id;
        /// <summary>
        /// 技能的名称（用户交互用）
        /// </summary>
        public string skillName;
        /// <summary>
        /// 技能的描述（用户交互用）
        /// </summary>
        public string description;
        /// <summary>
        /// 直接触发事件名称
        /// </summary>
        public string triggerEventName;
        /// <summary>
        /// 准备事件名称
        /// </summary>
        public string prepareEventName;
        /// <summary>
        /// 取消事件名称列表
        /// </summary>
        public List<string> cancelEventName;
        /// <summary>
        /// 技能的冷却cd
        /// </summary>
        public float cd;

        [HideInInspector] public float cdEndTime;

        /// <summary>
        /// 最大可积累技能数量
        /// </summary>
        public int maxActiveCount;

        /// <summary>
        /// 当前可用技能数量
        /// </summary>
        [HideInInspector] public int activeCount;

        /// <summary>
        /// 是否满足前置技能要求
        /// </summary>
        public bool isActive;
        /// <summary>
        /// 前置技能要求
        /// </summary>
        public List<string> needActiveSkillID;
        /// <summary>
        /// 技能触发类型
        /// </summary>
        public TriggerType triggerType;
    }

    public enum TriggerType
    {
        /// <summary>
        /// 立即触发模式，即事件直接触发技能
        /// </summary>
        Instance,
        /// <summary>
        /// 准备触发模式，即准备事件激活准备状态，进入准备状态后才可以由触发事件触发技能
        /// </summary>
        Prepared
    }
}