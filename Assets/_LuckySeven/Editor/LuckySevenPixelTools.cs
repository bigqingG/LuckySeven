using System;
using System.IO;
using System.Linq;
using LuckySeven.UI;
using LuckySeven.Presentation;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LuckySeven.Editor
{
    public static class LuckySevenPixelTools
    {
        [Serializable] sealed class Row {public string key,zh,en;}
        [Serializable] sealed class Rows {public Row[] entries;}
        [MenuItem("Lucky Seven/Apply Pixel Adventure")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode before applying the skin.");
            for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Save modified scenes before applying the skin.");
            if(!File.Exists("Backups/BeforePixelAdventure20261107/_LuckySeven/Scenes/Main.unity"))throw new InvalidOperationException("Pre-migration backup required.");
            const string art="Assets/_LuckySeven/Art/PixelAdventure/Characters.png";
            var importer=(TextureImporter)AssetImporter.GetAtPath(art);
            importer.GetSourceTextureWidthAndHeight(out int w,out int h);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.filterMode=FilterMode.Point;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=1024;importer.isReadable=false;
            var names=new[]{"Hero","Die","Shadow","Rogue"};
#pragma warning disable 0618
            importer.spritesheet=Enumerable.Range(0,4).Select(i=>new SpriteMetaData{name=names[i],rect=new Rect((i%2)*w/2,(i<2?1:0)*h/2,w/2,h/2),alignment=(int)SpriteAlignment.Center,pivot=new Vector2(.5f,.5f)}).ToArray();
#pragma warning restore 0618
            importer.SaveAndReimport();
            var sprites=AssetDatabase.LoadAllAssetsAtPath(art).OfType<Sprite>().ToArray();
            var ordered=names.Select(n=>sprites.Single(s=>s.name==n)).ToArray();
            SyncText();
            var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_LuckySeven/Fonts/ChineseSDF.asset");
            var scene=EditorSceneManager.OpenScene(LuckySevenProjectTools.ScenePath);
            var view=UnityEngine.Object.FindFirstObjectByType<BattleHUDView>();
            view.ConfigurePixelLayout(ordered);
            var fx=UnityEngine.Object.FindFirstObjectByType<BattleFxPlayer>();
            var pixel=fx.GetComponent<PixelBattleFx>()??fx.gameObject.AddComponent<PixelBattleFx>();
            pixel.Configure(view.PixelEnemy,view.BattleRoot,font);
            fx.Configure(view.Feedback,view.EnemyVisual,fx.GetComponent<AudioSource>());fx.ConfigurePixel(pixel);
            EditorUtility.SetDirty(view);EditorUtility.SetDirty(fx);EditorUtility.SetDirty(pixel);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Directory.CreateDirectory("Docs/Validation/PixelAdventure");
            File.WriteAllText("Docs/Validation/PixelAdventure/skin-setup.txt","Pixel adventure applied; 4 character sprites; localized UI; original rules/config preserved.\n");
        }
        [MenuItem("Lucky Seven/Sync Localized Text")]
        public static void SyncText()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode before syncing text.");
            var rows=JsonUtility.FromJson<Rows>(File.ReadAllText("Assets/_LuckySeven/Localization/GameText.json")).entries;
            var collection=LocalizationEditorSettings.GetStringTableCollection("GameText");
            foreach(var table in collection.StringTables){foreach(var row in rows)table.AddEntry(row.key,table.LocaleIdentifier.Code=="en"?row.en:row.zh);EditorUtility.SetDirty(table);}
            EditorUtility.SetDirty(collection.SharedData);EditorUtility.SetDirty(collection);
            var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_LuckySeven/Fonts/ChineseSDF.asset");
            string required=new string(string.Concat(rows.Select(r=>r.zh+r.en)).Distinct().Where(c=>!char.IsWhiteSpace(c)&&!font.HasCharacter(c)).ToArray());
            if(required.Length>0&&!font.TryAddCharacters(required,out var missing))throw new InvalidOperationException("Missing glyphs: "+missing);
            foreach(var atlas in font.atlasTextures){if(!AssetDatabase.Contains(atlas))AssetDatabase.AddObjectToAsset(atlas,font);EditorUtility.SetDirty(atlas);}
            EditorUtility.SetDirty(font);AssetDatabase.SaveAssets();
        }
    }
}

