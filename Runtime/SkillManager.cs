using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FinTOKMAK.SkillSystem
{
    [RequireComponent(typeof(SkillLogicManager))]
    public class SkillManager : MonoBehaviour
    {
        public float cdDetectionInterval = 0.1f;

        //所有可用技能的实现
        public List<Skill> skills = new List<Skill>();

        //所有的技能事件名称
        public List<string> skillEventsName = new List<string>();

        //逻辑管理器(BUFF)执行具体的技能逻辑
        private SkillLogicManager _manager;

        public readonly Dictionary<string, Action> skillEvents = new Dictionary<string, Action>();

        private float _time;

        private void Awake()
        {
            _manager = GetComponent<SkillLogicManager>();

            //获取所有的技能事件名称，并创建对应的匿名委托
            foreach (var name in skillEventsName) skillEvents.Add(name, () => { });

            //遍历所有的技能，并且将执行逻辑的触发条件，加入对应的事件监听中
            foreach (var skill in skills)
            {
                skill.info.activeCount = skill.info.maxActiveCount;
                skill.logic.id = skill.info.id;
                //如果技能为立即触发模式
                if (skill.info.triggerType == TriggerType.Instance)
                {
                    //监听技能对应的触发事件，当该事件触发时，将技能加入manager，并执行对应onAdd
                    skillEvents[skill.info.triggerEventName] += () =>
                    {
                        if (skill.info.activeCount > 0)
                        {
                            _manager.Add(skill.logic);
                            skill.info.activeCount--;
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
                    skillEvents[skill.info.prepareEventName] += skill.PrepareAction;
                    //PrepareAction应该实现的内容：
                    //public void PrepareAction()
                    //{
                    //    skillEvents[skill.TriggerActionName] += () => {
                    //        manager.Add(skill.skillLogic);
                    //    };
                    //}
                    //监听技能取消事件
                    foreach (var CancelAction in skill.info.cancelEventName)
                        skillEvents[CancelAction] += () =>
                        {
                            skillEvents[skill.info.prepareEventName] -= skill.PrepareAction;
                        };
                }
            }
        }

        private void Start()
        {
        }

        private void Update()
        {
            _time += Time.deltaTime;
            if (_time < cdDetectionInterval) //技能检测间隔
                return;
            _time = 0;
            foreach (var skill in skills) //遍历所有技能，检查CD时间
                if (skill.info.cdEndTime < Time.realtimeSinceStartup &&
                    skill.info.activeCount < skill.info.maxActiveCount)
                {
                    skill.info.cdEndTime = Time.realtimeSinceStartup + skill.info.cd;
                    skill.info.activeCount++;
                }
        }

        /// <summary>
        ///     添加技能到可用技能列表
        /// </summary>
        /// <param name="logic">要添加的技能类型</param>
        public void Add(Skill skill)
        {
            Debug.Log($"AddSKill:{skill.info.id}");
            var theSkillLogic = skills.FirstOrDefault(cus => cus.info.id == skill.info.id); //拿到第一个ID相同的技能
            if (theSkillLogic == null)
                skills.Add(skill);
            else
                Debug.Log($"该技能已存在:{skill.info.id}");
        }

        /// <summary>
        ///     将技能从可用列表中移除
        /// </summary>
        /// <param name="ID">技能ID</param>
        public void Remove(string ID)
        {
            var removeCount = skills.RemoveAll(cus => cus.info.id == ID); //拿到第一个ID相同的技能
            if (removeCount >= 1)
                Debug.Log("该技能已移除");
            else
                Debug.Log("该技能不存在");
        }

        /// <summary>
        ///     获取列表里的技能
        /// </summary>
        /// <param name="ID">技能ID</param>
        /// <returns>返回对应技能，如果技能不存在则返回null</returns>
        public Skill Get(string ID)
        {
            return skills.FirstOrDefault(cus => cus.info.id == ID); //拿到第一个ID相同的技能
        }

        public void Clear()
        {
            skills.Clear();
        }

        public void SkillEvnetsInvoke(string SkillEventName)
        {
            skillEvents[SkillEventName]?.Invoke();
        }
    }
}