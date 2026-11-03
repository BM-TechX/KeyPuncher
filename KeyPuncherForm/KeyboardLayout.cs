using System.Text;

namespace KeyPuncherForm
{
    /// <summary>One physical key press, identified by scan code so the result does not depend on the local keyboard layout.</summary>
    public readonly record struct KeyStroke(ushort ScanCode, bool Shift = false, bool AltGr = false);

    /// <summary>What to press for a piece of text, and what could not be typed faithfully.</summary>
    public sealed record TypingPlan(
        IReadOnlyList<KeyStroke> Strokes,
        IReadOnlyList<string> WithoutAccent,
        IReadOnlyList<string> Skipped);

    /// <summary>
    /// Describes the layout of the machine that RECEIVES the keystrokes (e.g. the Citrix desktop):
    /// which physical key, with which modifiers, produces each character there.
    /// </summary>
    public sealed class KeyboardLayout
    {
        private const ushort Enter = 0x1C, Tab = 0x0F, Space = 0x39;

        // Physical keys, left to right. Row strings below line up with these; ' ' means "nothing on this level".
        private static readonly ushort[] NumberRow = { 0x29, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D };
        private static readonly ushort[] TopRow = { 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B };
        private static readonly ushort[] HomeRow = { 0x1E, 0x1F, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x2B };
        private static readonly ushort[] BottomRow = { 0x56, 0x2C, 0x2D, 0x2E, 0x2F, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35 };

        // The combining mark each dead key applies. Must stay above the layouts, which use it while initializing.
        private static readonly IReadOnlyDictionary<char, string> Accents = new Dictionary<char, string>
        {
            ['´'] = "́", ['`'] = "̀", ['^'] = "̂", ['¨'] = "̈", ['~'] = "̃",
        };

        private static readonly IReadOnlyDictionary<char, string> Substitutions = new Dictionary<char, string>
        {
            // Typographic characters word processors and web pages put on the clipboard.
            ['‘'] = "'", ['’'] = "'", ['‚'] = "'", ['‛'] = "'", ['ʼ'] = "'", ['′'] = "'",
            ['“'] = "\"", ['”'] = "\"", ['„'] = "\"", ['‟'] = "\"", ['″'] = "\"",
            ['«'] = "\"", ['»'] = "\"", ['‹'] = "'", ['›'] = "'",
            ['‐'] = "-", ['‑'] = "-", ['‒'] = "-", ['–'] = "-", ['—'] = "-", ['―'] = "-", ['−'] = "-",
            ['…'] = "...", ['•'] = "*",
            [' '] = " ", [' '] = " ", [' '] = " ", [' '] = " ", [' '] = " ", [' '] = " ", [' '] = " ",
            // Invisible: nothing to type.
            ['­'] = "", ['​'] = "", ['‌'] = "", ['‍'] = "", ['⁠'] = "", ['﻿'] = "",
            // Ligatures, used only on layouts without a key for them.
            ['œ'] = "oe", ['Œ'] = "OE", ['æ'] = "ae", ['Æ'] = "AE", ['ß'] = "ss",
        };

        private readonly Dictionary<char, KeyStroke[]> _map = new();
        private readonly Dictionary<KeyStroke, char> _deadKeys = new();
        private readonly List<(KeyStroke Key, char Literal, string Letters)> _deadKeyDefinitions = new();

        private KeyboardLayout(string name, string windowsLayoutId)
        {
            Name = name;
            WindowsLayoutId = windowsLayoutId;
        }

        public static KeyboardLayout Danish { get; } = new KeyboardLayout("Danish", "00000406")
            .Rows(NumberRow, "½1234567890+´", "§!\"#¤%&/()=?`", "  @£$€ {[]} |")
            .Rows(TopRow, "qwertyuiopå¨", "QWERTYUIOPÅ^", "  €        ~")
            .Rows(HomeRow, "asdfghjklæø'", "ASDFGHJKLÆØ*", "            ")
            .Rows(BottomRow, "<zxcvbnm,.-", ">ZXCVBNM;:_", "\\      µ   ")
            .Dead(new KeyStroke(0x0D), '´', "aeiouyAEIOUY")
            .Dead(new KeyStroke(0x0D, Shift: true), '`', "aeiouAEIOU")
            .Dead(new KeyStroke(0x1B), '¨', "aeiouyAEIOU")
            .Dead(new KeyStroke(0x1B, Shift: true), '^', "aeiouAEIOU")
            .Dead(new KeyStroke(0x1B, AltGr: true), '~', "anoANO")
            .Build();

        public static KeyboardLayout EnglishUs { get; } = new KeyboardLayout("English (US)", "00000409")
            .Rows(NumberRow, "`1234567890-=", "~!@#$%^&*()_+", "             ")
            .Rows(TopRow, "qwertyuiop[]", "QWERTYUIOP{}", "            ")
            .Rows(HomeRow, "asdfghjkl;'\\", "ASDFGHJKL:\"|", "            ")
            .Rows(BottomRow, " zxcvbnm,./", " ZXCVBNM<>?", "           ")
            .Build();

