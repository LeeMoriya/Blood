using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

public static class BloodData
{
    public static string configPath = $"{Application.persistentDataPath}\\ModConfigs\\Blood";
    public static string filePath = $"{Application.persistentDataPath}\\ModConfigs\\Blood\\colors.txt";
    public static void Save(Dictionary<string, Color> colors)
    {
        if (!Directory.Exists(configPath))
        {
            Directory.CreateDirectory(configPath);
        }
        StringBuilder sb = new StringBuilder();
        foreach (KeyValuePair<string, Color> pair in colors)
        {
            sb.AppendLine($"{pair.Key}<>{pair.Value.r}:{pair.Value.g}:{pair.Value.b}");
        }
        File.WriteAllText(filePath, sb.ToString());
    }

    public static Dictionary<string, Color> Load()
    {
        try
        {
            Dictionary<string, Color> dict = new Dictionary<string, Color>();
            string[] text;
            if (File.Exists(filePath))
            {
                text = File.ReadAllLines(filePath);
                for (int i = 0; i < text.Length; i++)
                {
                    string[] split = Regex.Split(text[i], "<>");
                    string[] colors = Regex.Split(split[1], ":");
                    dict.Add(split[0], ParseColor(colors));
                }
            }
            return dict;
        }
        catch(Exception e)
        {
            Debug.LogException(e);
            if(File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        return BloodMod.defaultColors;
    }

    public static Color ParseColor(string[] colors)
    {
        float r = 0.5f;
        float g = 0;
        float b = 0;
        float.TryParse(colors[0], out r);
        float.TryParse(colors[1], out g);
        float.TryParse(colors[2], out b);
        return new Color(r, g, b, 1f);
    }
}


