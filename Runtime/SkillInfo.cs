using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace FinTOKMAK.SkillSystem
{
    [CreateAssetMenu(fileName = "Skill Info Config", menuName = "FinTOKMAK/Skill System/Create Skill Info Config",
        order = 0)]
    public class SkillInfo : ScriptableObject
    {
        /// <summary>
        /// The unique ID of the skill
        /// </summary>
        public string id;
        /// <summary>
        /// The skill display name (for UI usage)
        /// </summary>
        public LocalizedString skillName;
        /// <summary>
        /// The detailed description of the skill (for UI usage)
        /// </summary>
        public LocalizedString description;

        /// <summary>
        /// The Icon of the skill
        /// </summary>
        public Texture skillIcon;

        /// <summary>
        /// The config file of skill events
        /// </summary>
        public SkillEventNameConfig eventNameConfig;
        /// <summary>
        /// The event name for direct skill trigger
        /// </summary>
        [Tooltip("This event will directly trigger and release the skill")]
        public string triggerEventName;
        /// <summary>
        /// The event name to trigger the event into prepare mode
        /// </summary>
        [Tooltip("This event will trigger the skill into the prepare mode, no need to config this event in Instance mode")]
        public string prepareEventName;
        /// <summary>
        /// The list of event to cancel the prepare state.
        /// </summary>
        [Tooltip("These events will cancel the prepare state of the event, no need to config this event in Instance mode")]
        public List<string> cancelEventName;
        
        /// <summary>
        /// The CD of the skill
        /// </summary>
        public float cd;
        
        /// <summary>
        /// The time skill will finish its cd
        /// </summary>
        [HideInInspector] public float cdEndTime;

        /// <summary>
        /// The maximum skill cumulate count.
        /// </summary>
        public int maxCumulateCount;

        /// <summary>
        /// Current available skill number
        /// </summary>
        [HideInInspector] public int cumulateCount;

        /// <summary>
        /// Does the skill fulfill the prerequisite skills
        /// </summary>
        [HideInInspector]
        public bool isActive;
        /// <summary>
        /// Skill prerequisite
        /// </summary>
        public List<string> needActiveSkillID;
        /// <summary>
        /// Skill trigger type
        /// </summary>
        public TriggerType triggerType;
    }

    public enum TriggerType
    {
        /// <summary>
        /// Instance trigger type, trigger the skill logic immediately when the event is triggered.
        /// </summary>
        Instance,
        /// <summary>
        /// Prepare event mode.
        /// Enter the prepare state when the prepare event is triggered.
        /// After that, the trigger event can trigger the main skill logic.
        /// </summary>
        Prepared
    }
}