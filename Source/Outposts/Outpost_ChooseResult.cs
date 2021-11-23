﻿using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Outposts
{
    public class Outpost_ChooseResult : Outpost
    {
        private ThingDef choice;

        protected OutpostExtension_Choose ChooseExt => Ext as OutpostExtension_Choose;

        public override List<ResultOption> ResultOptions => Ext.ResultOptions.Where(ro => ro.Thing == choice).ToList();

        public override void PostMake()
        {
            base.PostMake();
            choice = Ext.ResultOptions.Concat(GetExtraOptions()).MinBy(ro => ro.MinSkills.Sum(abs => abs.Count)).Thing;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            return base.GetGizmos().Append(new Command_Action
            {
                action = () => Find.WindowStack.Add(new FloatMenu(Ext.ResultOptions.Concat(GetExtraOptions()).Select(ro => ro.MinSkills.SatisfiedBy(AllPawns)
                    ? new FloatMenuOption(ro.Thing.LabelCap, () => choice = ro.Thing, ro.Thing)
                    : new FloatMenuOption(ro.Thing.LabelCap + " - " + "Outposts.SkillTooLow".Translate(ro.MinSkills.Max(abs => abs.Count)), null, ro.Thing)).ToList())),
                defaultLabel = ChooseExt.ChooseLabel.Formatted(choice.label),
                defaultDesc = ChooseExt.ChooseDesc,
                icon = choice.uiIcon
            });
        }

        public virtual IEnumerable<ResultOption> GetExtraOptions()
        {
            yield break;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref choice, "choice");
        }
    }

    public class OutpostExtension_Choose : OutpostExtension
    {
        public string ChooseDesc;
        public string ChooseLabel;
    }
}