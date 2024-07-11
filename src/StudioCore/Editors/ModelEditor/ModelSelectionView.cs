﻿using HKLib.hk2018.hk;
using ImGuiNET;
using SoulsFormats.KF4;
using StudioCore.Banks.AliasBank;
using StudioCore.Core;
using StudioCore.Editors.AssetBrowser;
using StudioCore.Interface;
using StudioCore.Locators;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.ParamEditor.Toolbar.ParamToolbar;

namespace StudioCore.Editors.ModelEditor
{
    public class ModelSelectionView
    {
        private string _searchInput = "";
        private string _selectedEntry = "";
        private ModelSelectionType _selectedEntryType = ModelSelectionType.None;

        private ModelEditorScreen Screen;

        public ModelSelectionView(ModelEditorScreen screen)
        {
            Screen = screen;
        }

        public void OnProjectChanged()
        {
            if (Smithbox.ProjectType != ProjectType.Undefined)
            {

            }
        }

        public void OnGui()
        {
            var scale = Smithbox.GetUIScale();

            if (Smithbox.ProjectType == ProjectType.Undefined)
                return;

            if (!Smithbox.AliasCacheHandler.AliasCache.UpdateCacheComplete)
                return;

            if (!CFG.Current.Interface_ModelEditor_ModelSelection)
                return;

            ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

            if (ImGui.Begin($@"Model Selection##ModelSelectionView"))
            {
                ImGui.InputText($"Search", ref _searchInput, 255);
                ImguiUtils.ShowHoverTooltip("Separate terms are split via the + character.");

                DisplayCharacterList();
                DisplayAssetList();
                DisplayPartList();
                DisplayMapPieceList();
            }

            ImGui.End();
            ImGui.PopStyleColor(1);
        }

        private bool FilterSelectionList(string name, Dictionary<string, AliasReference> referenceDict)
        {
            var lowerName = name.ToLower();

            var refName = "";
            var refTagList = new List<string>();

            if (referenceDict.ContainsKey(lowerName))
            {
                refName = referenceDict[lowerName].name;
                refTagList = referenceDict[lowerName].tags;
            }

            if (!CFG.Current.AssetBrowser_ShowLowDetailParts)
            {
                if (name.Substring(name.Length - 2) == "_l")
                {
                    return false;
                }
            }

            if (!SearchFilters.IsAssetBrowserSearchMatch(_searchInput, lowerName, refName, refTagList))
            {
                return false;
            }

            return true;
        }

        private void DisplaySelectableAlias(string name, Dictionary<string, AliasReference> referenceDict)
        {
            var lowerName = name.ToLower();

            if (referenceDict.ContainsKey(lowerName))
            {
                if (CFG.Current.AssetBrowser_ShowAliasesInBrowser)
                {
                    var aliasName = referenceDict[lowerName].name;

                    AliasUtils.DisplayAlias(aliasName);
                }

                // Tags
                if (CFG.Current.AssetBrowser_ShowTagsInBrowser)
                {
                    var tagString = string.Join(" ", referenceDict[lowerName].tags);
                    AliasUtils.DisplayTagAlias(tagString);
                }
            }
        }

        private void DisplayCharacterList()
        {
            if (Smithbox.BankHandler.CharacterAliases.Aliases == null)
                return;

            if (ImGui.CollapsingHeader("Characters"))
            {
                foreach(var entry in Smithbox.AliasCacheHandler.AliasCache.CharacterList)
                {
                    if (FilterSelectionList(entry, Smithbox.AliasCacheHandler.AliasCache.Characters))
                    {
                        if (ImGui.Selectable(entry, entry == _selectedEntry))
                        {
                            _selectedEntry = entry;
                            _selectedEntryType = ModelSelectionType.Character;
                        }
                        DisplaySelectableAlias(entry, Smithbox.AliasCacheHandler.AliasCache.Characters);

                        if(entry == _selectedEntry)
                        {
                            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                            {
                                Screen.LoadCharacter(_selectedEntry);
                            }
                        }
                    }
                }
            }
        }

        private void DisplayAssetList()
        {
            if (Smithbox.BankHandler.AssetAliases.Aliases == null)
                return;

            var assetLabel = "Objects";

            if (Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6)
            {
                assetLabel = "Assets";
            }

            if (ImGui.CollapsingHeader(assetLabel))
            {
                foreach (var entry in Smithbox.AliasCacheHandler.AliasCache.AssetList)
                {
                    if (FilterSelectionList(entry, Smithbox.AliasCacheHandler.AliasCache.Assets))
                    {
                        if (ImGui.Selectable(entry, entry == _selectedEntry))
                        {
                            _selectedEntry = entry;
                            _selectedEntryType = ModelSelectionType.Asset;
                        }
                        DisplaySelectableAlias(entry, Smithbox.AliasCacheHandler.AliasCache.Assets);

                        if (entry == _selectedEntry)
                        {
                            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                            {
                                Screen.LoadAsset(_selectedEntry);
                            }
                        }
                    }
                }
            }
        }

        private void DisplayPartList()
        {
            if (Smithbox.BankHandler.PartAliases.Aliases == null)
                return;

            if (ImGui.CollapsingHeader("Parts"))
            {
                foreach (var entry in Smithbox.AliasCacheHandler.AliasCache.PartList)
                {
                    if (FilterSelectionList(entry, Smithbox.AliasCacheHandler.AliasCache.Parts))
                    {
                        if (ImGui.Selectable(entry, entry == _selectedEntry))
                        {
                            _selectedEntry = entry;
                            _selectedEntryType = ModelSelectionType.Part;
                        }
                        DisplaySelectableAlias(entry, Smithbox.AliasCacheHandler.AliasCache.Parts);

                        if (entry == _selectedEntry)
                        {
                            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                            {
                                Screen.LoadPart(_selectedEntry);
                            }
                        }
                    }
                }
            }
        }

        private void DisplayMapPieceList()
        {
            if (Smithbox.BankHandler.MapPieceAliases.Aliases == null)
                return;

            var maps = ResourceMapLocator.GetFullMapList();

            if (ImGui.CollapsingHeader("Map Pieces"))
            {
                foreach (var map in maps)
                {
                    var displayedMapName = $"{map} - {AliasUtils.GetMapNameAlias(map)}";

                    if (ImGui.CollapsingHeader($"{displayedMapName}"))
                    {
                        var displayedName = $"{map}";
                        var modelName = map.Replace($"{map}_", "m");
                        displayedName = $"{modelName}";

                        if (Smithbox.ProjectType == ProjectType.DS1 || Smithbox.ProjectType == ProjectType.DS1R)
                        {
                            displayedName = displayedName.Replace($"A{map.Substring(1, 2)}", "");
                        }

                        foreach (var entry in Smithbox.AliasCacheHandler.AliasCache.MapPieceDict[map])
                        {
                            var mapPieceName = $"{entry.Replace(map, "m")}";

                            if (ImGui.Selectable(mapPieceName, entry == _selectedEntry))
                            {
                                _selectedEntry = entry;
                                _selectedEntryType = ModelSelectionType.MapPiece;
                            }
                            DisplaySelectableAlias(entry, Smithbox.AliasCacheHandler.AliasCache.MapPieces);

                            if (entry == _selectedEntry)
                            {
                                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                                {
                                    Screen.LoadMapPiece(_selectedEntry, map);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
