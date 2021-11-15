using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace FinTOKMAK.SkillSystem
{
    public delegate Task AsyncAction();

    [RequireComponent(typeof(SkillLogicManager))]
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
        /// The config file that store the name of all the skill events.
        /// </summary>
        public SkillEventNameConfig eventNameConfig;

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

        /// <summary>
        /// The skill event that available for skill instance to operate.
        /// Readonly.
        /// </summary>
        public Dictionary<string, AsyncAction> skillEvents
        {
            get
            {
                return _skillEvents;
            }
        }

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
        /// The skill event dictionary that work locally 
        /// </summary>
        private Dictionary<string, AsyncAction> _skillEvents = new Dictionary<string, AsyncAction>();

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
            
            // Initialize all the skills to the
            foreach (Skill skill in preLoadSkills)
            {
                Add(skill);
            }

            // Get the name of all the skills, create correspond Action
            foreach (var name in eventNameConfig.eventNames) _skillEvents.Add(name, async () => { });

            // Traverse all the skill and add the condition of event trigger into the correspond Action
            foreach (var skill in skills.Values)
            {
                // Initialize the cumulateCount
                skill.info.cumulateCount = skill.info.maxCumulateCount;
                // Initialize the cdEndTime
                skill.info.cdEndTime = Time.realtimeSinceStartup;
                skill.id = skill.info.id;
                // If the skill is Instance mode, trigger the event immediately when the event is invoked.
                if (skill.info.triggerType == TriggerType.Instance)
                {
                    // 监听技能对应的触发事件，当该事件触发时，将技能加入manager，并执行对应onAdd
                    // Add the trigger logic into the corresponding event.
                    // When the event is triggered, add the skill logic through SkillLogicManager.
                    // OnAdd method will be execute at that time by the SkillLogicManager.
                    _skillEvents[skill.info.triggerEventName] += async () =>
                    {
                        if (skill.info.cumulateCount > 0)
                        {
                            // Decrease the cumulateCount and update cd only if the skill execution failed.
                            bool success = await _logicManager.Add(skill);
                            if (success)
                            {
                                skill.info.cumulateCount--;
                                // Reset the cdEndTime to the cd + realtime only if the cdEndTime < realtime.
                                // If cdEndTime > realtime, don't change the cdEndTime.
                                // The skill cumulateCount will increment next time achieve the cdEndTime.
                                if (skill.info.cdEndTime < Time.realtimeSinceStartup)
                                {
                                    skill.info.cdEndTime = Time.realtimeSinceStartup + skill.info.cd;
                                } 
                            }
                        }
                        else
                        {
                            Debug.Log("The skill is still cooling.");
                        }
                    };
                }

                // If the skill is prepare mode, add the prepareAction to the prepare event
                else if (skill.info.triggerType == TriggerType.Prepared)
                {
                    // Start listening to the prepare event.
                    _skillEvents[skill.info.prepareEventName] += async () =>
                    {
                        // Check if the cumulateCount is enough to enter the prepare state
                        if (skill.info.cumulateCount <= 0)
                        {
                            Debug.Log("The skill is still cooling.");
                            return;
                        }

                        _skillEvents[skill.info.triggerEventName] += skill.ExecuteAction;
                        Debug.Log("The skill prepared.");
                    };
                    
                    // The event to cancel the prepare event.
                    foreach (var cancelAction in skill.info.cancelEventName)
                        _skillEvents[cancelAction] += async () =>
                        {
                            // Unregister the execute event
                            _skillEvents[skill.info.triggerEventName] -= skill.ExecuteAction;
                            Debug.Log("The skill prepare state canceled.");
                        };
                }
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
        public void Add(Skill skill)
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

            skills.Add(skill.info.id, skill);
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