using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using AutoHook.Configurations;
using AutoHook.FishTimer;
using AutoHook.Utils;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Logging;
using ImGuiNET;

namespace AutoHook.Ui;

class TabPresets : TabBaseConfig
{
    public override bool Enabled => true;
    public override string TabName => "自定义预设";

    private bool _hasPreset = false;

    private BaitPresetConfig? _tempImport = null;

    private readonly static Configuration cfg = Service.Configuration;

    public TabPresets()
    { }

    public override void DrawHeader()
    {
        _hasPreset = cfg.CurrentPreset != null;

        ImGui.TextWrapped("在这里，你可以根据当前的钓饵或用于以小钓大的鱼来定制使用的提钩。\n如果没有指定钓饵/鱼，将使用默认的行为（一般设置Tab）来代替。");
        if (ImGui.Button("添加新预设"))
        {
            try
            {
                BaitPresetConfig preset = new($"New Preset{cfg.BaitPresetList.Count + 1}");
                cfg.BaitPresetList.Add(preset);
                cfg.BaitPresetList.OrderBy(s => s);
                cfg.CurrentPreset = preset;
                cfg.Save();
            }
            catch (Exception e)
            {
                PluginLog.Error(e.ToString());
            }
        }

        ImGui.SetNextItemWidth(130);

        if (ImGui.BeginCombo("Hook Presets", cfg.CurrentPreset == null ? "None" : cfg.CurrentPreset.PresetName))
        {
            foreach (BaitPresetConfig preset in cfg.BaitPresetList)
            {
                if (ImGui.Selectable(preset.PresetName, preset == cfg.CurrentPreset))
                {
                    cfg.CurrentPreset = preset;
                }
            }
            ImGui.EndCombo();
        }

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("右键单击以重命名");

        if (_hasPreset)
        {
            if (ImGui.BeginPopupContextItem("PresetName###name"))
            {
                string name = cfg.CurrentPreset?.PresetName ?? "-";
                ImGui.Text("编辑预设名称（按回车键确认）");

                if (ImGui.InputText("预设名称", ref name, 64, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    if (cfg.CurrentPreset != null && !Service.Configuration.BaitPresetList.Contains(new(name)))
                    {
                        cfg.CurrentPreset.RenamePreset(name);
                        cfg.BaitPresetList.OrderBy(s => s);
                    }
                }

                if (ImGui.Button("关闭"))
                    ImGui.CloseCurrentPopup();

                ImGui.EndPopup();
            }
        }

        ImGui.SameLine();

        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{FontAwesomeIcon.Trash.ToIconChar()}", new Vector2(ImGui.GetFrameHeight(), 0)) && ImGui.GetIO().KeyShift)
        {
            if (cfg.CurrentPreset != null && cfg.BaitPresetList != null)
            {
                cfg.BaitPresetList.Remove(cfg.CurrentPreset);
                cfg.CurrentPreset = null;
            }

            cfg.Save();
        }
        ImGui.PopFont();

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("按住SHIFT键删除");

        ImGui.Spacing();

        DrawImportExport();

        ImGui.Separator();
        ImGui.Spacing();

        if (_hasPreset)
        {
            if (ImGui.Button("添加"))
            {
                var setting = new BaitConfig("EditMe");
                if (cfg.CurrentPreset != null && !cfg.CurrentPreset.ListOfBaits.Contains(setting))
                    cfg.CurrentPreset.ListOfBaits.Add(setting);

                cfg.Save();
            }

            ImGui.SameLine();
            ImGui.Text($"新的钓饵/鱼 ({cfg.CurrentPreset?.ListOfBaits.Count})");
            ImGui.SameLine();
            ImGuiComponents.HelpMarker("请确保钓饵/鱼的名称与游戏中的一样正确无误（例如：万能拟饵）");

            // I hate ImGui and i dont care to make this look good
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudYellow);
            ImGui.TextWrapped("自动以小钓大");
            ImGui.PopStyleColor();
            ImGui.SameLine();
            ImGui.TextWrapped("当添加新的钓饵/鱼时默认启用");

            if (ImGui.Button("添加当前钓饵/鱼"))
            {
                var setting = new BaitConfig(HookingManager.CurrentBait ?? "-");

                if (cfg.CurrentPreset != null && !cfg.CurrentPreset.ListOfBaits.Contains(setting))
                    cfg.CurrentPreset.ListOfBaits.Add(setting);

                cfg.Save();
            }

            ImGui.SameLine();
            if (ImGui.Button($"添加上次捕获的鱼: {HookingManager.LastCatch ?? "-"}"))
            {
                var setting = new BaitConfig(HookingManager.LastCatch ?? "-");

                if (cfg.CurrentPreset != null && !cfg.CurrentPreset.ListOfBaits.Contains(setting))
                    cfg.CurrentPreset.ListOfBaits.Add(setting);

                cfg.Save();
            }

            ImGui.Text($"当前钓饵/鱼:");
            ImGui.SameLine();
            ImGui.TextColored(ImGuiColors.HealerGreen, HookingManager.CurrentBait ?? "-");
        }
    }

