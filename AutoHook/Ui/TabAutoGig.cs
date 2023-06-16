using AutoHook.Classes;
using AutoHook.Configurations;
using AutoHook.Utils;
using Dalamud.Interface;
using GatherBuddy.Enums;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoHook.Ui;
internal class TabAutoGig : TabBaseConfig
{
    public override string TabName => "自动刺鱼";
    public override bool Enabled => true;

    private readonly List<SpearfishSpeed> _speedTypes = Enum.GetValues(typeof(SpearfishSpeed)).Cast<SpearfishSpeed>().ToList();
    private readonly List<SpearfishSize> _sizeTypes = Enum.GetValues(typeof(SpearfishSize)).Cast<SpearfishSize>().ToList();

    public override void DrawHeader()
    {
        ImGui.Spacing();
        ImGui.TextWrapped("这是一个实验性的功能，它可能会错过鱼。如果你发现它漏掉了太多的鱼，请尝试将SpearFishing窗口的比例调整为不同的内容。");
        ImGui.Spacing();
    }

    public override void Draw()
    {
        if (DrawUtil.Checkbox("启用 AutoGig", ref Service.Configuration.AutoGigEnabled))
        {
            if (Service.Configuration.AutoGigEnabled)
            {
                Service.Configuration.AutoGigHideOverlay = false;
                Service.Configuration.Save();
            }
        }

        if (!Service.Configuration.AutoGigEnabled)
        {
            ImGui.Indent();
            if (DrawUtil.Checkbox("刺鱼时隐藏overlay", ref Service.Configuration.AutoGigHideOverlay, "只有在禁用AutoGig选项的情况下，它才会隐藏"))
            {
                Service.Configuration.Save();
            }

            ImGui.Unindent();
        } else
        {
            ImGui.Indent();
            if (DrawUtil.Checkbox("绘制鱼的hitbox(判定框)", ref Service.Configuration.AutoGigDrawFishHitbox, "hitbox(判定框)仅适用于选定大小和速度的鱼"))
            {
                Service.Configuration.Save();
            }
            if (DrawUtil.Checkbox("绘制gig(鱼叉) hitbox", ref Service.Configuration.AutoGigDrawGigHitbox))
            {
                Service.Configuration.Save();
            }
            ImGui.Unindent();
        }

        ImGui.Separator();

        DrawSpeedSize();
    }

    private void DrawSpeedSize()
    {
        ImGui.Spacing();
        ImGui.TextWrapped("选择你想要的鱼的大小和速度(Gatherbuddy的刺鱼overlay有很大帮助)");
        ImGui.Spacing();

        ImGui.SetNextItemWidth(130);
        if (ImGui.BeginCombo("大小", Service.Configuration.currentSize.ToName()))
        {

            foreach (SpearfishSize size in _sizeTypes.Where(size =>
                        ImGui.Selectable(size.ToName(), size == Service.Configuration.currentSize)))
            {
                Service.Configuration.currentSize = size;
            }
            ImGui.EndCombo();
        }

        ImGui.SameLine();

        ImGui.SetNextItemWidth(130);
        if (ImGui.BeginCombo("移速", Service.Configuration.currentSpeed.ToName()))
        {
            foreach (SpearfishSpeed speed in _speedTypes.Where(speed =>
                        ImGui.Selectable(speed.ToName(), speed == Service.Configuration.currentSpeed)))
            {
                Service.Configuration.currentSpeed = speed;
            }
            ImGui.EndCombo();
        }
    }
}
