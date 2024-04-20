using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(JobDriver_ManTurret), nameof(JobDriver_ManTurret.FindAmmoForTurret))]
    public static class JobDriver_ManTurret_FindAmmoForTurret
    {
        private static bool Prefix(Pawn pawn, Building_TurretGun gun, ref Thing __result)
        {
            if (gun.TryGetArtillery(out var group))
            {
                StorageSettings allowedShellsSettings = pawn.IsColonist ? gun.gun.TryGetComp<CompChangeableProjectile>().allowedShellsSettings : RetrieveParentSettings(gun);
                bool validator(Thing t) => !t.IsForbidden(pawn) && pawn.CanReserve(t, 10, 1, null, false) && (allowedShellsSettings == null || allowedShellsSettings.AllowedToAccept(t));
                __result = GenClosest.ClosestThingReachable(gun.Position, gun.Map, ThingRequest.ForGroup(group), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false),
                    40f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
                return false;
            }
            return true;
        }

        private static StorageSettings RetrieveParentSettings(Building_TurretGun gun)
        {
            return gun.gun.TryGetComp<CompChangeableProjectile>().GetParentStoreSettings();
        }
    }
}
