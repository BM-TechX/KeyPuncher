using Xunit;

namespace KeyPuncherForm.Tests
{
    /// <summary>Checks the hand-written tables against the keyboard drivers that ship with Windows.</summary>
    public class WindowsDriverTests
    {
        private static readonly ushort[] PrintableKeys =
        {
            0x29, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D,
            0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B,
            0x1E, 0x1F, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x2B,
            0x56, 0x2C, 0x2D, 0x2E, 0x2F, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35,
        };

        public static TheoryData<string> Layouts()
        {
            var data = new TheoryData<string>();
            foreach (KeyboardLayout layout in KeyboardLayout.All)
            {
                data.Add(layout.Name);
            }

            return data;
        }

        private static KeyboardLayout Find(string name) => KeyboardLayout.All.Single(l => l.Name == name);

        private static IEnumerable<KeyStroke> AllStrokes()
        {
            foreach (ushort key in PrintableKeys)
            {
                yield return new KeyStroke(key);
                yield return new KeyStroke(key, Shift: true);
                yield return new KeyStroke(key, AltGr: true);
                yield return new KeyStroke(key, Shift: true, AltGr: true);
            }
        }

        [Theory]
        [MemberData(nameof(Layouts))]
        public void EveryMappedCharacterIsWhatWindowsProduces(string name)
        {
            KeyboardLayout layout = Find(name);
            using var windows = new WindowsLayout(layout.WindowsLayoutId);

            var wrong = layout.Characters
                .Where(entry => entry.Key is not ('\n' or '\t'))
                .Select(entry => (Expected: entry.Key.ToString(), Actual: windows.Produce(entry.Value)))
                .Where(r => r.Expected != r.Actual)
                .Select(r => $"'{r.Expected}' gave '{r.Actual}'")
                .ToList();

            Assert.True(wrong.Count == 0, string.Join(", ", wrong));
        }

        [Theory]
        [MemberData(nameof(Layouts))]
        public void EveryCharacterOnTheKeyboardIsMapped(string name)
        {
            KeyboardLayout layout = Find(name);
            using var windows = new WindowsLayout(layout.WindowsLayoutId);

            var missing = AllStrokes()
                .Select(stroke => windows.Produce(new[] { stroke }))
                .Where(text => text.Length == 1 && text[0] >= ' ' && !layout.Characters.ContainsKey(text[0]))
                .ToList();

            Assert.True(missing.Count == 0, string.Join(" ", missing));
        }

        [Theory]
        [MemberData(nameof(Layouts))]
        public void DeadKeysMatchWindows(string name)
        {
            KeyboardLayout layout = Find(name);
            using var windows = new WindowsLayout(layout.WindowsLayoutId);

            var actual = AllStrokes().Where(windows.IsDead).Select(s => s.ToString()).Order();

            Assert.Equal(actual, layout.DeadKeys.Keys.Select(s => s.ToString()).Order());
        }

        [Theory]
        [MemberData(nameof(Layouts))]
        public void EveryAccentWindowsCanComposeIsMapped(string name)
        {
            KeyboardLayout layout = Find(name);
            using var windows = new WindowsLayout(layout.WindowsLayoutId);

            var missing = new List<string>();
            foreach (KeyStroke deadKey in layout.DeadKeys.Keys)
            {
                foreach (ushort key in PrintableKeys)
                {
                    foreach (bool shift in new[] { false, true })
                    {
                        string text = windows.Produce(new[] { deadKey, new KeyStroke(key, Shift: shift) });
                        if (text.Length == 1 && !layout.Characters.ContainsKey(text[0]))
                        {
                            missing.Add(text);
                        }
                    }
                }
            }

            Assert.True(missing.Count == 0, string.Join(" ", missing));
        }
    }

    public class PlanTests
    {
        private const ushort Enter = 0x1C, Tab = 0x0F, Space = 0x39;

        [Fact]
        public void SpaceTabAndNewlinePressTheirOwnKeys()
        {
            var plan = KeyboardLayout.Danish.Plan(" \t\n");
            Assert.Equal(new[] { Space, Tab, Enter }, plan.Strokes.Select(s => s.ScanCode));
        }

        [Theory]
        [InlineData("a\r\nb")]
        [InlineData("a\nb")]
        [InlineData("a\rb")]
        public void AnyLineEndingIsOneEnter(string text)
        {
            var strokes = KeyboardLayout.EnglishUs.Plan(text).Strokes;
            Assert.Equal(3, strokes.Count);
            Assert.Equal(Enter, strokes[1].ScanCode);
        }

        [Fact]
        public void DecomposedAccentsAreTypedLikeComposedOnes()
        {
            Assert.Equal(KeyboardLayout.French.Plan("é").Strokes, KeyboardLayout.French.Plan("é").Strokes);
        }

        [Fact]
        public void TypographicCharactersBecomePlainOnes()
        {
            KeyboardLayout layout = KeyboardLayout.EnglishUs;
            var plan = layout.Plan("“it’s” – a b…​");
            Assert.Equal(layout.Plan("\"it's\" - a b...").Strokes, plan.Strokes);
            Assert.Empty(plan.Skipped);
        }

        [Fact]
        public void LigaturesAreOnlySplitWhereThereIsNoKey()
        {
            Assert.Equal(KeyboardLayout.French.Plan("coeur").Strokes, KeyboardLayout.French.Plan("cœur").Strokes);
            Assert.Single(KeyboardLayout.Danish.Plan("æ").Strokes);
        }

        [Fact]
        public void AccentsWithoutAKeyAreTypedPlainAndReported()
        {
            var plan = KeyboardLayout.French.Plan("État");
            Assert.Equal(KeyboardLayout.French.Plan("Etat").Strokes, plan.Strokes);
            Assert.Equal(new[] { "É" }, plan.WithoutAccent);
        }

        [Fact]
        public void UntypableCharactersAreReportedNotGuessed()
        {
            var plan = KeyboardLayout.Danish.Plan("a✓\U0001F600b");
            Assert.Equal(2, plan.Strokes.Count);
            Assert.Equal(new[] { "✓", "\U0001F600" }, plan.Skipped);
        }

        [Fact]
        public void LoneSurrogateIsReportedNotThrown()
        {
            Assert.Single(KeyboardLayout.Danish.Plan("\uD83D").Skipped);
        }
    }
}
