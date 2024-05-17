using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    public class GameComponent_QuestFinder : GameComponent
    {
        public static GameComponent_QuestFinder Instance;
        private HashSet<QuestScriptDef> completed = new HashSet<QuestScriptDef>();
        private List<CompQuestFinder> finders = new List<CompQuestFinder>();
        private HashSet<Quest> givenByFinder = new HashSet<Quest>();

        public GameComponent_QuestFinder(Game game) => GameComponent_QuestFinder.Instance = this;

        public void Notify_QuestComplete(Quest quest, QuestEndOutcome outcome)
        {
            if (!this.givenByFinder.Contains(quest))
                return;
            this.givenByFinder.Remove(quest);
            if (outcome == QuestEndOutcome.Success)
            {
                this.completed.Add(quest.root);
                foreach (CompQuestFinder finder in this.finders)
                    finder.Notify_QuestCompleted();
            }
        }

        public bool Completed(QuestScriptDef quest) => this.completed.Contains(quest);

        public void Notify_QuestIssued(Quest quest) => this.givenByFinder.Add(quest);

        public void RegisterFinder(CompQuestFinder finder)
        {
            if (this.finders.Contains(finder))
                return;
            this.finders.Add(finder);
        }

        public void DeregisterFinder(CompQuestFinder finder) => this.finders.Remove(finder);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<QuestScriptDef>(ref this.completed, "completed", LookMode.Def);
            Scribe_Collections.Look<Quest>(ref this.givenByFinder, "givenByFinder", LookMode.Reference);
            List<ThingWithComps> list = this.finders.Select<CompQuestFinder, ThingWithComps>((Func<CompQuestFinder, ThingWithComps>)(f => f.parent)).ToList<ThingWithComps>();
            Scribe_Collections.Look<ThingWithComps>(ref list, "finders", LookMode.Reference);
            if (this.completed == null)
                this.completed = new HashSet<QuestScriptDef>();
            if (this.givenByFinder == null)
                this.givenByFinder = new HashSet<Quest>();
            if (this.finders == null)
                this.finders = new List<CompQuestFinder>();
            if (Scribe.mode != LoadSaveMode.PostLoadInit)
                return;
            this.finders = list.Select<ThingWithComps, CompQuestFinder>((Func<ThingWithComps, CompQuestFinder>)(t => t.TryGetComp<CompQuestFinder>())).ToList<CompQuestFinder>();
        }
    }
}
