using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Containers;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Items;
using ItemChanger.Util;
using ItemChanger;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GrubDay2025Plando;

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
        var mimicContainer = MimicUtil.CreateNewMimic(info);

        var mimicItem = info.giveInfo.items.OfType<AdvancedMimic>().FirstOrDefault();
        var fsm = mimicContainer.FindChild("Grub Mimic Top")!.FindChild("Grub Mimic 1")!.LocateMyFSM("Grub Mimic");

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

        if (mimicItem != null)
        {
            if (mimicItem.Scale != null) mimicContainer.transform.localScale *= mimicItem.Scale.Value;

            List<ChaseObjectGround> chase = [fsm.GetState("Chase").GetFirstActionOfType<ChaseObjectGround>(), fsm.GetState("Cooldown").GetFirstActionOfType<ChaseObjectGround>()];
            if (mimicItem.MaxSpeed != null) chase.ForEach(c => c.speedMax = mimicItem.MaxSpeed.Value);
            if (mimicItem.Acceleration != null) chase.ForEach(c => c.acceleration = mimicItem.Acceleration.Value);
        }

        return mimicContainer;
    }

    public override void AddGiveEffectToFsm(PlayMakerFSM fsm, ContainerGiveInfo containerGiveInfo)
    {
        if (fsm.FsmName != "Grub Control") return;

        GameObject mimic = fsm.gameObject.FindChild("Grub Mimic 1")!;
        HealthManager hm = mimic.GetComponent<HealthManager>();

        FsmState init = fsm.GetState("Init");
        init.SetActions(
            init.Actions[0],
            init.Actions[1],
            init.Actions[2],
            init.Actions[6],
            init.Actions[7],
            new DelegateBoolTest(() => containerGiveInfo.placement.CheckVisitedAny(VisitState.Opened), (BoolTest)init.Actions[8])
        // the removed actions are all various tests to check if the mimic is dead
        // we tie it to the placement to make it easier to control
        );

        fsm.GetState("Activate").AddFirstAction(new Lambda(GiveAll));
        hm.OnDeath += GiveAll;

        void GiveAll()
        {
            Vector2 pos = mimic.transform.position;

            GiveInfo giveInfo = new()
            {
                Container = Mimic,
                FlingType = containerGiveInfo.flingType,
                Transform = mimic.transform,
                MessageType = MessageType.Corner,
            };

            foreach (AbstractItem item in containerGiveInfo.placement.Items)
            {
                if (!item.IsObtained())
                {
                    if (containerGiveInfo.flingType == FlingType.DirectDeposit) GiveDirectly(containerGiveInfo, mimic.transform);
                    else if (item.GiveEarly(Mimic)) item.Give(containerGiveInfo.placement, giveInfo);
                    else
                    {
                        GameObject shiny = ShinyUtility.MakeNewShiny(containerGiveInfo.placement, item, containerGiveInfo.flingType);
                        shiny.transform.position = pos;
                        shiny.SetActive(true);
                    }
                }
            }
            containerGiveInfo.placement.AddVisitFlag(VisitState.Opened);
        }
    }

    private void GiveDirectly(ContainerGiveInfo info, Transform t)
    {
        ItemUtility.GiveSequentially(info.placement.Items, info.placement, new GiveInfo
        {
            Container = "Enemy",
            FlingType = info.flingType,
            MessageType = MessageType.Corner,
            Transform = t,
        });
    }
}

internal class AdvancedMimic : MimicItem
{
    static AdvancedMimic() => Container.DefineContainer<AdvancedMimicContainer>();

    public float? Scale;
    public float? MaxSpeed;
    public float? Acceleration;

    public override string GetPreferredContainer() => AdvancedMimicContainer.AdvancedMimic;

    public override bool GiveEarly(string containerType) => containerType == Container.Mimic || containerType == AdvancedMimicContainer.AdvancedMimic;

    public override void GiveImmediate(GiveInfo info)
    {
        if (info.Container != AdvancedMimicContainer.AdvancedMimic) base.GiveImmediate(info);
    }
}
