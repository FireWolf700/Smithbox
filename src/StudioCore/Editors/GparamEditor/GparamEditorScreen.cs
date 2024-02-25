﻿using DotNext.Collections.Generic;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Banks;
using StudioCore.Banks.FormatBank;
using StudioCore.BanksMain;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Editors.GparamEditor;
using StudioCore.Editors.GraphicsEditor;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.UserProject;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using Veldrid;
using Veldrid.Sdl2;
using static SoulsFormats.GPARAM;

namespace StudioCore.GraphicsEditor;

public class GparamEditorScreen : EditorScreen
{
    private ProjectSettings _projectSettings;

    private ActionManager EditorActionManager = new();

    private GparamParamBank.GparamInfo _selectedGparamInfo;
    private GPARAM _selectedGparam;
    private string _selectedGparamKey;

    private string _fileSearchInput = "";
    private string _fileSearchInputCache = "";

    private GPARAM.Param _selectedParamGroup;
    private int _selectedParamGroupKey;

    private string _paramGroupSearchInput = "";
    private string _paramGroupSearchInputCache = "";

    private GPARAM.IField _selectedParamField;
    private int _selectedParamFieldKey;

    private string _paramFieldSearchInput = "";
    private string _paramFieldSearchInputCache = "";

    private GPARAM.IFieldValue _selectedFieldValue = null;
    private int _selectedFieldValueKey;

    private string _fieldIdSearchInput = "";
    private string _fieldIdSearchInputCache = "";

    private string _copyFileNewName = "";

    private int _duplicateValueRowId = 0;

    private bool[] displayTruth;

