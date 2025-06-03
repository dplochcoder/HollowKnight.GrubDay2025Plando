using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GrubDay2025Plando;

internal class MimicInvincibilityModule : ItemChanger.Modules.Module
{
    public override void Initialize()
    {
        Events.OnSceneChange += AdjustHazardFSMs;
        On.DamageEnemies.DoDamage += OverrideDoDamage;
    }

    public override void Unload()
    {
        Events.OnSceneChange -= AdjustHazardFSMs;
        On.DamageEnemies.DoDamage -= OverrideDoDamage;
    }

    private static bool IsHazardDamager(GameObject go)
    {
        if (go.GetComponent<DamageHero>() is DamageHero dh && dh.hazardType > 1) return true;
        if (go.LocateMyFSM("damages_hero") != null) return true;

        return false;
    }

    private void AdjustHazardFSMs(Scene to)
    {
        foreach (var fsm in Object.FindObjectsOfType<PlayMakerFSM>())
        {
            if (fsm.FsmName == "damages_enemy" && IsHazardDamager(fsm.gameObject))
            {
                fsm.GetState("Send Event").AddFirstAction(new Lambda(() =>
                {
                    var target = fsm.FsmVariables.GetFsmGameObject("Collider").Value;
                    if (target.name.ToUpper().Contains("MIMIC")) fsm.SendEvent("CANCEL");
                }));
            }
            if (fsm.FsmName == "enemy_message" && fsm.FsmVariables.GetFsmString("Event")?.Value == "ACID")
            {
                fsm.GetState("Send").AddFirstAction(new Lambda(() =>
                {
                    var target = fsm.FsmVariables.GetFsmGameObject("Collider").Value;
                    if (target.name.ToUpper().Contains("MIMIC")) fsm.SendEvent("FINISHED");
                }));
            }
        }
    }

    private void OverrideDoDamage(On.DamageEnemies.orig_DoDamage orig, DamageEnemies self, GameObject target)
    {
        if (self.attackType == AttackTypes.Acid && target.name.ToUpper().Contains("MIMIC")) return;
        orig(self, target);
    }
}
