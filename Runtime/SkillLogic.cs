using UnityEngine;

namespace FinTOKMAK.SkillSystem
{
    /// <summary>
    ///     技能效果接口
    /// </summary>
    public class SkillLogic : ScriptableObject
    {
        /// <summary>
        ///     技能的ID
        /// </summary>
        [HideInInspector] public string id;

        /// <summary>
        ///     技能的持续时间(秒)
        /// </summary>
        public float continueTime;

        /// <summary>
        ///     技能的停止时间
        /// </summary>
        [HideInInspector] public float continueStopTime;

        /// <summary>
        ///     技能效果类型
        /// </summary>
        public SkillEffectType effectType;

        /// <summary>
        ///     技能停止时间是否使用覆盖模式
        /// </summary>
        public bool continueStopTimeOverlay;

        /// <summary>
        ///     技能在持续模式下的执行间隔(秒)
        /// </summary>
        public float continueDeltaTime;

        /// <summary>
        ///     技能在持续模式下的下一次间隔执行时间
        /// </summary>
        [HideInInspector] public float continueDeltaTimeNext;

        /// <summary>
        ///     技能被添加时执行的方法
        /// </summary>
        /// <param name="targer">添加技能的manager</param>
        /// <param name="self">可能存在的技能，如果不存在则为空</param>
        public virtual void OnAdd(SkillLogicManager targer, SkillLogic self)
        {
        }

        /// <summary>
        ///     技能被移除时执行的方法
        /// </summary>
        public virtual void OnRemove()
        {
        }

        /// <summary>
        ///     技能持续运行时执行的方法
        /// </summary>
        public virtual void OnContinue()
        {
        }
    }

    /// <summary>
    ///     技能效果类型
    /// </summary>
    public enum SkillEffectType
    {
        /// <summary>
        ///     添加、移除时生效
        /// </summary>
        ARMode,

        /// <summary>
        ///     持续生效
        /// </summary>
        ContinueMode,

        /// <summary>
        ///     添加、移除、持续生效
        /// </summary>
        ARContinueMode
    }
}