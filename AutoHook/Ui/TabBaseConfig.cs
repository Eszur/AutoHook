using System;
using System.Numerics;
using AutoHook.Configurations;
using AutoHook.Enums;
using AutoHook.Utils;
using Dalamud.Hooking;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using ImGuiNET;

namespace AutoHook.Ui;
abstract class TabBaseConfig : IDisposable
{
    public abstract string TabName { get; }
    public abstract bool Enabled { get; }
    public static string StrHookWeak => "咬钩强度低 (!)";
    public static string StrHookStrong => "咬钩强度高 (!!)";
    public static string StrHookLegendary => "咬钩强度非常高 (!!!)";

    public abstract void DrawHeader();

    public abstract void Draw();

    public virtual void Dispose() { }

    public void DrawDeleteBaitButton(BaitConfig cfg)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{FontAwesomeIcon.Trash.ToIconChar()}", new Vector2(ImGui.GetFrameHeight(), 0)) && ImGui.GetIO().KeyShift)
        {
            Service.Configuration.CurrentPreset?.ListOfBaits.RemoveAll(x => x.BaitName == cfg.BaitName);
            Service.Configuration.Save();
        }
        ImGui.PopFont();

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("按住SHIFT键删除");
    }

    public void DrawHookCheckboxes(BaitConfig cfg)
    {
        DrawSelectTugs(StrHookWeak, ref cfg.HookWeakEnabled, ref cfg.HookTypeWeak);
        DrawSelectTugs(StrHookStrong, ref cfg.HookStrongEnabled, ref cfg.HookTypeStrong);
        DrawSelectTugs(StrHookLegendary, ref cfg.HookLegendaryEnabled, ref cfg.HookTypeLegendary);
    }

    public void DrawSelectTugs(string hook, ref bool enabled, ref HookType type)
    {
       
        ImGui.Checkbox(hook, ref enabled);
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("如果耐心不足，则使用 “提钩”。");

        if (enabled)
        {
            ImGui.Indent();
            if (ImGui.RadioButton($"精准提钩###{TabName}{hook}1", type == HookType.Precision))
            {
                type = HookType.Precision;
                Service.Configuration.Save();
            }

            if (ImGui.RadioButton($"强力提钩###{TabName}{hook}2", type == HookType.Powerful))
            {
                type = HookType.Powerful;
                Service.Configuration.Save();
            }
            ImGui.Unindent();
        }
    }

    public void DrawInputTextName(BaitConfig cfg)
    {
        string matchText = new string(cfg.BaitName);
        ImGui.SetNextItemWidth(-260 * ImGuiHelpers.GlobalScale);
        if (ImGui.InputText("以小钓大/钓饵 Name", ref matchText, 64, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
        {
            if (cfg.BaitName != matchText && Service.Configuration.CurrentPreset != null && Service.Configuration.CurrentPreset.ListOfBaits.Contains(new BaitConfig(matchText)))
                cfg.BaitName = "钓饵已存在";
            else
                cfg.BaitName = matchText;

            Service.Configuration.Save();
        };
    }

    public void DrawInputDoubleMaxTime(BaitConfig cfg)
    {
        ImGui.SetNextItemWidth(100 * ImGuiHelpers.GlobalScale);
        if (ImGui.InputDouble("最多等待时间", ref cfg.MaxTimeDelay, .1, 1, "%.1f%"))
        {
            switch (cfg.MaxTimeDelay)
            {
                case 0.1:
                    cfg.MaxTimeDelay = 2;
                    break;
                case <= 0:
                case <= 1.9: //This makes the option turn off if delay = 2 seconds when clicking the minus.
                    cfg.MaxTimeDelay = 0;
                    break;
                case > 99:
                    cfg.MaxTimeDelay = 99;
                    break;
            }
        }
        ImGui.SameLine();
        // Hook will be used after the defined amount of time has passed\nMin. time: 2s (because of animation lock)\n\nSet Zero (0) to disable, and dont make this lower than the Min. Wait
        ImGuiComponents.HelpMarker("提钩将在到达设置的时间后使用\n该值至少设置2秒（因为有动画锁定）。\n\n设置零（0）时禁用，并且不要使其低于最少等待时间");
    }

    public void DrawInputDoubleMinTime(BaitConfig cfg)
    {
        ImGui.SetNextItemWidth(100 * ImGuiHelpers.GlobalScale);
        if (ImGui.InputDouble("最少等待时间", ref cfg.MinTimeDelay, .1, 1, "%.1f%"))
        {
            switch (cfg.MinTimeDelay)
            {
                case <= 0:
                    cfg.MinTimeDelay = 0;
                    break;
                case > 99:
                    cfg.MinTimeDelay = 99;
                    break;
            }
        }

        ImGui.SameLine();
        //Hook will NOT be used until the minimum time has passed.\n\nEx: If you set the number as 14 and something bites after 8 seconds, the fish will not to be hooked\n\nSet Zero (0) to disable
        ImGuiComponents.HelpMarker("在最少等待时间之前，不会使用提钩。\n\n例如：如果你把数值设置为14，在8秒后有鱼咬钩，则不会使用提钩来钓起该鱼。\n\n设置零（0）时禁用");
    }

    public void DrawChumMinMaxTime(BaitConfig cfg)
    {

        if (ImGui.Button("撒饵计时器"))
        {
            ImGui.OpenPopup(str_id: "chum_timer");
        }

        if (ImGui.BeginPopup("chum_timer"))
        {
            ImGui.Spacing();
            Utils.DrawUtil.Checkbox("启用撒饵计时器", ref cfg.UseChumTimer, "在撒饵作用下，启用最少/多等待时间");
            ImGui.Separator();

            ImGui.SetNextItemWidth(100 * ImGuiHelpers.GlobalScale);
            if (ImGui.InputDouble("最少等待时间", ref cfg.MinChumTimeDelay, .1, 1, "%.1f%"))
            {
                switch (cfg.MinTimeDelay)
                {
                    case <= 0:
                        cfg.MinTimeDelay = 0;
                        break;
                    case > 99:
                        cfg.MinTimeDelay = 99;
                        break;
                }
            }

            ImGui.SameLine();
            ImGuiComponents.HelpMarker("在最少等待时间之前，不会使用提钩。\n\n例如：如果你把数值设置为14，在8秒后有鱼咬钩，则不会使用提钩来钓起该鱼。\n\n设置零（0）时禁用");


            ImGui.SetNextItemWidth(100 * ImGuiHelpers.GlobalScale);
            if (ImGui.InputDouble("最多等待时间", ref cfg.MaxChumTimeDelay, .1, 1, "%.1f%"))
            {
                switch (cfg.MaxTimeDelay)
                {
                    case 0.1:
                        cfg.MaxTimeDelay = 2;
                        break;
                    case <= 0:
                    case <= 1.9: //This makes the option turn off if delay = 2 seconds when clicking the minus.
                        cfg.MaxTimeDelay = 0;
                        break;
                    case > 99:
                        cfg.MaxTimeDelay = 99;
                        break;
                }
            }
            
            ImGui.SameLine();
            
            ImGuiComponents.HelpMarker("提钩将在到达设置的时间后使用\n该值至少设置2秒（因为有动画锁定）。\n\n设置零（0）时禁用，并且不要使其低于最少等待时间");

        

            ImGui.EndPopup();
        }
    }


    public void DrawEnabledButtonCustomBait(BaitConfig cfg)
    {
        ImGui.Checkbox("启用的配置 ->", ref cfg.Enabled);
        ImGuiComponents.HelpMarker("重要!!!\n\n如果禁用，鱼将不会提起或以小钓大\n要使用默认行为（一般设置Tab），请删除此配置。");
    }

    public void DrawCheckBoxDoubleTripleHook(BaitConfig cfg)
    {

        if (ImGui.Button("双重/三重提钩设置###DHTH"))
        {
            ImGui.OpenPopup("Double/Triple SettingsHook###DHTH");
        }
        if (ImGui.BeginPopup("Double/Triple SettingsHook###DHTH"))
        {

            ImGui.TextColored(ImGuiColors.DalamudYellow, "双重/三重提钩设置");
            ImGui.Spacing();

            ImGui.Checkbox("仅在 “专一垂钓”时使用##surface_slap", ref cfg.UseDHTHOnlySurfaceSlap);
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.Checkbox("使用双重提钩（当gp>400）", ref cfg.UseDoubleHook))
            {
                if (cfg.UseDoubleHook && !ImGui.GetIO().KeyShift) cfg.UseTripleHook = false;
                Service.Configuration.Save();
            }
            ImGuiComponents.HelpMarker("按住SHIFT键可同时选择双重和三重提钩（不推荐）");

            if (ImGui.Checkbox("使用三重提钩（当gp>400）", ref cfg.UseTripleHook))
            {
                if (cfg.UseTripleHook && !ImGui.GetIO().KeyShift) cfg.UseDoubleHook = false;
                Service.Configuration.Save();
            }
            ImGuiComponents.HelpMarker("按住SHIFT键可同时选择双重和三重提钩（不推荐）");

            if (cfg.UseTripleHook || cfg.UseDoubleHook)
            {
                ImGui.Indent();

                ImGui.Checkbox("在“耐心”期间使用（不推荐）。", ref cfg.UseDHTHPatience);
                ImGuiComponents.HelpMarker("重要!!!\n\n如果禁用，在“耐心”期间，将使用精准/强力提钩来代替");
                ImGui.Checkbox("如果GP低于技能要求，则让鱼逃走。", ref cfg.LetFishEscape);
                ImGui.Unindent();

                ImGui.Separator();
                ImGui.Spacing();

                ImGui.Checkbox(StrHookWeak, ref cfg.HookWeakDHTHEnabled);
                ImGui.Checkbox(StrHookStrong, ref cfg.HookStrongDHTHEnabled);
                ImGui.Checkbox(StrHookLegendary, ref cfg.HookLegendaryDHTHEnabled);
            }

            ImGui.EndPopup();
        }

    }

    public void DrawFishersIntuitionConfig(BaitConfig cfg)
    {
        if (ImGui.Button("捕鱼人之识 设置###FishersIntuition"))
        {
            ImGui.OpenPopup("fisher_intuition_settings");
        }

        if (ImGui.BeginPopup("fisher_intuition_settings"))
        {
            ImGui.TextColored(ImGuiColors.DalamudYellow, "捕鱼人之识 设置");
            ImGui.Spacing();
            Utils.DrawUtil.Checkbox("启用", ref cfg.UseCustomIntuitionHook, "检测到捕鱼人之识时启用自定义提钩");
            ImGui.Separator();

            DrawSelectTugs(StrHookWeak, ref cfg.HookWeakIntuitionEnabled, ref cfg.HookTypeWeakIntuition);
            DrawSelectTugs(StrHookStrong, ref cfg.HookStrongIntuitionEnabled, ref cfg.HookTypeStrongIntuition);
            DrawSelectTugs(StrHookLegendary, ref cfg.HookLegendaryIntuitionEnabled, ref cfg.HookTypeLegendaryIntuition);

            ImGui.EndPopup();
        }
    }

    public void DrawAutoMooch(BaitConfig cfg)
    {

        if (ImGui.Button("自动以小钓大"))
        {
            ImGui.OpenPopup("auto_mooch");
        }

        if (ImGui.BeginPopup("auto_mooch"))
        {
            ImGui.TextColored(ImGuiColors.DalamudYellow, "自动以小钓大");
            ImGui.Spacing();
            ImGui.Text("- 如果这是一个钓饵（例如：万能拟饵），所有可以使用该钓饵钓到的鱼都会使用以小钓大。");
            ImGui.Text("- 如果这是一条鱼/鱼饵（例如：海港鲱），当它被钓到时将会使用以小钓大");
            ImGui.Text("如果该选项被禁用，即使在“一般设置”选项卡中也启用了“自动以小钓大”，该鱼也不会被以小钓大。");
            if (Utils.DrawUtil.Checkbox("自动以小钓大", ref cfg.UseAutoMooch, "这个选项的优先级高于自动抛竿"))
            {
                if (!cfg.UseAutoMooch)
                    cfg.UseAutoMooch2 = false;
            }

            if (cfg.UseAutoMooch)
            {
                ImGui.Indent();

                if (ImGui.Checkbox("使用以小钓大II", ref cfg.UseAutoMooch2))
                {
                    Service.Configuration.Save();
                }

                if (ImGui.Checkbox("只在“捕鱼人之识”中使用##Mooch", ref cfg.OnlyMoochIntuition))
                {
                    Service.Configuration.Save();
                }
                ImGui.Unindent();
            }
            ImGui.EndPopup();
        }
    }

    public void DrawSurfaceSlapIdenticalCast(BaitConfig cfg)
    {

        if (ImGui.Button("拍击水面 & 专一垂钓"))
        {
            ImGui.OpenPopup("surface_slap_identical_cast");
        }

        if (ImGui.BeginPopup("surface_slap_identical_cast"))
        {
            ImGui.TextColored(ImGuiColors.DalamudYellow, "拍击水面 & 专一垂钓");
            ImGui.Spacing();
            if (DrawUtil.Checkbox("使用 拍击水面", ref cfg.UseSurfaceSlap, "覆盖专一垂钓"))
            {
                cfg.UseIdenticalCast = false;
            }

            if (DrawUtil.Checkbox("使用 专一垂钓", ref cfg.UseIdenticalCast, "覆盖拍击水面"))
            {
                cfg.UseSurfaceSlap = false;
            }

            ImGui.EndPopup();
        }
    }

    public void DrawStopAfter(BaitConfig cfg)
    {

        if (ImGui.Button("在以下情况下停止钓鱼..."))
        {
            ImGui.OpenPopup(str_id: "stop_after");
        }

        if (ImGui.BeginPopup("stop_after"))
        {
            ImGui.TextColored(ImGuiColors.DalamudYellow, "停止钓鱼");
            ImGui.Spacing();
            if (DrawUtil.Checkbox("被捕获后...", ref cfg.StopAfterCaught, "- 如果这个配置是钓饵: 在垂钓X次后停止钓鱼\n- 如果这个配置是一条鱼: 在钓到X条指定鱼后停止钓鱼"))
            {

            }

            if (cfg.StopAfterCaught)
            {
                ImGui.Indent();
                ImGui.SetNextItemWidth(90 * ImGuiHelpers.GlobalScale);
                if (ImGui.InputInt("Time(s)", ref cfg.StopAfterCaughtLimit))
                {
                    if (cfg.StopAfterCaughtLimit < 1)
                        cfg.StopAfterCaughtLimit = 1;
                }

                ImGui.Unindent();
            }

            ImGui.EndPopup();
        }
    }
}
