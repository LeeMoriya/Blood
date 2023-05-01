using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using UnityEngine;
using Menu;
using RWCustom;
using System.Text.RegularExpressions;
using Menu.Remix;
using System.IO;

public class BloodConfig : OptionInterface
{
    public Configurable<int> splatterRate;
    public BloodConfig(BloodMod mod)
    {
        splatterRate = config.Bind<int>("splatterRate", 60);
    }

    public override void Initialize()
    {
        var opTab = new OpTab(this, "Options");
        Tabs = new[]
        {
            opTab
        };

        byte[] bytes = File.ReadAllBytes(AssetManager.ResolveFilePath("atlases\\bloodbanner.png"));
        Texture2D texture = new Texture2D(0, 0);
        texture.filterMode = FilterMode.Point;
        texture.LoadImage(bytes);
        OpImage banner = new OpImage(new Vector2(0f, 420f), texture);

        OpLabel version = new OpLabel(300f, 425f, $"Version: {BloodMod.version}     -     By LeeMoriya", false);
        version.label.alignment = FLabelAlignment.Center;

        opTab.AddItems(version, banner);

        OpLabel splatterTitle = new OpLabel(300f, 320f, $"SPLATTER RATE", true);
        splatterTitle.label.alignment = FLabelAlignment.Center;

        OpLabel splatterDesc = new OpLabel(300f, 288f, $"Controls how often blood splatters appear when blood particles collide with surfaces.\nMay affect performance.", false);
        splatterDesc.label.alignment = FLabelAlignment.Center;

        OpSlider splatterSlider = new OpSlider(splatterRate, new Vector2(150f, 240f), 3f, false);
        opTab.AddItems(splatterTitle, splatterDesc, splatterSlider);

        OpLabel colorTitle = new OpLabel(300f, 160f, $"BLOOD COLORS", true);
        colorTitle.label.alignment = FLabelAlignment.Center;

        OpLabel colorDesc = new OpLabel(300f, 135f, $"Adjust blood colors for specific creatures or apply a preset", false);
        colorDesc.label.alignment = FLabelAlignment.Center;

        OpSimpleButton custom = new OpSimpleButton(new Vector2(300f - 70f, 50f), new Vector2(140f, 60f), "OPEN COLOR\nCUSTOMISER");
        custom.OnClick += Custom_OnClick;
        opTab.AddItems(colorTitle, colorDesc, custom);
    }

    private void Custom_OnClick(UIfocusable trigger)
    {
        var rw = GameObject.FindObjectOfType<RainWorld>();
        Dialog dialog = new BloodDialog(rw.processManager, this);
        rw.processManager.ShowDialog(dialog);
    }

    public class BloodDialog : Dialog, CheckBox.IOwnCheckBox
    {
        public float uAlpha;
        public float lastAlpha;
        public float currentAlpha;
        public float targetAlpha;
        public bool opening, closing;

        public float leftAnchor;

        public BloodConfig config;
        public List<CreatureButton> critButtons;
        public Dictionary<string, Color> creatureColors;
        public RoundedRect rect;
        public MenuLabel creatureName;
        public string currentCreature = "Slugcat";
        public CheckBox showColorToggle;
        public bool showColors;

        public MenuTabWrapper tabWrapper;
        public OpColorPicker picker;

        public FSprite oldColor;
        public FSprite newColor;

        public Color currentColor = new Color(0.65f, 0f, 0f);
        public Color lastColor = new Color(0.65f, 0f, 0f);

        public SimpleButton close, discard, realisticPreset, vibrantPreset;

