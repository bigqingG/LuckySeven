using LuckySeven.Localization;
using System;
using LuckySeven.Core;
using TMPro;
using UnityEngine;

namespace LuckySeven.Presentation
{
    public sealed class BattleFxPlayer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI feedback;
        [SerializeField] private RectTransform enemy;
        [SerializeField] private AudioSource source;
        [SerializeField] private PixelBattleFx pixel;
        public void ConfigurePixel(PixelBattleFx value){pixel=value;}
        private AudioClip lucky, hit;
        private float remaining;
        private string feedbackKey;
        private object[] feedbackArgs;
        private CardEffect lastEffect;
        private bool attackFeedback;
        public void RefreshText(){if(string.IsNullOrEmpty(feedbackKey)){feedback.text="";return;}if(attackFeedback)feedbackArgs[1]=L10n.Effect(lastEffect);feedback.text=L10n.T(feedbackKey,feedbackArgs);}
        void Show(string key,params object[] args){feedbackKey=key;feedbackArgs=args;RefreshText();}
        public bool Muted { get; private set; }

        public void Configure(TextMeshProUGUI label, RectTransform visual, AudioSource audio)
        { feedback = label; enemy = visual; source = audio; }
        private void Awake()
        {
            // Original synthesized tones; no downloaded audio or autoplay.
            lucky = Tone("Lucky seven", 660, true);
            hit = Tone("Ticket impact", 190, false);
        }
        private static AudioClip Tone(string name, float frequency, bool chord)
        {
            const int rate = 22050;
            var samples = new float[rate / 4];
            for (int i = 0; i < samples.Length; i++)
            {
                double t = (double)i / rate;
                double wave = Math.Sin(2 * Math.PI * frequency * t);
                if (chord) wave = (wave + Math.Sin(2 * Math.PI * frequency * 1.25 * t) + Math.Sin(2 * Math.PI * frequency * 1.5 * t)) / 3;
                samples[i] = (float)(wave * .2 * Math.Min(1, t * 80) * (1.0 - (double)i / samples.Length));
            }
            var clip = AudioClip.Create(name, samples.Length, 1, rate, false);
            clip.SetData(samples, 0); return clip;
        }
        public void Attack(AttackResult result)
        {
            if(pixel!=null)pixel.Hit(result.Damage);
            attackFeedback=true;lastEffect=result.Effect;Show(result.Breached?"fx.attack.break":"fx.attack.normal",result.Damage,L10n.Effect(result.Effect));
            feedback.color = result.Breached ? BattleHUDColors.Gold : BattleHUDColors.Teal;
            remaining = .6f;
            if (!Muted) source.PlayOneShot(result.Breached ? lucky : hit);
        }
        public void Break(int target) {
            if(pixel!=null)pixel.Break();
            attackFeedback=false;Show("fx.break");
            feedback.color = BattleHUDColors.Gold; remaining=.6f;
            if(!Muted)source.PlayOneShot(lucky);
        }
        public void Counter(int attack, int shield)
        {
            attackFeedback=false;Show("fx.counter",attack,Math.Min(attack,shield));
            feedback.color = BattleHUDColors.Coral;
        }
        public void ToggleMute() { Muted = !Muted; source.mute = Muted; }
        public void ResetFeedback()
        {
            if(pixel!=null)pixel.ResetVisuals();
            remaining = 0; feedbackKey=null;attackFeedback=false;feedback.text = ""; enemy.localScale = Vector3.one;
            if (source != null) source.Stop();
        }
        private void Update()
        {
            if (remaining <= 0) return;
            remaining = Mathf.Max(0, remaining - Time.unscaledDeltaTime);
            enemy.localScale = Vector3.one * (1 + Mathf.Sin(remaining * 40) * remaining * .12f);
        }
        private void OnDestroy()
        {
            if (lucky != null) Destroy(lucky);
            if (hit != null) Destroy(hit);
        }
    }

    public static class BattleHUDColors
    {
        public static readonly Color Gold = new Color32(255, 207, 100, 255);
        public static readonly Color Teal = new Color32(100, 225, 203, 255);
        public static readonly Color Coral = new Color32(255, 132, 113, 255);
    }
}
