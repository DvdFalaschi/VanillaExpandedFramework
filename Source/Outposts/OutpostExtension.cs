﻿using System.Collections.Generic;
using System.Linq;
using System.Xml;
using RimWorld;
using Verse;

namespace Outposts
{
    public class OutpostExtension : DefModExtension
    {
        public List<BiomeDef> AllowedBiomes;
        public List<BiomeDef> DisallowedBiomes;
        public List<SkillDef> DisplaySkills;
        public int MinPawns;
        public ThingDef ProvidedFood;
        public int Range = -1;
        public List<AmountBySkill> RequiredSkills;
        public List<ResultOption> ResultOptions;
        public int TicksPerProduction = 15 * 60000;
        public int TicksToPack = 7 * 60000;

        public List<SkillDef> RelevantSkills =>
            new HashSet<SkillDef>(RequiredSkills.Select(rq => rq.Skill)
                    .Concat(ResultOptions.SelectMany(ro => ro.AmountsPerSkills.Select(aps => aps.Skill).Concat(ro.MinSkills.Select(ms => ms.Skill)))).Concat(DisplaySkills))
                .ToList();
    }

    public class ResultOption
    {
        public int AmountPerPawn;
        public List<AmountBySkill> AmountsPerSkills;
        public int BaseAmount;
        public List<AmountBySkill> MinSkills;
        public ThingDef Thing;

        public int Amount(List<Pawn> pawns) => BaseAmount + AmountPerPawn * pawns.Count + AmountsPerSkills.Sum(x => x.Amount(pawns));
        public IEnumerable<Thing> Make(List<Pawn> pawns) => Outpost.MakeThings(Thing, Amount(pawns));
        public string Explain(List<Pawn> pawns) => $"{Amount(pawns)}x {Thing.LabelCap}";
    }

    public class AmountBySkill
    {
        public int Count;
        public SkillDef Skill;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            if (xmlRoot.ChildNodes.Count != 1)
            {
                Log.Error("Misconfigured AmountBySkill: " + xmlRoot.OuterXml);
                return;
            }

            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "Skill", xmlRoot.Name);
            Count = ParseHelper.FromString<int>(xmlRoot.FirstChild.Value);
        }

        public int Amount(List<Pawn> pawns) => Count * pawns.Sum(p => p.skills.GetSkill(Skill).Level);
    }

    public static class Utils
    {
        public static bool SatisfiedBy(this List<AmountBySkill> minSkills, IEnumerable<Pawn> pawns)
        {
            return minSkills.All(abs => pawns.Sum(p => p.skills.GetSkill(abs.Skill).Level) >= abs.Count);
        }
    }
}