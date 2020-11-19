using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OptionalUI;
using UnityEngine;
using RWCustom;
using System.IO;
using Partiality.Modloader;
using Partiality;

public class BloodConfig : OptionInterface
{
    public static bool startup = false;
    public Vector2 texPos = new Vector2(350f, 420f);
    public OpLabel creatureLabel;
    public OpColorPicker colorPicker;
    public Dictionary<string, Color> configColors = new Dictionary<string, Color>();
    public Vector2 buttonOffset2 = new Vector2(50f, 0f);
    public Vector2 buttonOffset = new Vector2(0f, -51f);
    public Vector2 spriteOffset = new Vector2(20f, 20f);
    public string selected = "None Selected";
    public OpImage previewOld;
    public OpImage previewNew;
    public OpLabel oldLabel;
    public OpLabel newLabel;
    public OpSimpleButton resetButton;
    public OpLabel modName;
    public OpLabel modCredit;
    public OpLabel modVersion;
    public OpRect rect;
    public OpLabel guide;
    public OpLabel guide2;
    public OpCheckBox bloodPreview;
    public OpLabel bloodPreviewLabel;
    public OpImage selectedSprite;
    private Color rainbowColor;
    private Color lightRainbowColor;
    public Vector2 selectPos;
    public Vector2 selectOffset = new Vector2(-517f, -102f);
    public Color rainbowPulseColor;
    public OpSimpleButton realisticPreset;
    public OpSimpleButton vibrantPreset;
    public bool real;
    public bool vibrant;
    public OpCheckBox bloodlust;
    public OpLabel bloodlustLabel;
    public OpCheckBox preset;
    public OpCheckBox compat;
    public OpLabel compatLabel;

    public BloodConfig() : base(BloodMod.mod)
    {
    }

    public override bool Configuable()
    {
        return true;
    }

