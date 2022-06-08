using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTOKMAK.EventSystem.Runtime;
using FinTOKMAK.SkillSystem.RunTime.SkillEvent;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace FinTOKMAK.SkillSystem.RunTime
{
    [System.Serializable]
    public class SerializableSkillCallback: SerializableCallback<string, Task>{}
    
    [System.Serializable]
    public class SkillMountPointDict : SerializableDictionary<string, Transform>{}

    [RequireComponent(typeof(SkillLogicManager))]
    [RequireComponent(typeof(TimelineSystem.Runtime.TimelineSystem))]
    public class SkillManager : MonoBehaviour
    {
        #region Public Field

        /// <summary>
        /// The time interval of cd detection. Smaller means higher cd detect frequency.
        /// </summary>
        public float cdDetectionInterval = 0.1f;

        /// <summary>
        /// Skills that are added to the player when initialized.
        /// </summary>
        public List<Skill> preLoadSkills;
        /// <summary>
        /// All the skills currently in the system.
        /// Keys are skill id, values are skill.
        /// </summary>
        public Dictionary<string, Skill> skills = new Dictionary<string, Skill>();

        /// <summary>
        /// The skill event hook that can inform external system the skill event has been called
        /// </summary>
        public Action<string> skillEventHook;

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

        [InterfaceType(typeof(IRemoteSkillAgent))]
        public MonoBehaviour remoteSkillAgent;

        [BoxGroup("Mount Point")]
        public SkillMountPointDict mountPoints;

        [BoxGroup("Skill Event Manager")]
        public SkillEventManager eventManager;

        [BoxGroup("Skill Events")]
        public UnityEvent<string> skillExecute;
        
        [BoxGroup("Skill Events")]
        public SerializableSkillCallback skillPrepared;
        
        [BoxGroup("Skill Events")]
        public SerializableSkillCallback skillCanceled;

        #endregion

        #region Hide Public Field

        /// <summary>
        /// The core timeline system manager used by current skill system.
        /// </summary>
        [HideInInspector]
        public TimelineSystem.Runtime.TimelineSystem _timelineSystem;

        #endregion

        #region Private Field
        
        /// <summary>
        /// The skill logic manager that add and remove the skill logic.
        /// Works in a similar way comparing to the Buff system.
        /// </summary>
        private SkillLogicManager _logicManager;
        
        /// <summary>
        /// The timer to detect cd, the cdDetectionInterval control the frequency of cd detection.
        /// </summary>
        private float _time;

        /// <summary>
        /// If use local skill system.
        /// When unchecked, the skill system will be taken over by the outer system
        /// such as the one on the server.
        /// </summary>
        private bool _useLocalSkillSystem = true;

        #endregion

        private void Awake()
        {
            _logicManager = GetComponent<SkillLogicManager>();
            _timelineSystem = GetComponent<TimelineSystem.Runtime.TimelineSystem>();

            // Initialize all the skills to the
            foreach (Skill skill in preLoadSkills)
            {
                Add(skill, false);
            }

            ((IRemoteSkillAgent) remoteSkillAgent).skillManager = this;
        }

        private void Start()
        {
            // Initialize all the skills
            foreach (Skill skill in skills.Values)
            {
                // TODO: separate skill logic here
                skill.OnInitialization(_logicManager, this);
            }
        }

        private void Update()
        {
            if (_useLocalSkillSystem)
            {
                _time += Time.deltaTime;
                // The cd detect time interval
                if (_time < cdDetectionInterval)
                    return;
                _time = 0;
                // Traverse all the skills and check the cd time
                foreach (var skill in skills.Values) // 遍历所有技能，检查CD时间
                    if (skill.info.cdEndTime < Time.realtimeSinceStartup &&
                        skill.info.cumulateCount < skill.info.maxCumulateCount)
                    {
                        skill.info.cumulateCount++;
                        // Increment the next cdEndTime only if not reach the max cumulateCount
                        if (skill.info.cumulateCount < skill.info.maxCumulateCount)
                        {
                            skill.info.cdEndTime = Time.realtimeSinceStartup + skill.info.cd;
                        }
                    }
            }
        }

        /// <summary>
        /// Add a new skill to the skill dictionary.
        /// </summary>
        /// <param name="skill">The skill instance to add.</param>
        /// <param name="initialze">if initialize the added skill</param>
        public void Add(Skill skill, bool initialze)
        {
            // Instantiate (deep copy skill)
            skill = Instantiate(skill);
            skill.info = Instantiate(skill.info);
            
            Debug.Log($"AddSKill:{skill.info.id}");

            if (skills.ContainsKey(skill.info.id))
            {
                Debug.Log($"The same skill already exists:{skill.info.id}");
                return;
            }
            
            // Initialize the cumulateCount
            skill.info.cumulateCount = skill.info.maxCumulateCount;
            // Initialize the cdEndTime
            skill.info.cdEndTime = Time.realtimeSinceStartup;
            // If the skill is Instance mode, trigger the event immediately when the event is invoked.
            if (skill.info.triggerType == TriggerType.Instance)
            {
                async Task InstanceExecutionAction(IEventData data)
                {
                    await skill.ExecuteAction();
                    skillExecute?.Invoke(skill.id);
                }
                
                // Add the trigger logic into the corresponding event.
                // When the event is triggered, add the skill logic through SkillLogicManager.
                // OnAdd method will be execute at that time by the SkillLogicManager.
                eventManager.RegisterEvent(skill.info.triggerEventName, InstanceExecutionAction);
            }

            // If the skill is prepare mode, add the prepareAction to the prepare event
            else if (skill.info.triggerType == TriggerType.Prepared)
            {
                // Cancel the preparation.
                async Task CancelAction(IEventData data)
                {
                    Task cancelTask = skillCanceled?.Invoke(skill.id);
                    if (cancelTask != null)
                        await cancelTask;
                    
                    // Unregister the execute event
                    eventManager.UnRegisterEvent(skill.info.triggerEventName, PrepareExecuteAction);
                    foreach (var cancelEvent in skill.info.cancelEventName)
                        eventManager.UnRegisterEvent(cancelEvent.eventName, CancelAction);
                    Debug.Log("The skill prepare state canceled.");
                    
                    skill.prepared = false;
                }
                
                // Execution process when in prepare.
                async Task PrepareExecuteAction(IEventData data)
                {
                    await skill.ExecuteAction();
                    skillExecute?.Invoke(skill.id);
                    await CancelAction(null);
                }
                
                // Start listening to the prepare event.
                eventManager.RegisterEvent(skill.info.prepareEventName, async data =>
                {
                    // Check if the cumulateCount is enough to enter the prepare state
                    if (skill.info.cumulateCount <= 0)
                    {
                        Debug.Log("The skill is still cooling.");
                        return;
                    }

                    if (skill.prepared)
                    {
                        Debug.Log("Skill is already prepared");
                        return;
                    }
                    
                    Task prepareTask = skillPrepared?.Invoke(skill.id);
                    if (prepareTask != null)
                        await prepareTask;

                    eventManager.RegisterEvent(skill.info.triggerEventName, PrepareExecuteAction);
                    
                    // The event to cancel the prepare event.
                    foreach (var cancelEvent in skill.info.cancelEventName)
                    {
                        eventManager.RegisterEvent(cancelEvent.eventName, CancelAction);
                    }

                    skill.prepared = true;
                    
                    Debug.Log("The skill prepared.");
                });
            }

            skills.Add(skill.info.id, skill);
            if (initialze)
                skill.OnInitialization(_logicManager, this);
        }

        /// <summary>
        /// Remove a skill from the skill dictionary.
        /// </summary>
        /// <param name="ID">The unique ID of the skill</param>
        public void Remove(string ID)
        {
            if (!skills.ContainsKey(ID))
            {
                Debug.Log("The skill with certain name does not exist.");
                return;
            }
            // Remove the skill with correspond ID
            var removeCount = skills.Remove(ID);
        }

        /// <summary>
        /// Get the skill in the skill dictonary.
        /// </summary>
        /// <param name="ID">The unique ID of the skill.</param>
        /// <returns>Return the instance of the skill.</returns>
        public Skill Get(string ID)
        {
            return skills[ID]; // 拿到第一个ID相同的技能
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
                eventManager.InvokeEvent(skillEventName, null);
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