        public static KeyboardLayout French { get; } = new KeyboardLayout("French (AZERTY)", "0000040C")
            .Rows(NumberRow, "²&é\"'(-è_çà)=", " 1234567890°+", "  ~#{[|`\\^@]}")
            .Rows(TopRow, "azertyuiop^$", "AZERTYUIOP¨£", "  €        ¤")
            .Rows(HomeRow, "qsdfghjklmù*", "QSDFGHJKLM%µ", "            ")
            .Rows(BottomRow, "<wxcvbn,;:!", ">WXCVBN?./§", "           ")
            .Dead(new KeyStroke(0x1A), '^', "aeiouAEIOU")
            .Dead(new KeyStroke(0x1A, Shift: true), '¨', "aeiouyAEIOU")
            .Dead(new KeyStroke(0x03, AltGr: true), '~', "anoANO")
            .Dead(new KeyStroke(0x08, AltGr: true), '`', "aeiouAEIOU")
            .Build();

        public static IReadOnlyList<KeyboardLayout> All { get; } = new[] { Danish, EnglishUs, French };

        public string Name { get; }

        /// <summary>The Windows keyboard layout this table mirrors.</summary>
        public string WindowsLayoutId { get; }

        /// <summary>Every character this layout can type directly or through a dead key.</summary>
        public IReadOnlyDictionary<char, KeyStroke[]> Characters => _map;

        /// <summary>The dead keys of this layout and the character each one types when followed by Space.</summary>
        public IReadOnlyDictionary<KeyStroke, char> DeadKeys => _deadKeys;

        public static KeyboardLayout ForLanguage(string twoLetterLanguage) => twoLetterLanguage switch
        {
            "da" => Danish,
            "fr" => French,
            _ => EnglishUs,
        };

        public override string ToString() => Name;

        public TypingPlan Plan(string text)
        {
            var strokes = new List<KeyStroke>(text.Length);
            var withoutAccent = new List<string>();
            var skipped = new List<string>();

            foreach (Rune rune in Normalize(text).EnumerateRunes())
            {
                if (rune.IsBmp)
                {
                    AddCharacter((char)rune.Value, strokes, withoutAccent, skipped);
                }
                else
                {
                    skipped.Add(rune.ToString());
                }
            }

            return new TypingPlan(strokes, withoutAccent, skipped);
        }

        private void AddCharacter(char c, List<KeyStroke> strokes, List<string> withoutAccent, List<string> skipped)
        {
            if (_map.TryGetValue(c, out KeyStroke[]? known))
            {
                strokes.AddRange(known);
            }
            else if (Substitutions.TryGetValue(c, out string? substitute) && substitute.All(_map.ContainsKey))
            {
                foreach (char s in substitute)
                {
                    strokes.AddRange(_map[s]);
                }
            }
            else if (StripAccent(c) is char plain && _map.TryGetValue(plain, out KeyStroke[]? plainKeys))
            {
                strokes.AddRange(plainKeys);
                withoutAccent.Add(c.ToString());
            }
            else
            {
                skipped.Add(c.ToString());
            }
        }

        /// <summary>Line endings become one Enter each, and decomposed accents (e + U+0301) are recombined.</summary>
        private static string Normalize(string text)
        {
            var clean = new StringBuilder(text.Length);
            foreach (Rune rune in text.EnumerateRunes())
            {
                clean.Append(rune.ToString()); // Lone surrogates become U+FFFD, which Normalize accepts.
            }

            return clean.ToString().Normalize(NormalizationForm.FormC)
                .Replace("\r\n", "\n")
                .Replace('\r', '\n');
        }

        private static char? StripAccent(char c)
        {
            string decomposed = c.ToString().Normalize(NormalizationForm.FormD);
            return decomposed.Length > 1 && char.IsLetter(decomposed[0]) ? decomposed[0] : null;
        }

        private KeyboardLayout Rows(ushort[] keys, string normal, string shifted, string altGr)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                Add(normal[i], new KeyStroke(keys[i]));
                Add(shifted[i], new KeyStroke(keys[i], Shift: true));
                Add(altGr[i], new KeyStroke(keys[i], AltGr: true));
            }

            return this;
        }

        private void Add(char c, KeyStroke stroke)
        {
            if (c != ' ')
            {
                _map[c] = new[] { stroke };
            }
        }

        /// <summary>Declares a dead key: pressed before a letter it adds an accent, pressed before Space it types itself.</summary>
        private KeyboardLayout Dead(KeyStroke key, char literal, string letters)
        {
            _deadKeys[key] = literal;
            _deadKeyDefinitions.Add((key, literal, letters));
            return this;
        }

        private KeyboardLayout Build()
        {
            foreach (var (key, literal, _) in _deadKeyDefinitions)
            {
                // The row strings list dead keys too, but pressing one alone types nothing yet.
                if (_map[literal][0] == key)
                {
                    _map[literal] = new[] { key, new KeyStroke(Space) };
                }
            }

            foreach (var (key, literal, letters) in _deadKeyDefinitions)
            {
                foreach (char letter in letters)
                {
                    string composed = (letter + Accents[literal]).Normalize(NormalizationForm.FormC);
                    _map.TryAdd(composed[0], new[] { key }.Concat(_map[letter]).ToArray());
                }
            }

            _map[' '] = new[] { new KeyStroke(Space) };
            _map['\t'] = new[] { new KeyStroke(Tab) };
            _map['\n'] = new[] { new KeyStroke(Enter) };
            return this;
        }
    }
}
