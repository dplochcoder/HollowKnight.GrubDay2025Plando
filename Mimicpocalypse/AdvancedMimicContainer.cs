using ItemChanger.Containers;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Util;
using ItemChanger;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;

namespace Mimicpocalypse;

internal record AdvancedMimicInfo
{
    public int? hp;
    public float? Scale;
    public float? MaxSpeed;
    public float? Acceleration;
}

internal class AdvancedMimicContainer : MimicContainer
{
    internal const string AdvancedMimic = "AdvancedMimic";

    public override string Name => AdvancedMimic;

    private static void AddGlobalTransition(PlayMakerFSM fsm, FsmTransition transition)
    {
        var arr = new FsmTransition[fsm.Fsm.GlobalTransitions.Length + 1];
        System.Array.Copy(fsm.Fsm.GlobalTransitions, arr, arr.Length - 1);
        arr[arr.Length - 1] = transition;
        fsm.Fsm.GlobalTransitions = arr;
    }

    public override GameObject GetNewContainer(ContainerInfo info)
    {
        var mimicParent = MimicUtil.CreateNewMimic(info);
        var mimicItem = info.giveInfo.placement.Items.OfType<AdvancedMimic>().First();
        ModifyMimicContainer(mimicParent, mimicItem.ToInfo());
        return mimicParent;
    }

    public static void ModifyMimicContainer(GameObject mimicParent, AdvancedMimicInfo mimicInfo)
    {
        var fsm = mimicParent.FindChild("Grub Mimic Top")!.FindChild("Grub Mimic 1")!.LocateMyFSM("Grub Mimic");

        var quickKill = fsm.AddState("Quick Kill");
        quickKill.AddLastAction(new Lambda(() =>
        {
            var hm = fsm.gameObject.GetComponent<HealthManager>();
            hm.ApplyExtraDamage(hm.hp);
        }));
        AddGlobalTransition(fsm, new()
        {
            ToState = "Quick Kill",
            ToFsmState = quickKill,
            FsmEvent = FsmEvent.GetFsmEvent("GRIMM DEFEATED")
        });

        if (mimicInfo.Scale != null) mimicParent.transform.localScale *= mimicInfo.Scale.Value;

        List<ChaseObjectGround> chase = [fsm.GetState("Chase").GetFirstActionOfType<ChaseObjectGround>(), fsm.GetState("Cooldown").GetFirstActionOfType<ChaseObjectGround>()];
        if (mimicInfo.MaxSpeed != null) chase.ForEach(c => c.speedMax = mimicInfo.MaxSpeed.Value);
        if (mimicInfo.Acceleration != null) chase.ForEach(c => c.acceleration = mimicInfo.Acceleration.Value);
    }

    public override void AddGiveEffectToFsm(PlayMakerFSM fsm, ContainerGiveInfo info)
    {
        if (fsm.FsmName != "Grub Control") return;

        // Copied from ItemChanger MimicUtility.
        FsmState init = fsm.GetState("Init");
        init.SetActions(
            init.Actions[0],
            init.Actions[1],
            init.Actions[2],
            init.Actions[6],
            init.Actions[7],
            new DelegateBoolTest(() => info.placement.CheckVisitedAny(VisitState.Opened), (BoolTest)init.Actions[8])
        // the removed actions are all various tests to check if the mimic is dead
        // we tie it to the placement to make it easier to control
        );

        GameObject mimic = fsm.gameObject.FindChild("Grub Mimic 1")!;
        fsm.GetState("Activate").AddFirstAction(new Lambda(GiveAll));
        mimic.GetComponent<HealthManager>().OnDeath += GiveAll;

        void GiveAll()
        {
            info.placement.AddVisitFlag(VisitState.Opened);
            ItemUtility.GiveSequentially(info.placement.Items, info.placement, new GiveInfo
            {
                Container = AdvancedMimic,
                FlingType = FlingType.DirectDeposit,
                MessageType = MessageType.Corner,
                Transform = mimic.transform
            });
        }
    }
}
