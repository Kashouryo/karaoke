﻿// Copyright (c) andy840119 <andy840119@gmail.com>. Licensed under the GPL Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Karaoke.Tests.Mods
{
    [TestFixture]
    public class TestSceneKaraokeModAutoplay : ModTestScene
    {
        public TestSceneKaraokeModAutoplay()
            : base(new KaraokeRuleset())
        {
        }

        // mod auto-play will cause crash
        /*
        [Test]
        public void TestMod() => CreateModTest(new ModTestData
        {
            Mod = new KaraokeModAutoplay(),
            Autoplay = true,
            Beatmap = new TestKaraokeBeatmap(null),
            PassCondition = () => true
        });
        */
    }
}
