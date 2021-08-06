using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class BloodColor
{
    public BloodColor()
    {

    }

    public static void GenerateBloodTextures(Dictionary<string, Color> creatureColors)
    {
        //Create a new instance of the dictionary
        Dictionary<string, Color> colorDictionary = creatureColors;
        Debug.Log("Generating blood textures...");
        //Get colors from texture
        Color[] defaultColors = BloodMod.bloodTex.GetPixels();
        //Modify the colors to match each one in the dictionary
        foreach (KeyValuePair<string, Color> creatureColor in colorDictionary)
        {
            //Debug.Log("Attempting to create blood texture for " + creatureColor.Key + "...");
            try
            {
                Color[] newColors = defaultColors;
                for (int i = 0; i < defaultColors.Length; i++)
                {
                    if (newColors[i].a > 0f)
                    {
                        newColors[i] = Color.Lerp(defaultColors[i], creatureColor.Value, 10f);
                        newColors[i].a = defaultColors[i].a;
                    }
                }
                //Create a new texture with modified color
                BloodMod.bloodTextures[creatureColor.Key] = new Texture2D(BloodMod.w, BloodMod.h);
                BloodMod.bloodTextures[creatureColor.Key].SetPixels(newColors);
                BloodMod.bloodTextures[creatureColor.Key].Apply(true);
                //Add the new texture to the AtlasManager, remove existing texture first if present
                if (Futile.atlasManager.DoesContainElementWithName(creatureColor.Key + "Tex"))
                {
                    Futile.atlasManager.UnloadAtlas(creatureColor.Key + "Tex");
                }
                Futile.atlasManager.LoadAtlasFromTexture(creatureColor.Key + "Tex", BloodMod.bloodTextures[creatureColor.Key]);
                if (Futile.atlasManager.DoesContainElementWithName(creatureColor.Key + "Tex"))
                {
                    //Debug.Log("Success!");
                }
            }
            catch
            {
                Debug.Log("Failed! [" + creatureColor.Key + "]");
            }
        }
        Debug.Log("Finished generating blood textures.");
    }

    public static void GenerateBloodTexture(string creature, Color creatureColor)
    {
        //Get colors from texture
        Color[] defaultColors = BloodMod.bloodTex.GetPixels();
        //Modify the colors to match each one in the dictionary
        Color[] newColors = defaultColors;
        for (int i = 0; i < defaultColors.Length; i++)
        {
            if (newColors[i].a > 0f)
            {
                newColors[i] = Color.Lerp(defaultColors[i], creatureColor, 1f);
                newColors[i].a = defaultColors[i].a;
            }
        }
        //Create a new texture with modified color
        BloodMod.bloodTextures[creature] = new Texture2D(BloodMod.w, BloodMod.h);
        BloodMod.bloodTextures[creature].SetPixels(newColors);
        BloodMod.bloodTextures[creature].Apply(true);
        //Add the new texture to the AtlasManager
        if (Futile.atlasManager.DoesContainElementWithName(creature + "Tex"))
        {
            Futile.atlasManager.UnloadAtlas(creature + "Tex");
        }
        Futile.atlasManager.LoadAtlasFromTexture(creature + "Tex", BloodMod.bloodTextures[creature]);
    }
    //[NEW]
    public static string ReturnCreatureSprite(string critName)
    {
        if(critName == "TempleGuard")
        {
            return "Sandbox_Randomize";
        }
        CreatureTemplate.Type type = (CreatureTemplate.Type)Enum.Parse(typeof(CreatureTemplate.Type), critName);
        CreatureSymbol.IconSymbolData data = new IconSymbol.IconSymbolData(type, AbstractPhysicalObject.AbstractObjectType.Creature, 0);
        return CreatureSymbol.SpriteNameOfCreature(data);
    }

    public static Color ReturnSpriteColor(string critName)
    {
        CreatureTemplate.Type type = (CreatureTemplate.Type)Enum.Parse(typeof(CreatureTemplate.Type), critName);
        CreatureSymbol.IconSymbolData data = new IconSymbol.IconSymbolData(type, AbstractPhysicalObject.AbstractObjectType.Creature, 0);
        return CreatureSymbol.ColorOfCreature(data);
    }

    //[OUTDATED] Function for inputting the CreatureName and returning it's associated icon sprite
    public static string GetCreatureSprite(string creatureName)
    {
        switch (creatureName)
        {
            case "Slugcat":
                return "Kill_Slugcat";
            case "PinkLizard":
            case "BlueLizard":
            case "RedLizard":
            case "CyanLizard":
                return "Kill_Standard_Lizard";
            case "GreenLizard":
                return "Kill_Green_Lizard";
            case "YellowLizard":
                return "Kill_Yellow_Lizard";
            case "WhiteLizard":
                return "Kill_White_Lizard";
            case "BlackLizard":
                return "Kill_Black_Lizard";
            case "Salamander":
                return "Kill_Salamander";
            case "Fly":
                return "Kill_Bat";
            case "Leech":
            case "SeaLeech":
                return "Kill_Leech";
            case "Snail":
                return "Kill_Snail";
            case "Vulture":
                return "Kill_Vulture";
            case "GarbageWorm":
                return "Kill_Garbageworm";
            case "LanternMouse":
                return "Kill_Mouse";
            case "CicadaA":
            case "CicadaB":
                return "Kill_Cicada";
            case "Spider":
                return "Kill_SmallSpider";
            case "JetFish":
                return "Kill_Jetfish";
            case "BigEel":
                return "Kill_BigEel";
            case "Deer":
                return "Kill_RainDeer";
            case "TubeWorm":
                return "Kill_Tubeworm";
            case "DaddyLongLegs":
            case "BrotherLongLegs":
                return "Kill_Daddy";
            case "TentaclePlant":
                return "Kill_TentaclePlant";
            case "PoleMimic":
                return "Kill_PoleMimic";
            case "MirosBird":
                return "Kill_MirosBird";
            case "Centipede":
                return "Kill_Centipede3";
            case "RedCentipede":
                return "Kill_Centipede3";
            case "Centiwing":
                return "Kill_Centiwing";
            case "SmallCentipede":
                return "Kill_Centipede1";
            case "Scavenger":
                return "Kill_Scavenger";
            case "Overseer":
                return "Kill_Overseer";
            case "VultureGrub":
                return "Kill_VultureGrub";
            case "EggBug":
                return "Kill_EggBug";
            case "BigSpider":
            case "SpitterSpider":
                return "Kill_BigSpider";
            case "SmallNeedleWorm":
                return "Kill_SmallNeedleWorm";
            case "BigNeedleWorm":
                return "Kill_NeedleWorm";
            case "DropBug":
                return "Kill_DropBug";
            case "KingVulture":
                return "Kill_KingVulture";
            case "Hazer":
                return "Kill_Hazer";
            case "TempleGuard":
                return "Sandbox_Randomize";
        }
        return "Futile_White";
    }

    //Function for getting the creature icon sprite color using CreatureName
    public static Color GetCreatureSpriteColor(string creatureName)
    {
        switch (creatureName)
        {
            case "Slugcat":
                return new Color(0.9019608f, 0.9019608f, 0.9019608f);
            case "PinkLizard":
                return new Color(0.9019608f, 0.0019608f, 0.7019608f);
            case "GreenLizard":
                return new Color(0.0119608f, 0.9019608f, 0.1019608f);
            case "BlueLizard":
                return new Color(0.0019608f, 0.4019608f, 0.9019608f);
            case "YellowLizard":
            case "Centipede":
            case "SmallCentipede":
                return new Color(1f, 0.6f, 0f);
            case "WhiteLizard":
                return new Color(0.9019608f, 0.9019608f, 0.9019608f);
            case "RedLizard":
                return new Color(0.9019608f, 0.05490196f, 0.05490196f);
            case "BlackLizard":
                return new Color(0.368627459f, 0.368627459f, 0.435294122f);
            case "Salamander":
                return new Color(0.933333337f, 0.78039217f, 0.894117653f);
            case "CyanLizard":
            case "Overseer":
                return new Color(0f, 0.9098039f, 0.9019608f);
            case "Leech":
            case "SpitterSpider":
                return new Color(0.68235296f, 0.156862751f, 0.117647059f);
            case "SeaLeech":
            case "TubeWorm":
                return new Color(0.05f, 0.3f, 0.7f);
            case "CicadaA":
                return new Color(1f, 1f, 1f);
            case "CicadaB":
                return new Color(0.368627459f, 0.368627459f, 0.435294122f);
            case "DaddyLongLegs":
                return new Color(0f, 0f, 1f);
            case "BrotherLongLegs":
                return new Color(0.454901963f, 0.5254902f, 0.305882365f);
            case "RedCentipede":
                return new Color(0.9019608f, 0.05490196f, 0.05490196f);
            case "Centiwing":
                return new Color(0.05490196f, 0.698039234f, 0.235294119f);
            case "VultureGrub":
                return new Color(0.831372559f, 0.7921569f, 0.435294122f);
            case "EggBug":
                return new Color(0f, 1f, 0.470588237f);
            case "SmallNeedleWorm":
            case "BigNeedleWorm":
                return new Color(1f, 0.596078455f, 0.596078455f);
            case "Hazer":
                return new Color(0.211764708f, 0.7921569f, 0.3882353f);
        }
        return Color.Lerp(Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey), new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value),0.15f);
    }
}
