﻿using System;
using System.Collections.Generic;
using System.Linq;
using MVCF.Commands;
using MVCF.Comps;
using MVCF.VerbComps;
using Verse;

namespace MVCF
{
    public class VerbWithComps : ManagedVerb
    {
        public override void Initialize(Verb verb, AdditionalVerbProps props, IEnumerable<VerbCompProperties> additionalComps)
        {
            base.Initialize(verb, props, additionalComps);

            var comps = (props?.comps ?? Enumerable.Empty<VerbCompProperties>()).Concat(additionalComps ?? Enumerable.Empty<VerbCompProperties>());
            foreach (var compProps in comps)
            {
                var comp = (VerbComp) Activator.CreateInstance(compProps.compClass);
                comp.parent = this;
                AllComps.Add(comp);
                comp.Initialize(compProps);
            }
        }

        public override bool Available() => base.Available() && AllComps.All(comp => comp.Available());

        public override void Notify_ProjectileFired()
        {
            base.Notify_ProjectileFired();
            for (var i = 0; i < AllComps.Count; i++) AllComps[i].Notify_ShotFired();
        }

        protected override Command_ToggleVerbUsage GetToggleCommand(Thing ownerThing)
        {
            var command = base.GetToggleCommand(ownerThing);
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < AllComps.Count; i++)
            {
                var newCommand = AllComps[i].OverrideToggleCommand(command);
                if (newCommand is not null) return newCommand;
            }

            return command;
        }

        protected override Command_VerbTargetExtended GetTargetCommand(Thing ownerThing)
        {
            var command = base.GetTargetCommand(ownerThing);
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < AllComps.Count; i++)
            {
                var newCommand = AllComps[i].OverrideTargetCommand(command);
                if (newCommand is not null) return newCommand;
            }

            return command;
        }

        public override void Tick()
        {
            base.Tick();
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < AllComps.Count; i++) AllComps[i].CompTick();
        }

        public override IEnumerable<Gizmo> GetGizmos(Thing ownerThing) => base.GetGizmos(ownerThing).Concat(AllComps.SelectMany(comp => comp.CompGetGizmosExtra()));

        public override void ModifyProjectile(ref ThingDef projectile)
        {
            base.ModifyProjectile(ref projectile);
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < AllComps.Count; i++)
            {
                var newProj = AllComps[i].ProjectileOverride(projectile);
                if (newProj is null) continue;
                projectile = newProj;
                return;
            }
        }
    }
}