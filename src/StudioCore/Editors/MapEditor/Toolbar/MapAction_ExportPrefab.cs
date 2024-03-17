﻿using ImGuiNET;
using StudioCore.Editors.MapEditor.Prefabs;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Toolbar
{
    public class MapAction_ExportPrefab
    {
        public static void Select(ViewportSelection _selection)
        {
            if (!MapEditorToolbar.IsSupportedProjectTypeForPrefabs())
                return;

            if (ImGui.RadioButton("Export Prefab##tool_Selection_ExportPrefab", MapEditorState.SelectedAction == MapEditorAction.ExportPrefab))
            {
                MapEditorState.SelectedAction = MapEditorAction.ExportPrefab;
            }
        }
        public static void Configure(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.ExportPrefab)
            {
                ImGui.Text("Export the current selection as a prefab.");
                ImGui.Text("");

                ImGui.Text("Prefab Name:");
                ImGui.InputText("##prefabName", ref MapEditorToolbar._prefabName, 255);

                if (_selection.GetSelection().Count != 0)
                {
                    ImGui.SameLine();
                    if (ImGui.Button("Set Automatic Name"))
                    {
                        AssignUniquePrefabName(_selection);
                    }
                    ImguiUtils.ShowHoverTooltip("Get an unique prefab name based on the first element of the current selection.");
                }

                ImGui.Text("Prefab Tags:");
                ImGui.InputText("##prefabTags", ref MapEditorToolbar._prefabTags, 255);
                ImguiUtils.ShowHoverTooltip("The set of tags to save this prefab under. Split each tag with the , character.");
                ImGui.Text("");

                ImGui.Checkbox("Retain Entity ID", ref CFG.Current.Prefab_IncludeEntityID);
                ImguiUtils.ShowHoverTooltip("Saved objects within a prefab will retain their Entity ID. If false, their Entity ID is set to 0.");

                ImGui.Checkbox("Retain Entity Group IDs", ref CFG.Current.Prefab_IncludeEntityGroupIDs);
                ImguiUtils.ShowHoverTooltip("Saved objects within a prefab will retain their Entity Group IDs. If false, their Entity Group IDs will be set to 0.");
                ImGui.Text("");
            }
        }

        public static void Act(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.ExportPrefab)
            {
                if (ImGui.Button("Export##action_Selection_ExportPrefab", new Vector2(200, 32)))
                {
                    ExportPrefab(_selection);
                }
                ImguiUtils.ShowHoverTooltip("Save the current selection as a prefab.\n\nNote, map object fields that refer other map objects will be set to empty when saved as a prefab.");
            }

        }

        public static void Shortcuts()
        {
            if (MapEditorState.SelectedAction == MapEditorAction.ExportPrefab)
            {
                ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_ExportPrefab.HintText)}");
            }
        }

        public static void ExportPrefab(ViewportSelection _selection)
        {
            if (!MapEditorToolbar.IsSupportedProjectTypeForPrefabs())
                return;

            if (File.Exists($"{MapEditorToolbar._prefabDir}{MapEditorToolbar._prefabName}{MapEditorToolbar._prefabExt}"))
            {
                PlatformUtils.Instance.MessageBox("Prefab already exists with this name, try another.", "Prefab Error", MessageBoxButtons.OK);
            }
            else if (MapEditorToolbar._prefabName == "" || MapEditorToolbar._prefabName == null)
            {
                PlatformUtils.Instance.MessageBox("Prefab name cannot be blank.", "Prefab Error", MessageBoxButtons.OK);
            }
            else
            {
                ExportCurrentSelection(_selection);
            }
        }

        public static void AssignUniquePrefabName(ViewportSelection _selection)
        {
            var ent = _selection.GetSelection().First() as Entity;
            int count = 1000;
            MapEditorToolbar._prefabName = $"{ent.Name}_{count}"; // Use First entity's name as prefab name

            // Loop until we reach a filename that isn't used
            while (File.Exists($"{MapEditorToolbar._prefabDir}{MapEditorToolbar._prefabName}{MapEditorToolbar._prefabExt}"))
            {
                count++;
                MapEditorToolbar._prefabName = $"{ent.Name}_{count}";
            }
        }

        // <summary>
        /// Export current selection as prefab.
        /// </summary>
        /// <param name="filepath"></param>
        public static void ExportCurrentSelection(ViewportSelection _selection)
        {
            var filepath = $"{MapEditorToolbar._prefabDir}{MapEditorToolbar._prefabName}{MapEditorToolbar._prefabExt}";

            switch (Project.Type)
            {
                case ProjectType.ER:
                    Prefab_ER.ExportSelection(filepath, _selection, MapEditorToolbar._prefabTags);
                    break;
                case ProjectType.SDT:
                    Prefab_SDT.ExportSelection(filepath, _selection, MapEditorToolbar._prefabTags);
                    break;
                case ProjectType.DS3:
                    Prefab_DS3.ExportSelection(filepath, _selection, MapEditorToolbar._prefabTags);
                    break;
                case ProjectType.DS2S:
                    Prefab_DS2.ExportSelection(filepath, _selection, MapEditorToolbar._prefabTags);
                    break;
                case ProjectType.DS1:
                case ProjectType.DS1R:
                    Prefab_DS1.ExportSelection(filepath, _selection, MapEditorToolbar._prefabTags);
                    break;
                default: break;
            }

            MapEditorToolbar.RefreshPrefabList();
        }
    }
}