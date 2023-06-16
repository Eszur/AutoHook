using AutoHook.Classes;
using AutoHook.Configurations;
using Dalamud.Interface;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoHook.Ui;
internal class TabGPConfig : TabBaseConfig
{
    public override string TabName => "GP 配置";
    public override bool Enabled => true;

    private static readonly AutoCastsConfig cfg = Service.Configuration.AutoCastsCfg;

    private static readonly List<BaseActionCast> _actionsAvailable = new()
            {
                cfg.AutoChum,
                cfg.AutoCordial,
                cfg.AutoHICordial,
                cfg.AutoHQCordial,
                cfg.AutoWateredCordial,
                cfg.AutoHQWateredCordial,
                cfg.AutoFishEyes,
                cfg.AutoIdenticalCast,
                cfg.AutoMakeShiftBait,
                cfg.AutoPatienceI,
                cfg.AutoPatienceII,
                cfg.AutoPrizeCatch,
                cfg.AutoSurfaceSlap,
                cfg.AutoThaliaksFavor,
            };

    public override void Draw()
    {
        DrawGPTab();
    }

    public override void DrawHeader()
    {
        ImGui.Spacing();
        ImGui.TextWrapped("在这里，您可以为AutoCast功能使用的技能和物品定制GP阈值。");
        ImGui.Spacing();
    }

    private void DrawGPTab()
    {
        foreach (var action in _actionsAvailable)
        {
            bool above = action.GPThresholdAbove;
            int gpThreshold = (int)action.GPThreshold;

            ImGui.PushID(action.Name);
            ImGui.SetWindowFontScale(1.2f);
            ImGui.Text(action.Name);
            ImGui.SetWindowFontScale(1f);

            if (ImGui.IsItemHovered())
                ImGui.SetTooltip($"{action.Name}将在你的GP等于或 {(above ? "高于" : "低于")} {gpThreshold}时使用");

            if (ImGui.RadioButton($"高于##1", above == true))
            {
                action.GPThresholdAbove = true;
                Service.Configuration.Save();
            }

            ImGui.SameLine();

            if (ImGui.RadioButton($"低于##1", above == false))
            {
                action.GPThresholdAbove = false;
                Service.Configuration.Save();
            }

            ImGui.SameLine();

            ImGui.SetNextItemWidth(100 * ImGuiHelpers.GlobalScale);
            if (ImGui.InputInt("GP", ref gpThreshold, 1, 1))
            {
                action.SetThreshold((uint)gpThreshold);
            }

            ImGui.PopID();

            ImGui.Separator();
        }
    }
}
