using Verse;
using RimWorld;
using ProcessorFramework;
using DefOf = RimWorld.DefOf;

namespace MedievalOverhaul
{
    [DefOf]
    public static class MedievalOverhaulDefOf
    {
        public static ThingDef DankPyon_Artillery_Trebuchet;
        public static ThingDef DankPyon_Artillery_Boulder;
        public static TerrainDef DankPyon_PlowedSoil;
        public static DamageDef DankPyon_SchratCollapse;
        public static BodyPartGroupDef HeadAttackTool;

        public static ThingDef DankPyon_Fat;
        public static ThingDef DankPyon_Bone;

        public static ThingCategoryDef DankPyon_Wood;
        public static ThingDef DankPyon_Leather_Rawhide;
        public static ProcessDef DankPyon_RawHidesProcess;

        public static HediffDef DankPyon_UnpleasantAftermath;

        public static ThingCategoryDef DankPyon_Hides;
        public static ThingCategoryDef DankPyon_RawWood;

        public static PawnKindDef DankPyon_Schrat_Dark;
        public static PawnKindDef DankPyon_SchratDark_Sapling;
        public static PawnKindDef DankPyon_SchratPlain_Sapling;
        public static FactionDef DankPyon_Forest_Faction;
        public static FactionDef DankPyon_Hornets;

        public static ThingDef DankPyon_MineableIron;
        public static ThingDef DankPyon_MineableGold;
        public static ThingDef MineableSilver;
        public static ThingDef MineablePlasteel;

        public static PawnKindDef DankPyon_Golem_Iron;
        public static PawnKindDef DankPyon_Golem_Silver;
        public static PawnKindDef DankPyon_Golem_Gold;
        public static PawnKindDef DankPyon_Golem_Steel;

        public static ThingDef DankPyon_GolemRock_Iron_Incident;

        public static ThingDef DankPyon_MineShaft;
        public static ThingDef DankPyon_Citrine;
        public static ThingDef DankPyon_Amber;
        public static ThingDef DankPyon_Onyx;
        public static ThingDef DankPyon_Emerald;
        public static ThingDef DankPyon_Sapphire;
        public static ThingDef DankPyon_Ruby;
        public static ThingDef DankPyon_GoldOre;

        public static ThingDef DankPyon_Tree_GriffonBerry;
        public static ThingDef DankPyon_ComponentBasic;

        public static JobDef DankPyon_Slop_Refuel_StatAtomic;
        public static JobDef DankPyon_Slop_Refuel_Stat;
        public static JobDef DankPyon_DoBillMending;
        public static JobDef DankPyon_Mine_Golem;
        public static JobDef DankPyon_OperateQuest;

        public static BodyDef Bird;

        static MedievalOverhaulDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(MedievalOverhaulDefOf));
    }

}
