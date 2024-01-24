﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Editor;
using System.Reflection;

namespace StudioCore.ScriptEditor;

public class PropertyEditor
{
    private Dictionary<string, PropertyInfo[]> _propCache = new();

    public ActionManager ContextActionManager;

    public PropertyEditor(ActionManager manager)
    {
        ContextActionManager = manager;
    }
}
