using System.Collections.Generic;
using UnityEngine;

namespace FinTOKMAK.SkillSystem
{
    [CreateAssetMenu(fileName = "Skill Info Config", menuName = "FinTOKMAK/Skill System/Create Skill Info Config",
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
        [TextArea]
        public string description;

        /// <summary>
        /// The Icon of the skill
        /// </summary>
        public Texture skillIcon;

        /// <summary>
        /// 技能事件配置文件
        /// </summary>
        public SkillEventNameConfig eventNameConfig;
        /// <summary>
        /// 直接触发事件名称
        /// </summary>
        [Tooltip("该事件会直接触发技能的释放")]
        public string triggerEventName;
        /// <summary>
        /// 准备事件名称
        /// </summary>
        [Tooltip("该事件会使技能进入准备状态，Instance触发模式的技能不需要配置")]
        public string prepareEventName;
        /// <summary>
        /// 取消事件名称列表
        /// </summary>
        [Tooltip("这些事件会使技能取消准备状态，Instance触发模式的技能不需要配置")]
        public List<string> cancelEventName;
        
        /// <summary>
        /// 技能的冷却cd
        /// </summary>
        public float cd;
        
        /// <summary>
        /// 结束冷却的时间
        /// </summary>
        [HideInInspector] public float cdEndTime;

        /// <summary>
        /// 最大可积累技能数量
        /// </summary>
        public int maxCumulateCount;

        /// <summary>
        /// 当前可用技能数量
        /// </summary>
        [HideInInspector] public int cumulateCount;

        /// <summary>
        /// 是否满足前置技能要求
        /// </summary>
        [HideInInspector]
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