        public BloodDialog(ProcessManager manager, BloodConfig config) : base(manager)
        {
            this.config = config;
            leftAnchor = Custom.GetScreenOffsets()[0];
            pages[0].pos.y += 2000f;
            pages[0].pos.x = leftAnchor + 0.01f;

            rect = new RoundedRect(this, pages[0], new Vector2((manager.rainWorld.options.ScreenSize.x / 2) - (60f * 13 / 2) - 110f, 50f), new Vector2(990f, manager.rainWorld.screenSize.y - 80f), true);
            pages[0].subObjects.Add(rect);

            //Colors
            creatureColors = new Dictionary<string, Color>();
            foreach (string name in ExtEnumBase.GetNames(typeof(CreatureTemplate.Type)))
            {
                if (!BloodMod.creatureBlacklist.Contains(name))
                {
                    try
                    {
                        Color color = new Color(0.6f, 0f, 0f);
                        BloodMod.creatureColors.TryGetValue(name, out color);
                        creatureColors.Add(name, color);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
            MenuLabel title = new MenuLabel(this, pages[0], "Select a creature to customise its blood color", new Vector2(manager.rainWorld.options.ScreenSize.x / 2, 710f), new Vector2(), false);
            title.label.color = new Color(0.7f, 0.7f, 0.7f);
            pages[0].subObjects.Add(title);

            critButtons = new List<CreatureButton>();
            int row = 0;
            int col = 0;
            float centerPos = (manager.rainWorld.options.ScreenSize.x / 2) - (60f * 15 / 2);
            foreach (string name in ExtEnumBase.GetNames(typeof(CreatureTemplate.Type)))
            {
                if (!BloodMod.creatureBlacklist.Contains(name))
                {
                    CreatureButton crit = new CreatureButton(this, pages[0], ReturnCreatureSprite(name), $"crit-{name}", new Vector2(centerPos + (60f * col), 630f - (60f * row)));
                    crit.roundedRect.size = new Vector2(50f, 50f);
                    crit.size = crit.roundedRect.size;
                    crit.name = name;

                    crit.symbolSprite.color = ReturnSpriteColor(name);
                    crit.borderColor = ReturnSpriteColor(name);
                    crit.fillColor = creatureColors[name];

                    col++;
                    if (col == 15)
                    {
                        row++;
                        col = 0;
                    }
                    critButtons.Add(crit);
                    pages[0].subObjects.Add(crit);
                }
            }

            creatureName = new MenuLabel(this, pages[0], "", new Vector2(manager.rainWorld.options.ScreenSize.x / 2, critButtons.Last().pos.y - 25f), new Vector2(), true);
            creatureName.label.color = new Color(0.7f, 0.7f, 0.7f);
            pages[0].subObjects.Add(creatureName);

            showColorToggle = new CheckBox(this, pages[0], this, new Vector2(rect.pos.x + 55f, title.pos.y - 15f), 60f, "Display all blood colors", "SHOWALL", true);
            pages[0].subObjects.Add(showColorToggle);

            close = new SimpleButton(this, pages[0], "SAVE", "CLOSE", new Vector2(manager.rainWorld.options.ScreenSize.x / 2 + 20f, 80f), new Vector2(100f, 30f));
            close.rectColor = new HSLColor(0.4f, 0.8f, 0.45f);
            close.labelColor = new HSLColor(0.4f, 0.8f, 0.45f);
            pages[0].subObjects.Add(close);

            discard = new SimpleButton(this, pages[0], "DISCARD", "DISCARD", new Vector2(manager.rainWorld.options.ScreenSize.x / 2 - 120f, 80f), new Vector2(100f, 30f));
            discard.rectColor = new HSLColor(0f, 0.8f, 0.45f);
            discard.labelColor = new HSLColor(0f, 0.8f, 0.45f);
            pages[0].subObjects.Add(discard);

            tabWrapper = new MenuTabWrapper(this, pages[0]);
            pages[0].subObjects.Add(tabWrapper);
            picker = new OpColorPicker(new Configurable<Color>(new Color(1f, 1f, 1f)), pages[0].pos + new Vector2(240f, 120f));
            picker.OnValueUpdate += Picker_OnValueUpdate;
            picker.OnValueChanged += Picker_OnValueChanged;
            UIelementWrapper wrapper = new UIelementWrapper(tabWrapper, picker);

            creatureColors.TryGetValue(currentCreature, out currentColor);
            lastColor = currentColor;
            picker.valueColor = currentColor;

            MenuLabel pickerLabel = new MenuLabel(this, pages[0], "Adjust blood color", rect.pos + new Vector2(405f, 265f), new Vector2(), false);
            pickerLabel.label.color = MenuRGB(MenuColors.MediumGrey);
            pages[0].subObjects.Add(pickerLabel);

            newColor = new FSprite("pixel", true);
            newColor.scaleX = 150f;
            newColor.scaleY = 45f;
            container.AddChild(newColor);

            MenuLabel newLabel = new MenuLabel(this, pages[0], "NEW", rect.pos + new Vector2(595f, 245f), new Vector2(), false);
            newLabel.label.color = MenuRGB(MenuColors.MediumGrey);
            pages[0].subObjects.Add(newLabel);

            oldColor = new FSprite("pixel", true);
            oldColor.scaleX = 150f;
            oldColor.scaleY = 45f;
            container.AddChild(oldColor);

            MenuLabel oldLabel = new MenuLabel(this, pages[0], "OLD", rect.pos + new Vector2(595f, 165f), new Vector2(), false);
            oldLabel.label.color = MenuRGB(MenuColors.MediumGrey);
            pages[0].subObjects.Add(oldLabel);

            MenuLabel presetLabel = new MenuLabel(this, pages[0], "PRESETS", rect.pos + new Vector2(170f, 230f), new Vector2(), true);
            presetLabel.label.color = MenuRGB(MenuColors.MediumGrey);
            pages[0].subObjects.Add(presetLabel);

            realisticPreset = new SimpleButton(this, pages[0], "REALISTIC", "REALISTIC", presetLabel.pos + new Vector2(-50f, -55f), new Vector2(100f, 30f));
            pages[0].subObjects.Add(realisticPreset);

            vibrantPreset = new SimpleButton(this, pages[0], "VIBRANT", "VIBRANT", realisticPreset.pos + new Vector2(0f, -50f), new Vector2(100f, 30f));
            pages[0].subObjects.Add(vibrantPreset);

            opening = true;
            targetAlpha = 1;
        }

        private void Picker_OnValueChanged(UIconfig config, string value, string oldValue)
        {
            creatureColors[currentCreature] = picker.valueColor;
            for (int i = 0; i < critButtons.Count; i++)
            {
                critButtons[i].updateColor = true;
            }
        }

        private void Picker_OnValueUpdate(UIconfig config, string value, string oldValue)
        {
            if (picker != null)
            {
                currentColor = picker.valueColor;
            }
        }

        public override void Singal(MenuObject sender, string message)
        {
            base.Singal(sender, message);
            if (message == "CLOSE")
            {
                BloodData.Save(creatureColors);
                BloodMod.creatureColors = creatureColors;
                BloodColor.GenerateBloodTextures(creatureColors);
                PlaySound(SoundID.HUD_Exit_Game);
                targetAlpha = 0f;
                closing = true;
            }
            if (message == "DISCARD")
            {
                PlaySound(SoundID.HUD_Exit_Game);
                targetAlpha = 0f;
                closing = true;
            }
            if (message == "REALISTIC")
            {
                for (int i = 0; i < critButtons.Count; i++)
                {
                    Color col;
                    BloodMod.defaultColors.TryGetValue(critButtons[i].name, out col);
                    if (col != null)
                    {
                        creatureColors[critButtons[i].name] = col;
                    }
                    critButtons[i].updateColor = true;
                }
                PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
            }
            if (message == "VIBRANT")
            {
                for (int i = 0; i < critButtons.Count; i++)
                {
                    Color col;
                    BloodMod.vibrantColors.TryGetValue(critButtons[i].name, out col);
                    if (col != null)
                    {
                        creatureColors[critButtons[i].name] = col;
                    }
                    critButtons[i].updateColor = true;
                }
                PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
            }
            if (message.StartsWith("crit-"))
            {
                PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
                string crit = Regex.Split(message, "-")[1];
                currentCreature = crit;
                creatureColors.TryGetValue(crit, out currentColor);
                picker.valueColor = currentColor;
                lastColor = currentColor;
            }
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            if (opening || closing)
            {
                uAlpha = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastAlpha, currentAlpha, timeStacker)), 1.5f);
                darkSprite.alpha = uAlpha * 0.25f;
            }
            pages[0].pos.y = Mathf.Lerp(manager.rainWorld.options.ScreenSize.y + 100f, 0.01f, uAlpha < 0.999f ? uAlpha : 1f);
            picker.pos = pages[0].pos + new Vector2(510f, 150f);

            rect.sprites[rect.FillSideSprite(0)].alpha = 1f;
            rect.sprites[rect.FillSideSprite(1)].alpha = 1f;
            rect.sprites[rect.FillSideSprite(2)].alpha = 1f;
            rect.sprites[rect.FillSideSprite(3)].alpha = 1f;
            rect.sprites[rect.FillCornerSprite(0)].alpha = 1f;
            rect.sprites[rect.FillCornerSprite(1)].alpha = 1f;
            rect.sprites[rect.FillCornerSprite(2)].alpha = 1f;
            rect.sprites[rect.FillCornerSprite(3)].alpha = 1f;
            rect.sprites[rect.MainFillSprite].alpha = 1f;

            newColor.x = Mathf.Lerp(pages[0].lastPos.x + 780f - leftAnchor, pages[0].pos.x + 780f - leftAnchor, timeStacker);
            newColor.y = Mathf.Lerp(pages[0].lastPos.y + 260f, pages[0].pos.y + 260f, timeStacker);
            newColor.color = currentColor;

            oldColor.x = Mathf.Lerp(pages[0].lastPos.x + 780f - leftAnchor, pages[0].pos.x + 780f - leftAnchor, timeStacker);
            oldColor.y = Mathf.Lerp(pages[0].lastPos.y + 180f, pages[0].pos.y + 180f, timeStacker);
            oldColor.color = lastColor;
        }

