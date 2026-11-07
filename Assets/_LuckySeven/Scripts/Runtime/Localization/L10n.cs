using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Runtime.InteropServices;
using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace LuckySeven.Localization
{
    // Locale authority belongs to Unity Localization. Tables/assets load asynchronously for WebGL.
    public static class L10n
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void LuckySevenSetLocale(string code);
#endif
        public const string PreferenceKey="LuckySeven.Locale";
        static StringTable table;
        static AsyncOperationHandle<StringTable> heldTable;
        static AsyncOperationHandle<TMP_FontAsset> heldFont;
        public static TMP_FontAsset Font { get; private set; }
        public static string LocaleCode => LocalizationSettings.SelectedLocale.Identifier.Code;
        public static IEnumerator Initialize()
        {
            yield return LocalizationSettings.InitializationOperation;
            var code=PlayerPrefs.GetString(PreferenceKey,"zh-Hans");
            LocalizationSettings.SelectedLocale=LocalizationSettings.AvailableLocales.GetLocale(code)
                ?? LocalizationSettings.AvailableLocales.GetLocale("zh-Hans");
            yield return LoadLocale();
            SavePreference();
        }
        public static IEnumerator LoadLocale()
        {
            var locale=LocalizationSettings.SelectedLocale;
            var strings=LocalizationSettings.StringDatabase.GetTableAsync("GameText",locale);
            var font=LocalizationSettings.AssetDatabase.GetLocalizedAssetAsync<TMP_FontAsset>("GameAssets","UIFont",locale);
            // Localization asset operations auto-release on completion; retain them across coroutine frames.
            Addressables.ResourceManager.Acquire(strings);Addressables.ResourceManager.Acquire(font);
            yield return strings;yield return font;
            if(strings.Result==null || font.Result==null)throw new InvalidOperationException("Localization table/font failed to load.");
            table=strings.Result;Font=font.Result;
            if(heldTable.IsValid())Addressables.Release(heldTable);
            if(heldFont.IsValid())Addressables.Release(heldFont);
            heldTable=strings;heldFont=font;
        }
        public static string T(string key,params object[] args)
        {
            var entry=table?.GetEntry(key);
            if(entry==null)throw new InvalidOperationException("Missing localized key: "+key);
            return args.Length==0?entry.LocalizedValue:string.Format(CultureInfo.InvariantCulture,entry.LocalizedValue,args);
        }
        public static string Effect(Core.CardEffect effect)=>T("effect."+effect.ToString().ToLowerInvariant());
        public static void SavePreference(){PlayerPrefs.SetString(PreferenceKey,LocaleCode);PlayerPrefs.Save();
#if UNITY_WEBGL && !UNITY_EDITOR
            LuckySevenSetLocale(LocaleCode);
#endif
        }
    }
}
