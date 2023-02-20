using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using UnityEngine;

public class BloodConfig : OptionInterface
{
    public Configurable<Color> colorTest;
    public UIelement[] ColorConfig;
    public BloodConfig(BloodMod mod) 
    { 
        colorTest = config.Bind<Color>("colorTest", new Color(0.5f,0f,0f));
        OnConfigChanged += ApplyColors;
    }

    public override void Initialize()
    {
        var opTab = new OpTab(this, "Options");
        Tabs = new[]
        {
            opTab
        };

        ColorConfig = new UIelement[]
        {
            new OpLabel(10f, 550f, Translate("Change all blood colors at once"), false),
            new OpColorPicker(colorTest, new Vector2(10f, 400f))
        };
        opTab.AddItems(ColorConfig);
    }

    public void ApplyColors()
    {
        Debug.Log("Apply Colors!");
        Dictionary<string, Color> newColors = new Dictionary<string, Color>(BloodMod.creatureColors);
        foreach(KeyValuePair<string, Color> item in newColors)
        {
            newColors[item.Key] = colorTest.Value;
        }
        BloodColor.GenerateBloodTextures(newColors);
        BloodMod.creatureColors = new Dictionary<string, Color>(newColors);
    }
}