        public override void Update()
        {
            base.Update();

            lastAlpha = currentAlpha;
            currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, 0.2f);

            if (opening && pages[0].pos.y <= 0.01f) { opening = false; }

            if (closing && Math.Abs(currentAlpha - targetAlpha) < 0.09f)
            {
                manager.StopSideProcess(this);
                closing = false;
            }

            if (critButtons != null)
            {
                string text = currentCreature;
                for (int i = 0; i < critButtons.Count; i++)
                {
                    if (critButtons[i].Selected)
                    {
                        text = critButtons[i].name;
                    }
                }
                creatureName.text = text;
            }
        }

        public static string ReturnCreatureSprite(string critName)
        {
            if (critName == "TempleGuard")
            {
                return "Sandbox_Randomize";
            }
            try
            {
                CreatureTemplate.Type type = new CreatureTemplate.Type(critName, false);
                CreatureSymbol.IconSymbolData data = new IconSymbol.IconSymbolData(type, AbstractPhysicalObject.AbstractObjectType.Creature, 0);
                return CreatureSymbol.SpriteNameOfCreature(data);
            }
            catch
            {
                Debug.LogError($"No sprite found for {critName}");
            };
            return "Futile_White";
        }

        public static Color ReturnSpriteColor(string critName)
        {
            try
            {
                CreatureTemplate.Type type = new CreatureTemplate.Type(critName, false);
                CreatureSymbol.IconSymbolData data = new IconSymbol.IconSymbolData(type, AbstractPhysicalObject.AbstractObjectType.Creature, 0);
                return CreatureSymbol.ColorOfCreature(data);
            }
            catch
            {
                Debug.LogError($"No color found for {critName}");
            };
            return new Color(1f, 1f, 1f);
        }

