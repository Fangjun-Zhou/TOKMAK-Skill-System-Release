using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FinTOKMAK.SkillSystem
{
    /// <summary>
    ///     附加在游戏对象上的技能管理系统
    /// </summary>
    public class SkillLogicManager : MonoBehaviour
    {
        /// <summary>
        ///     技能列表
        /// </summary>
        private readonly List<SkillLogic> skillList = new List<SkillLogic>();

        private int time;

        public void Update()
        {
            time = (int) (Time.realtimeSinceStartup * 1000f);
            skillList.RemoveAll(x =>
            {
                if (x.continueStopTime >= time) //停止时间大于当前时间，说明技能没失效
                {
                    if (x.effectType != SkillEffectType.ARMode && x.continueDeltaTimeNext <= time) //检查技能模式和持续执行间隔
                    {
                        Debug.Log($"ContinueSkill:{x.id},ContinueDeltaTimeNext:{x.continueDeltaTimeNext},Time:{time}");
                        x.OnContinue();
                        x.continueDeltaTimeNext += x.continueDeltaTime * 1000f; //计算下次执行间隔
                        Debug.Log($"NewContinueDeltaTimeNext={x.continueDeltaTimeNext}");
                    }

                    return false;
                }

                Debug.Log($"RemoveSkill:{x.id},Time:{time}");
                x.OnRemove();
                return true;
            });
        }


        /// <summary>
        ///     触发技能
        /// </summary>
        /// <param name="logic">要添加的技能类型</param>
        public void Add(SkillLogic logic)
        {
            var theSkillLogic = skillList.FirstOrDefault(cus => cus.id == logic.id); //拿到第一个ID相同的技能

            if (theSkillLogic == null)
                skillList.Add(logic);
            else
                logic = theSkillLogic;


            //if (logic.continueStopTime > time)//停止时间大于当前时间，说明技能没失效
            //{

            //}
            //else//停止时间小于当前时间，说明技能过期，应该被移除
            //{

            //}

            if (logic.continueStopTimeOverlay) //技能覆盖模式
                logic.continueStopTime = (int) (time + logic.continueTime * 1000f); //覆盖
            else
                logic.continueStopTime += (int) (logic.continueTime * 1000f); //非覆盖，时间累加模式

            logic.continueDeltaTimeNext = time + logic.continueDeltaTime * 1000f;
            logic.OnAdd(this, theSkillLogic);
            Debug.Log(
                $"AddSKill[{logic.continueStopTimeOverlay}]:{logic.id},time{time},stop{logic.continueStopTime},[{logic.continueTime * 1000f}]");
        }

        /// <summary>
        ///     移除技能
        /// </summary>
        /// <param name="id">技能ID</param>
        public void Remove(string id)
        {
            skillList.RemoveAll(x =>
            {
                if (x.id == id)
                {
                    x.OnRemove();

                    return true;
                }

                return false;
            });
        }

        /// <summary>
        ///     清理所有的技能
        /// </summary>
        public void Clear()
        {
            if (skillList.Count != 0)
                skillList.Clear();
        }

        /// <summary>
        ///     获取已有的技能
        /// </summary>
        /// <param name="id">技能的ID</param>
        /// <returns>返回已有的技能,如果没有，则返回Null</returns>
        public SkillLogic Get(string id)
        {
            return skillList.FirstOrDefault(cus => cus.id == id); //拿到第一个ID相同的技能
        }
    }
}