    public override void Initialize()
    {
        //Create a copy of the creature color dictionary that will be modified by the config screen
        this.configColors = new Dictionary<string, Color>(BloodMod.creatureColors);
        this.Tabs = new OpTab[1];
        int num = 0;
        int num2 = 0;
        int num3 = 0;
        int num4 = 0;
        this.Tabs[0] = new OpTab("Options");
        this.creatureLabel = new OpLabel(new Vector2(490f, 375f), new Vector2(0f, 0f), "None Selected", FLabelAlignment.Center, true);
        this.colorPicker = new OpColorPicker(new Vector2(250f, 250f), "bloodColor");
        this.previewOld = new OpImage(new Vector2(250f, 150f), "Futile_White");
        this.previewOld.sprite.scaleX = 10f;
        this.previewOld.sprite.scaleY = 4f;
        this.oldLabel = new OpLabel(new Vector2(250f, 220f), new Vector2(0f, 0f), "Default", FLabelAlignment.Left, true);
        this.previewNew = new OpImage(new Vector2(420f, 150f), "Futile_White");
        this.previewNew.sprite.scaleX = 10f;
        this.previewNew.sprite.scaleY = 4f;
        this.newLabel = new OpLabel(new Vector2(420f, 220f), new Vector2(0f, 0f), "Modified", FLabelAlignment.Left, true);
        this.resetButton = new OpSimpleButton(new Vector2(425f, 305f), new Vector2(150f, 45f), "reset", "Reset to Default");
        this.resetButton.description = "Reset this creature's blood color to the mod's default.";
        this.modName = new OpLabel(new Vector2(400f, 552f), new Vector2(0f, 0f), "BLOOD MOD", FLabelAlignment.Center, true);
        this.modCredit = new OpLabel(new Vector2(400f, 532f), new Vector2(0f, 0f), "Created by LeeMoriya", FLabelAlignment.Center, false);
        this.modVersion = new OpLabel(new Vector2(400f, 518f), new Vector2(0f, 0f), "Version: 1.01", FLabelAlignment.Center, false);
        this.rect = new OpRect(new Vector2(230f, 130f), new Vector2(370f, 360f), 0f);
        this.guide = new OpLabel(new Vector2(405f, 462f), new Vector2(0f, 0f), "Select creatures on the left and adjust their blood color using", FLabelAlignment.Center, false);
        this.guide2 = new OpLabel(new Vector2(405f, 442f), new Vector2(0f, 0f), "the color picker below. Finalize changes by clicking 'Apply'", FLabelAlignment.Center, false);
        this.bloodPreview = new OpCheckBox(new Vector2(30f, 2f), "_prev", false);
        this.bloodPreviewLabel = new OpLabel(new Vector2(66f, 5f), new Vector2(0f, 0f), "Preview all blood colors", FLabelAlignment.Left, false);
        this.selectedSprite = new OpImage(new Vector2(0f, 0f), "Futile_White");
        this.selectedSprite.sprite.scale = 17f;
        this.selectedSprite.sprite.alpha = 0.05f;
        this.selectedSprite.description = "?";
        this.selectedSprite.sprite.SetAnchor(new Vector2(0.5f, 0.5f));
        this.realisticPreset = new OpSimpleButton(new Vector2(265f, 85f), new Vector2(130f, 31f), "real", "Realistic Preset");
        this.vibrantPreset = new OpSimpleButton(new Vector2(435f, 85f), new Vector2(130f, 31f), "vibrant", "Vibrant Preset");
        this.realisticPreset.description = "Blood colors will be more realistic.";
        this.vibrantPreset.description = "Blood colors will more closely match the creature's color.";
        this.bloodlust = new OpCheckBox(new Vector2(265f, 45f), "gore", false);
        this.bloodlust.description = "Blood effects are increased dramatically";
        this.bloodlustLabel = new OpLabel(new Vector2(295f, 48f), new Vector2(), "Bloodlust Mode", FLabelAlignment.Left, false);
        this.compat = new OpCheckBox(new Vector2(265f, 12f), "wash", false);
        this.compat.description = "Rainfall from the mod Downpour will wash away blood.";
        this.compatLabel = new OpLabel(new Vector2(295f, 15f), new Vector2(), "Downpour Compatibility", FLabelAlignment.Left, false);
        this.preset = new OpCheckBox(new Vector2(), "preset", false);
        foreach (PartialityMod mod in PartialityManager.Instance.modManager.loadedMods)
        {
            if (mod.ModID == "Downpour")
            {
                this.Tabs[0].AddItems(this.compat, this.compatLabel);
            }
        }
        this.Tabs[0].AddItems(this.selectedSprite, this.colorPicker, this.creatureLabel, this.previewOld, this.previewNew, this.oldLabel, this.newLabel, this.resetButton, this.bloodPreviewLabel, this.modName, this.modCredit, this.rect, this.guide, this.guide2, this.modVersion, this.bloodPreview, this.realisticPreset, this.vibrantPreset, this.bloodlust, this.bloodlustLabel, this.preset);
        //Create individual buttons/sprites/color pickers for each creature
        foreach (KeyValuePair<string, Color> creature in this.configColors)
        {
            if (creature.Key != "Overseer")
            {
                if (num < 11)
                {
                    OpColorPicker picker = new OpColorPicker(new Vector2(250f, 270f), creature.Key, OpColorPicker.ColorToHex(BloodMod.defaultColors[creature.Key]));
                    OpSimpleButton button = new OpSimpleButton(new Vector2(21f, 548f) + (buttonOffset * num), new Vector2(40f, 40f), creature.Key + "but", "");
                    OpImage sprite = new OpImage(new Vector2(21f, 548f) + (buttonOffset * num) + spriteOffset, BloodColor.GetCreatureSprite(creature.Key));
                    sprite.color = BloodColor.GetCreatureSpriteColor(creature.Key);
                    sprite.sprite.SetAnchor(new Vector2(0.5f, 0.5f));
                    sprite.description = creature.Key;
                    button.description = creature.Key;
                    this.Tabs[0].AddItems(button, sprite, picker);
                    num++;
                }
                else if (num2 < 11)
                {
                    OpColorPicker picker = new OpColorPicker(new Vector2(250f, 270f), creature.Key, OpColorPicker.ColorToHex(BloodMod.defaultColors[creature.Key]));
                    OpSimpleButton button = new OpSimpleButton(new Vector2(21f, 548f) + (buttonOffset * num2) + buttonOffset2, new Vector2(40f, 40f), creature.Key + "but", "");
                    OpImage sprite = new OpImage(new Vector2(21f, 548f) + (buttonOffset * num2) + buttonOffset2 + spriteOffset, BloodColor.GetCreatureSprite(creature.Key));
                    sprite.color = BloodColor.GetCreatureSpriteColor(creature.Key);
                    sprite.sprite.SetAnchor(new Vector2(0.5f, 0.5f));
                    sprite.description = creature.Key;
                    button.description = creature.Key;
                    this.Tabs[0].AddItems(button, sprite, picker);
                    num2++;
                }
                else if (num3 < 11)
                {
                    OpColorPicker picker = new OpColorPicker(new Vector2(250f, 270f), creature.Key, OpColorPicker.ColorToHex(BloodMod.defaultColors[creature.Key]));
                    OpSimpleButton button = new OpSimpleButton(new Vector2(21f, 548f) + (buttonOffset * num3) + (buttonOffset2 * 2), new Vector2(40f, 40f), creature.Key + "but", "");
                    OpImage sprite = new OpImage(new Vector2(21f, 548f) + (buttonOffset * num3) + (buttonOffset2 * 2) + spriteOffset, BloodColor.GetCreatureSprite(creature.Key));
                    sprite.color = BloodColor.GetCreatureSpriteColor(creature.Key);
                    sprite.sprite.SetAnchor(new Vector2(0.5f, 0.5f));
                    sprite.description = creature.Key;
                    button.description = creature.Key;
                    this.Tabs[0].AddItems(button, sprite, picker);
                    num3++;
                }
                else
                {
                    OpColorPicker picker = new OpColorPicker(new Vector2(250f, 270f), creature.Key, OpColorPicker.ColorToHex(BloodMod.defaultColors[creature.Key]));
                    OpSimpleButton button = new OpSimpleButton(new Vector2(21f, 548f) + (buttonOffset * num4) + (buttonOffset2 * 3), new Vector2(40f, 40f), creature.Key + "but", "");
                    OpImage sprite = new OpImage(new Vector2(21f, 548f) + (buttonOffset * num4) + (buttonOffset2 * 3) + spriteOffset, BloodColor.GetCreatureSprite(creature.Key));
                    sprite.color = BloodColor.GetCreatureSpriteColor(creature.Key);
                    sprite.sprite.SetAnchor(new Vector2(0.5f, 0.5f));
                    sprite.description = creature.Key;
                    button.description = creature.Key;
                    this.Tabs[0].AddItems(button, sprite, picker);
                    num4++;
                }
            }
            this.selected = "Slugcat";
        }
    }

