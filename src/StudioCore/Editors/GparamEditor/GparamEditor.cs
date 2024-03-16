﻿using ImGuiNET;
using Microsoft.Extensions.Logging;
using Octokit;
using SoulsFormats;
using StudioCore.BanksMain;
using StudioCore.Editor;
using StudioCore.GraphicsEditor;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using static SoulsFormats.GPARAM;
using static StudioCore.Editor.GparamValueChangeAction;
using static StudioCore.Editors.GraphicsEditor.GparamParamBank;

namespace StudioCore.Editors.GparamEditor;
public class GparamEditor
{
    private static object _editedValueCache;

    // Value has been changed via input
    private static bool _changedCache;

    // Value can be changed in the GPARAM
    private static bool _committedCache;

    // Value to change without commit
    private static bool _uncommittedCache;

    private static bool _isHoldingColor;
    private static Vector4 _heldColor;

    public static unsafe void ValueField(int idx, IField field, IFieldValue value, GparamInfo _selectedGparamInfo)
    {
        _changedCache = false;
        _committedCache = false;
        _uncommittedCache = false;

        ImGui.SetNextItemWidth(-1);

        object oldValue = null;
        object newValue = null;

        // INT
        if (field is GPARAM.IntField intField)
        {
            int fieldValue = intField.Values[idx].Value;
            int intInput = fieldValue;
            oldValue = fieldValue;

            if (GparamFormatBank.Bank.IsBooleanProperty(field.Key))
            {
                bool boolInput = false;
                if (fieldValue > 0)
                    boolInput = true;

                if (ImGui.Checkbox($"##value{idx}", ref boolInput))
                {
                    if (boolInput)
                    {
                        newValue = 1;
                    }
                    else
                    {
                        newValue = 0;
                    }

                    _editedValueCache = newValue;
                    _changedCache = true;
                }

                _committedCache = ImGui.IsItemDeactivatedAfterEdit();
            }
            else
            {
                if (ImGui.InputInt($"##value{idx}", ref intInput))
                {
                    newValue = intInput;

                    _editedValueCache = newValue;
                    _changedCache = true;
                }
                _committedCache = ImGui.IsItemDeactivatedAfterEdit();
            }
        }
        // UINT
        else if (field is GPARAM.UintField uintField)
        {
            uint fieldValue = uintField.Values[idx].Value;
            uint uintInput = fieldValue;
            oldValue = fieldValue;

            var strval = $@"{uintInput}";

            if (GparamFormatBank.Bank.IsBooleanProperty(field.Key))
            {
                bool boolInput = false;
                if (fieldValue > 0)
                    boolInput = true;

                if (ImGui.Checkbox($"##value{idx}", ref boolInput))
                {
                    if (boolInput)
                    {
                        newValue = 1;
                    }
                    else
                    {
                        newValue = 0;
                    }
                    _editedValueCache = newValue;
                    _changedCache = true;
                }
                _committedCache = ImGui.IsItemDeactivatedAfterEdit();
            }
            else
            {
                if (ImGui.InputText($"##value{idx}", ref strval, 16))
                {
                    bool result = uint.TryParse(strval, out uintInput);

                    if (result)
                    {
                        newValue = uintInput;
                        _editedValueCache = newValue;
                        _changedCache = true;
                    }
                }
                _committedCache = ImGui.IsItemDeactivatedAfterEdit();
            }
        }
        // SHORT
        else if (field is GPARAM.ShortField shortField)
        {
            short fieldValue = shortField.Values[idx].Value;
            int shortInput = fieldValue;
            oldValue = fieldValue;

            if (GparamFormatBank.Bank.IsBooleanProperty(field.Key))
            {
                bool boolInput = false;
                if (fieldValue > 0)
                    boolInput = true;

                if (ImGui.Checkbox($"##value{idx}", ref boolInput))
                {
                    if (boolInput)
                    {
                        newValue = 1;
                    }
                    else
                    {
                        newValue = 0;
                    }
                    _editedValueCache = newValue;
                    _changedCache = true;
                }
                _committedCache = ImGui.IsItemDeactivatedAfterEdit();
            }
            else
            {
                if (ImGui.InputInt($"##value{idx}", ref shortInput))
                {
                     newValue = shortInput;
                    _editedValueCache = newValue;
                    _changedCache = true;
                }
                _committedCache = ImGui.IsItemDeactivatedAfterEdit();
            }
        }
        // SBYTE
        else if (field is GPARAM.SbyteField sbyteField)
        {
            sbyte fieldValue = sbyteField.Values[idx].Value;
            int sbyteInput = fieldValue;
            oldValue = fieldValue;

            if (GparamFormatBank.Bank.IsBooleanProperty(field.Key))
            {
                bool boolInput = false;
                if (fieldValue > 0)
                    boolInput = true;

                if (ImGui.Checkbox($"##value{idx}", ref boolInput))
                {
                    if (boolInput)
                    {
                        newValue = 1;
                    }
                    else
                    {
                        newValue = 0;
                    }
                    _editedValueCache = newValue;
                    _changedCache = true;
                }
                _committedCache = ImGui.IsItemDeactivatedAfterEdit();
            }
            else
            {
                if (ImGui.InputInt($"##value{idx}", ref sbyteInput))
                {
                    newValue = sbyteInput;
                    _editedValueCache = newValue;
                    _changedCache = true;
                }
                _committedCache = ImGui.IsItemDeactivatedAfterEdit();
            }
        }
        // BYTE
        else if (field is GPARAM.ByteField byteField)
        {
            byte fieldValue = byteField.Values[idx].Value;
            byte byteInput = fieldValue;
            oldValue = fieldValue;

            var strval = $@"{byteInput}";

            if (GparamFormatBank.Bank.IsBooleanProperty(field.Key))
            {
                bool boolInput = false;
                if (fieldValue > 0)
                    boolInput = true;

                if (ImGui.Checkbox($"##value{idx}", ref boolInput))
                {
                    if (boolInput)
                    {
                        newValue = 1;
                    }
                    else
                    {
                        newValue = 0;
                    }
                    _editedValueCache = newValue;
                    _changedCache = true;
                }
                _committedCache = ImGui.IsItemDeactivatedAfterEdit();
            }
            else
            {
                if (ImGui.InputText($"##value{idx}", ref strval, 16))
                {
                    bool result = byte.TryParse(strval, out byteInput);

                    if (result)
                    {
                        newValue = byteInput;
                        _editedValueCache = newValue;
                        _changedCache = true;
                    }
                }
                _committedCache = ImGui.IsItemDeactivatedAfterEdit();
            }
        }
        // BOOL
        else if (field is GPARAM.BoolField boolField)
        {
            bool fieldValue = boolField.Values[idx].Value;
            bool boolInput = fieldValue;
            oldValue = fieldValue;

            if (ImGui.Checkbox($"##value{idx}", ref boolInput))
            {
                newValue = boolInput;
                _editedValueCache = newValue;
                _changedCache = true;
            }
            _committedCache = ImGui.IsItemDeactivatedAfterEdit();
        }
        // FLOAT
        else if (field is GPARAM.FloatField floatField)
        {
            float fieldValue = floatField.Values[idx].Value;
            float floatInput = fieldValue;
            oldValue = fieldValue;

            if (ImGui.InputFloat($"##value{idx}", ref floatInput, 0.1f, 1.0f, 
                Utils.ImGui_InputFloatFormat(floatInput)))
            {
                newValue = floatInput;
                _editedValueCache = newValue;
                _changedCache = true;
            }
            _committedCache = ImGui.IsItemDeactivatedAfterEdit();
        }
        // VECTOR2
        else if (field is GPARAM.Vector2Field vector2Field)
        {
            Vector2 fieldValue = vector2Field.Values[idx].Value;
            Vector2 vector2Input = fieldValue;
            oldValue = fieldValue;

            if (ImGui.InputFloat2($"##value{idx}", ref vector2Input))
            {
                newValue = vector2Input;
                _editedValueCache = newValue;
                _changedCache = true;
            }
            _committedCache = ImGui.IsItemDeactivatedAfterEdit();
        }
        // VECTOR3
        else if (field is GPARAM.Vector3Field vector3Field)
        {
            Vector3 fieldValue = vector3Field.Values[idx].Value;
            Vector3 vector3Input = fieldValue;
            oldValue = fieldValue;

            if (ImGui.InputFloat3($"##value{idx}", ref vector3Input))
            {
                newValue = vector3Input;
                _editedValueCache = newValue;
                _changedCache = true;
            }
            _committedCache = ImGui.IsItemDeactivatedAfterEdit();
        }
        // VECTOR4
        else if (field is GPARAM.Vector4Field vector4Field &&
            !CFG.Current.Gparam_DisplayColorEditForVector4Fields)
        {
            Vector4 fieldValue = vector4Field.Values[idx].Value;
            Vector4 vector4Input = fieldValue;
            oldValue = fieldValue;

            if (ImGui.InputFloat4($"##value{idx}", ref vector4Input))
            {
                newValue = vector4Input;
                _editedValueCache = newValue;
                _changedCache = true;
            }
            _committedCache = ImGui.IsItemDeactivatedAfterEdit();
        }
        // VECTOR4 (COLOR EDIT)
        else if (field is GPARAM.Vector4Field vectorColorField && 
            CFG.Current.Gparam_DisplayColorEditForVector4Fields)
        {
            Vector4 fieldValue = vectorColorField.Values[idx].Value;
            Vector4 colorInput = fieldValue;
            oldValue = fieldValue;

            if (!_isHoldingColor)
            {
                _isHoldingColor = true;
                _heldColor = (Vector4)oldValue;
            }

            var flags = ImGuiColorEditFlags.None;

            if(CFG.Current.Gparam_ColorEdit_RGB)
            {
                flags = ImGuiColorEditFlags.DisplayRGB;
            }
            if (CFG.Current.Gparam_ColorEdit_Decimal)
            {
                flags = ImGuiColorEditFlags.Float;
            }
            if (CFG.Current.Gparam_ColorEdit_HSV)
            {
                flags = ImGuiColorEditFlags.DisplayHSV;
            }

            if (ImGui.ColorEdit4($"##value{idx}", ref colorInput, flags))
            {
                newValue = colorInput;
                _editedValueCache = newValue;
                _changedCache = true;
                _uncommittedCache = true;
            }

            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                newValue = colorInput;
                _editedValueCache = newValue;
                _changedCache = true;
                _committedCache = true;
                TaskLogs.AddLog($"IsItemDeactivatedAfterEdit");
            }
        }
        // COLOR
        else if (field is GPARAM.ColorField colorField)
        {
            Color raw = colorField.Values[idx].Value;
            Vector4 fieldValue = new(raw.R / 255.0f, raw.G / 255.0f, raw.B / 255.0f, raw.A / 255.0f);
            Vector4 colorInput = fieldValue;
            oldValue = fieldValue;

            if (ImGui.ColorEdit4($"##value{idx}", ref colorInput))
            {
                Color trueColorInput = Color.FromArgb(
                    (int)(colorInput.W * 255.0f),
                    (int)(colorInput.X * 255.0f),
                    (int)(colorInput.Y * 255.0f),
                    (int)(colorInput.Z * 255.0f)
                    );

                newValue = trueColorInput;
                _editedValueCache = newValue;
                _changedCache = true;
            }
            _committedCache = ImGui.IsItemDeactivatedAfterEdit();
        }
        else
        {
            TaskLogs.AddLog($"{field.Name} {field.GetType()} is not supported");
        }

