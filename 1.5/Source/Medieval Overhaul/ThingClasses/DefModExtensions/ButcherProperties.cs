using Verse;

namespace MedievalOverhaul
{
    class ButcherProperties : DefModExtension
    {
        public bool hasBone = true;

        public bool hasFat = true;

        public static ButcherProperties Get(Def def)
        {
            return def.GetModExtension<ButcherProperties>();
        }
    }
}