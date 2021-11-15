using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace FinTOKMAK.SkillSystem
{
    /// <summary>
    /// The logic manager for the skill system.
    /// The main logic inside the skill is mostly handled by this system.
    /// </summary>
    public class SkillLogicManager : MonoBehaviour
    {
        /// <summary>
        /// The skill list.
        /// </summary>
        private readonly List<Skill> skillList = new List<Skill>();

        /// <summary>
        /// A timer to keep track of the realtimeSinceStartup.
        /// </summary>
        private int time;

        public void Update()
        {
            time = (int) (Time.realtimeSinceStartup * 1000f);
            skillList.RemoveAll(x =>
            {
                // When the skill is still active
                if (x.skillTerminateTime >= time) // 停止时间大于当前时间，说明技能没失效
                {
                    // Execute the continue lifecycle
                    if (x.effectType != SkillEffectType.ARMode && x.nextContinueExecuteTime <= time) // 检查技能模式和持续执行间隔
                    {
                        Debug.Log($"ContinueSkill:{x.id},ContinueDeltaTimeNext:{x.nextContinueExecuteTime},Time:{time}");
                        x.OnContinue();
                        x.nextContinueExecuteTime += x.continueDeltaTime * 1000f; // 计算下次执行间隔
                        Debug.Log($"NewContinueDeltaTimeNext={x.nextContinueExecuteTime}");
                    }

                    return false;
                }

                Debug.Log($"RemoveSkill:{x.id},continueStopTime:{x.skillTerminateTime},Time:{time}");
                x.OnRemove();
                return true;
            });
        }


        /// <summary>
        /// Trigger the skill logic
        /// </summary>
        /// <param name="logic">The skill to add</param>
        public async Task<bool> Add(Skill logic)
        {
            var theSkillLogic = skillList.FirstOrDefault(cus => cus.id == logic.id); // 拿到第一个ID相同的技能

            if (theSkillLogic == null)
            {
                logic.skillTerminateTime = time;
                skillList.Add(logic);
            }
            else
                logic = theSkillLogic;


            //if (logic.continueStopTime > time) // 停止时间大于当前时间，说明技能没失效
            //{

            //}
            //else // 停止时间小于当前时间，说明技能过期，应该被移除
            //{

            //}

            // When the skill cd overlay the original cd
            if (logic.skillTerminateTimeOverlay) // 技能覆盖模式
                logic.skillTerminateTime = (int) (time + logic.skillTime * 1000f); // 覆盖
            // TODO: 非Overlay模式第一次调用会导致技能释放失效，因为continueTime初始值为0
            else
                logic.skillTerminateTime += (int) (logic.skillTime * 1000f); // 非覆盖，时间累加模式

            logic.nextContinueExecuteTime = time + logic.continueDeltaTime * 1000f;
            bool success = await logic.OnAdd(theSkillLogic);
            Debug.Log(
                $"AddSKill[{logic.skillTerminateTimeOverlay}]:{logic.id},time{time},stop{logic.skillTerminateTime},[{logic.skillTime * 1000f}],success:{success}");
            return success;
        }

        /// <summary>
        /// Remove the skill
        /// </summary>
        /// <param name="id">The ID of the skill</param>
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
        /// Remove all the skill currently running.
        /// </summary>
        public void Clear()
        {
            if (skillList.Count != 0)
                skillList.Clear();
        }

        /// <summary>
        /// Get a skill in the skill list.
        /// </summary>
        /// <param name="id">The ID of the skill.</param>
        /// <returns>If the skill exist, return the skill. If not, return null</returns>
        public Skill Get(string id)
        {
            return skillList.FirstOrDefault(cus => cus.id == id); // 拿到第一个ID相同的技能
        }
    }
}