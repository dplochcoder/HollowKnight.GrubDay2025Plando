using ItemChanger.Items;
using ItemChanger;

namespace GrubDay2025Plando;

internal class AdvancedMimic : MimicItem
{
    static AdvancedMimic() => Container.DefineContainer<AdvancedMimicContainer>();

    public float? Scale;
    public float? MaxSpeed;
    public float? Acceleration;

    public override string GetPreferredContainer() => AdvancedMimicContainer.AdvancedMimic;

    public override bool GiveEarly(string containerType) => containerType == AdvancedMimicContainer.AdvancedMimic;

    public override void GiveImmediate(GiveInfo info)
    {
        if (info.Container != AdvancedMimicContainer.AdvancedMimic) base.GiveImmediate(info);
    }

    internal AdvancedMimicInfo ToInfo() => new()
    {
        hp = hp,
        Scale = Scale,
        MaxSpeed = MaxSpeed,
        Acceleration = Acceleration,
    };
}
