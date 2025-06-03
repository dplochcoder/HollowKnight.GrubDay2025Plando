using ItemChanger;
using ItemChanger.FsmStateActions;
using UnityEngine.SceneManagement;
using UnityEngine;
using ItemChanger.Extensions;

namespace GrubDay2025Plando;

internal class MimicHazardRemoverDeployer : IDeployer
{
    public string SceneName { get; set; } = "";

    public void OnSceneChange(Scene to)
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

        GameObject damageEnemiesModder = new("DamageEnemiesModder");
        damageEnemiesModder.AddComponent<DamageEnemiesModder>();
    }

    private bool IsHazardDamager(GameObject go)
    {
        if (go.GetComponent<DamageHero>() is DamageHero dh && dh.hazardType > 1) return true;
        if (go.LocateMyFSM("damages_hero") != null) return true;

        return false;
    }
}

internal class DamageEnemiesModder : MonoBehaviour
{
    private void Awake() => On.DamageEnemies.DoDamage += OverrideDoDamage;

    private void OnDestroy() => On.DamageEnemies.DoDamage -= OverrideDoDamage;

    private void OverrideDoDamage(On.DamageEnemies.orig_DoDamage orig, DamageEnemies self, GameObject target)
    {
        if (self.attackType == AttackTypes.Acid && target.name.ToUpper().Contains("MIMIC")) return;
        orig(self, target);
    }
}