    private void DrawImportExport()
    {
        ImGui.PushFont(UiBuilder.IconFont);

        var buttonSize = ImGui.CalcTextSize(FontAwesomeIcon.SignOutAlt.ToIconString()) + ImGui.GetStyle().FramePadding * 2;

        if (ImGui.Button(FontAwesomeIcon.SignOutAlt.ToIconString(), buttonSize))
        {
            try
            {
                ImGui.SetClipboardText(Configuration.ExportActionStack(cfg.CurrentPreset!));

                _alertMessage = "预设已导出至剪贴板";
                _alertTimer.Start();
            }
            catch (Exception e)
            {
                PluginLog.Debug(e.Message);
                _alertMessage = "e.Message";
                _alertTimer.Start();
            }
        }

        ImGui.PopFont();

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("将预设导出到剪贴板");

        ImGui.SameLine();

        ImGui.PushFont(UiBuilder.IconFont);

        if (ImGui.Button(FontAwesomeIcon.SignInAlt.ToIconString(), buttonSize))
        {
            try
            {
                _tempImport = Configuration.ImportActionStack(ImGui.GetClipboardText());

                if (_tempImport != null)
                {
                    ImGui.OpenPopup("import_new_preset");
                }
            }
            catch (Exception e)
            {
                PluginLog.Debug(e.Message);
                _alertMessage = e.Message;
                _alertTimer.Start();
            }
        }

        ImGui.PopFont();

        if (_tempImport != null)
        {
            if (ImGui.BeginPopup("import_new_preset"))
            {
                string name = _tempImport.PresetName;
                ImGui.Text("导入该预设?");

                if (ImGui.InputText("预设名称", ref name, 64, ImGuiInputTextFlags.AutoSelectAll))
                {
                    _tempImport.RenamePreset(name);
                }

                if (ImGui.Button("导入"))
                {
                    if (Service.Configuration.BaitPresetList.Contains(new(name)))
                    {
                        _alertMessage = "存在同名预设";
                        _alertTimer.Start();
                    }
                    else
                    {
                        cfg.BaitPresetList.Add(_tempImport);
                        cfg.BaitPresetList.OrderBy(s => s);
                        cfg.CurrentPreset = _tempImport;
                        _tempImport = null;
                        cfg.Save();
                    }
                }

                ImGui.SameLine();

                if (ImGui.Button("取消"))
                {
                    _tempImport = null;
                    ImGui.CloseCurrentPopup();
                }

                TimedWarning();

                ImGui.EndPopup();
            }
        }

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("从剪切板中导入");

        TimedWarning();
    }

    private static readonly double _timelimit = 5000;
    private readonly Stopwatch _alertTimer = new();
    private string _alertMessage = "-";
    private void TimedWarning()
    {
        if (_alertTimer.IsRunning)
        {
            ImGui.TextColored(ImGuiColors.DalamudYellow, _alertMessage);

            if (_alertTimer.ElapsedMilliseconds > _timelimit)
            {
                _alertTimer.Reset();
            }
        }
    }
    public override void Draw()
    {
        if (_hasPreset)
        {
            ImGui.BeginGroup();

            for (int idx = 0; idx < cfg.CurrentPreset?.ListOfBaits.Count; idx++)
            {
                var bait = cfg.CurrentPreset.ListOfBaits[idx];
                ImGui.PushID($"id###{idx}");
                if (ImGui.CollapsingHeader($"{bait.BaitName}###{idx}"))
                {
                    DrawEnabledButtonCustomBait(bait);
                    ImGui.Indent();
                    ImGui.SameLine();
                    DrawDeleteBaitButton(bait);
                    DrawInputTextName(bait);
                    DrawInputDoubleMinTime(bait);
                    DrawInputDoubleMaxTime(bait);
                    DrawChumMinMaxTime(bait);
                    DrawHookCheckboxes(bait);
                    ImGui.Spacing();

                    /*
                    DrawFishersIntuitionConfig(bait);
                    ImGui.Spacing();
                    DrawCheckBoxDoubleTripleHook(bait);
                    ImGui.Spacing();
                    DrawSurfaceSlapIdenticalCast(bait);
                    ImGui.Spacing();
                    DrawAutoMooch(bait);
                    //DrawPatienceConfig(bait);
                    //ImGui.Separator();
                    */

                    ImGuiTableFlags flags = ImGuiTableFlags.SizingFixedFit | /*ImGuiTableFlags.Resizable |*/ ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.NoHostExtendX | ImGuiTableFlags.ContextMenuInBody;

                    if (ImGui.BeginTable("table2", 2, flags))
                    {

                        // Collumn 1
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        DrawFishersIntuitionConfig(bait);
                        ImGui.TableNextColumn();
                        DrawCheckBoxDoubleTripleHook(bait);

                        // Collumn 2
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        DrawSurfaceSlapIdenticalCast(bait);
                        ImGui.TableNextColumn();
                        DrawAutoMooch(bait);

                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        DrawStopAfter(bait);

                        ImGui.EndTable();
                    }

                    ImGui.Unindent();
                }
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
                ImGui.PopID();
            }
            ImGui.EndGroup();
        }
    }
}
