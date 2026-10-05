# Abgerechnet

Abgerechnet ist eine kleine Windows-Anwendung für Freiberufler: Rechnung anlegen, Positionen erfassen,
PDF erzeugen – fertig. Das Aussehen der Rechnung bestimmen Sie selbst über eine HTML-Vorlage (Logo, Farben,
Schriften). Alle Daten liegen als lesbare Dateien in einem Ordner Ihrer Wahl, ohne Datenbank und ohne Cloud.

> **Abgerechnet ist kostenlos.** Die einzige offizielle Quelle ist dieses Repository,
> [github.com/Schelawski/Abgerechnet](https://github.com/Schelawski/Abgerechnet). Wenn Sie für Abgerechnet
> bezahlt haben, haben Sie für etwas bezahlt, das es hier kostenlos gibt – und Kopien aus anderen Quellen
> können verändert sein.

> **Stand:** in Entwicklung. Die erste Version entsteht entlang der Issues mit dem Label
> [`v1`](https://github.com/Schelawski/Abgerechnet/issues?q=label%3Av1).

## Download

**[Abgerechnet.exe herunterladen](https://github.com/Schelawski/Abgerechnet/releases/latest/download/Abgerechnet.exe)** –
immer die neueste Version. Keine Installation nötig: einfach starten. Alle Versionen und ihre Änderungen:
[Releases](https://github.com/Schelawski/Abgerechnet/releases).

Jedes Release enthält zusätzlich `Abgerechnet.exe.sha256`. Zum Prüfen des Downloads in PowerShell
`Get-FileHash Abgerechnet.exe` ausführen und den Wert vergleichen. Beim ersten Start zeigt Windows eventuell
„Der Computer wurde durch Windows geschützt“ – dann „Weitere Informationen“ → „Trotzdem ausführen“.

## Funktionen (geplant für Version 1)

- **Ein Ordner für alles:** `rechnungen.json`, `kunden.json`, `abgerechnet.json`, Vorlagen und PDFs liegen
  in einem Rechnungsordner – ideal zum Sichern über OneDrive oder Dropbox.
- **Meine Daten:** Absender, Steuernummer, Bankverbindung, Kleinunternehmer nach § 19 UStG, Nummernkreis.
- **Kunden:** eine minimale Kundenliste mit Rechnungsadresse.
- **Rechnungsliste** mit Offen, Bezahlt und Gesamt; Stornieren statt Löschen, damit der Nummernkreis
  lückenlos bleibt.
- **Rechnungsformular** mit Menge und Einheit, live berechneten Summen und „Als neue Rechnung kopieren“.
- **Eigene HTML-Vorlage** mit Platzhaltern, drei mitgelieferte Vorlagen (Klassisch, Modern, Schlicht).
- **PDF-Erzeugung** über WebView2 mit Vorschau.
- **Erinnerung an offene Rechnungen** und schnelles Erfassen des Zahlungseingangs.
- **Hilfe** in einfacher Sprache, inklusive Anleitung „Vorlage anpassen“ mit KI-Prompt.
- Eine einzige, selbstständige `Abgerechnet.exe` – keine .NET-Installation nötig.

## Ihre Daten

Abgerechnet speichert alles in einem **Rechnungsordner**, den Sie beim ersten Start wählen und später über
**Datei → Rechnungsordner wechseln…** ändern können:

```
Rechnungsordner/
├── abgerechnet.json      Ihre Absenderdaten und Rechnungs-Einstellungen
├── kunden.json           Kunden
├── rechnungen.json       Rechnungen mit ihren Positionen
├── Vorlagen/             HTML-Vorlagen und Logo
└── PDF/                  erzeugte Rechnungen
```

- Die Dateien sind lesbares JSON (Datum als `JJJJ-MM-TT`, Beträge mit Punkt) mit einem Feld `"version"`.
- Gespeichert wird sicher: erst in eine temporäre Datei, dann wird ersetzt. Die vorige Fassung bleibt als
  `*.bak.json` erhalten (z. B. `rechnungen.bak.json`).
- Ist eine Datei beschädigt, meldet Abgerechnet das und verändert sie nicht.
- Legen Sie den Ordner zum Beispiel in OneDrive oder Dropbox, dann sind Ihre Rechnungen automatisch gesichert.

Ihre eigenen Angaben – Absender, Steuernummer, Bankverbindung, Kleinunternehmer-Regelung, Zahlungsziel,
Nummernkreis und das Muster für den PDF-Dateinamen – pflegen Sie unter **Datei → Meine Daten…**. Sie stehen
in `abgerechnet.json` in den Abschnitten `absender`, `bank`, `rechnung` und `neuePosition`.

Ihre Kunden verwalten Sie unter **Datei → Kunden…**. Eine Rechnung merkt sich beim Erzeugen des PDFs die
Anschrift des Kunden (Feld `empfaenger` in `rechnungen.json`), damit spätere Änderungen am Kunden bereits
gestellte Rechnungen nicht verändern.

Welcher Ordner zuletzt geöffnet war und wo das Fenster stand, merkt sich Abgerechnet in
`Abgerechnet.settings.json` neben der exe (oder in `%APPDATA%\Abgerechnet\`, wenn der Ordner der exe
schreibgeschützt ist).

## Rechnungsvorlage

Das Aussehen der Rechnung bestimmt eine HTML-Datei im Ordner `Vorlagen/`. Abgerechnet setzt Platzhalter in
doppelten geschweiften Klammern ein, z. B. `{{absender_firma}}`, `{{rechnung_nummer}}` oder `{{brutto}}`:

- **Text-Platzhalter** für Absender, Kunde, Rechnung und Beträge (`{{kunde_anschrift}}` liefert die ganze
  Anschrift, `{{faellig_am}}` das Fälligkeitsdatum, `{{steuerhinweis}}` den § 19-Hinweis bei Kleinunternehmern).
- **Blöcke** mit fertigem HTML: `{{positionen_tabelle}}` (Tabelle `.positionen` mit den Spalten `.pos-nr`,
  `.pos-zeitraum`, `.pos-beschreibung` mit `.pos-detail`, `.pos-menge`, `.pos-einheit`, `.pos-preis`, `.pos-betrag`)
  und `{{summen_tabelle}}` (Tabelle `.summen` mit den Zeilen `.netto`, `.ust`, `.brutto`). Rahmen, Abstände und
  ausgeblendete Spalten regeln Sie per CSS.
- Relative Pfade (z. B. `logo.png`) beziehen sich auf den Ordner der Vorlage. Platzhalter in HTML-Kommentaren
  werden nicht ersetzt; unbekannte Platzhalter bleiben stehen und werden gemeldet.

Die vollständige Liste der Platzhalter steht in der Hilfe der App.

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
