# KeyPuncher

For anyone working in a Citrix desktop or vSphere web console where copy/paste is blocked: KeyPuncher types the text on your clipboard into the remote window, as if you were typing it on your keyboard.

## Download

[Download the latest version](https://github.com/BM-TechX/KeyPuncher/releases/latest/download/KeyPuncher-win-x64.zip), unzip it and run `KeyPuncher.exe`. Nothing needs to be installed. Windows may warn about an unknown publisher, because the program is not signed.

## How to use it

1. Start KeyPuncher on your own machine and choose the keyboard layout **of the remote desktop**: Danish, English (US) or French (AZERTY).
2. Copy the text.
3. Click into the remote window where the text should go.
4. Press `Ctrl+Alt+V` (`Ctrl+Alt+G` and `Ctrl+Alt+L` do the same, in case another program uses that shortcut) and let go of the keys. Typing starts when you let go.

Leave the keyboard and mouse alone until it has finished. Line breaks are typed as Enter, so multi-line text can run commands in a console.

## Good to know

- **Layout:** if the wrong characters come out, the selected layout does not match the remote desktop.
- **Characters with no key:** curly quotes, long dashes and similar are typed as their plain versions (`"`, `-`). A letter whose accent the layout cannot type loses the accent, such as `É` on French AZERTY. Anything else, such as emoji, is left out. KeyPuncher shows a warning in its window when characters lost an accent or were left out.
- **Hide text:** this keeps the clipboard text and the characters in those warnings out of the KeyPuncher window, which is useful for passwords.
