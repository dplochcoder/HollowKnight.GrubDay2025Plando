using ItemChanger.Locations;
using ItemChanger.Placements;
using Modding;

namespace GrubDay2025Plando;

public class GrubDay2025PlandoMod : Mod
{
    private static readonly string VERSION = PurenailCore.ModUtil.VersionUtil.ComputeVersion<GrubDay2025PlandoMod>();

    public override string GetVersion() => VERSION;

    public GrubDay2025PlandoMod() : base("GrubDay2025Plando") { }

    public override void Initialize()
    {
        On.UIManager.StartNewGame += (orig, self, permaDeath, bossRush) =>
        {
            orig(self, permaDeath, bossRush);

            // ItemChangerMod.CreateSettingsProfile(false);
        };
    }

    public static void RewriteContext()
    {
        var lm = RandomizerMod.RandomizerMod.RS.TrackerData.lm;
        foreach (var e in ItemChanger.Internal.Ref.Settings.Placements)
        {
            if (lm.LogicLookup.ContainsKey(e.Key)) continue;

            var placement = e.Value;
            if (placement.Items.Count != 1 || placement.Items[0] is not AdvancedMimic item) continue;
            if (placement is not MutablePlacement mutable || mutable.Location is not CoordinateLocation loc) continue;

            AdvancedMimicDeployer deployer = new()
            {
                Name = placement.Name,
                SceneName = loc.UnsafeSceneName,
                X = loc.x,
                Y = loc.y,
                hp = item.hp,
                Scale = item.Scale,
                MaxSpeed = item.MaxSpeed,
                Acceleration = item.Acceleration
            };
            ItemChanger.ItemChangerMod.AddDeployer(deployer);

            GrubDay2025Plando.GrubDay2025PlandoMod.RewriteContext();
        }
    }
}
