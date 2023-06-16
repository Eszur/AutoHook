using ImGuiNET;
using AutoHook.Utils;
using AutoHook.Configurations;
using AutoHook.Data;
using System.Numerics;
using System.Collections.Generic;
using AutoHook.Classes;
using System;
using System.Diagnostics;

namespace AutoHook.Ui;

internal class TabAutoCasts : TabBaseConfig
{
    public override bool Enabled => true;
    public override string TabName => "自动钓鱼";

    private readonly static AutoCastsConfig cfg = Service.Configuration.AutoCastsCfg;
    public override void DrawHeader()
    {
        //ImGui.TextWrapped("The new Auto Cast/Mooch is a experimental feature and can be a little confusing at first. I'll be trying to find a more simple and intuitive solution later\nPlease report any issues you encounter.");

        // Disable all casts
        ImGui.Spacing();
        if (DrawUtil.Checkbox("启用 Auto Casts", ref cfg.EnableAll))
        {
            Service.Configuration.Save();
        }

        if (cfg.EnableAll)
        {
            ImGui.SameLine();
            if (DrawUtil.Checkbox("不取消“以小钓大”机会", ref cfg.DontCancelMooch, "不使用会取消“以小钓大”机会的技能 (例如：撒饵, 鱼眼, 大鱼猎手 等)"))
            {
                Service.Configuration.Save();
            }
        }

        ImGui.Spacing();

        if (ImGui.Button("指南： 如何自动将道具收为收藏品"))
        {
            Process.Start(new ProcessStartInfo { FileName = "https://github.com/InitialDet/AutoHook/blob/main/AcceptCollectable.md", UseShellExecute = true });
        }
    }

    public override void Draw()
    {
        if (cfg.EnableAll)
        {
            DrawAutoCast();
            DrawAutoMooch();
            DrawChum();
            DrawCordials();
            DrawFishEyes();
            DrawMakeShiftBait();
            DrawPatience();
            DrawPrizeCatch();
            DrawThaliaksFavor();
        }
    }

    private void DrawAutoCast()
    {
        if (DrawUtil.Checkbox("全局自动抛竿", ref cfg.EnableAutoCast, "抛竿 (捕鱼人技能) 将在鱼咬钩后使用\n\n重要!!!\n如果你启用了这个选项，而你没有自定义自动以小钓大或没有启用全局自动以小钓大选项，鱼线将被正常抛出，你将失去以小钓大的机会（如果有的话）。"))
        {
            Service.Configuration.Save();
        }

        if (cfg.EnableAutoCast)
        {
            ImGui.Indent();
            DrawExtraOptionsAutoCast();
            ImGui.Unindent();
        }
    }

    private void DrawExtraOptionsAutoCast()
    {

    }

    private void DrawAutoMooch()
    {
        if (DrawUtil.Checkbox("全局自动以小钓大", ref cfg.EnableMooch, "这个选项优先于自动抛竿\n\n如果你想只对特定的鱼进行自动以小钓大，而忽略其他的鱼，请禁用此选项并添加自定义预设"))
        {
            Service.Configuration.Save();
        }

        if (cfg.EnableMooch)
        {
            ImGui.Indent();
            DrawExtraOptionsAutoMooch();
            ImGui.Unindent();
        }
    }

    private void DrawExtraOptionsAutoMooch()
    {
        if (ImGui.Checkbox("使用 以小钓大II", ref cfg.EnableMooch2))
        {
            Service.Configuration.Save();
        }

        if (ImGui.Checkbox("只在“捕鱼人之识”中使用##fi_mooch", ref cfg.OnlyMoochIntuition))
        {
            Service.Configuration.Save();
        }
    }

    private void DrawPatience()
    {

        var enabled = cfg.AutoPatienceII.Enabled;
        if (DrawUtil.Checkbox("使用 耐心I/II", ref enabled, "当你当前的GP大于等于技能消耗+20时，将使用耐心I/II（例如：I为220，II为580），这有助于避免使用强力/精准提钩时GP不够。"))
        {
            cfg.AutoPatienceII.Enabled = enabled;
            cfg.AutoPatienceI.Enabled = enabled;
            Service.Configuration.Save();
        }

        if (enabled)
        {
            ImGui.Indent();
            DrawExtraOptionsPatience();
            ImGui.Unindent();
        }
    }

    private void DrawExtraOptionsPatience()
    {

        var enabled = cfg.EnableMakeshiftPatience;

        if (DrawUtil.Checkbox("在“熟练渔技”中使用##patience_makeshift", ref enabled))
        {
            cfg.EnableMakeshiftPatience = enabled;
            Service.Configuration.Save();
        }

        if (ImGui.RadioButton("耐心I###1", cfg.SelectedPatienceID == IDs.Actions.Patience))
        {
            cfg.SelectedPatienceID = IDs.Actions.Patience;
            Service.Configuration.Save();
        }

        if (ImGui.RadioButton("耐心II###2", cfg.SelectedPatienceID == IDs.Actions.Patience2))
        {
            cfg.SelectedPatienceID = IDs.Actions.Patience2;
            Service.Configuration.Save();
        }
    }