        // Update and Commit
        if (_editedValueCache != null && _editedValueCache != oldValue)
        {
            _changedCache = true;
        }

        if (_changedCache)
        {
            if (_committedCache)
            {
                if(_isHoldingColor)
                {
                    _isHoldingColor = false;

                    // Reset color to original color befor edit
                    // so undo reverts in the expected fashion
                    if (field is GPARAM.Vector4Field vector4Field)
                    {
                        vector4Field.Values[idx].Value = _heldColor;
                    }
                }

                if (newValue == null)
                {
                    return;
                }
                else
                {
                    _selectedGparamInfo.WasModified = true;
                    GparamValueChangeAction action = null;
                    action = new GparamValueChangeAction(field, value, newValue, idx, GparamValueChangeAction.ValueChangeType.Set);
                    GparamEditorScreen.EditorActionManager.ExecuteAction(action);
                    TaskLogs.AddLog($"GparamValueChangeAction");
                }
            }
            // Only used for Vec4 color
            else if(_uncommittedCache)
            {
                if (newValue == null)
                {
                    return;
                }
                else
                {
                    if (field is GPARAM.Vector4Field vector4Field)
                    {
                        var assignedValue = (Vector4)newValue;
                        var result = assignedValue;

                        vector4Field.Values[idx].Value = result;
                    }
                }
            }
        }
    }

    public static unsafe void TimeOfDayField(int idx, IField field, IFieldValue value, GparamInfo _selectedGparamInfo)
    {
        _changedCache = false;
        _committedCache = false;

        ImGui.SetNextItemWidth(-1);

        object oldValue = null;
        object newValue = null;

        float fieldValue = field.Values[idx].Unk04;
        float floatInput = fieldValue;
        oldValue = fieldValue;

        if (ImGui.InputFloat($"##tod{idx}", ref floatInput))
        {
            newValue = floatInput;
            _editedValueCache = newValue;
            _changedCache = true;
        }
        _committedCache = ImGui.IsItemDeactivatedAfterEdit();

        // Update and Commit
        if (_editedValueCache != null && _editedValueCache != oldValue)
        {
            _changedCache = true;
        }

        if (_changedCache)
        {
            if (_committedCache)
            {
                if (newValue == null)
                {
                    return;
                }
                else
                {
                    _selectedGparamInfo.WasModified = true;
                    GparamTimeOfDayChangeAction action = null;
                    action = new GparamTimeOfDayChangeAction(field, value, newValue, idx);
                    GparamEditorScreen.EditorActionManager.ExecuteAction(action);
                }
            }
        }
    }

    /// <summary>
    /// Update the group index lists to reflect any additions
    /// or removals in terms of value row ids
    /// </summary>
    public static void UpdateGroupIndexes(GPARAM gparam)
    {
        var newGroupIndexes = new List<UnkParamExtra>();
        int idx = 0;

        foreach (var param in gparam.Params)
        {
            var newGroupIndexList = new UnkParamExtra();
            newGroupIndexList.Unk00 = idx;
            newGroupIndexList.Unk0c = 0; // Always 0

            foreach (var field in param.Fields)
            {
                var values = field.Values;

                foreach(var val in values)
                {
                    if (!newGroupIndexList.Ids.Contains(val.Id))
                    {
                        newGroupIndexList.Ids.Add(val.Id);
                    }
                }
            }

            newGroupIndexes.Add(newGroupIndexList);

            ++idx;
        }

        gparam.UnkParamExtras = newGroupIndexes;
    }

    /// <summary>
    /// Add selected value row (with specified new ID) to target field value list
    /// </summary>
    /// <param name="targetField"></param>
    /// <param name="targetValue"></param>
    /// <param name="newRowId"></param>
    public static void AddPropertyValueRow(IField targetField, IFieldValue targetValue, int newRowId)
    {
        if (targetField is SbyteField sbyteField)
        {
            GPARAM.SbyteField castField = (SbyteField)targetField;

            var dupeVal = new GPARAM.FieldValue<sbyte>();
            dupeVal.Id = newRowId;
            dupeVal.Unk04 = targetValue.Unk04;
            dupeVal.Value = (sbyte)targetValue.Value;

            castField.Values.Add(dupeVal);
        }
        if (targetField is ByteField byteField)
        {
            GPARAM.ByteField castField = (ByteField)targetField;

            var dupeVal = new GPARAM.FieldValue<byte>();
            dupeVal.Id = newRowId;
            dupeVal.Unk04 = targetValue.Unk04;
            dupeVal.Value = (byte)targetValue.Value;

            castField.Values.Add(dupeVal);
        }
        if (targetField is ShortField shortField)
        {
            GPARAM.ShortField castField = (ShortField)targetField;

            var dupeVal = new GPARAM.FieldValue<short>();
            dupeVal.Id = newRowId;
            dupeVal.Unk04 = targetValue.Unk04;
            dupeVal.Value = (short)targetValue.Value;

            castField.Values.Add(dupeVal);
        }
        if (targetField is IntField intField)
        {
            GPARAM.IntField castField = (IntField)targetField;

            var dupeVal = new GPARAM.FieldValue<int>();
            dupeVal.Id = newRowId;
            dupeVal.Unk04 = targetValue.Unk04;
            dupeVal.Value = (int)targetValue.Value;

            castField.Values.Add(dupeVal);
        }
        if (targetField is UintField uintField)
        {
            GPARAM.UintField castField = (UintField)targetField;

            var dupeVal = new GPARAM.FieldValue<uint>();
            dupeVal.Id = newRowId;
            dupeVal.Unk04 = targetValue.Unk04;
            dupeVal.Value = (uint)targetValue.Value;

            castField.Values.Add(dupeVal);
        }
        if (targetField is FloatField floatField)
        {
            GPARAM.FloatField castField = (FloatField)targetField;

            var dupeVal = new GPARAM.FieldValue<float>();
            dupeVal.Id = newRowId;
            dupeVal.Unk04 = targetValue.Unk04;
            dupeVal.Value = (float)targetValue.Value;

            castField.Values.Add(dupeVal);
        }
        if (targetField is BoolField boolField)
        {
            GPARAM.BoolField castField = (BoolField)targetField;

            var dupeVal = new GPARAM.FieldValue<bool>();
            dupeVal.Id = newRowId;
            dupeVal.Unk04 = targetValue.Unk04;
            dupeVal.Value = (bool)targetValue.Value;

            castField.Values.Add(dupeVal);
        }
        if (targetField is Vector2Field vector2Field)
        {
            GPARAM.Vector2Field castField = (Vector2Field)targetField;

            var dupeVal = new GPARAM.FieldValue<Vector2>();
            dupeVal.Id = newRowId;
            dupeVal.Unk04 = targetValue.Unk04;
            dupeVal.Value = (Vector2)targetValue.Value;

            castField.Values.Add(dupeVal);
        }
        if (targetField is Vector3Field vector3Field)
        {
            GPARAM.Vector3Field castField = (Vector3Field)targetField;

            var dupeVal = new GPARAM.FieldValue<Vector3>();
            dupeVal.Id = newRowId;
            dupeVal.Unk04 = targetValue.Unk04;
            dupeVal.Value = (Vector3)targetValue.Value;

            castField.Values.Add(dupeVal);
        }
        if (targetField is Vector4Field vector4Field)
        {
            GPARAM.Vector4Field castField = (Vector4Field)targetField;

            var dupeVal = new GPARAM.FieldValue<Vector4>();
            dupeVal.Id = newRowId;
            dupeVal.Unk04 = targetValue.Unk04;
            dupeVal.Value = (Vector4)targetValue.Value;

            castField.Values.Add(dupeVal);
        }
        if (targetField is ColorField colorField)
        {
            GPARAM.ColorField castField = (ColorField)targetField;

            var dupeVal = new GPARAM.FieldValue<Color>();
            dupeVal.Id = newRowId;
            dupeVal.Unk04 = targetValue.Unk04;
            dupeVal.Value = (Color)targetValue.Value;

            castField.Values.Add(dupeVal);
        }
    }

    /// <summary>
    /// Removed selected value row from target field value list
    /// </summary>
    /// <param name="targetField"></param>
    /// <param name="targetValue"></param>
    public static void RemovePropertyValueRow(IField targetField, IFieldValue targetValue)
    {
        if (targetField is SbyteField sbyteField)
        {
            GPARAM.SbyteField castField = (SbyteField)targetField;
            castField.Values.Remove((FieldValue<sbyte>)targetValue);
        }
        if (targetField is ByteField byteField)
        {
            GPARAM.ByteField castField = (ByteField)targetField;
            castField.Values.Remove((FieldValue<byte>)targetValue);
        }
        if (targetField is ShortField shortField)
        {
            GPARAM.ShortField castField = (ShortField)targetField;
            castField.Values.Remove((FieldValue<short>)targetValue);
        }
        if (targetField is IntField intField)
        {
            GPARAM.IntField castField = (IntField)targetField;
            castField.Values.Remove((FieldValue<int>)targetValue);
        }
        if (targetField is UintField uintField)
        {
            GPARAM.UintField castField = (UintField)targetField;
            castField.Values.Remove((FieldValue<uint>)targetValue);
        }
        if (targetField is FloatField floatField)
        {
            GPARAM.FloatField castField = (FloatField)targetField;
            castField.Values.Remove((FieldValue<float>)targetValue);
        }
        if (targetField is BoolField boolField)
        {
            GPARAM.BoolField castField = (BoolField)targetField;
            castField.Values.Remove((FieldValue<bool>)targetValue);
        }
        if (targetField is Vector2Field vector2Field)
        {
            GPARAM.Vector2Field castField = (Vector2Field)targetField;
            castField.Values.Remove((FieldValue<Vector2>)targetValue);
        }
        if (targetField is Vector3Field vector3Field)
        {
            GPARAM.Vector3Field castField = (Vector3Field)targetField;
            castField.Values.Remove((FieldValue<Vector3>)targetValue);
        }
        if (targetField is Vector4Field vector4Field)
        {
            GPARAM.Vector4Field castField = (Vector4Field)targetField;
            castField.Values.Remove((FieldValue<Vector4>)targetValue);
        }
        if (targetField is ColorField colorField)
        {
            GPARAM.ColorField castField = (ColorField)targetField;
            castField.Values.Remove((FieldValue<Color>)targetValue);
        }
    }

    public static unsafe void AddValueField(IField field)
    {
        // INT
        if (field is GPARAM.IntField intField)
        {
            FieldValue<int> fieldValueRow = new FieldValue<int>();
            fieldValueRow.Id = 0;
            fieldValueRow.Unk04 = 0;
            fieldValueRow.Value = 0;
            intField.Values.Add(fieldValueRow);
        }
        // UINT
        else if (field is GPARAM.UintField uintField)
        {
            FieldValue<uint> fieldValueRow = new FieldValue<uint>();
            fieldValueRow.Id = 0;
            fieldValueRow.Unk04 = 0;
            fieldValueRow.Value = 0;
            uintField.Values.Add(fieldValueRow);
        }
        // SHORT
        else if (field is GPARAM.ShortField shortField)
        {
            FieldValue<short> fieldValueRow = new FieldValue<short>();
            fieldValueRow.Id = 0;
            fieldValueRow.Unk04 = 0;
            fieldValueRow.Value = 0;
            shortField.Values.Add(fieldValueRow);
        }
        // SBYTE
        else if (field is GPARAM.SbyteField sbyteField)
        {
            FieldValue<sbyte> fieldValueRow = new FieldValue<sbyte>();
            fieldValueRow.Id = 0;
            fieldValueRow.Unk04 = 0;
            fieldValueRow.Value = 0;
            sbyteField.Values.Add(fieldValueRow);
        }
        // BYTE
        else if (field is GPARAM.ByteField byteField)
        {
            FieldValue<byte> fieldValueRow = new FieldValue<byte>();
            fieldValueRow.Id = 0;
            fieldValueRow.Unk04 = 0;
            fieldValueRow.Value = 0;
            byteField.Values.Add(fieldValueRow);
        }
        // BOOL
        else if (field is GPARAM.BoolField boolField)
        {
            FieldValue<bool> fieldValueRow = new FieldValue<bool>();
            fieldValueRow.Id = 0;
            fieldValueRow.Unk04 = 0;
            fieldValueRow.Value = false;
            boolField.Values.Add(fieldValueRow);
        }
        // FLOAT
        else if (field is GPARAM.FloatField floatField)
        {
            FieldValue<float> fieldValueRow = new FieldValue<float>();
            fieldValueRow.Id = 0;
            fieldValueRow.Unk04 = 0;
            fieldValueRow.Value = 0;
            floatField.Values.Add(fieldValueRow);
        }
        // VECTOR2
        else if (field is GPARAM.Vector2Field vector2Field)
        {
            FieldValue<Vector2> fieldValueRow = new FieldValue<Vector2>();
            fieldValueRow.Id = 0;
            fieldValueRow.Unk04 = 0;
            fieldValueRow.Value = new Vector2(0, 0);
            vector2Field.Values.Add(fieldValueRow);
        }
        // VECTOR3
        else if (field is GPARAM.Vector3Field vector3Field)
        {
            FieldValue<Vector3> fieldValueRow = new FieldValue<Vector3>();
            fieldValueRow.Id = 0;
            fieldValueRow.Unk04 = 0;
            fieldValueRow.Value = new Vector3(0, 0, 0);
            vector3Field.Values.Add(fieldValueRow);
        }
        // VECTOR4
        else if (field is GPARAM.Vector4Field vector4Field)
        {
            FieldValue<Vector4> fieldValueRow = new FieldValue<Vector4>();
            fieldValueRow.Id = 0;
            fieldValueRow.Unk04 = 0;
            fieldValueRow.Value = new Vector4(0, 0, 0, 0);
            vector4Field.Values.Add(fieldValueRow);
        }
        // COLOR
        else if (field is GPARAM.ColorField colorField)
        {
            FieldValue<Color> fieldValueRow = new FieldValue<Color>();
            fieldValueRow.Id = 0;
            fieldValueRow.Unk04 = 0;
            fieldValueRow.Value = new Color();
            colorField.Values.Add(fieldValueRow);
        }
        else
        {
            TaskLogs.AddLog($"{field.Name} {field.GetType()} is not supported");
        }
    }
}
