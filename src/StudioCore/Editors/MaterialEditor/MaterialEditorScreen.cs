﻿using ImGuiNET;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Editors.CutsceneEditor;
using StudioCore.Editors.GraphicsEditor;
using StudioCore.Editors.MaterialEditor;
using StudioCore.Editors.ParticleEditor;
using StudioCore.Settings;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using static StudioCore.Editors.MaterialEditor.MaterialBank;

namespace StudioCore.MaterialEditor;

public class MaterialEditorScreen : EditorScreen
{
    private readonly PropertyEditor _propEditor;
    private ProjectSettings _projectSettings;

    public ActionManager EditorActionManager = new();

    private MaterialFileInfo _selectedFileInfo;
    private IBinder _selectedBinder;
    private string _selectedBinderKey;

    private MTD _selectedMaterial;
    private int _selectedMaterialKey;

    public MaterialEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        _propEditor = new PropertyEditor(EditorActionManager);
    }

    public string EditorName => "Material Editor##MaterialEditor";
    public string CommandEndpoint => "material";
    public string SaveType => "Material";

    public void DrawEditorMenu()
    {
    }

    public void OnGUI(string[] initcmd)
    {
        var scale = Smithbox.GetUIScale();

        // Docking setup
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4) * scale);
        Vector2 wins = ImGui.GetWindowSize();
        Vector2 winp = ImGui.GetWindowPos();
        winp.Y += 20.0f * scale;
        wins.Y -= 20.0f * scale;
        ImGui.SetNextWindowPos(winp);
        ImGui.SetNextWindowSize(wins);

        if (Project.Type is ProjectType.BB or ProjectType.DS2S)
        {
            ImGui.Text($"This editor does not support {Project.Type}.");
            ImGui.PopStyleVar();
            return;
        }
        else if (_projectSettings == null)
        {
            ImGui.Text("No project loaded. File -> New Project");
        }

        if (!MaterialBank.IsLoaded)
        {
            if (!CFG.Current.AutoLoadBank_Material)
            {
                if (ImGui.Button("Load Material Editor"))
                {
                    MaterialBank.LoadMaterials();
                }
            }
        }

        var dsid = ImGui.GetID("DockSpace_MaterialEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (MaterialBank.IsLoaded)
        {
            MaterialFileView();
        }

        ImGui.PopStyleVar();
    }

    public void MaterialFileView()
    {
        // File List
        ImGui.Begin("Files##MaterialFileList");

        ImGui.Text($"File");
        ImGui.Separator();

        foreach (var (info, binder) in MaterialBank.FileBank)
        {
            if (ImGui.Selectable($@" {info.Name}", info.Name == _selectedBinderKey))
            {
                _selectedMaterialKey = -1; // Clear mtd key if file is changed

                _selectedBinderKey = info.Name;
                _selectedFileInfo = info;
                _selectedBinder = binder;
            }
        }

        ImGui.End();

        // File List
        ImGui.Begin("Materials##MaterialList");

        if (_selectedFileInfo.MaterialFiles != null)
        {
            ImGui.Text($"Materials");
            ImGui.Separator();

            for(int i = 0; i < _selectedFileInfo.MaterialFiles.Count; i++)
            {
                MTD entry = _selectedFileInfo.MaterialFiles[i];

                if (ImGui.Selectable($@" {entry.Description}", i == _selectedMaterialKey))
                {
                    _selectedMaterialKey = i;
                    _selectedMaterial = entry;
                }
            }
        }

        ImGui.End();
    }

    public void OnProjectChanged(ProjectSettings newSettings)
    {
        _projectSettings = newSettings;

        if (CFG.Current.AutoLoadBank_Material)
            MaterialBank.LoadMaterials();

        ResetActionManager();
    }

    public void Save()
    {
        if (MaterialBank.IsLoaded)
            MaterialBank.SaveMaterial(_selectedFileInfo, _selectedBinder);
    }

    public void SaveAll()
    {
        if (MaterialBank.IsLoaded)
            MaterialBank.SaveMaterials();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }
}