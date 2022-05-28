using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTOKMAK.TimelineSystem.Runtime;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace FinTOKMAK.SkillSystem.RunTime
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
        /// The timeline that can be played by this Skill
        /// </summary>
        [BoxGroup("Skill Info")]
        [Tooltip("The timeline of this skill")]
        public Timeline timeline;

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
        public string id => info.id;

        /// <summary>
        /// If the skill is prepared.
        /// </summary>
        [HideInInspector]
        public bool prepared = false;
        
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
        /// The SkillManager passed in when initialized.
        /// </summary>
        protected SkillManager _manager;

        /// <summary>
        /// The SkillLogicManager passed in when initialized.
        /// </summary>
        protected SkillLogicManager _logicManager;

        #endregion
        
        /// <summary>
        /// The logic that should be execute when prepare
        /// </summary>
        public virtual async Task ExecuteAction()
        {
            if (info.cumulateCount > 0)
            {
                // Decrease the cumulateCount and update cd only if the skill execution failed.
                bool success = await _logicManager.Add(this);
                if (success)
                {
                    info.cumulateCount--;
                    // Reset the cdEndTime to the cd + realtime only if the cdEndTime < realtime.
                    // If cdEndTime > realtime, don't change the cdEndTime.
                    // The skill cumulateCount will increment next time achieve the cdEndTime.
                    if (info.cdEndTime < Time.realtimeSinceStartup)
                    {
                        info.cdEndTime = Time.realtimeSinceStartup + info.cd;
                    } 
                }
            }
            else
            {
                Debug.Log("The skill is still cooling.");
            }
        }

        /// <summary>
        /// Call this method to initialize the skill, including getting necessary components
        /// <param name="logicManager">The SkillLogicManager to add the skill (in the SkillManager)</param>
        /// </summary>
        public virtual void OnInitialization(SkillLogicManager logicManager, SkillManager manager)
        {
            _manager = manager;
            _logicManager = logicManager;
        }

        /// <summary>
        /// The OnAdd callback function.
        /// Called when the skill logic is added to the skill system (execute).
        /// Main logic of the skill should be write here.
        /// </summary>
        /// <param name="self">Possible another instance of current skill </param>
        /// <returns>true if the skill execute successfully, false if failed</returns>
        public virtual async Task<bool> OnAdd(Skill self)
        {
            return true;
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

        /// <summary>
        /// Call this method to play the timeline using the
        /// Timeline System in the _manager.
        /// </summary>
        public async Task PlayTimeline()
        {
            if (timeline == null)
            {
                throw new NullReferenceException($"Skill {info.id} need a timeline to play.");
            }
            _manager._timelineSystem.PlayTimeline(timeline);
        }

        /// <summary>
        /// Call the RPC using IRemoteSkillAgent.
        /// </summary>
        /// <param name="methodName">The name of method inside the current skill.</param>
        /// <param name="methodParams">The param of the method.</param>
        /// <returns>the return value of RPC call.</returns>
        /// <exception cref="NullReferenceException">If the manager does not have a IRemoteSkillAgent.</exception>
        protected async Task<object> CallRPC(string methodName, params object[] methodParams)
        {
            if (_manager.remoteSkillAgent == null)
            {
                throw new NullReferenceException("No remote skill agent in manager, RPC call failed.");
            }

            object res = await (_manager.remoteSkillAgent as IRemoteSkillAgent).RPCCall(this, methodName,
                methodParams);

            return res;
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