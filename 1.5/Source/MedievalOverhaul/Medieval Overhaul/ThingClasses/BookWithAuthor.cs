using Verse;

namespace MedievalOverhaul
{
    public class BookWithAuthor : Book
    {
        public Pawn author;
        public override void GenerateBook(Pawn author = null, long? fixedDate = null)
        {
            base.GenerateBook(author, fixedDate);
            this.author = author;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref author, "author");
        }
    }
}