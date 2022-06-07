using FinTOKMAK.EventSystem.Editor;
using FinTOKMAK.EventSystem.Runtime;
using FinTOKMAK.SkillSystem.RunTime.SkillEvent;
using Hextant;
using UnityEditor;

namespace FinTOKMAK.SkillSystem.Editor
{
    [CustomPropertyDrawer(typeof(SkillEventAttribute))]
    public class SkillEventDrawer: UniversalEventDrawer
    {
        public override UniversalEventConfig GetEventConfig()
        {
            return Settings<SkillEventSettings>.instance.universalEventConfig;
        }
    }
}