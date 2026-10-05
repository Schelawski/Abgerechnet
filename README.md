# Abgerechnet

Mit Abgerechnet schreiben Sie Rechnungen für Ihre selbstständige Arbeit. Sie tragen ein, was Sie geleistet
haben, und Abgerechnet macht daraus eine fertige Rechnung als PDF-Datei. Eine Liste zeigt Ihnen jederzeit,
welche Rechnungen noch offen und welche schon bezahlt sind.

> **Abgerechnet ist kostenlos.** Die einzige offizielle Quelle ist dieses Repository,
> [github.com/Schelawski/Abgerechnet](https://github.com/Schelawski/Abgerechnet). Wenn Sie für Abgerechnet
> bezahlt haben, haben Sie für etwas bezahlt, das es hier kostenlos gibt – und Kopien aus anderen Quellen
> können verändert sein.

> **Stand:** in Entwicklung. Die erste Version entsteht entlang der Issues mit dem Label
> [`v1`](https://github.com/Schelawski/Abgerechnet/issues?q=label%3Av1).

## Ihre Daten bleiben bei Ihnen

Abgerechnet arbeitet nur auf Ihrem Computer. **Ihre Rechnungen, Kunden und Bankdaten werden nirgendwohin
übertragen.** Abgerechnet stellt keine Verbindung zum Internet her. Sie brauchen kein Benutzerkonto, kein Abo
und keine Cloud.

Alles liegt in einem Ordner, den Sie selbst auswählen – als ganz normale Dateien, die Sie sehen, sichern und
mitnehmen können. Wenn Sie Abgerechnet nicht mehr benutzen, bleiben Ihre Rechnungen als PDF-Dateien erhalten.

## Bewusst einfach

Abgerechnet kann nur das, was man für Rechnungen wirklich braucht. Es gibt keine Buchhaltung, keine Anbindung
an Banken, Steuerprogramme oder andere Systeme und keine komplizierten Einstellungen. Sie sollen in wenigen
Minuten Ihre erste Rechnung schreiben können – und am Monatsende mit ein paar Klicks die nächste.

## Download

**[Abgerechnet.exe herunterladen](https://github.com/Schelawski/Abgerechnet/releases/latest/download/Abgerechnet.exe)** –
immer die neueste Version. Sie müssen nichts installieren: Datei speichern und starten. Alle Versionen und ihre
Änderungen finden Sie unter [Releases](https://github.com/Schelawski/Abgerechnet/releases).

Beim ersten Start zeigt Windows vielleicht „Der Computer wurde durch Windows geschützt“. Klicken Sie dann auf
„Weitere Informationen“ und „Trotzdem ausführen“.

Wer den Download prüfen möchte: Zu jeder Version gibt es die Datei `Abgerechnet.exe.sha256`. In PowerShell zeigt
`Get-FileHash Abgerechnet.exe` denselben Wert.

## Was Abgerechnet kann

- **Ihre Daten einmal eintragen:** Name, Anschrift, Steuernummer, Bankverbindung. Auch als Kleinunternehmer
  nach § 19 UStG.
- **Kunden** mit ihrer Anschrift speichern.
- **Rechnungen schreiben:** Positionen mit Menge, Einheit und Preis. Die Summen rechnen sich sofort.
- **Monatsabschluss in Sekunden:** Rechnung vom Vormonat kopieren, Stunden ändern, fertig.
- **PDF erzeugen** – mit Vorschau und einem Hinweis, wenn eine wichtige Angabe fehlt.
- **Überblick:** Was ist offen, was ist bezahlt? Ein dezenter Hinweis erinnert an Rechnungen, die schon länger offen
  sind – mit einem Klick als bezahlt erfasst. Rechnungsnummern bleiben lückenlos.
- **Aussehen nach Wunsch:** drei fertige Vorlagen, Ihr eigenes Logo, Farben und Schriften anpassbar – auch mit
  Hilfe einer KI, ganz ohne HTML-Kenntnisse.
- **Hilfe in einfacher Sprache** direkt im Programm (Taste **F1**).
- Eine einzige Datei `Abgerechnet.exe` – keine Installation, kein .NET nötig.

## So fangen Sie an

1. `Abgerechnet.exe` starten. Ein kurzer Assistent hilft Ihnen beim Einrichten: Ordner für Ihre Rechnungen wählen,
   Ihre Daten eintragen, Aussehen und Logo wählen. Auf Wunsch kopiert er Abgerechnet aus dem Download-Ordner in Ihren
   Benutzerordner und legt Verknüpfungen auf dem Desktop und im Startmenü an.
2. **Erste Rechnung erstellen** klicken, Kunde und Positionen eintragen.
3. **PDF erzeugen…** und **PDF speichern** – die Rechnung liegt im Ordner `PDF`. Per E-Mail an Ihren Kunden
   schicken, fertig.

Alles Weitere erklärt die Hilfe im Programm.

## Wo liegen meine Daten?

Alles steht in Ihrem Rechnungsordner:

```
Rechnungsordner/
├── abgerechnet.json      Ihre Angaben aus „Meine Daten“
├── kunden.json           Ihre Kunden
├── rechnungen.json       Ihre Rechnungen mit allen Positionen
├── Vorlagen/             das Aussehen Ihrer Rechnungen und Ihr Logo
└── PDF/                  die fertigen Rechnungen
```

- **Sichern:** Legen Sie den Ordner in OneDrive oder Dropbox – oder kopieren Sie ihn ab und zu auf einen
  USB-Stick. Rechnungen müssen Sie in der Regel zehn Jahre aufbewahren.
- **Umziehen:** Ordner und `Abgerechnet.exe` auf den neuen Computer kopieren, fertig.
- **Sicher gespeichert:** Vor jedem Speichern bleibt die vorige Fassung als `*.bak.json` erhalten. Ist eine Datei
  beschädigt, meldet Abgerechnet das und verändert sie nicht.

Abgerechnet merkt sich außerdem, welcher Ordner zuletzt offen war und wo das Fenster stand – in
`Abgerechnet.settings.json` neben der exe (oder in `%APPDATA%\Abgerechnet\`, wenn dort nicht geschrieben werden
darf).

## Das Aussehen der Rechnung

Wie Ihre Rechnung aussieht, bestimmt eine Vorlage im Ordner `Vorlagen`. Drei liegen bei: **Klassisch**,
**Modern** und **Schlicht**. Ihr Logo legen Sie einfach als `logo.png` in diesen Ordner.

Eine Vorlage ist eine HTML-Datei. Farben und Schriften ändern Sie oben in der Datei. Abgerechnet setzt Ihre
Daten an Platzhaltern wie `{{absender_firma}}` oder `{{brutto}}` ein. Die Positionen und Summen kommen als
fertige Tabellen (`{{positionen_tabelle}}`, `{{summen_tabelle}}`), deren Aussehen Sie per CSS bestimmen.

Kein HTML-Wissen? Die Hilfe im Programm enthält unter „Vorlage mit KI anpassen“ einen fertigen Text für eine
KI wie ChatGPT oder Claude. Ein Klick kopiert ihn zusammen mit Ihrer Vorlage. Die Vorlage enthält keine
persönlichen Daten – nur Platzhalter.

Die vollständige Liste aller Platzhalter und CSS-Klassen steht in der Hilfe („Vorlage anpassen“). Geht etwas
schief, holt **Vorlagen → Original wiederherstellen** die ursprüngliche Vorlage zurück.

## Steuern

Abgerechnet ersetzt keine Steuerberatung. Für Rechnungen zwischen Unternehmen wird in Deutschland ab 2027/2028
schrittweise die E-Rechnung Pflicht (Kleinunternehmer sind ausgenommen). Abgerechnet erzeugt derzeit PDF-Rechnungen;
die E-Rechnung ist für eine spätere Version geplant. Einzelheiten stehen in der Hilfe („Steuern und E-Rechnung“).

## Voraussetzungen

- Windows 10 oder 11, 64 Bit.
- Microsoft Edge WebView2 Runtime (auf Windows 10/11 in der Regel vorinstalliert) – für die PDF-Erzeugung.

## Build

Benötigt das [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) unter Windows.

```powershell
dotnet build            # App und Tests bauen
dotnet test             # Unit-Tests ausführen
dotnet publish src/Abgerechnet -c Release
```

`dotnet publish` erzeugt eine einzige selbstständige exe (die .NET-Laufzeit ist eingebettet):

```
src\Abgerechnet\bin\Release\net10.0-windows\win-x64\publish\Abgerechnet.exe
```

### Projektaufbau

```
Abgerechnet.sln
src/Abgerechnet/
  Core/    UI-unabhängige Logik: Datenmodell, Speicherung, Berechnung, Vorlagen
  UI/      WinForms: Fenster und Dialoge, alle Texte zentral in UiText
  Help/    Hilfetexte (help.de.md), in die exe eingebettet
tests/Abgerechnet.Tests/   xUnit-Tests für Core
docs/MANUAL-TESTING.md     manueller Testplan
.github/workflows/         build.yml (jeder Push und Pull Request), release.yml (Versions-Tags)
```

### Branches

- `develop` – laufende Entwicklung, Ziel aller Pull Requests.
- `master` – Stand der letzten veröffentlichten Version.

### Automatische Builds

Jeder Push auf `develop` oder `master` und jeder Pull Request wird mit GitHub Actions gebaut und getestet
([build.yml](.github/workflows/build.yml)). Bei Pushes bleibt die gebaute `Abgerechnet.exe` zwei Wochen lang
als Test-Build erhalten (Actions → Lauf → Artifacts).

### Release erstellen

1. Die Einträge unter `## [Unreleased]` in [CHANGELOG.md](CHANGELOG.md) in einen neuen Abschnitt
   `## [1.2.0]` verschieben (Version ohne „v“) und committen.
2. Den Commit taggen und den Tag pushen:
   ```powershell
   git tag v1.2.0
   git push origin v1.2.0
   ```
3. [release.yml](.github/workflows/release.yml) baut und testet, schreibt die Version in `Abgerechnet.exe`
   (Fenstertitel, Dateieigenschaften) und erstellt das GitHub-Release mit `Abgerechnet.exe`,
   `Abgerechnet.exe.sha256` und dem Changelog-Abschnitt als Release-Notes.

Ein Tag mit Zusatz wie `v1.3.0-beta.1` wird zur Vorabversion und ist über den `latest/download`-Link nicht
erreichbar. Fehlt der Changelog-Abschnitt für die Version, bricht das Release ab, bevor etwas veröffentlicht
wird.

## Mitmachen

Fehlermeldungen, Ideen und Pull Requests sind willkommen – bitte zuerst [CONTRIBUTING.md](CONTRIBUTING.md)
lesen.

## Lizenz

© 2026 A. Schelawski – [GNU General Public License v3.0](LICENSE)

Fremdkomponenten:
- [Microsoft Edge WebView2 SDK](https://www.nuget.org/packages/Microsoft.Web.WebView2) (© Microsoft, BSD-Lizenz) –
  zeigt die Vorschau an und erzeugt das PDF; die WebView2 Runtime selbst ist Teil von Windows.