        public bool GetChecked(CheckBox box)
        {
            if (box.IDString == "SHOWALL")
            {
                return showColors;
            }
            return false;
        }

        public void SetChecked(CheckBox box, bool c)
        {
            if (box.IDString == "SHOWALL")
            {
                showColors = c;
            }
        }

        public class CreatureButton : SymbolButton
        {
            public string name;
            public bool showColor;
            public Color fillColor;
            public Color borderColor;
            public float colorCounter = 0f;
            public bool updateColor = false;
            public CreatureButton(Menu.Menu menu, MenuObject owner, string symbolName, string singalText, Vector2 pos) : base(menu, owner, symbolName, singalText, pos)
            {

            }

            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);
                if (borderColor != null)
                {
                    float alpha;
                    Color color = Color.Lerp(borderColor, Color.white, Mathf.Lerp(roundedRect.lasFillAplha, roundedRect.fillAlpha, timeStacker));
                    if ((menu as BloodDialog).currentCreature == name)
                    {
                        color = Color.Lerp(borderColor, Color.black, Mathf.Sin(colorCounter * 0.4f));
                        alpha = 1f;
                    }
                    else
                    {
                        alpha = 0.25f;
                    }

                    symbolSprite.color = color;
                    roundedRect.sprites[roundedRect.SideSprite(0)].color = color;
                    roundedRect.sprites[roundedRect.SideSprite(1)].color = color;
                    roundedRect.sprites[roundedRect.SideSprite(2)].color = color;
                    roundedRect.sprites[roundedRect.SideSprite(3)].color = color;
                    roundedRect.sprites[roundedRect.CornerSprite(0)].color = color;
                    roundedRect.sprites[roundedRect.CornerSprite(1)].color = color;
                    roundedRect.sprites[roundedRect.CornerSprite(2)].color = color;
                    roundedRect.sprites[roundedRect.CornerSprite(3)].color = color;

                    symbolSprite.alpha = 1f;
                    roundedRect.sprites[roundedRect.SideSprite(0)].alpha = alpha;
                    roundedRect.sprites[roundedRect.SideSprite(1)].alpha = alpha;
                    roundedRect.sprites[roundedRect.SideSprite(2)].alpha = alpha;
                    roundedRect.sprites[roundedRect.SideSprite(3)].alpha = alpha;
                    roundedRect.sprites[roundedRect.CornerSprite(0)].alpha = alpha;
                    roundedRect.sprites[roundedRect.CornerSprite(1)].alpha = alpha;
                    roundedRect.sprites[roundedRect.CornerSprite(2)].alpha = alpha;
                    roundedRect.sprites[roundedRect.CornerSprite(3)].alpha = alpha;
                }
                if (fillColor != null && (menu as BloodDialog).showColors)
                {
                    roundedRect.sprites[roundedRect.FillSideSprite(0)].color = fillColor;
                    roundedRect.sprites[roundedRect.FillSideSprite(1)].color = fillColor;
                    roundedRect.sprites[roundedRect.FillSideSprite(2)].color = fillColor;
                    roundedRect.sprites[roundedRect.FillSideSprite(3)].color = fillColor;
                    roundedRect.sprites[roundedRect.FillCornerSprite(0)].color = fillColor;
                    roundedRect.sprites[roundedRect.FillCornerSprite(1)].color = fillColor;
                    roundedRect.sprites[roundedRect.FillCornerSprite(2)].color = fillColor;
                    roundedRect.sprites[roundedRect.FillCornerSprite(3)].color = fillColor;
                    roundedRect.sprites[roundedRect.MainFillSprite].color = fillColor;
                }
                else
                {
                    Color none = Color.Lerp(borderColor, Color.black, 0.7f);
                    roundedRect.sprites[roundedRect.FillSideSprite(0)].color = none;
                    roundedRect.sprites[roundedRect.FillSideSprite(1)].color = none;
                    roundedRect.sprites[roundedRect.FillSideSprite(2)].color = none;
                    roundedRect.sprites[roundedRect.FillSideSprite(3)].color = none;
                    roundedRect.sprites[roundedRect.FillCornerSprite(0)].color = none;
                    roundedRect.sprites[roundedRect.FillCornerSprite(1)].color = none;
                    roundedRect.sprites[roundedRect.FillCornerSprite(2)].color = none;
                    roundedRect.sprites[roundedRect.FillCornerSprite(3)].color = none;
                    roundedRect.sprites[roundedRect.MainFillSprite].color = none;
                }
            }

            public override void Update()
            {
                base.Update();
                colorCounter += 0.25f;
                if (updateColor)
                {
                    updateColor = false;
                    fillColor = (menu as BloodDialog).creatureColors[name];
                }
            }
        }
    }
}


