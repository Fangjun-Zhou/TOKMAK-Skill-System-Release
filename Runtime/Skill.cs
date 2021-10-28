using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace FinTOKMAK.SkillSystem
{
    /// <summary>
    /// The base class of all the skill.
    /// Implement the main skill logic here.
    /// </summary>
    public class Skill : ScriptableObject
    {
        #region Public Field
        
        [BoxGroup("Skill Info")]
        [Tooltip("The universal skill info of the skill")]
        public SkillInfo info;

        /// <summary>
        /// The logic execution effect of the Skill.
        /// ARMode means execute OnAdd and OnRemove.
        /// ContinueMode means execute OnContinue.
        /// ARContinueMode means execute all of theme.
        /// </summary>
        [BoxGroup("Universal Skill Logic Settings")]
        [Tooltip("The execution type of the skill.")]
        public SkillEffectType effectType;
        
        // TODO: To be remove this continueTime. Use the timeline and callback function to control.
        /// <summary>
        /// The time span the Skill last.
        /// </summary>
        [FormerlySerializedAs("continueTime")]
        [BoxGroup("Universal Skill Logic Settings")]
        [Tooltip("The time span the skill can last.")]
        public float skillTime;

        /// <summary>
        /// Will a new instance of the skill adding to the skill system overwrite the current skill instance's stop time.
        /// </summary>
        [FormerlySerializedAs("continueStopTimeOverlay")]
        [BoxGroup("Universal Skill Logic Settings")]
        [Tooltip("If override the skill end time. If overlay, a new skill will not add the execution time to the remaining time")]
        public bool skillTerminateTimeOverlay;

        /// <summary>
        /// The delta time of OnContinue callback execution.
        /// </summary>
        [BoxGroup("Universal Skill Logic Settings")]
        [Tooltip("The deltaTime of calling Continue() method.")]
        public float continueDeltaTime;

        #endregion

        #region Hide Public Field

        /// <summary>
        /// The unique ID of the skill.
        /// </summary>
        [HideInInspector] public string id;
        
        
        /// <summary>
        /// The time the skill will be removed.
        /// </summary>
        [FormerlySerializedAs("continueStopTime")] [HideInInspector] public float skillTerminateTime;
        
        
        /// <summary>
        /// The next time OnContinue will execute.
        /// </summary>
        [FormerlySerializedAs("continueDeltaTimeNext")] [HideInInspector] public float nextContinueExecuteTime;

        #endregion

        #region Private Field

        /// <summary>
        /// The SkillLogicManager passed in when initialized.
        /// </summary>
        protected SkillLogicManager _manager;

        #endregion
        
        /// <summary>
        /// The logic that should be execute when prepare
        /// </summary>
        public virtual void PrepareAction()
        {
            
        }

        /// <summary>
        /// Call this method to initialize the skill, including getting necessary components
        /// <param name="manager">The SkillLogicManager to add the skill (in the SkillManager)</param>
        /// </summary>
        public virtual void OnInitialization(SkillLogicManager manager)
        {
            _manager = manager;
        }

        /// <summary>
        /// The OnAdd callback function.
        /// Called when the skill logic is added to the skill system (execute).
        /// Main logic of the skill should be write here.
        /// </summary>
        /// <param name="self">Possible another instance of current skill </param>
        public virtual void OnAdd(Skill self)
        {
        }

        /// <summary>
        /// The OnRemove callback function.
        /// The callback function execute when the skill logic is removed from the skill system.
        /// Most of the cleanup code should write here.
        /// </summary>
        public virtual void OnRemove()
        {
        }

        /// <summary>
        /// The OnContinue callback function.
        /// Similar to the Update callback function in the MonoBehaviour.
        /// </summary>
        public virtual void OnContinue()
        {
        }
    }

    /// <summary>
    /// Skill execution effect.
    /// </summary>
    public enum SkillEffectType
    {
        /// <summary>
        /// Effective when add and remove.
        /// Execute OnAdd and OnRemove.
        /// </summary>
        ARMode,

        /// <summary>
        /// Effective continuously.
        /// Execute OnContinue.
        /// </summary>
        ContinueMode,

        /// <summary>
        /// Effective all the time.
        /// Execute OnAdd, OnRemove, and OnContinue.
        /// </summary>
        ARContinueMode
    }
}