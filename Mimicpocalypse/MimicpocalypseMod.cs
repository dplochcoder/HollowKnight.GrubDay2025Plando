using Modding;

namespace Mimicpocalypse;

public class MimicpocalypseMod : Mod
{
    private static readonly string VERSION = PurenailCore.ModUtil.VersionUtil.ComputeVersion<MimicpocalypseMod>();

    public override string GetVersion() => VERSION;

    public MimicpocalypseMod() : base("MimicpocalypseMod") { }

    public override void Initialize()
    {
        On.UIManager.StartNewGame += (orig, self, permaDeath, bossRush) =>
        {
            orig(self, permaDeath, bossRush);

            // ItemChangerMod.CreateSettingsProfile(false);
        };
    }
}
