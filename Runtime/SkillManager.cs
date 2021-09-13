using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FinTOKMAK.SkillSystem
{
    [RequireComponent(typeof(SkillLogicManager))]
    public class SkillManager : MonoBehaviour
    {
        #region Public Field

        public float cdDetectionInterval = 0.1f;

        //所有可用技能的实现
        public Dictionary<string, Skill> skills = new Dictionary<string, Skill>();

        //所有的技能事件名称
        public SkillEventNameConfig eventNameConfig;

        //逻辑管理器(BUFF)执行具体的技能逻辑
        private SkillLogicManager _manager;

        public readonly Dictionary<string, Action> skillEvents = new Dictionary<string, Action>();

        // the timer for the cd counter
        private float _time;

        /// <summary>
        /// if localCD is enabled
        /// </summary>
        public bool canCountLocalCD => _canCountLocalCD;

        #endregion

        #region Private Field

        private bool _canCountLocalCD = true;

        #endregion

        private void Awake()
        {
            _manager = GetComponent<SkillLogicManager>();

            //获取所有的技能事件名称，并创建对应的匿名委托
            foreach (var name in eventNameConfig.eventNames) skillEvents.Add(name, () => { });

            //遍历所有的技能，并且将执行逻辑的触发条件，加入对应的事件监听中
            foreach (var skill in skills.Values)
            {
                skill.info.cumulateCount = skill.info.maxCumulateCount;
                skill.logic.id = skill.info.id;
                //如果技能为立即触发模式
                if (skill.info.triggerType == TriggerType.Instance)
                {
                    //监听技能对应的触发事件，当该事件触发时，将技能加入manager，并执行对应onAdd
                    skillEvents[skill.info.triggerEventName] += () =>
                    {
                        if (skill.info.cumulateCount > 0)
                        {
                            _manager.Add(skill.logic);
                            skill.info.cumulateCount--;
                            skill.info.cdEndTime = Time.realtimeSinceStartup + skill.info.cd;
                        }
                        else
                        {
                            Debug.Log("技能冷却中");
                        }
                    };
                }

                //如果技能为准备模式，则将
                else if (skill.info.triggerType == TriggerType.Prepared)
                {
                    //开始监听技能准备事件
                    skillEvents[skill.info.prepareEventName] += skill.logic.PrepareAction;
                    //PrepareAction应该实现的内容：
                    //public void PrepareAction()
                    //{
                    //    skillEvents[skill.TriggerActionName] += () => {
                    //        manager.Add(skill.skillLogic);
                    //    };
                    //}
                    //监听技能取消事件
                    foreach (var cancelAction in skill.info.cancelEventName)
                        skillEvents[cancelAction] += () =>
                        {
                            skillEvents[skill.info.prepareEventName] -= skill.logic.PrepareAction;
                        };
                }
            }
        }

        private void Start()
        {
            // Initialize all the skills
            foreach (Skill skill in skills.Values)
            {
                skill.logic.OnInitialization(_manager);
            }
        }

        private void Update()
        {
            if (_canCountLocalCD)
            {
                _time += Time.deltaTime;
                if (_time < cdDetectionInterval) //技能检测间隔
                    return;
                _time = 0;
                foreach (var skill in skills.Values) //遍历所有技能，检查CD时间
                    if (skill.info.cdEndTime < Time.realtimeSinceStartup &&
                        skill.info.cumulateCount < skill.info.maxCumulateCount)
                    {
                        skill.info.cdEndTime = Time.realtimeSinceStartup + skill.info.cd;
                        skill.info.cumulateCount++;
                    }
            }
        }

        /// <summary>
        ///     添加技能到可用技能列表
        /// </summary>
        /// <param name="logic">要添加的技能类型</param>
        public void Add(Skill skill)
        {
            Debug.Log($"AddSKill:{skill.info.id}");

            if (skills.ContainsKey(skill.info.id))
            {
                Debug.Log($"该技能已存在:{skill.info.id}");
                return;
            }

            skills.Add(skill.info.id, skill);
        }

        /// <summary>
        ///     将技能从可用列表中移除
        /// </summary>
        /// <param name="ID">技能ID</param>
        public void Remove(string ID)
        {
            if (skills.ContainsKey(ID))
            {
                Debug.Log("该技能不存在");
                return;
            }
            // Remove the skill with correspond ID
            var removeCount = skills.Remove(ID);
        }

        /// <summary>
        ///     获取列表里的技能
        /// </summary>
        /// <param name="ID">技能ID</param>
        /// <returns>返回对应技能，如果技能不存在则返回null</returns>
        public Skill Get(string ID)
        {
            return skills[ID]; //拿到第一个ID相同的技能
        }

        /// <summary>
        /// Clear all the skills
        /// </summary>
        public void Clear()
        {
            skills.Clear();
        }

        /// <summary>
        /// Invoke a skill event
        /// </summary>
        /// <param name="SkillEventName">the name of the skill event</param>
        public void SkillEvnetsInvoke(string SkillEventName)
        {
            skillEvents[SkillEventName]?.Invoke();
        }

        /// <summary>
        /// Call this method to get the skill cumulate count for all the skills
        /// </summary>
        /// <returns>The list of the cumulate count for all the skills</returns>
        public List<int> GetSkillCumulateCount()
        {
            List<int> res = new List<int>();
            foreach (Skill skill in skills.Values)
            {
                res.Add(skill.info.cumulateCount);
            }

            return res;
        }
    }
}