    private void DrawThaliaksFavor()
    {
        ImGui.PushID("ThaliaksFavor");
        var enabled = cfg.AutoThaliaksFavor.Enabled;
        if (DrawUtil.Checkbox("使用 沙利亚克的恩宠", ref enabled, "这可能会与自动“熟练渔技”相冲突"))
        {
            cfg.AutoThaliaksFavor.Enabled = enabled;
            Service.Configuration.Save();
        }

        if (enabled)
        {
            ImGui.Indent();
            DrawExtraOptionsThaliaksFavor();
            ImGui.Unindent();
        }
        ImGui.PopID();
    }

    private void DrawExtraOptionsThaliaksFavor()
    {
        var stack = cfg.AutoThaliaksFavor.ThaliaksFavorStacks;
        if (DrawUtil.EditNumberField("当层数 =", ref stack))
        {
            if (stack < 3)
                cfg.AutoThaliaksFavor.ThaliaksFavorStacks = 3;
            else if (stack > 10)
                cfg.AutoThaliaksFavor.ThaliaksFavorStacks = 10;
            else
                cfg.AutoThaliaksFavor.ThaliaksFavorStacks = stack;

            Service.Configuration.Save();
        }
    }

    private void DrawMakeShiftBait()
    {
        ImGui.PushID("MakeShiftBait");

        var enabled = cfg.AutoMakeShiftBait.Enabled;
        if (DrawUtil.Checkbox("使用 熟练渔技", ref enabled, "这可能会与自动“沙利亚克的恩宠”相冲突"))
        {
            cfg.AutoMakeShiftBait.Enabled = enabled;
            Service.Configuration.Save();
        }

        if (enabled)
        {
            ImGui.Indent();
            DrawExtraOptionsMakeShiftBait();
            ImGui.Unindent();
        }
        ImGui.PopID();
    }

    private void DrawExtraOptionsMakeShiftBait()
    {
        var stack = cfg.AutoMakeShiftBait.MakeshiftBaitStacks;
        if (DrawUtil.EditNumberField($"当层数 = ", ref stack))
        {
            if (stack < 5)
                cfg.AutoMakeShiftBait.MakeshiftBaitStacks = 5;
            else if (stack > 10)
                cfg.AutoMakeShiftBait.MakeshiftBaitStacks = 10;
            else
                cfg.AutoMakeShiftBait.MakeshiftBaitStacks = stack;

            Service.Configuration.Save();
        }
    }

    private void DrawPrizeCatch()
    {
        var enabled = cfg.AutoPrizeCatch.Enabled;
        if (DrawUtil.Checkbox("使用 大鱼猎手", ref enabled, "会取消当前以小钓大机会。在“大鱼猎手”期间，不使用“耐心”和“熟练渔技”"))
        {
            cfg.AutoPrizeCatch.Enabled = enabled;
            Service.Configuration.Save();
        }

        if (enabled)
        {
            ImGui.Indent();
            DrawExtraOptionPrizeCatch();
            ImGui.Unindent();
        }
    }

    private void DrawExtraOptionPrizeCatch()
    {
        if (DrawUtil.Checkbox("仅在以小钓大II“无法使用”时使用 - 注意 >>>", ref cfg.AutoPrizeCatch.UseWhenMoochIIOnCD, ">确保“使用以小钓大II”已启用，否则它不会起作用<\n如果只是为了以小钓大，这可以为你节省100gp。"))
        { }

        if (DrawUtil.Checkbox("只在“专一垂钓”中使用##ic_prize_catch", ref cfg.AutoPrizeCatch.UseOnlyWithIdenticalCast))
        { }
    }

    private void DrawChum()
    {
        var enabled = cfg.AutoChum.Enabled;
        if (DrawUtil.Checkbox("使用 撒饵", ref enabled, "会取消当前以小钓大机会"))
        {
            cfg.AutoChum.Enabled = enabled;
            Service.Configuration.Save();
        }

        if (enabled)
        {
            ImGui.Indent();
            DrawExtraOptionsChum();
            ImGui.Unindent();
        }
    }

    private void DrawExtraOptionsChum()
    {
        if (DrawUtil.Checkbox("仅在“捕鱼人之识”期间使用##fi_chum", ref cfg.AutoChum.OnlyUseWithIntuition))
        { }
    }

    private void DrawFishEyes()
    {
        var enabled = cfg.AutoFishEyes.Enabled;
        if (DrawUtil.Checkbox("使用 鱼眼", ref enabled, "会取消当前以小钓大机会"))
        {
            cfg.AutoFishEyes.Enabled = enabled;
            Service.Configuration.Save();

        }
    }

    private void DrawCordials()
    {

        var enabled = cfg.AutoHICordial.Enabled;
        if (DrawUtil.Checkbox("使用 强心剂 (优先高级强心剂)", ref enabled, "如果没有高级强心剂，将使用强心剂代替"))
        {
            cfg.AutoHICordial.Enabled = enabled;
            cfg.AutoHQCordial.Enabled = enabled;
            cfg.AutoCordial.Enabled = enabled;
            cfg.AutoHQWateredCordial.Enabled = enabled;
            cfg.AutoWateredCordial.Enabled = enabled;
        }

        if (enabled)
        {
            ImGui.Indent();
            DrawExtraOptionsCordials();
            ImGui.Unindent();
        }
    }

    private void DrawExtraOptionsCordials()
    {
        if (DrawUtil.Checkbox("更改优先级: 轻型强心剂 > 强心剂 > 高级强心剂", ref cfg.EnableCordialFirst, "如果没有强心剂，将使用高级强心剂代替"))
        { }
    }
}
