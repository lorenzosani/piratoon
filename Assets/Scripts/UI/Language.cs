using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class Language {
    /// <summary>
    /// Text Field
    /// Useage: Field[key]
    /// Example: Language.Field["world"]
    /// </summary>
    public static Dictionary<String, String> Field { get; private set; }

    /// <summary>
    /// Init on first use
    /// </summary>
    static Language() {
        LoadLanguage();
    }

    /// <summary>
    /// Load language files from resources
    /// </summary>
    public static void LoadLanguage() {
        if (Field == null)
            Field = new Dictionary<string, string>();

        Field.Clear();
        string lang = GetLanguage();
        //lang = "de";
        var textAsset = Resources.Load(@"Languages/" + lang); //no .txt needed
        string allTexts = "";
        if (textAsset == null)
            textAsset = Resources.Load(@"Languages/en")as TextAsset; //no .txt needed
        if (textAsset == null)
            Debug.LogError("File not found for Language: Assets/Resources/Languages/" + lang + ".txt");
        allTexts = (textAsset as TextAsset).text;
        string[] lines = allTexts.Split(new string[] { "\r\n", "\n" },
            StringSplitOptions.None);
        string key, value;
        for (int i = 0; i < lines.Length; i++) {
            if (lines[i].IndexOf("=") >= 0 && !lines[i].StartsWith("#")) {
                key = lines[i].Substring(0, lines[i].IndexOf("=")).Trim();
                value = lines[i].Substring(lines[i].IndexOf("=") + 1,
                    lines[i].Length - lines[i].IndexOf("=") - 1).Replace("\\n", Environment.NewLine).Trim();
                Field.Add(key, value);
            }
        }
    }

    /// <summary>
    /// get the current language
    /// </summary>
    /// <returns></returns>
    public static string GetLanguage() {
        string savedLang = PlayerPrefs.GetString("Language", null);
        return savedLang ?? Get2LetterISOCodeFromSystemLanguage().ToLower();
    }

    /// <summary>
    /// Helps to convert Unity's Application.systemLanguage to a 
    /// 2 letter ISO country code. There is unfortunately not more
    /// countries available as Unity's enum does not enclose all
    /// countries.
    /// </summary>
    /// <returns>The 2-letter ISO code from system language.</returns>
    public static string Get2LetterISOCodeFromSystemLanguage() {
        SystemLanguage lang = Application.systemLanguage;
        string res = "EN";
        switch (lang) {
            case SystemLanguage.Afrikaans:
                res = "AF";
                break;
            case SystemLanguage.Arabic:
                res = "AR";
                break;
            case SystemLanguage.Basque:
                res = "EU";
                break;
            case SystemLanguage.Belarusian:
                res = "BY";
                break;
            case SystemLanguage.Bulgarian:
                res = "BG";
                break;
            case SystemLanguage.Catalan:
                res = "CA";
                break;
            case SystemLanguage.Chinese:
                res = "ZH";
                break;
            case SystemLanguage.ChineseSimplified:
                res = "ZH";
                break;
            case SystemLanguage.ChineseTraditional:
                res = "ZH";
                break;
            case SystemLanguage.Czech:
                res = "CS";
                break;
            case SystemLanguage.Danish:
                res = "DA";
                break;
            case SystemLanguage.Dutch:
                res = "NL";
                break;
            case SystemLanguage.English:
                res = "EN";
                break;
            case SystemLanguage.Estonian:
                res = "ET";
                break;
            case SystemLanguage.Faroese:
                res = "FO";
                break;
            case SystemLanguage.Finnish:
                res = "FI";
                break;
            case SystemLanguage.French:
                res = "FR";
                break;
            case SystemLanguage.German:
                res = "DE";
                break;
            case SystemLanguage.Greek:
                res = "EL";
                break;
            case SystemLanguage.Hebrew:
                res = "IW";
                break;
            case SystemLanguage.Hungarian:
                res = "HU";
                break;
            case SystemLanguage.Icelandic:
                res = "IS";
                break;
            case SystemLanguage.Indonesian:
                res = "IN";
                break;
            case SystemLanguage.Italian:
                res = "IT";
                break;
            case SystemLanguage.Japanese:
                res = "JA";
                break;
            case SystemLanguage.Korean:
                res = "KO";
                break;
            case SystemLanguage.Latvian:
                res = "LV";
                break;
            case SystemLanguage.Lithuanian:
                res = "LT";
                break;
            case SystemLanguage.Norwegian:
                res = "NO";
                break;
            case SystemLanguage.Polish:
                res = "PL";
                break;
            case SystemLanguage.Portuguese:
                res = "PT";
                break;
            case SystemLanguage.Romanian:
                res = "RO";
                break;
            case SystemLanguage.Russian:
                res = "RU";
                break;
            case SystemLanguage.SerboCroatian:
                res = "SH";
                break;
            case SystemLanguage.Slovak:
                res = "SK";
                break;
            case SystemLanguage.Slovenian:
                res = "SL";
                break;
            case SystemLanguage.Spanish:
                res = "ES";
                break;
            case SystemLanguage.Swedish:
                res = "SV";
                break;
            case SystemLanguage.Thai:
                res = "TH";
                break;
            case SystemLanguage.Turkish:
                res = "TR";
                break;
            case SystemLanguage.Ukrainian:
                res = "UK";
                break;
            case SystemLanguage.Unknown:
                res = "EN";
                Debug.Log("Unknown language");
                break;
            case SystemLanguage.Vietnamese:
                res = "VI";
                break;
        }
        Debug.Log("Lang: " + res);
        return res;
    }

}