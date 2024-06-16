using RimWorld;
using Verse;

namespace MedievalOverhaul
{
    public class TreatiseSkill : DefModExtension
    {
        public SkillDef skillDef;
    }
    public class RecipeWorker_MakeSkillBook : RecipeWorkerCounter
    {
        public override bool CanCountProducts(Bill_Production bill)
        {
            return true;
        }

        public override int CountProducts(Bill_Production bill)
        {
            int num = 0;
            var books = bill.Map.listerThings.ThingsOfDef(this.recipe.ProducedThingDef);
            var extension = recipe.GetModExtension<TreatiseSkill>();
            foreach (var book in books)
            {
                var compBook = book.TryGetComp<CompBook>();
                var doer = compBook.GetDoer<BookOutcomeDoerGainSkillExp>();
                if (doer != null && doer.Values.ContainsKey(extension.skillDef))
                {
                    num++;
                }
            }
            return num;
        }
    }
}
