using System;

namespace GatherBuddy.Enums;

public enum SpearfishSpeed : ushort
{
    All = 0,
    SuperSlow     = 100,
    ExtremelySlow = 150,
    VerySlow      = 200,
    Slow          = 250,
    Average       = 300,
    Fast          = 350,
    VeryFast      = 400,
    ExtremelyFast = 450,
    SuperFast     = 500,
    HyperFast     = 550,
    LynFast       = 600,

    
}

public static class SpearFishSpeedExtensions
{
    public static string ToName(this SpearfishSpeed speed)
        => speed switch
        {
            SpearfishSpeed.All => "全部",
            SpearfishSpeed.SuperSlow     => "超慢Super Slow",
            SpearfishSpeed.ExtremelySlow => "极慢Extremely Slow",
            SpearfishSpeed.VerySlow      => "非常慢Very Slow",
            SpearfishSpeed.Slow          => "慢Slow",
            SpearfishSpeed.Average       => "一般Average",
            SpearfishSpeed.Fast          => "快Fast",
            SpearfishSpeed.VeryFast      => "很快Very Fast",
            SpearfishSpeed.ExtremelyFast => "极快Extremely Fast",
            SpearfishSpeed.SuperFast     => "超快Super Fast",
            SpearfishSpeed.HyperFast     => "超级速Hyper Fast",
            SpearfishSpeed.LynFast       => "百万级快Mega Fast",
            
            _                            => $"{(ushort)speed}",
        };
}