    public override void Signal(UItrigger trigger, string signal)
    {
        base.Signal(trigger, signal);
        //Clicking a creature button
        if (signal != "reset" && signal != "real" && signal != "vibrant")
        {
            this.selected = signal.Remove(signal.Length - 3, 3);
            if (this.configColors[this.selected] != null)
            {
                this.colorPicker.valueColor = this.configColors[this.selected];
                if (this.vibrant)
                {

                    this.previewOld.sprite.color = BloodMod.vibrantColors[this.selected];
                }
                else
                {
                    this.previewOld.sprite.color = BloodMod.defaultColors[this.selected];
                }
            }
        }
        //Clicking reset button
        if (this.selected != "" && signal == "reset")
        {
            for (int i = 0; i < this.Tabs[0].items.Count; i++)
            {
                if (this.Tabs[0].items[i] is OpColorPicker)
                {
                    if ((this.Tabs[0].items[i] as OpColorPicker).key == this.selected)
                    {
                        (this.Tabs[0].items[i] as OpColorPicker).value = OpColorPicker.ColorToHex(BloodMod.defaultColors[this.selected]);
                    }
                }
            }
            this.configColors[this.selected] = BloodMod.defaultColors[this.selected];
            this.previewOld.sprite.color = BloodMod.defaultColors[this.selected];
        }
        if (signal == "real")
        {
            this.vibrant = false;
            this.preset.value = "false";
            this.configColors = new Dictionary<string, Color>(BloodMod.defaultColors);
            this.previewOld.sprite.color = BloodMod.defaultColors[this.selected];
            foreach (KeyValuePair<string, Color> key in BloodMod.defaultColors)
            {
                for (int i = 0; i < this.Tabs[0].items.Count; i++)
                {
                    if (this.Tabs[0].items[i] is OpColorPicker)
                    {
                        if ((this.Tabs[0].items[i] as OpColorPicker).key == key.Key)
                        {
                            (this.Tabs[0].items[i] as OpColorPicker).value = OpColorPicker.ColorToHex(BloodMod.defaultColors[key.Key]);
                        }
                    }
                }
            }
        }
        if (signal == "vibrant")
        {
            this.vibrant = true;
            this.preset.value = "true";
            this.configColors = new Dictionary<string, Color>(BloodMod.vibrantColors);
            this.previewOld.sprite.color = BloodMod.vibrantColors[this.selected];
            foreach(KeyValuePair<string,Color> key in BloodMod.vibrantColors)
            {
                for (int i = 0; i < this.Tabs[0].items.Count; i++)
                {
                    if (this.Tabs[0].items[i] is OpColorPicker)
                    {
                        if ((this.Tabs[0].items[i] as OpColorPicker).key == key.Key)
                        {
                            (this.Tabs[0].items[i] as OpColorPicker).value = OpColorPicker.ColorToHex(BloodMod.vibrantColors[key.Key]);
                        }
                    }
                }
            }
        }
    }
    public override void Update(float dt)
    {
        base.Update(dt);
        this.preset.Hide();
        rainbowColor = Custom.HSL2RGB(Mathf.Lerp(0f, 1f, Mathf.PingPong(Time.time * 0.7f, 1)), 0.85f, 0.65f);
        rainbowPulseColor = Color.Lerp(rainbowColor, new Color(0f, 0f, 0f, 0f), Mathf.Lerp(0.3f, 0.8f, Mathf.PingPong(Time.time * 0.7f, 1)));
        lightRainbowColor = Color.Lerp(Color.white, rainbowColor, 0.2f);
        this.creatureLabel.text = this.selected;
        if (this.bloodlust.value == "true")
        {
            this.modName.color = new Color(0.55f, 0f, 0f);
        }
        else
        {
            this.modName.color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
        }
        for (int i = 0; i < this.Tabs[0].items.Count; i++)
        {
            //Adjust sprite colors based on mouse distance
            if (this.Tabs[0].items[i] is OpImage)
            {
                if (this.selected == (this.Tabs[0].items[i] as OpImage).description)
                {
                    (this.Tabs[0].items[i] as OpImage).sprite.color = rainbowColor;
                }
                else if ((this.Tabs[0].items[i] as OpImage).description != "?")
                {
                    if (Vector2.Distance((this.Tabs[0].items[i] as OpImage).pos + spriteOffset, Input.mousePosition) < 150f)
                    {
                        if (this.bloodPreview.valueBool == true && (this.Tabs[0].items[i] as OpImage).description != "")
                        {
                            (this.Tabs[0].items[i] as OpImage).sprite.color = Color.Lerp(this.configColors[(this.Tabs[0].items[i] as OpImage).description], (this.Tabs[0].items[i] as OpImage).color, Mathf.InverseLerp(-10f, 150f, Vector2.Distance((this.Tabs[0].items[i] as OpImage).pos + spriteOffset, Input.mousePosition)));
                        }
                        else
                        {
                            (this.Tabs[0].items[i] as OpImage).sprite.color = Color.Lerp(lightRainbowColor, (this.Tabs[0].items[i] as OpImage).color, Mathf.InverseLerp(-120f, 150f, Vector2.Distance((this.Tabs[0].items[i] as OpImage).pos + spriteOffset, Input.mousePosition)));
                        }
                    }
                    else
                    {
                        (this.Tabs[0].items[i] as OpImage).sprite.color = (this.Tabs[0].items[i] as OpImage).color;
                    }
                }
                else
                {
                    //Selected fade sprite
                    this.selectedSprite.pos = this.selectPos + selectOffset;
                    this.selectedSprite.sprite.color = rainbowPulseColor;
                    this.selectedSprite.sprite.alpha = 0.4f;
                    var rw = GameObject.FindObjectOfType<RainWorld>();
                    if (rw != null)
                    {
                        this.selectedSprite.sprite.shader = rw.Shaders["FlatLight"];
                    }
                }
                if (this.vibrant)
                {
                    this.previewOld.sprite.color = BloodMod.vibrantColors[this.selected];
                }
                else
                {
                    this.previewOld.sprite.color = BloodMod.defaultColors[this.selected];
                }
            }
            //Change button colors when selected
            if (this.Tabs[0].items[i] is OpSimpleButton)
            {
                if (this.bloodPreview.valueBool == true && (this.Tabs[0].items[i] as OpSimpleButton).description != this.resetButton.description && (this.Tabs[0].items[i] as OpSimpleButton).description != this.realisticPreset.description && (this.Tabs[0].items[i] as OpSimpleButton).description != this.vibrantPreset.description)
                {
                    if ((this.Tabs[0].items[i] as OpSimpleButton).description == this.selected)
                    {
                        (this.Tabs[0].items[i] as OpSimpleButton).colorEdge = rainbowColor;
                    }
                    else if ((this.Tabs[0].items[i] as OpSimpleButton).signal != "real" && (this.Tabs[0].items[i] as OpSimpleButton).signal != "vibrant" && (this.Tabs[0].items[i] as OpSimpleButton).signal != "reset")
                    {
                        (this.Tabs[0].items[i] as OpSimpleButton).colorEdge = this.configColors[(this.Tabs[0].items[i] as OpSimpleButton).description];
                        if (Vector2.Distance((this.Tabs[0].items[i] as OpSimpleButton).pos + spriteOffset, Input.mousePosition) < 150f)
                        {
                            (this.Tabs[0].items[i] as OpSimpleButton).colorFill = Color.Lerp(this.configColors[(this.Tabs[0].items[i] as OpSimpleButton).description], new Color(0.0f, 0.0f, 0.0f), Mathf.InverseLerp(-50f, 150f, Vector2.Distance((this.Tabs[0].items[i] as OpSimpleButton).pos + spriteOffset, Input.mousePosition)));
                        }
                        else
                        {
                            (this.Tabs[0].items[i] as OpSimpleButton).colorFill = new Color(0.0f, 0.0f, 0.0f);
                        }
                    }
                }
                else
                {
                    if ((this.Tabs[0].items[i] as OpSimpleButton).description == this.selected)
                    {
                        this.selectPos = (this.Tabs[0].items[i] as OpSimpleButton).pos;
                        (this.Tabs[0].items[i] as OpSimpleButton).colorEdge = rainbowColor;
                        (this.Tabs[0].items[i] as OpSimpleButton).colorFill = new Color(0f, 0f, 0f, 0f);
                    }
                    else if ((this.Tabs[0].items[i] as OpSimpleButton).signal != "real" && (this.Tabs[0].items[i] as OpSimpleButton).signal != "vibrant" && (this.Tabs[0].items[i] as OpSimpleButton).signal != "reset")
                    {
                        if (Vector2.Distance((this.Tabs[0].items[i] as OpSimpleButton).pos + spriteOffset, Input.mousePosition) < 150f)
                        {
                            (this.Tabs[0].items[i] as OpSimpleButton).colorEdge = Color.Lerp(lightRainbowColor, new Color(0.3f, 0.3f, 0.3f), Mathf.InverseLerp(-50f, 150f, Vector2.Distance((this.Tabs[0].items[i] as OpSimpleButton).pos + spriteOffset, Input.mousePosition)));
                            (this.Tabs[0].items[i] as OpSimpleButton).colorFill = Color.Lerp(lightRainbowColor, new Color(0f, 0f, 0f, 0f), Mathf.InverseLerp(-70f, 150f, Vector2.Distance((this.Tabs[0].items[i] as OpSimpleButton).pos + spriteOffset, Input.mousePosition)));
                        }
                        else
                        {
                            (this.Tabs[0].items[i] as OpSimpleButton).colorEdge = new Color(0.3f, 0.3f, 0.3f);
                            (this.Tabs[0].items[i] as OpSimpleButton).colorFill = new Color(0f, 0f, 0f, 0f);
                        }
                    }
                }
            }
            //Hide and show per-creature color pickers and adjust preview sprite
            if (this.Tabs[0].items[i] is OpColorPicker)
            {
                if ((this.Tabs[0].items[i] as OpColorPicker).key == this.selected)
                {
                    (this.Tabs[0].items[i] as OpColorPicker).Show();
                    if ((this.Tabs[0].items[i] as OpColorPicker).valueColor != null)
                    {
                        this.previewNew.sprite.color = (this.Tabs[0].items[i] as OpColorPicker).valueColor;
                        this.configColors[this.selected] = (this.Tabs[0].items[i] as OpColorPicker).valueColor;
                    }
                    else
                    {
                        this.previewNew.sprite.color = new Color(1f, 1f, 1f);
                    }
                }
                else
                {
                    (this.Tabs[0].items[i] as OpColorPicker).Hide();
                }
            }
        }
    }

    public override void ConfigOnChange()
    {
        //Load colors on startup
        if (!startup && config != null)
        {
            Dictionary<string, Color> temp = new Dictionary<string, Color>();
            foreach (KeyValuePair<string, Color> crit in this.configColors)
            {
                if (config.TryGetValue(crit.Key, out string val))
                {
                    temp.Add(crit.Key, OpColorPicker.HexToColor(val));
                }
            }
            this.configColors = temp;
            startup = true;
        }
        if(config["gore"] == "true")
        {
            BloodMod.goreMultiplier = 1f;
        }
        else
        {
            BloodMod.goreMultiplier = 0f;
        }
        if (config.ContainsKey("wash") && config["wash"] == "true")
        {
            BloodMod.compat = true;
        }
        else
        {
            BloodMod.compat = false;
        }
        if (config["preset"] == "true")
        {
            this.vibrant = true;
        }
        else
        {
            this.vibrant = false;
        }
        //Generate new blood textures using modified colors
        BloodColor.GenerateBloodTextures(this.configColors);
        BloodMod.creatureColors = new Dictionary<string, Color>(this.configColors);
    }
}