    public GparamEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        
    }

    public string EditorName => "Gparam Editor##GparamEditor";
    public string CommandEndpoint => "gparam";
    public string SaveType => "Gparam";

    public void Init()
    {
        
    }

    public void DrawEditorMenu()
    {
    }

    public void OnGUI(string[] initcmd)
    {
        var scale = Smithbox.GetUIScale();

        // Docking setup
        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4) * scale);
        Vector2 wins = ImGui.GetWindowSize();
        Vector2 winp = ImGui.GetWindowPos();
        winp.Y += 20.0f * scale;
        wins.Y -= 20.0f * scale;
        ImGui.SetNextWindowPos(winp);
        ImGui.SetNextWindowSize(wins);

        if (Project.Type is ProjectType.DS1 or ProjectType.DS1R or ProjectType.BB or ProjectType.DS2S)
        {
            ImGui.Text($"This editor does not support {Project.Type}.");
            ImGui.PopStyleVar();
            ImGui.PopStyleColor(1);
            return;
        }
        else if (_projectSettings == null)
        {
            ImGui.Text("No project loaded. File -> New Project");
        }

        if (!GparamParamBank.IsLoaded)
        {
            if (!CFG.Current.AutoLoadBank_Gparam)
            {
                if (ImGui.Button("Load Gparam Editor"))
                {
                    GparamParamBank.LoadGraphicsParams();
                }
            }
        }

        var dsid = ImGui.GetID("DockSpace_GparamEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        GparamShortcuts();

        if (GparamParamBank.IsLoaded)
        {
            GparamListView();
            GparamGroupList();
            GparamFieldList();
            GparamValueProperties();
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }

    public void GparamShortcuts()
    {
        
    }

    private void GparamListView()
    {
        ImGui.Begin("Files##GparamFileList");

        ImGui.Separator();

        ImGui.InputText($"Search", ref _fileSearchInput, 255);
        ImGui.SameLine();
        ImguiUtils.ShowHelpMarker("Separate terms are split via the + character.");

        ImGui.Separator();

        if (_fileSearchInput != _fileSearchInputCache)
        {
            _fileSearchInputCache = _fileSearchInput;
        }

        foreach (var (name, info) in GparamParamBank.ParamBank)
        {
            if (SearchFilters.IsEditorSearchMatch(_fileSearchInput, info.Name, "_"))
            {
                var prettyName = "";
                if (CFG.Current.Gparam_DisplayMapNames)
                {
                    prettyName = MapAliasBank.GetMapNameFromFilename(info.Name);
                }

                ImGui.BeginGroup();
                if (ImGui.Selectable($@" {info.Name}", info.Name == _selectedGparamKey))
                {
                    ResetGroupSelection();
                    ResetFieldSelection();
                    ResetValueSelection();

                    _selectedGparamKey = info.Name;
                    _selectedGparamInfo = info;
                    _selectedGparam = info.Gparam;
                }
                if (prettyName != "")
                {
                    ImGui.SameLine();
                    ImGui.PushTextWrapPos();

                    ImGui.TextColored(CFG.Current.ImGui_AliasName_Text, @$"<{prettyName}>");

                    ImGui.PopTextWrapPos();
                }

                ImGui.EndGroup();
            }

            GparamFileContextMenu(name, info);
        }

        ImGui.End();
    }

    public void GparamFileContextMenu(string name, GparamParamBank.GparamInfo info)
    {
        if (info.Name == _selectedGparamKey)
        {
            if (ImGui.BeginPopupContextItem($"Options##Gparam_File_Context"))
            {
                if (ImGui.Selectable("Duplicate"))
                {
                    DuplicateGparamFile();

                    ImGui.CloseCurrentPopup();
                }
                ImguiUtils.ShowButtonTooltip("Duplicate this file, incrementing the numeric four digit ID at the end of the file name if possible.");

                if (ImGui.Selectable("Copy"))
                {
                    CopyGparamFile(info);

                    ImGui.CloseCurrentPopup();
                }
                ImguiUtils.ShowButtonTooltip("Copy the selected file and rename it to the name specified below");

                if (ImGui.Selectable("Remove"))
                {
                    RemoveGparamFile(info);

                    ImGui.CloseCurrentPopup();
                }
                ImguiUtils.ShowButtonTooltip("Delete the selected file from your project.");
                ImGui.Separator();

                // Copy
                if (_copyFileNewName == "")
                    _copyFileNewName = name;

                ImGui.InputText("##copyInputName", ref _copyFileNewName, 255);

                ImGui.EndPopup();
            }
        }
    }

    public void GparamGroupList()
    {
        ImGui.Begin("Groups##GparamGroups");

        ImGui.Separator();

        ImGui.InputText($"Search", ref _paramGroupSearchInput, 255);
        ImGui.SameLine();
        ImguiUtils.ShowHelpMarker("Separate terms are split via the + character.");

        ImGui.Separator();

        if (_paramGroupSearchInput != _paramGroupSearchInputCache)
        {
            _paramGroupSearchInputCache = _paramGroupSearchInput;
        }

        if (_selectedGparam != null && _selectedGparamKey != "")
        {
            GPARAM data = _selectedGparam;

            ImGui.Text($"Group");
            ImGui.Separator();

            // Available groups
            for (int i = 0; i < data.Params.Count; i++)
            {
                GPARAM.Param entry = data.Params[i];

                var name = GparamFormatBank.Bank.GetReferenceName(entry.Key, entry.Name);

                var display = true;

                if (!CFG.Current.Gparam_DisplayEmptyGroups)
                {
                    if(entry.Fields.Count < 1)
                    {
                        display = false;
                    }
                }

                if (SearchFilters.IsEditorSearchMatch(_paramGroupSearchInput, entry.Name, " "))
                {
                    if (display)
                    {
                        if (ImGui.Selectable($@" {name}##{entry.Key}", i == _selectedParamGroupKey))
                        {
                            ResetFieldSelection();
                            ResetValueSelection();

                            _selectedParamGroup = entry;
                            _selectedParamGroupKey = i;
                        }
                    }
                }

                ShowGparamGroupContext(i);
            }

            if (CFG.Current.Gparam_DisplayAddGroups)
            {
                ImGui.Separator();

                GparamGroupAddSection();
            }
        }

        ImGui.End();
    }

    public void ShowGparamGroupContext(int index)
    {
        if (index == _selectedParamGroupKey)
        {
            if (ImGui.BeginPopupContextItem($"Options##Gparam_Group_Context"))
            {
                if (ImGui.Selectable("Remove"))
                {
                    _selectedGparam.Params.Remove(_selectedParamGroup);

                    ImGui.CloseCurrentPopup();
                }
                ImguiUtils.ShowButtonTooltip("Delete the selected group.");

                ImGui.EndPopup();
            }
        }
    }

    public void GparamGroupAddSection()
    {
        GPARAM data = _selectedGparam;

        List<FormatReference> missingGroups = new List<FormatReference>();

        // Get source Format Reference
        foreach (var entry in GparamFormatBank.Bank.Entries.list)
        {
            bool isPresent = false;

            foreach (var param in data.Params)
            {
                if (entry.id == param.Key)
                {
                    isPresent = true;
                }
            }

            if (!isPresent)
            {
                missingGroups.Add(entry);
            }
        }

        foreach (var missing in missingGroups)
        {
            if (ImGui.Button($"Add##{missing.id}"))
            {
                AddMissingGroup(missing);
            }
            ImGui.SameLine();
            ImGui.Text($"{missing.name}");
        }
    }

    public void AddMissingGroup(FormatReference missingGroup)
    {
        var newGroup = new GPARAM.Param();
        newGroup.Key = missingGroup.id;
        newGroup.Name = missingGroup.name;
        newGroup.Fields = new List<GPARAM.IField>();

        _selectedGparam.Params.Add(newGroup);
    }

    public void GparamFieldList()
    {
        ImGui.Begin("Fields##GparamFields");

        ImGui.Separator();

        ImGui.InputText($"Search", ref _paramFieldSearchInput, 255);
        ImGui.SameLine();
        ImguiUtils.ShowHelpMarker("Separate terms are split via the + character.");

        ImGui.Separator();

        if (_paramFieldSearchInput != _paramFieldSearchInputCache)
        {
            _paramFieldSearchInputCache = _paramFieldSearchInput;
        }

        if (_selectedParamGroup != null && _selectedParamGroupKey != -1)
        {
            GPARAM.Param data = _selectedParamGroup;

            ImGui.Text($"Field");
            ImGui.Separator();

            for (int i = 0; i < data.Fields.Count; i++)
            {
                GPARAM.IField entry = data.Fields[i];

                var name = GparamFormatBank.Bank.GetReferenceName(entry.Key, entry.Name);

                if (SearchFilters.IsEditorSearchMatch(_paramFieldSearchInput, entry.Name, " "))
                {
                    if (ImGui.Selectable($@" {name}##{entry.Key}{i}", i == _selectedParamFieldKey))
                    {
                        ResetValueSelection();

                        _selectedParamField = entry;
                        _selectedParamFieldKey = i;
                    }
                }

                ShowGparamFieldContext(i);
            }

            if (CFG.Current.Gparam_DisplayAddFields)
            {
                ImGui.Separator();

                GparamFieldAddSection();
            }
        }

        ImGui.End();
    }

    public void GparamFieldAddSection()
    {
        GPARAM.Param data = _selectedParamGroup;

        List<FormatMember> missingFields = new List<FormatMember>();

        // Get source Format Reference
        foreach(var entry in GparamFormatBank.Bank.Entries.list)
        {
            if (entry.id == _selectedParamGroup.Key)
            {
                foreach (var member in entry.members)
                {
                    bool isPresent = false;

                    foreach (var pField in data.Fields)
                    {
                        if (member.id == pField.Key)
                        {
                            isPresent = true;
                        }
                    }

                    if (!isPresent)
                    {
                        missingFields.Add(member);
                    }
                }
            }
        }

        foreach (var missing in missingFields)
        {
            // Unknown should be skipped
            if (missing.id != "Unknown")
            {
                if (ImGui.Button($"Add##{missing.id}"))
                {
                    AddMissingField(_selectedParamGroup, missing);
                }
                ImGui.SameLine();
                ImGui.Text($"{missing.name}");
            }
        }
    }

    public void AddMissingField(Param targetParam, FormatMember missingField)
    {
        var typeName = GparamFormatBank.Bank.GetTypeForProperty(missingField.id);

        if (typeName == "Byte")
        {
            GPARAM.ByteField newField = new GPARAM.ByteField();
            newField.Key = missingField.id;
            newField.Name = missingField.name;

            // Empty values
            var valueList = new GPARAM.FieldValue<byte>();
            valueList.Id = 0;
            valueList.Unk04 = 0;
            valueList.Value = 0;

            newField.Values = new List<FieldValue<byte>> { valueList };

            targetParam.Fields.Add(newField);
        }
        if (typeName == "Short")
        {
            GPARAM.ShortField newField = new GPARAM.ShortField();
            newField.Key = missingField.id;
            newField.Name = missingField.name;

            // Empty values
            var valueList = new GPARAM.FieldValue<short>();
            valueList.Id = 0;
            valueList.Unk04 = 0;
            valueList.Value = 0;

            newField.Values = new List<FieldValue<short>> { valueList };

            targetParam.Fields.Add(newField);
        }
        if (typeName == "IntA" || typeName == "IntB")
        {
            GPARAM.IntField newField = new GPARAM.IntField();
            newField.Key = missingField.id;
            newField.Name = missingField.name;

            // Empty values
            var valueList = new GPARAM.FieldValue<int>();
            valueList.Id = 0;
            valueList.Unk04 = 0;
            valueList.Value = 0;

            newField.Values = new List<FieldValue<int>> { valueList };

            targetParam.Fields.Add(newField);
        }
        if (typeName == "Float")
        {
            GPARAM.FloatField newField = new GPARAM.FloatField();
            newField.Key = missingField.id;
            newField.Name = missingField.name;

            // Empty values
            var valueList = new GPARAM.FieldValue<float>();
            valueList.Id = 0;
            valueList.Unk04 = 0;
            valueList.Value = 0;

            newField.Values = new List<FieldValue<float>> { valueList };

            targetParam.Fields.Add(newField);
        }
        if (typeName == "BoolA" || typeName == "BoolB")
        {
            GPARAM.BoolField newField = new GPARAM.BoolField();
            newField.Key = missingField.id;
            newField.Name = missingField.name;

            // Empty values
            var valueList = new GPARAM.FieldValue<bool>();
            valueList.Id = 0;
            valueList.Unk04 = 0;
            valueList.Value = false;

            newField.Values = new List<FieldValue<bool>> { valueList };

            targetParam.Fields.Add(newField);
        }
        if (typeName == "Float2")
        {
            GPARAM.Vector2Field newField = new GPARAM.Vector2Field();
            newField.Key = missingField.id;
            newField.Name = missingField.name;

            // Empty values
            var valueList = new GPARAM.FieldValue<Vector2>();
            valueList.Id = 0;
            valueList.Unk04 = 0;
            valueList.Value = new Vector2(0f, 0f);

            newField.Values = new List<FieldValue<Vector2>> { valueList };

            targetParam.Fields.Add(newField);
        }
        if (typeName == "Float3")
        {
            GPARAM.Vector3Field newField = new GPARAM.Vector3Field();
            newField.Key = missingField.id;
            newField.Name = missingField.name;

            // Empty values
            var valueList = new GPARAM.FieldValue<Vector3>();
            valueList.Id = 0;
            valueList.Unk04 = 0;
            valueList.Value = new Vector3(0f, 0f, 0f);

            newField.Values = new List<FieldValue<Vector3>> { valueList };

            targetParam.Fields.Add(newField);
        }
        if (typeName == "Float4")
        {
            GPARAM.Vector4Field newField = new GPARAM.Vector4Field();
            newField.Key = missingField.id;
            newField.Name = missingField.name;

            // Empty values
            var valueList = new GPARAM.FieldValue<Vector4>();
            valueList.Id = 0;
            valueList.Unk04 = 0;
            valueList.Value = new Vector4(0f, 0f, 0f, 0f);

            newField.Values = new List<FieldValue<Vector4>> { valueList };

            targetParam.Fields.Add(newField);
        }

        // Unknown
    }

    public void ShowGparamFieldContext(int index)
    {
        if (index == _selectedParamFieldKey)
        {
            if (ImGui.BeginPopupContextItem($"Options##Gparam_Field_Context"))
            {
                if (ImGui.Selectable("Remove"))
                {
                    _selectedParamGroup.Fields.Remove(_selectedParamField);

                    ImGui.CloseCurrentPopup();
                }
                ImguiUtils.ShowButtonTooltip("Delete the selected row.");

                ImGui.EndPopup();
            }
        }
    }

    private void GparamValueProperties()
    {
        ImGui.Begin("Values##GparamValues");

        ImGui.Separator();

        ImGui.InputText($"Search", ref _fieldIdSearchInput, 255);
        ImGui.SameLine();
        ImguiUtils.ShowHelpMarker("Separate terms are split via the + character.");

        ImGui.Separator();

        if (_fieldIdSearchInput != _fieldIdSearchInputCache)
        {
            _fieldIdSearchInputCache = _fieldIdSearchInput;
        }

        if (_selectedParamField != null && _selectedParamFieldKey != -1)
        {
            GPARAM.IField field = _selectedParamField;

            ResetDisplayTruth(field);

            ImGui.Columns(4);

            // ID
            ImGui.BeginChild("IdList##GparamPropertyIds");
            ImGui.Text($"ID");
            ImGui.Separator();

            for (int i = 0; i < field.Values.Count; i++)
            {
                GPARAM.IFieldValue entry = field.Values[i];

                displayTruth[i] = SearchFilters.IsIdSearchMatch(_fieldIdSearchInput, entry.Id.ToString());

                if (displayTruth[i])
                {
                    GparamProperty_ID(i, field, entry);
                }
            }

            ImGui.EndChild();

            ImGui.NextColumn();

            // Time of Day
            ImGui.BeginChild("IdList##GparamTimeOfDay");
            ImGui.Text($"Time of Day");
            ImGui.Separator();

            for (int i = 0; i < field.Values.Count; i++)
            {
                if (displayTruth[i])
                {
                    GPARAM.IFieldValue entry = field.Values[i];
                    GparamProperty_TimeOfDay(i, field, entry);
                }
            }

            ImGui.EndChild();

            ImGui.NextColumn();

            // Value
            ImGui.BeginChild("ValueList##GparamPropertyValues");
            ImGui.Text($"Value");
            ImGui.Separator();

            for (int i = 0; i < field.Values.Count; i++)
            {
                if (displayTruth[i])
                {
                    GPARAM.IFieldValue entry = field.Values[i];
                    GparamProperty_Value(i, field, entry);
                }
            }

            ImGui.EndChild();

            // Information
            ImGui.NextColumn();

            // Value
            ImGui.BeginChild("InfoList##GparamPropertyInfo");
            ImGui.Text($"Information");
            ImGui.Separator();

            // Only show once
            GparamProperty_Info(field);

            ImGui.EndChild();
        }

        ImGui.End();
    }

    public void ResetDisplayTruth(IField field)
    {
        displayTruth = new bool[field.Values.Count];

        for (int i = 0; i < field.Values.Count; i++)
        {
            displayTruth[i] = true;
        }
    }

    public void ExtendDisplayTruth(IField field)
    {
        displayTruth = new bool[field.Values.Count + 1];

        for (int i = 0; i < field.Values.Count + 1; i++)
        {
            displayTruth[i] = true;
        }
    }

    public void GparamProperty_ID(int index, IField field, IFieldValue value)
    {
        ImGui.AlignTextToFramePadding();

        string name = value.Id.ToString();

        if (ImGui.Selectable($"{name}##{index}", index == _selectedFieldValueKey))
        {
            _selectedFieldValue = value;
            _selectedFieldValueKey = index;
            _duplicateValueRowId = value.Id;
        }

        DisplayPropertyIdContext(index);
    }

    public void DisplayPropertyIdContext(int index)
    {
        if (index == _selectedFieldValueKey)
        {
            if (ImGui.BeginPopupContextItem($"Options##Gparam_PropId_Context"))
            {
                if (ImGui.Selectable("Remove"))
                {
                    RemoveProperyValueRow();

                    // Update the group index lists to account for the removed ID.
                    GparamEditor.UpdateGroupIndexes(_selectedGparam);

                    ImGui.CloseCurrentPopup();
                }
                ImguiUtils.ShowButtonTooltip("Delete the value row.");

                if (ImGui.Selectable("Duplicate"))
                {
                    DuplicateProperyValueRow();

                    // Update the group index lists to account for the new ID.
                    GparamEditor.UpdateGroupIndexes(_selectedGparam);

                    ImGui.CloseCurrentPopup();
                }
                ImguiUtils.ShowButtonTooltip("Duplicate the selected value row, assigning the specified ID below as the new id.");

                ImGui.InputInt("##valueIdInput", ref _duplicateValueRowId);

                if (_duplicateValueRowId < 0)
                {
                    _duplicateValueRowId = 0;
                }

                ImGui.EndPopup();
            }
        }
    }

    public void DuplicateProperyValueRow()
    {
        ExtendDisplayTruth(_selectedParamField);

        if (_selectedParamField is SbyteField sbyteField)
        {
            GPARAM.SbyteField castField = (SbyteField)_selectedParamField;

            var dupeVal = new GPARAM.FieldValue<sbyte>();
            dupeVal.Id = _duplicateValueRowId;
            dupeVal.Unk04 = _selectedFieldValue.Unk04;
            dupeVal.Value = (sbyte)_selectedFieldValue.Value;

            castField.Values.Add(dupeVal);
        }
        if (_selectedParamField is ByteField byteField)
        {
            GPARAM.ByteField castField = (ByteField)_selectedParamField;

            var dupeVal = new GPARAM.FieldValue<byte>();
            dupeVal.Id = _duplicateValueRowId;
            dupeVal.Unk04 = _selectedFieldValue.Unk04;
            dupeVal.Value = (byte)_selectedFieldValue.Value;

            castField.Values.Add(dupeVal);
        }
        if (_selectedParamField is ShortField shortField)
        {
            GPARAM.ShortField castField = (ShortField)_selectedParamField;

            var dupeVal = new GPARAM.FieldValue<short>();
            dupeVal.Id = _duplicateValueRowId;
            dupeVal.Unk04 = _selectedFieldValue.Unk04;
            dupeVal.Value = (short)_selectedFieldValue.Value;

            castField.Values.Add(dupeVal);
        }
        if (_selectedParamField is IntField intField)
        {
            GPARAM.IntField castField = (IntField)_selectedParamField;

            var dupeVal = new GPARAM.FieldValue<int>();
            dupeVal.Id = _duplicateValueRowId;
            dupeVal.Unk04 = _selectedFieldValue.Unk04;
            dupeVal.Value = (int)_selectedFieldValue.Value;

            castField.Values.Add(dupeVal);
        }
        if (_selectedParamField is UintField uintField)
        {
            GPARAM.UintField castField = (UintField)_selectedParamField;

            var dupeVal = new GPARAM.FieldValue<uint>();
            dupeVal.Id = _duplicateValueRowId;
            dupeVal.Unk04 = _selectedFieldValue.Unk04;
            dupeVal.Value = (uint)_selectedFieldValue.Value;

            castField.Values.Add(dupeVal);
        }
        if (_selectedParamField is FloatField floatField)
        {
            GPARAM.FloatField castField = (FloatField)_selectedParamField;

            var dupeVal = new GPARAM.FieldValue<float>();
            dupeVal.Id = _duplicateValueRowId;
            dupeVal.Unk04 = _selectedFieldValue.Unk04;
            dupeVal.Value = (float)_selectedFieldValue.Value;

            castField.Values.Add(dupeVal);
        }
        if (_selectedParamField is BoolField boolField)
        {
            GPARAM.BoolField castField = (BoolField)_selectedParamField;

            var dupeVal = new GPARAM.FieldValue<bool>();
            dupeVal.Id = _duplicateValueRowId;
            dupeVal.Unk04 = _selectedFieldValue.Unk04;
            dupeVal.Value = (bool)_selectedFieldValue.Value;

            castField.Values.Add(dupeVal);
        }
        if (_selectedParamField is Vector2Field vector2Field)
        {
            GPARAM.Vector2Field castField = (Vector2Field)_selectedParamField;

            var dupeVal = new GPARAM.FieldValue<Vector2>();
            dupeVal.Id = _duplicateValueRowId;
            dupeVal.Unk04 = _selectedFieldValue.Unk04;
            dupeVal.Value = (Vector2)_selectedFieldValue.Value;

            castField.Values.Add(dupeVal);
        }
        if (_selectedParamField is Vector3Field vector3Field)
        {
            GPARAM.Vector3Field castField = (Vector3Field)_selectedParamField;

            var dupeVal = new GPARAM.FieldValue<Vector3>();
            dupeVal.Id = _duplicateValueRowId;
            dupeVal.Unk04 = _selectedFieldValue.Unk04;
            dupeVal.Value = (Vector3)_selectedFieldValue.Value;

            castField.Values.Add(dupeVal);
        }
        if (_selectedParamField is Vector4Field vector4Field)
        {
            GPARAM.Vector4Field castField = (Vector4Field)_selectedParamField;

            var dupeVal = new GPARAM.FieldValue<Vector4>();
            dupeVal.Id = _duplicateValueRowId;
            dupeVal.Unk04 = _selectedFieldValue.Unk04;
            dupeVal.Value = (Vector4)_selectedFieldValue.Value;

            castField.Values.Add(dupeVal);
        }
        if (_selectedParamField is ColorField colorField)
        {
            GPARAM.ColorField castField = (ColorField)_selectedParamField;

            var dupeVal = new GPARAM.FieldValue<Color>();
            dupeVal.Id = _duplicateValueRowId;
            dupeVal.Unk04 = _selectedFieldValue.Unk04;
            dupeVal.Value = (Color)_selectedFieldValue.Value;

            castField.Values.Add(dupeVal);
        }
    }

    public void RemoveProperyValueRow()
    {
        if (_selectedParamField is SbyteField sbyteField)
        {
            GPARAM.SbyteField castField = (SbyteField)_selectedParamField;
            castField.Values.Remove((FieldValue<sbyte>)_selectedFieldValue);
        }
        if (_selectedParamField is ByteField byteField)
        {
            GPARAM.ByteField castField = (ByteField)_selectedParamField;
            castField.Values.Remove((FieldValue<byte>)_selectedFieldValue);
        }
        if (_selectedParamField is ShortField shortField)
        {
            GPARAM.ShortField castField = (ShortField)_selectedParamField;
            castField.Values.Remove((FieldValue<short>)_selectedFieldValue);
        }
        if (_selectedParamField is IntField intField)
        {
            GPARAM.IntField castField = (IntField)_selectedParamField;
            castField.Values.Remove((FieldValue<int>)_selectedFieldValue);
        }
        if (_selectedParamField is UintField uintField)
        {
            GPARAM.UintField castField = (UintField)_selectedParamField;
            castField.Values.Remove((FieldValue<uint>)_selectedFieldValue);
        }
        if (_selectedParamField is FloatField floatField)
        {
            GPARAM.FloatField castField = (FloatField)_selectedParamField;
            castField.Values.Remove((FieldValue<float>)_selectedFieldValue);
        }
        if (_selectedParamField is BoolField boolField)
        {
            GPARAM.BoolField castField = (BoolField)_selectedParamField;
            castField.Values.Remove((FieldValue<bool>)_selectedFieldValue);
        }
        if (_selectedParamField is Vector2Field vector2Field)
        {
            GPARAM.Vector2Field castField = (Vector2Field)_selectedParamField;
            castField.Values.Remove((FieldValue<Vector2>)_selectedFieldValue);
        }
        if (_selectedParamField is Vector3Field vector3Field)
        {
            GPARAM.Vector3Field castField = (Vector3Field)_selectedParamField;
            castField.Values.Remove((FieldValue<Vector3>)_selectedFieldValue);
        }
        if (_selectedParamField is Vector4Field vector4Field)
        {
            GPARAM.Vector4Field castField = (Vector4Field)_selectedParamField;
            castField.Values.Remove((FieldValue<Vector4>)_selectedFieldValue);
        }
        if (_selectedParamField is ColorField colorField)
        {
            GPARAM.ColorField castField = (ColorField)_selectedParamField;
            castField.Values.Remove((FieldValue<Color>)_selectedFieldValue);
        }
    }

    public void GparamProperty_TimeOfDay(int index, IField field, IFieldValue value)
    {
        ImGui.AlignTextToFramePadding();
        GparamEditor.TimeOfDayField(index, field, value);
    }

    public void GparamProperty_Value(int index, IField field, IFieldValue value)
    {
        ImGui.AlignTextToFramePadding();
        GparamEditor.ValueField(index, field, value);
    }

    public void GparamProperty_Info(IField field)
    {
        ImGui.AlignTextToFramePadding();

        string desc = GparamFormatBank.Bank.GetReferenceDescription(_selectedParamField.Key);

        // Skip if empty
        if (desc != "")
        {
            ImGui.Text($"{desc}");
        }

        // Show enum list if they exist
        var propertyEnum = GparamFormatBank.Bank.GetEnumForProperty(field.Key);
        if (propertyEnum != null)
        {
            foreach (var entry in propertyEnum.members)
            {
                ImGui.Text($"{entry.id} - {entry.name}");
            }
        }
    }

    public void OnProjectChanged(ProjectSettings newSettings)
    {
        _projectSettings = newSettings;

        if (CFG.Current.AutoLoadBank_Gparam)
            GparamParamBank.LoadGraphicsParams();

        ResetActionManager();
    }

    public void Save()
    {
        if (GparamParamBank.IsLoaded)
            GparamParamBank.SaveGraphicsParam(_selectedGparamInfo);
    }

    public void SaveAll()
    {
        if (GparamParamBank.IsLoaded)
            GparamParamBank.SaveGraphicsParams();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }


    private void ResetAllSelection()
    {
        ResetFileSelection();
        ResetGroupSelection();
        ResetFieldSelection();
        ResetValueSelection();
    }

    private void ResetFileSelection()
    {
        _selectedGparam = null;
        _selectedGparamKey = "";
    }

    private void ResetGroupSelection()
    {
        _selectedParamGroup = null;
        _selectedParamGroupKey = -1;
    }

    private void ResetFieldSelection()
    {
        _selectedParamField = null;
        _selectedParamFieldKey = -1;
    }

    private void ResetValueSelection()
    {
        _selectedFieldValue = null;
        _selectedFieldValueKey = -1;
    }

    public void RemoveGparamFile(GparamParamBank.GparamInfo info)
    {
        string filePath = info.Path;
        string baseFileName = info.Name;

        filePath = filePath.Replace($"{Project.GameRootDirectory}", $"{Project.GameModDirectory}");

        if (File.Exists(filePath))
        {
            TaskLogs.AddLog($"{baseFileName} was removed from your project.");
            File.Delete(filePath);
        }
        else
        {
            TaskLogs.AddLog($"{baseFileName} does not exist within your project.");
        }

        GparamParamBank.LoadGraphicsParams();
    }

    public void CopyGparamFile(GparamParamBank.GparamInfo info)
    {
        string filePath = info.Path;
        string baseFileName = info.Name;
        string tryFileName = _copyFileNewName;

        string newFilePath = filePath.Replace(baseFileName, tryFileName);

        // If the original is in the root dir, change the path to mod
        newFilePath = newFilePath.Replace($"{Project.GameRootDirectory}", $"{Project.GameModDirectory}");

        if (!File.Exists(newFilePath))
        {
            File.Copy(filePath, newFilePath);
        }
        else
        {
            TaskLogs.AddLog($"{newFilePath} already exists!");
        }

        GparamParamBank.LoadGraphicsParams();
    }

    public void DuplicateGparamFile()
    {
        bool isValidFile = false;
        string filePath = _selectedGparamInfo.Path;
        string baseFileName = _selectedGparamInfo.Name;
        string tryFileName = _selectedGparamInfo.Name;

        do
        {
            string currentfileName = CreateDuplicateFileName(tryFileName);
            string newFilePath = filePath.Replace(baseFileName, currentfileName);

            // If the original is in the root dir, change the path to mod
            newFilePath = newFilePath.Replace($"{Project.GameRootDirectory}", $"{Project.GameModDirectory}");

            if (!File.Exists(newFilePath))
            {
                File.Copy(filePath, newFilePath);
                isValidFile = true;
            }
            else
            {
                TaskLogs.AddLog($"{newFilePath} already exists!");
                tryFileName = currentfileName;
            }
        }
        while (!isValidFile);

        GparamParamBank.LoadGraphicsParams();
    }

    public string CreateDuplicateFileName(string fileName)
    {
        Match mapMatch = Regex.Match(fileName, @"[0-9]{4}");

        if(mapMatch.Success)
        {
            var res = mapMatch.Groups[0].Value;

            int slot = 0;
            string slotStr = "";

            try
            {
                int number;
                int.TryParse(res, out number);

                slot = number + 1;
            }
            catch { }

            if(slot >= 100 && slot < 999)
            {
                slotStr = "0";
            }
            if (slot >= 10 && slot < 99)
            {
                slotStr = "00";
            }
            if (slot >= 0 && slot < 9)
            {
                slotStr = "000";
            }

            var finalSlotStr = $"{slotStr}{slot}";
            var final = fileName.Replace(res, finalSlotStr);

            return final;
        }
        else
        {
            Match dupeMatch = Regex.Match(fileName, @"__[0-9]{1}");

            if (dupeMatch.Success)
            {
                var res = dupeMatch.Groups[0].Value;

                Match numMatch = Regex.Match(res, @"[0-9]{1}");

                var num = numMatch.Groups[0].Value;
                try
                {
                    int number;
                    int.TryParse(res, out number);

                    number = number + 1;

                    return $"{fileName}__{number}";
                }
                catch 
                {
                    return $"{fileName}__1";
                }
            }
            else
            {
                return $"{fileName}__1";
            }
        }
    }
}
