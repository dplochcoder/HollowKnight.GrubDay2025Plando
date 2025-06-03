using ItemChanger;
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

            ItemChangerMod.CreateSettingsProfile(false);
            ItemChangerMod.AddPlacements([Finder.GetLocation(LocationNames.Journal_Entry_Charged_Lumafly)!.Wrap().Add(new AdvancedMimic())]);
            ItemChangerMod.AddDeployer(new MimicHazardRemoverDeployer() { SceneName = SceneNames.Fungus3_archive_02 });
        };
    }
}
