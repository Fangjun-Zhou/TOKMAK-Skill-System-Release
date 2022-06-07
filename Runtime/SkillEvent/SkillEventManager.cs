using FinTOKMAK.EventSystem.Runtime;
using Hextant;

namespace FinTOKMAK.SkillSystem.RunTime.SkillEvent
{
    public class SkillEventManager: AsyncUniversalEventManager
    {
        public override UniversalEventConfig GetEventConfig()
        {
            return Settings<SkillEventSettings>.instance.universalEventConfig;
        }
    }
}