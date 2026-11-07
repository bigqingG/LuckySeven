using System;
using System.IO;
using System.Linq;
using LuckySeven.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Localization;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;

namespace LuckySeven.Editor
{
    public static class LuckySevenLocalizationTools
    {
        [Serializable] sealed class Row {public string key,zh,en;}
        [Serializable] sealed class Rows {public Row[] entries;}
        const string Root="Assets/_LuckySeven/Localization";
        public static void Setup()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode first.");
            for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Save modified scene before localization migration.");
            var data=JsonUtility.FromJson<Rows>(File.ReadAllText(Root+"/GameText.json"));
            var settings=LocalizationEditorSettings.ActiveLocalizationSettings;
            if(settings==null){settings=ScriptableObject.CreateInstance<LocalizationSettings>();AssetDatabase.CreateAsset(settings,Root+"/LocalizationSettings.asset");LocalizationEditorSettings.ActiveLocalizationSettings=settings;}
            foreach(string code in new[]{"zh-Hans","en"})if(!LocalizationEditorSettings.GetLocales().Any(l=>l.Identifier.Code==code)){
                var locale=Locale.CreateLocale(code);AssetDatabase.CreateAsset(locale,Root+"/"+code+".asset");LocalizationEditorSettings.AddLocale(locale);
            }
            LocalizationSettings.StartupLocaleSelectors.Clear();LocalizationSettings.StartupLocaleSelectors.Add(new SpecificLocaleSelector{LocaleId=new LocaleIdentifier("zh-Hans")});
            LocalizationSettings.InitializeSynchronously=false;EditorUtility.SetDirty(settings);
            var strings=LocalizationEditorSettings.GetStringTableCollection("GameText")??LocalizationEditorSettings.CreateStringTableCollection("GameText",Root);
            foreach(var table in strings.StringTables){foreach(var row in data.entries)table.AddEntry(row.key,table.LocaleIdentifier.Code=="en"?row.en:row.zh);EditorUtility.SetDirty(table);}
            strings.SetPreloadTableFlag(true);EditorUtility.SetDirty(strings);EditorUtility.SetDirty(strings.SharedData);
            var font=CreateChineseFont(data);
            var assets=LocalizationEditorSettings.GetAssetTableCollection("GameAssets")??LocalizationEditorSettings.CreateAssetTableCollection("GameAssets",Root);
            var addressables=AddressableAssetSettingsDefaultObject.GetSettings(true);
            // A single bilingual font avoids transient tofu while independent localized labels refresh.
            // Both locale entries still resolve through the Asset Table; no TMP fallback chain is used.
            var oldEnglishGuid=AssetDatabase.AssetPathToGUID("Assets/_LuckySeven/Fonts/EnglishSDF.asset");
            if(!string.IsNullOrEmpty(oldEnglishGuid))addressables.RemoveAssetEntry(oldEnglishGuid);
            foreach(var table in assets.AssetTables){
                var chosen=font;var guid=AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(chosen));
                addressables.CreateOrMoveEntry(guid,addressables.DefaultGroup);table.AddEntry("UIFont",guid);EditorUtility.SetDirty(table);
            }
            assets.SetPreloadTableFlag(true);EditorUtility.SetDirty(assets);EditorUtility.SetDirty(assets.SharedData);
            LocalizationEditorSettings.EditorEvents.RaiseCollectionModified(null,strings);LocalizationEditorSettings.EditorEvents.RaiseCollectionModified(null,assets);
            var scene=EditorSceneManager.OpenScene(LuckySevenProjectTools.ScenePath);var view=UnityEngine.Object.FindFirstObjectByType<BattleHUDView>();
            view.ConfigurePokerCards(font);
            var bindings=new[]{
                ("Brand","brand"),("Footer","footer"),("MainMenuPanel/Index","menu.index"),("MainMenuPanel/Title","menu.title"),("MainMenuPanel/Tagline","menu.tagline"),
                ("MainMenuPanel/Play/Label","play"),("MainMenuPanel/HowToPlay/Label","help"),("BattleHUD/PlayerName","player.name"),("BattleHUD/YourPlan","player.plan"),
                ("BattleHUD/LuckyCaption","target.caption"),("BattleHUD/TargetRule","target.rule"),("BattleHUD/Clear/Label","clear"),("BattleHUD/NormalAttack/Label","attack.normal"),
                ("ResultPanel/NextBattle/Label","next"),("ResultPanel/Replay/Label","replay"),("HowToPlayPanel/HelpTitle","help.title"),("HowToPlayPanel/HelpBody","help.body"),("HowToPlayPanel/CloseHelp/Label","close")
            };
            foreach(var binding in bindings){
                var label=view.transform.Find(binding.Item1).GetComponent<TextMeshProUGUI>();
                var localize=label.GetComponent<LocalizeStringEvent>()??label.gameObject.AddComponent<LocalizeStringEvent>();
                localize.StringReference=new LocalizedString("GameText",binding.Item2);
                while(localize.OnUpdateString.GetPersistentEventCount()>0)UnityEventTools.RemovePersistentListener(localize.OnUpdateString,0);
                var setter=(UnityAction<string>)Delegate.CreateDelegate(typeof(UnityAction<string>),label,"set_text");
                UnityEventTools.AddPersistentListener(localize.OnUpdateString,setter);localize.OnUpdateString.SetPersistentListenerState(0,UnityEventCallState.EditorAndRuntime);
                label.text=data.entries.First(r=>r.key==binding.Item2).zh;EditorUtility.SetDirty(localize);
            }
            // Authored dynamic placeholders are Chinese too, before asynchronous startup finishes.
            foreach(var label in view.GetComponentsInChildren<TextMeshProUGUI>(true)){
                if(label.GetComponent<LocalizeStringEvent>()!=null)continue;
                if(label.transform.parent.name=="Sound")label.text=data.entries.First(r=>r.key=="sound.on").zh;
                else if(label.transform.parent.name=="Language")label.text=data.entries.First(r=>r.key=="language").zh;
                else if(label.gameObject.name=="CardName")label.text="强攻";
                else if(label.gameObject.name=="Ability")label.text="伤害 +2";
                else if(System.Text.RegularExpressions.Regex.IsMatch(label.text,"[A-Za-z]"))label.text="";
                EditorUtility.SetDirty(label);
            }
            EditorUtility.SetDirty(view);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            var missing=strings.StringTables.SelectMany(t=>data.entries.Where(r=>string.IsNullOrWhiteSpace(t.GetEntry(r.key)?.LocalizedValue)).Select(r=>t.LocaleIdentifier+":"+r.key)).ToArray();
            if(missing.Length>0)throw new InvalidOperationException("Missing translations: "+string.Join(",",missing));
            File.WriteAllText("Docs/Validation/LocalizedCards/localization-setup.txt",$"{data.entries.Length} keys x {strings.StringTables.Count} locales; {bindings.Length} static label bindings; Chinese font multi-atlas: {font.isMultiAtlasTexturesEnabled}; all translations present.");
        }
        static TMP_FontAsset CreateChineseFont(Rows data)
        {
            const string path="Assets/_LuckySeven/Fonts/ChineseSDF.asset";
            var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if(font==null){
                var source=AssetDatabase.LoadAssetAtPath<Font>("Assets/_LuckySeven/Fonts/LuckySevenSans.ttf");
                if(source==null)throw new InvalidOperationException("Chinese source font not imported.");
                font=TMP_FontAsset.CreateFontAsset(source,48,5,GlyphRenderMode.SDFAA,2048,2048,AtlasPopulationMode.Dynamic,true);
                AssetDatabase.CreateAsset(font,path);
                foreach(var texture in font.atlasTextures)AssetDatabase.AddObjectToAsset(texture,font);
                AssetDatabase.AddObjectToAsset(font.material,font);
            }
            font.atlasPopulationMode=AtlasPopulationMode.Dynamic;font.isMultiAtlasTexturesEnabled=true;
            string chars=string.Concat(data.entries.Select(r=>r.zh+r.en))+"0123456789±?：！。，·…+-=/ ";
            string needed=new string(chars.Distinct().Where(c=>!char.IsWhiteSpace(c)&&!font.HasCharacter(c)).ToArray());
            if(needed.Length>0&&!font.TryAddCharacters(needed,out var missing))throw new InvalidOperationException("Missing font characters: "+missing);
            foreach(var texture in font.atlasTextures){if(!AssetDatabase.Contains(texture))AssetDatabase.AddObjectToAsset(texture,font);EditorUtility.SetDirty(texture);}
            font.material.mainTexture=font.atlasTexture;EditorUtility.SetDirty(font.material);EditorUtility.SetDirty(font);AssetDatabase.SaveAssets();
            if(!AssetDatabase.LoadAllAssetsAtPath(path).Any(a=>a is Material))throw new InvalidOperationException("Font material was not saved.");
            return font;
        }
    }
}
