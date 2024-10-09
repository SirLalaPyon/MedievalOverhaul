using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ImprovedNeurotrainers
{
    internal class CompUseEffect_LearnSkillImproved : CompUseEffect
    {
        public new CompPropertiesUseEffect_LearnSkillImproved Props => (CompPropertiesUseEffect_LearnSkillImproved)this.props;

        private SkillDef Skill => DefDatabase<SkillDef>.GetNamed(this.Props.skillDefName);

        public override void DoEffect(Pawn user)
        {
            base.DoEffect(user);
            SkillDef skill = this.Skill;
            int level1 = user.skills.GetSkill(skill).Level;
            user.skills.Learn(skill, this.Props.xpAmount, true);
            int level2 = user.skills.GetSkill(skill).Level;
            if (!PawnUtility.ShouldSendNotificationAbout(user))
                return;
            Messages.Message((string)"SkillNeurotrainerUsed".Translate((NamedArgument)user.LabelShort, (NamedArgument)skill.LabelCap, (NamedArgument)level1, (NamedArgument)level2, user.Named("USER")), (LookTargets)(Thing)user, MessageTypeDefOf.PositiveEvent);
        }
    }
}
