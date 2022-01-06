using System.Collections.Generic;
using Hextant;

#if UNITY_EDITOR
using Hextant.Editor;
using UnityEditor;
#endif

namespace FinTOKMAK.SkillSystem.RunTime
{
    [Settings( SettingsUsage.RuntimeProject, "FinTOKMAK Skill System" )]
    public class SkillSettings : Settings<SkillSettings>
    {
        public List<Skill> availableSkills;
        
#if UNITY_EDITOR
        [SettingsProvider]
        static SettingsProvider GetSettingsProvider() =>
            instance.GetSettingsProvider();
#endif
    }
}