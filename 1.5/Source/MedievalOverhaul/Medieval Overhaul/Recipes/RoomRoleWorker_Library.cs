using RimWorld;
using Verse;

namespace MedievalOverhaul
{
    public class RoomRoleWorker_Library : RoomRoleWorker
    {
        public override float GetScore(Room room)
        {
            int num = 0;
            foreach (var t in room.ContainedAndAdjacentThings)
            {
                if (t is Book && t.Position.GetFirstBuilding(t.Map) is Building_Storage)
                {
                    num++;
                }
                else if (t is Building_Bookcase building_Bookcase)
                {
                    num++;
                    num += building_Bookcase.HeldBooks.Count;
                }
            }
            return 13.5f * (float)num;
        }
    }
}
