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
        /// <summary>
        /// Skills that are added to the player when initialized
        /// </summary>
        public List<Skill> preLoadSkills;
        public Dictionary<string, Skill> skills = new Dictionary<string, Skill>();

        //所有的技能事件名称
        public SkillEventNameConfig eventNameConfig;

        //逻辑管理器(BUFF)执行具体的技能逻辑
        private SkillLogicManager _manager;

        /// <summary>
        /// The skill event hook that can inform external system the skill event has been called
        /// </summary>
        public Action<string> skillEventHook;

        // the timer for the cd counter
        private float _time;

        /// <summary>
        /// if localCD is enabled
        /// </summary>
        public bool useLocalSkillSystem
        {
            get
            {
                return _useLocalSkillSystem;
            }
            set
            {
                _useLocalSkillSystem = value;
            }
        }

        #endregion

        #region Private Field
        
        /// <summary>
        /// The skill event dictionary that work locally 
        /// </summary>
        private Dictionary<string, Action> _skillEvents = new Dictionary<string, Action>();

        private bool _useLocalSkillSystem = true;

        #endregion

        private void Awake()
        {
            _manager = GetComponent<SkillLogicManager>();
            
            // Initialize all the skills to the
            foreach (Skill skill in preLoadSkills)
            {
                Add(skill);
            }

            //获取所有的技能事件名称，并创建对应的匿名委托
            foreach (var name in eventNameConfig.eventNames) _skillEvents.Add(name, () => { });

            //遍历所有的技能，并且将执行逻辑的触发条件，加入对应的事件监听中
            foreach (var skill in skills.Values)
            {
                // Initialize the cumulateCount
                skill.info.cumulateCount = skill.info.maxCumulateCount;
                // Initialize the cdEndTime
                skill.info.cdEndTime = Time.realtimeSinceStartup;
                skill.logic.id = skill.info.id;
                //如果技能为立即触发模式
                if (skill.info.triggerType == TriggerType.Instance)
                {
                    //监听技能对应的触发事件，当该事件触发时，将技能加入manager，并执行对应onAdd
                    _skillEvents[skill.info.triggerEventName] += () =>
                    {
                        if (skill.info.cumulateCount > 0)
                        {
                            _manager.Add(skill.logic);
                            skill.info.cumulateCount--;
                            // Reset the cdEndTime to the cd + realtime only if the cdEndTime < realtime
                            if (skill.info.cdEndTime < Time.realtimeSinceStartup)
                            {
                                skill.info.cdEndTime = Time.realtimeSinceStartup + skill.info.cd;
                            }
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
                    _skillEvents[skill.info.prepareEventName] += skill.logic.PrepareAction;
                    //PrepareAction应该实现的内容：
                    //public void PrepareAction()
                    //{
                    //    skillEvents[skill.TriggerActionName] += () => {
                    //        manager.Add(skill.skillLogic);
                    //    };
                    //}
                    //监听技能取消事件
                    foreach (var cancelAction in skill.info.cancelEventName)
                        _skillEvents[cancelAction] += () =>
                        {
                            _skillEvents[skill.info.prepareEventName] -= skill.logic.PrepareAction;
                        };
                }
            }
        }

        private void Start()
        {
            // Initialize all the skills
            foreach (Skill skill in skills.Values)
            {
                // TODO: seperate skill logic here
                skill.logic.OnInitialization(_manager);
            }
        }

        private void Update()
        {
            if (_useLocalSkillSystem)
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
            // Instantiate (deep copy skill)
            skill = Instantiate(skill);
            skill.logic = Instantiate(skill.logic);
            skill.info = Instantiate(skill.info);
            
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
        /// <param name="skillEventName">the name of the skill event</param>
        public void SkillEventsInvoke(string skillEventName)
        {
            // invoke the skill logic only when local
            if (useLocalSkillSystem)
            {
                _skillEvents[skillEventName]?.Invoke();
            }
            else
            {
                skillEventHook?.Invoke(skillEventName);
            }
        }

        /// <summary>
        /// The struct for skill status storage
        /// </summary>
        [System.Serializable]
        public struct SkillStatus
        {
            /// <summary>
            /// The number of cumulated skill that can be used
            /// </summary>
            public int cumulateCount;

            /// <summary>
            /// The time left for skill to finish cd
            /// </summary>
            public float restCdTime;

            public override string ToString()
            {
                return $"{{cumulateCount: {cumulateCount}; restCdTime: {restCdTime}}}";
            }
        }

        /// <summary>
        /// Call this method to get the skill cumulate count for all the skills
        /// </summary>
        /// <returns>The dictionary of the cumulate count for all the skills, key is the unique id, value is the cumulateCount</returns>
        public Dictionary<string, SkillStatus> GetSkillInfo()
        {
            Dictionary<string, SkillStatus> res = new Dictionary<string, SkillStatus>();
            foreach (Skill skill in skills.Values)
            {
                res.Add(skill.info.id, new SkillStatus()
                {
                    cumulateCount = skill.info.cumulateCount,
                    restCdTime = skill.info.cdEndTime - Time.realtimeSinceStartup
                });
            }

            return res;
        }
        
        // TODO: Finish SetSkillCumulateCount
        /// <summary>
        /// Call this method to set the cumulate count of all the skills
        /// </summary>
        /// <param name="cdStatus">all the cumulate count of the skills</param>
        public void SetSkillStatus(Dictionary<string, SkillStatus> cdStatus)
        {
            foreach (string id in cdStatus.Keys)
            {
                skills[id].info.cumulateCount = cdStatus[id].cumulateCount;
                skills[id].info.cdEndTime = Time.realtimeSinceStartup + cdStatus[id].restCdTime;
            }
        }
    }
}