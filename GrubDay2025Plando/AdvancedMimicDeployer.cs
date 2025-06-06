using ItemChanger.Components;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Internal;
using ItemChanger;
using UnityEngine;
using HutongGames.PlayMaker.Actions;
using HutongGames.PlayMaker;

namespace GrubDay2025Plando;

internal record AdvancedMimicDeployer : Deployer
{
    public string Name = "";
    public int? hp;
    public float? Scale;
    public float? MaxSpeed;
    public float? Acceleration;

    public bool Opened;

    // Mostly copied from ItemChanger.
    public override GameObject Instantiate()
    {
        GameObject mimicParent = new($"Grub Mimic Parent-{Name}");
        var box = mimicParent.AddComponent<BoxCollider2D>();
        box.size = new(2f, 2.1f);
        box.offset = new(0, -0.2f);
        mimicParent.layer = 19;
        mimicParent.SetActive(false);
        mimicParent.AddComponent<NonBouncer>();
        mimicParent.AddComponent<DropIntoPlace>();

        GameObject mimicTop = ObjectCache.MimicTop;
        mimicTop.name = "Grub Mimic Top";
        GameObject mimicBottle = ObjectCache.MimicBottle;
        mimicBottle.name = "Grub Mimic Bottle";

        mimicBottle.transform.SetParent(mimicParent.transform);
        mimicTop.transform.SetParent(mimicParent.transform);
        mimicBottle.SetActive(true);
        mimicTop.SetActive(true);

        mimicBottle.transform.localPosition = new(0, 0.3f, -0.1f);
        mimicTop.transform.localPosition = new(0, 0.15f, 0f);
        mimicTop.transform.Find("Grub Mimic 1").localPosition = new(-0.1f, 1.3f, 0f);
        mimicTop.transform.Find("Grub Mimic 1").GetComponent<SetZ>().z = 0f;

        PlayMakerFSM bottleControl = mimicBottle.LocateMyFSM("Bottle Control");
        FsmState init = bottleControl.GetState("Init");
        init.ReplaceAction(new DelegateBoolTest(() => Opened, (BoolTest)init.Actions[0]), 0);
        init.GetFirstActionOfType<SendEventByName>().eventTarget.gameObject.GameObject = mimicTop;
        FsmState shatter = bottleControl.GetState("Shatter");
        shatter.AddFirstAction(new Lambda(() => Opened = true));
        shatter.GetActionsOfType<SendEventByName>()[1].eventTarget.gameObject.GameObject = mimicTop;

        if (hp.HasValue) mimicTop.transform.Find("Grub Mimic 1").GetComponent<HealthManager>().hp = hp.Value;

        AdvancedMimicContainer.ModifyMimicContainer(mimicParent, ToInfo());
        return mimicParent;
    }

    private AdvancedMimicInfo ToInfo() => new()
    {
        hp = hp,
        Scale = Scale,
        MaxSpeed = MaxSpeed,
        Acceleration = Acceleration
    };
}
