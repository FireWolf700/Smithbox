﻿using StudioCore.Core;
using System;
using System.Collections.Generic;

namespace StudioCore.Locators;
public static class MiscLocator
{
    public static string GetGameIDForDir()
    {
        var gametype = Smithbox.ProjectHandler.CurrentProject.Config.GameType;

        switch (gametype)
        {
            case ProjectType.Undefined:
                return "UNDEFINED";
            case ProjectType.DES:
                return "DES";
            case ProjectType.DS1:
                return "DS1";
            case ProjectType.DS1R:
                return "DS1R";
            case ProjectType.DS2:
                return "DS2";
            case ProjectType.DS2S:
                return "DS2S";
            case ProjectType.BB:
                return "BB";
            case ProjectType.DS3:
                return "DS3";
            case ProjectType.SDT:
                return "SDT";
            case ProjectType.ER:
                return "ER";
            case ProjectType.AC6:
                return "AC6";
            default:
                throw new Exception("Game type not set");
        }
    }

    // TAE
    public static List<string> GetAnimationBinders()
    {
        // Not supported
        if (Smithbox.ProjectType is ProjectType.DS2S
            or ProjectType.DS2
            or ProjectType.BB
            or ProjectType.DES)
        {
            return new List<string>();
        }

        // DS1R + DS3 + Sekiro + ER + AC6
        var paramDir = @"\chr";
        var paramExt = @".anibnd.dcx";

        List<string> ret = LocatorUtils.GetAssetFiles(paramDir, paramExt);

        return ret;
    }

    public static List<string> GetBehaviorBinders()
    {
        // Not supported
        if (Smithbox.ProjectType is ProjectType.DS1
            or ProjectType.DS1R
            or ProjectType.DS2S
            or ProjectType.DS2
            or ProjectType.BB
            or ProjectType.DES)
        {
            return new List<string>();
        }

        // DS3 + Sekiro + ER + AC6
        var paramDir = @"\chr";
        var paramExt = @".behbnd.dcx";

        List<string> ret = LocatorUtils.GetAssetFiles(paramDir, paramExt);

        return ret;

    }
    public static List<string> GetCharacterBinders()
    {
        // Not supported
        if (Smithbox.ProjectType is ProjectType.DS2S
            or ProjectType.DS2
            or ProjectType.BB
            or ProjectType.DES)
        {
            return new List<string>();
        }

        // DS1R + DS3 + Sekiro + ER + AC6
        var paramDir = @"\chr";
        var paramExt = @".chrbnd.dcx";

        List<string> ret = LocatorUtils.GetAssetFiles(paramDir, paramExt);

        return ret;
    }

    // Cutscene
    public static List<string> GetCutsceneBinders()
    {
        // Not supported
        if (Smithbox.ProjectType is ProjectType.DS2S
            or ProjectType.DS2
            or ProjectType.BB
            or ProjectType.DES)
        {
            return new List<string>();
        }

        // DS1R + DS3
        var paramDir = @"\remo";
        var paramExt = @".remobnd.dcx";

        // Sekiro + ER + AC6
        if (Smithbox.ProjectType is ProjectType.SDT or ProjectType.ER or ProjectType.AC6)
        {
            paramDir = @"\cutscene";
            paramExt = @".cutscenebnd.dcx";
        }

        List<string> ret = LocatorUtils.GetAssetFiles(paramDir, paramExt);

        return ret;
    }

    // Material
    public static List<string> GetMaterialBinders()
    {
        // Not supported
        if (Smithbox.ProjectType is ProjectType.DS2S
            or ProjectType.DS2
            or ProjectType.BB
            or ProjectType.DES)
        {
            return new List<string>();
        }

        // DS1R + DS3 + Sekiro
        var paramDir = @"\mtd";
        var paramExt = @".mtdbnd.dcx";

        if (Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6)
        {
            paramDir = @"\material";
            paramExt = @".matbinbnd.dcx";
            // Account for .devpatch in ER (e.g. matbinbnd.devpatch.dcx)
        }

        List<string> ret = LocatorUtils.GetAssetFiles(paramDir, paramExt);

        return ret;
    }

    // Particle 
    public static List<string> GetParticleBinders()
    {
        // Not supported
        if (Smithbox.ProjectType is ProjectType.DS2S
            or ProjectType.DS2
            or ProjectType.BB
            or ProjectType.DES)
        {
            return new List<string>();
        }

        // DS1R + DS3 + Sekiro + ER + AC6
        var paramDir = @"\sfx";
        var paramExt = @".ffxbnd.dcx";

        List<string> ret = LocatorUtils.GetAssetFiles(paramDir, paramExt);

        return ret;
    }

    // Scipt
    public static List<string> GetEventBinders()
    {
        // Not supported
        if (Smithbox.ProjectType is ProjectType.DS2S
            or ProjectType.DS2
            or ProjectType.BB
            or ProjectType.DES)
        {
            return new List<string>();
        }

        // DS1R + DS3 + Sekiro + ER + AC6
        var paramDir = @"\event";
        var paramExt = @".emevd.dcx";

        List<string> ret = LocatorUtils.GetAssetFiles(paramDir, paramExt);

        return ret;
    }

    // Talk
    public static List<string> GetTalkBinders()
    {
        // Not supported + Sekiro
        if (Smithbox.ProjectType is ProjectType.DS2S
            or ProjectType.DS2
            or ProjectType.BB
            or ProjectType.DES)
        {
            return new List<string>();
        }

        // DS1R + DS3 + Sekiro + ER + AC6
        var paramDir = @"\script\talk";
        var paramExt = @".talkesdbnd.dcx";

        List<string> ret = LocatorUtils.GetAssetFiles(paramDir, paramExt);

        return ret;
    }
}
