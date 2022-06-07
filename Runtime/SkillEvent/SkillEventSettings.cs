using FinTOKMAK.EventSystem.Runtime;
using Hextant;
#if UNITY_EDITOR
using Hextant.Editor;
using UnityEditor;
#endif

namespace FinTOKMAK.SkillSystem.RunTime.SkillEvent
{
    /// <summary>
    /// The settings for global event, similar settings for local event
    /// </summary>
    [Settings( SettingsUsage.RuntimeProject, "FinTOKMAK Skill Event" )]
    public class SkillEventSettings :  Settings<SkillEventSettings>
    {
        public UniversalEventConfig universalEventConfig;
        
#if UNITY_EDITOR
        [SettingsProvider]
        static SettingsProvider GetSettingsProvider() =>
            instance.GetSettingsProvider();
#endif
    }
}