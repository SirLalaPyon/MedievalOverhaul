using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{
    public class CompQuestFinder : CompScanner
    {
        private CompRefuelable compRefuelable;
        private QuestScriptDef currentQuest;
        private CompAffectedByFacilities facilities;

        protected IEnumerable<QuestScriptDef> AvailableForFind => QuestFinderUtility.PossibleQuests.Where<QuestScriptDef>(new Func<QuestScriptDef, bool>(this.CanFind));

        protected virtual bool CanFind(QuestScriptDef quest)
        {
            QuestInformation ext = quest.FinderInfo();
            return ext.LinkablesNeeded <= this.facilities.LinkedFacilitiesListForReading.Count && (ext.requiredLinkable == null || this.facilities.LinkedFacilitiesListForReading.Any<Thing>((Predicate<Thing>)(f => f.def == ext.requiredLinkable))) && (!ext.onlyOnce || !GameComponent_QuestFinder.Instance.Completed(quest));
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this.compRefuelable = this.parent.TryGetComp<CompRefuelable>();
            this.facilities = this.parent.TryGetComp<CompAffectedByFacilities>();
            if (this.currentQuest == null)
                this.currentQuest = this.AvailableForFind.RandomElement<QuestScriptDef>();
            GameComponent_QuestFinder.Instance.RegisterFinder(this);
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            GameComponent_QuestFinder.Instance.DeregisterFinder(this);
        }

        protected override void DoFind(Pawn worker)
        {
            Quest andMakeAvailable = QuestUtility.GenerateQuestAndMakeAvailable(this.currentQuest, StorytellerUtility.DefaultSiteThreatPointsNow());
            GameComponent_QuestFinder.Instance.Notify_QuestIssued(andMakeAvailable);
            QuestUtility.SendLetterQuestAvailable(andMakeAvailable);
        }

        protected override bool TickDoesFind(float scanSpeed) => (double)this.daysWorkingSinceLastFinding >= (double)this.currentQuest.FinderInfo().WorkTillTrigger.TicksToDays();

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            IEnumerable<Gizmo> gizmosExtra = base.CompGetGizmosExtra();
            Command_Action element = new Command_Action();
            element.defaultLabel = this.currentQuest.LabelCap();
            element.defaultDesc = (string)"EEG.SelectQuest".Translate((NamedArgument)this.currentQuest.LabelCap());
            element.icon = (Texture)TexButton.Search;
            element.action = (Action)(() => Find.WindowStack.Add((Window)new FloatMenu(this.AvailableForFind.Select<QuestScriptDef, FloatMenuOption>((Func<QuestScriptDef, FloatMenuOption>)(quest => new FloatMenuOption(quest.LabelCap(), (Action)(() => this.currentQuest = quest)))).ToList<FloatMenuOption>())));
            return gizmosExtra.Append<Gizmo>((Gizmo)element);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look<QuestScriptDef>(ref this.currentQuest, "currentQuest");
        }

        public override string CompInspectStringExtra() => string.Format("{0}: {1}\n", (object)"EEG.SearchingFor".Translate(), (object)this.currentQuest.LabelCap()) + ((double)this.lastScanTick > (double)(Find.TickManager.TicksGame - 30) ? string.Format("{0}: {1}\n", (object)"UserScanAbility".Translate(), (object)this.lastUserSpeed.ToStringPercent()) : "") + string.Format("{0}: ", (object)"EEG.ScanningProgress".Translate()) + (this.daysWorkingSinceLastFinding / this.currentQuest.FinderInfo().WorkTillTrigger.TicksToDays()).ToStringPercent();

        public void Notify_QuestCompleted()
        {
            if (this.CanFind(this.currentQuest))
                return;
            this.currentQuest = this.AvailableForFind.RandomElement<QuestScriptDef>();
        }

        public bool CanUseNow
        {
            get
            {
                return this.parent.Spawned &&
                  (this.powerComp == null || this.powerComp.PowerOn) &&
                  (this.forbiddable == null || !this.forbiddable.Forbidden) &&
                  this.parent.Faction.IsPlayerSafe() &&
                  (this.compRefuelable == null || this.compRefuelable.HasFuel);
            }
        }
    }
}
