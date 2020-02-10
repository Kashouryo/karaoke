﻿// Copyright (c) andy840119 <andy840119@gmail.com>. Licensed under the GPL Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Karaoke.Configuration;
using osu.Game.Rulesets.Karaoke.Judgements;
using osu.Game.Rulesets.Karaoke.Objects;
using osu.Game.Rulesets.Karaoke.Objects.Drawables;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Karaoke.UI
{
    public class LyricPlayfield : Playfield
    {
        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; }

        public IBeatmap Beatmap => beatmap.Value.Beatmap;

        protected IWorkingBeatmap WorkingBeatmap => beatmap.Value;

        private readonly BindableBool translate = new BindableBool();
        private readonly Bindable<string> translateLanguage = new Bindable<string>();

        private readonly BindableDouble preemptTime = new BindableDouble();
        private readonly Bindable<LyricLine> nowLyric = new Bindable<LyricLine>();
        private readonly Cached seekCache = new Cached();

        public LyricPlayfield()
        {
            // Change need to translate
            translate.BindValueChanged(x => updateLyricTranslate());
            translateLanguage.BindValueChanged(x => updateLyricTranslate());

            // Switch to target time
            nowLyric.BindValueChanged(value =>
            {
                if (!seekCache.IsValid || value.NewValue == null)
                    return;

                var lyricStartTime = value.NewValue.LyricStartTime - preemptTime.Value;

                WorkingBeatmap.Track.Seek(lyricStartTime);
            });

            seekCache.Validate();
        }

        private void updateLyricTranslate()
        {
            var isTranslate = translate.Value;
            var translateLanguage = this.translateLanguage.Value;

            var lyric = Beatmap.HitObjects.OfType<LyricLine>().ToList();
            var translateDictionary = Beatmap.HitObjects.OfType<TranslateDictionary>().FirstOrDefault();

            // Clear exist translate
            lyric.ForEach(x => x.TranslateText = null);

            // If contain target language
            if (isTranslate && translateLanguage != null
                            && translateDictionary != null && translateDictionary.Translates.ContainsKey(translateLanguage))
            {
                var language = translateDictionary.Translates[translateLanguage];

                // Apply translate
                for (int i = 0; i < Math.Min(lyric.Count, language.Count); i++)
                {
                    lyric[i].TranslateText = language[i];
                }
            }
        }

        public override void Add(DrawableHitObject h)
        {
            if (h is DrawableLyricLine drawableLyric)
                drawableLyric.OnLyricStart += OnNewResult;

            h.OnNewResult += OnNewResult;
            base.Add(h);
        }

        public override bool Remove(DrawableHitObject h)
        {
            if (h is DrawableLyricLine drawableLyric)
                drawableLyric.OnLyricStart -= OnNewResult;

            h.OnNewResult -= OnNewResult;
            return base.Remove(h);
        }

        internal void OnNewResult(DrawableHitObject judgedObject, JudgementResult result)
        {
            if (result.Judgement is KaraokeLyricJudgement karaokeLyricJudgement)
            {
                // Update now lyric
                var targetLyric = karaokeLyricJudgement.Time == LyricTime.Available ? judgedObject.HitObject as LyricLine : null;
                seekCache.Invalidate();
                nowLyric.Value = targetLyric;
                seekCache.Validate();
            }
        }

        [BackgroundDependencyLoader]
        private void load(KaraokeRulesetConfigManager rulesetConfig, KaroakeSessionStatics session)
        {
            // Translate
            session.BindWith(KaraokeRulesetSession.UseTranslate, translate);
            session.BindWith(KaraokeRulesetSession.PreferLanguage, translateLanguage);

            // Practice
            rulesetConfig.BindWith(KaraokeRulesetSetting.PracticePreemptTime, preemptTime);
            session.BindWith(KaraokeRulesetSession.NowLyric, nowLyric);
        }
    }
}
