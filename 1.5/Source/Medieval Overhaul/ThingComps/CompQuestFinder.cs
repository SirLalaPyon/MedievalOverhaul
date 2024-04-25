using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static RimWorld.BaseGen.SymbolStack;

namespace MedievalOverhaul
{
    public class CompQuestFinder : CompScanner
    {
        private CompRefuelable compRefuelable;
        private QuestScriptDef currentQuest;
        private CompAffectedByFacilities facilities;
        private ThingDef targetMineable;        

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
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            this.SetDefaultTargetMineral();
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            GameComponent_QuestFinder.Instance.DeregisterFinder(this);
        }

        protected override void DoFind(Pawn worker)
        {
            if (this.currentQuest == QuestScriptDefOf.LongRangeMineralScannerLump)
            {
                Slate slate = new Slate();
                slate.Set<Map>("map", this.parent.Map, false);
                slate.Set<ThingDef>("targetMineable", this.targetMineable, false);
                slate.Set<ThingDef>("targetMineableThing", this.targetMineable.building.mineableThing, false);
                slate.Set<Pawn>("worker", worker, false);
                if (!QuestScriptDefOf.LongRangeMineralScannerLump.CanRun(slate))
                {
                    return;
                }
                Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(QuestScriptDefOf.LongRangeMineralScannerLump, slate);
                Find.LetterStack.ReceiveLetter(quest.name, quest.description, LetterDefOf.PositiveEvent, null, null, quest, null, null, 0, true);
            }
            else
            {
                Quest andMakeAvailable = QuestUtility.GenerateQuestAndMakeAvailable(this.currentQuest, StorytellerUtility.DefaultSiteThreatPointsNow());
                GameComponent_QuestFinder.Instance.Notify_QuestIssued(andMakeAvailable);
                QuestUtility.SendLetterQuestAvailable(andMakeAvailable);
            }
        }

        protected override bool TickDoesFind(float scanSpeed) => (double)this.daysWorkingSinceLastFinding >= (double)this.currentQuest.FinderInfo().WorkTillTrigger.TicksToDays();

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            IEnumerable<Gizmo> gizmosExtra = base.CompGetGizmosExtra();
            if (this.parent.Faction == Faction.OfPlayer)
            {
                Command_Action element = new Command_Action();
                element.defaultLabel = this.currentQuest.LabelCap();
                element.defaultDesc = (string)"EEG.SelectQuest".Translate((NamedArgument)this.currentQuest.LabelCap());
                element.icon = (Texture)TexButton.Search;
                element.action = (Action)(() => Find.WindowStack.Add((Window)new FloatMenu(this.AvailableForFind.Select<QuestScriptDef, FloatMenuOption>((Func<QuestScriptDef, FloatMenuOption>)(quest => new FloatMenuOption(quest.LabelCap(), (Action)(() => this.currentQuest = quest)))).ToList<FloatMenuOption>())));
                yield return element;
            }

            IEnumerator<Gizmo> enumerator = null;
            if (this.parent.Faction == Faction.OfPlayer && this.currentQuest == QuestScriptDefOf.LongRangeMineralScannerLump)
            {
                ThingDef mineableThing = this.targetMineable.building.mineableThing;
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "CommandSelectMineralToScanFor".Translate() + ": " + mineableThing.LabelCap;
                command_Action.defaultDesc = "CommandSelectMineralToScanForDesc".Translate();
                command_Action.icon = mineableThing.uiIcon;
                command_Action.iconAngle = mineableThing.uiIconAngle;
                command_Action.iconOffset = mineableThing.uiIconOffset;
                command_Action.action = delegate ()
                {
                    List<ThingDef> mineables = ((GenStep_PreciousLump)GenStepDefOf.PreciousLump.genStep).mineables;
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    foreach (ThingDef localD2 in mineables)
                    {
                        ThingDef localD = localD2;
                        FloatMenuOption item = new FloatMenuOption(localD.building.mineableThing.LabelCap, delegate ()
                        {
                            foreach (object obj in Find.Selector.SelectedObjects)
                            {
                                Thing thing = obj as Thing;
                                if (thing != null)
                                {
                                    CompQuestFinder compLongRangeMineralScanner = thing.TryGetComp<CompQuestFinder>();
                                    if (compLongRangeMineralScanner != null)
                                    {
                                        compLongRangeMineralScanner.targetMineable = localD;
                                    }
                                }
                            }
                        }, MenuOptionPriority.Default, null, null, 29f, (Rect rect) => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, localD.building.mineableThing), null, true, 0);
                        list.Add(item);
                    }
                    Find.WindowStack.Add(new FloatMenu(list));
                };
                yield return command_Action;
            }
            yield break;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look<QuestScriptDef>(ref this.currentQuest, "currentQuest");
            Scribe_Defs.Look<ThingDef>(ref this.targetMineable, "targetMineable");
            if (Scribe.mode == LoadSaveMode.PostLoadInit && this.targetMineable == null)
            {
                this.SetDefaultTargetMineral();
            }
        }
        private void SetDefaultTargetMineral()
        {
            this.targetMineable = ThingDefOf.MineableGold;
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
