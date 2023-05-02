using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class BloodColor
{
    public static void GenerateBloodTextures(Dictionary<string, Color> creatureColors)
    {
        //Create base texture that will be modified for each blood color
        BloodMod.bloodTex = new Texture2D(32, 16, TextureFormat.ARGB32, false);
        BloodMod.bloodTex.anisoLevel = 0;
        BloodMod.bloodTex.filterMode = FilterMode.Point;
        byte[] bloodTex = Convert.FromBase64String(BloodMod.bloodData);
        BloodMod.bloodTex.LoadImage(bloodTex);
        BloodMod.w = BloodMod.bloodTex.width;
        BloodMod.h = BloodMod.bloodTex.height;

        //Reset the bloodTexture Dictionary
        BloodMod.bloodTextures = new Dictionary<string, Texture2D>();
        foreach (string name in ExtEnumBase.GetNames(typeof(CreatureTemplate.Type)))
        {
            //If the creature is modded, add it to the colors Dictionary with a default color
            if (!BloodMod.creatureBlacklist.Contains(name))
            {
                if (!BloodMod.defaultColors.ContainsKey(name))
                {
                    BloodMod.defaultColors.Add(name, new Color(0.5f, 0f, 0f));
                }
                if (!BloodMod.vibrantColors.ContainsKey(name))
                {
                    BloodMod.vibrantColors.Add(name, new Color(0.5f, 0f, 0f));
                }
                BloodMod.bloodTextures.Add(name, new Texture2D(BloodMod.w, BloodMod.h));
            }
        }

        Debug.Log("BLOOD: Generating blood textures...");
        //Get colors from texture
        Color[] defaultColors = BloodMod.bloodTex.GetPixels();
        //Modify the colors to match each one in the dictionary
        foreach (KeyValuePair<string, Color> creatureColor in creatureColors)
        {
            Debug.Log("Attempting to create blood texture for " + creatureColor.Key + "...");
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
                    Debug.Log($"BLOOD: Remove old Texture for: {creatureColor.Key}");
                }
                Futile.atlasManager.LoadAtlasFromTexture(creatureColor.Key + "Tex", BloodMod.bloodTextures[creatureColor.Key], false);
                if (Futile.atlasManager.DoesContainElementWithName(creatureColor.Key + "Tex"))
                {
                    Debug.Log($"BLOOD: Success: {creatureColor.Key} - R: {creatureColor.Value.r} G: {creatureColor.Value.g} B: {creatureColor.Value.b}");
                }
            }
            catch
            {
                Debug.Log("BLOOD: Failed! [" + creatureColor.Key + "]");
            }
        }
        Debug.Log("BLOOD: Finished generating blood textures.");
    }
}
