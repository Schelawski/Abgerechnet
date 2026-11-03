# Avalonia: Analyse und Migrationsplan

Stand: 17.10.2026. Untersucht: lokaler Branch `develop`, Commit `05f1f12`.
Status: Planung, noch keine Umsetzung oder Paketinstallation und noch kein GitHub-Issue.

## Empfehlung

Abgerechnet eignet sich für eine schrittweise Migration zu Avalonia. Die fachliche Logik ist bereits überwiegend von der Oberfläche getrennt. Empfohlen wird ein eigenständiges Avalonia-Frontend mit XAML und MVVM, zunächst für **Windows 10/11 x64 und .NET 10**. Die bisherige WinForms-Anwendung bleibt während der Entwicklung als Vergleich und Rückfallmöglichkeit baubar; beide Frontends werden nicht gleichzeitig auf denselben Rechnungsordner angesetzt.

Avalonia ersetzt die Fenster und Controls, modernisiert aber nicht automatisch die Gestaltung. Deshalb gehören ein kleines Gestaltungssystem, abgestimmte Referenzansichten und visuelle Regressionstests zum Vorhaben. Rechnungsdaten, HTML-Vorlagen und die bestehende PDF-Darstellung sollen kompatibel bleiben.

Die erste technische Entscheidung betrifft die WebView2-Integration. Diese muss vor dem breiten Umbau durch einen Prototyp abgesichert werden. Ein Wechsel der PDF-Engine oder zusätzliche Betriebssysteme gehören nicht zum ersten Migrationsumfang.

## 1. Befund im Repository

| Bereich | Konkreter Befund | Konsequenz |
| --- | --- | --- |
| Anwendung | `src/Abgerechnet/Abgerechnet.csproj`: `net10.0-windows`, WinForms, WebView2, selbstständige einzelne `Abgerechnet.exe` | Eigenes Avalonia-Startprojekt ergänzen; Auslieferung explizit nachweisen. |
| Start | `Program.cs`: Einrichtung, Ordneröffnung, `--neue-rechnung`, Fehlerdialoge und WinForms-Nachrichtenschleife | Startablauf in Avalonia-Lebenszyklus übertragen, inklusive Abbruch und Neustart nach lokaler Kopie. |
| UI | 23 C#-Dateien mit insgesamt ca. 5.100 Zeilen unter `UI`, einschließlich Texte und Helfer | Überschaubarer, aber vollständiger Neuaufbau der Ansichten; keine automatische Konvertierung von WinForms-Layouts. |
| Hauptabläufe | `MainForm.cs`, `RechnungenView.cs`, `RechnungForm.cs`, `ZahlungseingangDialog.cs` enthalten neben Layout auch Ablauf- und Speicherlogik | Abläufe gezielt in testbare Services/ViewModels verschieben. |
| Fachliche Basis | `Core/Model`, `Core/Storage`, `Core/Vorlagen`, `Core/Help` und Einrichtungsregeln | Großteils wiederverwenden; JSON-Schema, IDs, Rundung und Rechnungsnummern erhalten. |
| Verbleibende Plattformbindung | `Core/Vorlagen/Logo.cs` verwendet `System.Drawing`; `Core/Einrichtung/LocalInstall.cs` nutzt Windows-Shell/COM | Core ist noch nicht vollständig plattformneutral. Windows-Funktionen in Adapter verschieben. |
| PDF | `UI/PdfDrucker.cs` ist direkt an `Microsoft.Web.WebView2.WinForms.WebView2` gebunden | PDF-Renderer hinter einer Schnittstelle isolieren; Hosting ist der größte technische Unsicherheitsfaktor. |
| PDF-Ablauf | `VorschauForm.cs` erzeugt ein temporäres PDF, zeigt dieses an und kopiert genau diese Datei beim Speichern; erst dann werden Status und Empfänger festgeschrieben | Diese Reihenfolge bewahren. Kein zweites Rendern beim Speichern. |
| Zweite WebView-Nutzung | `UI/Einrichtung/VorlageSeite.cs` erzeugt ebenfalls echte PDF-Vorschauen | Den Prototyp und Adapter für beide Einsatzorte auslegen. |
| Gestaltung | `UiStyle.cs`, `FormLayout.cs`, `Kachel.cs`, viele explizite Größen und DPI-Helfer | Farben weiterverwenden, Abstände und Layout in zentrale Avalonia-Ressourcen überführen. |
| Texte/Hilfe | Zentrales `UI/UiText.cs`, eingebettetes `Help/help.de.md`; `HelpForm.cs` verwendet RichTextBox und native Interop | Deutsche Texte zentral halten; vorhandenes HelpDocument-Modell mit Avalonia-Textcontrols darstellen. |
| Einstellungen | `AppSettings.WindowPlacement` speichert Bildschirmkoordinaten und Größen in Pixeln | Pixel und Avalonia-DIPs bewusst umrechnen; Monitorwechsel und alte Einstellungen testen. |
| Tests | xUnit, bisher Referenz auf die gesamte WinForms-exe; `HelpTests.cs` nutzt `UiText`, Einrichtungstests auch `System.Drawing` | Bei Projektaufteilung Testreferenzen und Zuordnung anpassen; bestehende Abdeckung erhalten. |
| CI/Release | Windows-Runner; Test und Publish; Artefakt heißt `Abgerechnet.exe`; `Directory.Build.targets` entfernt WebView2-WPF-Referenz | Workflows, Assembly-Version und WebView-Buildregel bei Umstellung gezielt prüfen. |

### Überprüfter Ausgangszustand

`dotnet test --configuration Release --nologo` wurde lokal erfolgreich ausgeführt: **200 bestanden, 0 fehlgeschlagen, 0 übersprungen**. Dabei wurden Anwendung und Tests gebaut. Das ist kein Nachweis für gerenderte PDFs oder das Aussehen der Fenster: `PdfTests.cs` prüft derzeit insbesondere Modellzustände nach PDF-Erzeugung. Einen manuellen Testplan gibt es bereits in `docs/MANUAL-TESTING.md` mit elf Bereichen.

Die Analyse beruht auf Quellcode, Build/Testlauf und offizieller Avalonia-Dokumentation. Die bestehende Anwendung wurde für diese Planung nicht visuell begutachtet; Gestaltungsdetails und endgültige Mindestgrößen sind im ersten Arbeitspaket festzulegen.

## 2. Zielarchitektur

Vorgeschlagene Struktur; neue Projekte und Namen sind Planungswerte:

```text
src/Abgerechnet/                    bestehendes WinForms-Frontend während der Migration
src/Abgerechnet.Core/               Modelle, Berechnung, JSON, Vorlagen, HelpDocument
src/Abgerechnet.Application/        Anwendungsabläufe und Schnittstellen, ohne UI-Controls
src/Abgerechnet.Platform.Windows/   PDF/WebView2, Logo-Konvertierung, lokale Installation, Shell
src/Abgerechnet.Avalonia/           Start, Views, ViewModels, Themes, Dialoge, Ressourcen
tests/Abgerechnet.Tests/            bisherige fachliche Tests, passend neu zugeordnet
tests/Abgerechnet.Ui.Tests/         ViewModels, Headless-Interaktion, Layout und Bildvergleich
tests/Abgerechnet.Windows.Tests/    echte WebView2-/PDF-Integration; Desktop-Smokes separat gefiltert
```

Abhängigkeiten: Core kennt weder Avalonia noch WinForms. Application verwendet Core und definiert die benötigten Verträge. Beide Frontends verwenden dieselben Anwendungsabläufe; Windows-Adapter implementieren die Verträge. Die Startprojekte verdrahten die konkreten Implementierungen. Neue Windows-Adapter können zunächst `net10.0-windows` verwenden; der plattformneutrale Teil zielt auf `net10.0`.

- MVVM mit `INotifyPropertyChanged` und Commands; `CommunityToolkit.Mvvm` als kleine optionale Hilfe prüfen. Kein zusätzlicher großer Navigations- oder DI-Stack erforderlich.
- Compiled Bindings mit `x:DataType` verwenden. Code-behind bleibt für Fokus, Fensterlebenszyklus und natives Hosting zulässig; fachliche Entscheidungen bleiben außerhalb der Views.
- Verträge nur für konkrete Abhängigkeiten einführen: PDF-Rendering/Vorschau, Dateiauswahl, Rückfragen, Clipboard, Shell und lokale Installation. Keine vollständige neue Repository-Abstraktion um die bereits testbare JSON-Speicherung bauen.
- Rechnungsbearbeitung arbeitet wie bisher mit einer bearbeitbaren Kopie. Abbrechen darf keine gebundenen Originaldaten verändern.
- `UiText` weiter als zentrale Textquelle verwenden, bei Bedarf in die gemeinsame Präsentationsschicht verschieben. WinForms-Mnemonics wie `&` müssen für Avalonia geprüft/übersetzt werden. Hilfetext-Konsistenztests erhalten.
- Eingebettete Hilfe und Vorlagen bei der Extraktion bewusst zuordnen: Ressourcenbezeichnungen und ladende Assembly prüfen. `AppVersion` liest aktuell die eigene Assembly; Release-Version muss nach Aufteilung weiterhin korrekt im Fenstertitel und in den Dateieigenschaften erscheinen.

### PDF-Entscheidung vor der UI-Migration

**Bevorzugte Ausgangsrichtung:** bestehende WebView2-Engine behalten und über einen Windows-Adapter in Avalonia hosten. Avalonia unterstützt native HWND-Controls über `NativeControlHost`. Das ist keine fertige Garantie für den vorhandenen WinForms-Wrapper: Handle-Erzeugung, STA/UI-Thread, Größenänderung, Fokus, Freigabe und Druck müssen praktisch bewiesen werden. Native Controls haben Einschränkungen bei Überlagerungen, Transparenz und Transformationen; Ladehinweise deshalb möglichst außerhalb der nativen Fläche vorsehen. Siehe [Windows-Interop](https://docs.avaloniaui.net/docs/platform-specific-guides/windows).

**Alternative im Prototyp:** offizielles `Avalonia.Controls.WebView`/`NativeWebView`. Die Dokumentation nennt unter Windows WebView2 und PDF-Ausgabe; daraus folgt noch keine Gleichwertigkeit mit der bisherigen Integration. Vor Auswahl Paketversion, Lizenz und Weitergaberechte einschließlich etwaiger Plus-/Pro-/Community-Bedingungen, Offlinebetrieb, lokalen Ressourcen und konkrete Druckoptionen prüfen. Kostenloses Abgerechnet und eine Open-Source-Framework-Lizenz garantieren keine passende Lizenz jedes Zusatzcontrols. Siehe [WebView-Einbettung](https://docs.avaloniaui.net/docs/app-development/embedding-web-content), [NativeWebView-Funktionen](https://docs.avaloniaui.net/controls/web/nativewebview) und [Avalonia-Komponenten](https://docs.avaloniaui.net/tools/).

**Vorübergehende Ausweichlösung:** vorhandenen WinForms-PDF-Dialog hinter einem Service weiterverwenden, falls natives Hosting noch nicht stabil ist. Nur als befristete Übergangsstufe mit geprüftem Owner, Fokus, Modalität und Nachrichtenschleife; nicht als Endzustand der UI-Migration.

Ein Einbetten von Avalonia in die bestehende WinForms-Shell ist ebenfalls offiziell möglich. Für dieses Projekt wird ein eigenes Frontend bevorzugt, weil dadurch der spätere vollständige Wechsel klarer und die Tests unabhängiger werden. Eine gemischte Shell erhöht insbesondere den Aufwand für Tastatur- und Fensterintegration. Die Wahl ist vor Umsetzung im Issue festzuhalten.

## 3. Gestaltung und vollständiger Funktionsumfang

Mit `FluentTheme` beginnen und ein kleines eigenes Gestaltungssystem darauf aufbauen: gemeinsame Farben, Schriftgrößen, Abstände, Rundungen, Eingabefelder, Buttons und Statusanzeigen. Zunächst eine sorgfältig ausgearbeitete helle Darstellung; Dark Mode als eigener optionaler Umfang, einschließlich eigener Referenzbilder. Die Rechnungs-PDFs behalten ihre eigenen HTML/CSS-Vorlagen.

| Bestehender Bereich | Avalonia-Ziel und zu erhaltendes Verhalten |
| --- | --- |
| Hauptfenster/Rechnungsliste | Klare Kopfzeile mit Hauptaktion „Neue Rechnung“, Summenkarten, Filter und Rechnungstabelle; Sortierung, Mehrfachauswahl, Kontextmenü, Erinnerung und Leerzustand. DataGrid ist ein eigener Paket-/Theme-Baustein und muss zur gewählten Avalonia-Version passen. |
| Rechnungseditor/PositionEditor | Kopf mit Kunde, Datum und Nummer; flexible Positionsliste; gut erkennbare Summen; Positionen hinzufügen, entfernen und verschieben; Vorlagenwahl; Speichern/Abbruch, Änderungsrückfrage und Schreibschutz gestellter Rechnungen. |
| Kunden | Liste und Bearbeitung mit verständlicher Validierung und stabiler Auswahl nach Speichern. |
| Meine Daten | Gruppierte Angaben zu Absender, Rechnung, Steuer und Bank; vollständige bisherige Einstellmöglichkeiten. |
| Zahlungseingang | Auswahl mehrerer Rechnungen, Zahlungsdatum und sofort erkennbare Gesamtbeträge; Status nicht ausschließlich farbig vermitteln. |
| PDF-Vorschau | Eigene Avalonia-Ansicht mit nativer PDF-Fläche, Hinweisen, Lade-/Fehlerzuständen und klarer Aktion „PDF speichern“. |
| Einrichtung | Alle fünf Schritte, Zurück/Weiter/Abbruch, verkürzter Ablauf bei bestehenden Daten, Logo, Vorlagenvorschau, lokale Kopie/Verknüpfungen und erste Rechnung. |
| Hilfe/Ordner/Vorlagen | F1 mit passendem Thema, nur ein Hilfefenster, Prompt kopieren, Ordner öffnen/wechseln und Originalvorlagen wiederherstellen. |

Layouts verwenden flexible Grids und Scrollbereiche statt unveränderlicher Pixelkoordinaten. Deutsche Texte, lange Namen, Währungswerte und Fehlermeldungen bestimmen den Platzbedarf. Tabellen dürfen gezielt horizontal scrollen; Formularaktionen und Validierungsfehler müssen erreichbar bleiben. Tastaturbedienung, sichtbarer Fokus und zugängliche Namen gehören zum Design.

## 4. Arbeitspakete und Reihenfolge

Jedes Paket endet mit einem prüfbaren Ergebnis und einem überschaubaren PR gegen `develop`.

| Paket | Arbeit | Fertig, wenn … | Grober Aufwand |
| --- | --- | --- | --- |
| A – Referenz und Design | Bisherige Hauptabläufe anhand des manuellen Testplans erfassen; synthetische Testdaten; Entwürfe für Übersicht, Editor und Vorschau mit Leer-, Fehler- und Ladezuständen | Funktionsliste und Referenzansichten sind abgestimmt; Zielgrößen und visuelle Kriterien stehen fest. | 2–3 Personentage |
| B – PDF- und Publish-Prototyp | Avalonia-Testfenster mit WebView2, lokaler Vorlage/Logo, A4-Druck und Anzeige derselben PDF-Datei; Einrichtungs-Vorschau und Single-File-Publish erproben | Hosting-Variante ist begründet entschieden; keine offenen Probleme bei Druck, Fokus, DPI, Schließen oder Auslieferung. | 2–4 PT |
| C – Gemeinsame Basis | Core extrahieren, Windows-Bindungen abtrennen, Ressourcen/Version prüfen; Speichern und Vorschau-Ablauf testbar herauslösen | WinForms läuft weiter; bisherige Tests bestehen; keine fachliche Logik wird für Avalonia dupliziert. | 3–5 PT |
| D – Avalonia-Grundgerüst | App-Lebenszyklus, Styles, Texte, Services, Hauptfenster, Liste, Filter, Karten und Testharness | Hauptansicht mit realem kopiertem Datenbestand und Referenztests funktioniert; Start-/Ordnerfehler sind bedienbar. | 3–5 PT |
| E – Bearbeitung | Kunden, Meine Daten, Rechnungseditor/Positionen, Zahlungseingang, Validierung, Commands und Dirty-State | Kernabläufe einschließlich Abbruch und Speicherfehler sind funktional und visuell geprüft. | 5–8 PT |
| F – Vollständigkeit | PDF-Ansicht produktiv anbinden; Einrichtung, Hilfe, Vorlagen, Logo, Ordner und Shell vervollständigen | Alle elf Bereiche des manuellen Testplans sind zugeordnet und überprüft. | 4–6 PT |
| G – Umstellung/Release | CI-Prüfungen verbindlich machen; Preview-Build, Windows-Abnahme, Veröffentlichungsworkflow, Doku; nach Abnahme WinForms entfernen | Neue einzelne exe ist releasefähig; Datenkompatibilität und Rückfallverfahren sind erprobt. | 3–5 PT |

Gesamt grob **22–36 Personentage**, einschließlich Tests und Reviewkorrekturen. Dies ist eine Planungsschätzung für jemanden mit C#/Avalonia-Erfahrung, keine Zusage; WebView-Hosting und visuelle Abstimmung sind die größten Unbekannten. Nach Paket B neu schätzen. Tests werden in jedem Paket mitgeliefert; G ist die abschließende Integration.

Abhängigkeiten: A und B liefern die Grundlage; danach C → D → E → F → G. Kein breiter Fensterumbau, solange B keinen tragfähigen PDF-/Publish-Weg nachweist. Bis zur Umstellung getrennte Entwicklungsartefakte verwenden; die stabile Download-exe erst in G ersetzen.

## 5. Teststrategie für Funktion und gutes Aussehen

### Fachliche und ViewModel-Tests

- Bestehende 200 Tests erhalten und nach Projektaufteilung sinnvoll zuordnen. JSON-Dateien aus der bisherigen Version öffnen, bearbeiten und erneut mit dem bisherigen Leser laden; inklusive Backups, Nummernkreis, Status, Empfänger-Kopie und Vorlagen-Fallback.
- ViewModels mit gewöhnlichem xUnit testen: Filter/Sortierung/Mehrfachauswahl, Commands, Zahleneingabe mit `de-DE` (`12,5`, `1.234,56`), Datum, Summen, Positionsreihenfolge, Dirty-State, Abbruch und Schreibschutz. Der Kern rechnet weiter mit `decimal`.
- PDF-Ablauf mit Testdoubles prüfen: Doppelklick startet keinen zweiten Export; Speichern bleibt bis zur fertigen Vorschau gesperrt; Abbruch ändert keinen Status; Renderer- und Dateifehler zeigen passende Zustände.
- Besonderer Fehlerfall: PDF-Datei erfolgreich kopiert, anschließendes Speichern von `rechnungen.json` fehlgeschlagen. Heute setzt `VorschauForm.PdfSpeichern()` nach dem Catch dennoch `PdfErzeugt = true`. Vor der Übertragung das gewünschte Verhalten festlegen und absichern: kein uneingeschränktes Erfolgssignal bei fehlgeschlagener Metadaten-Speicherung, nachvollziehbarer Hinweis und Wiederholungsmöglichkeit. Die zwei Dateien bilden derzeit keine atomare Transaktion.

### Automatisierte UI- und Layouttests

`Avalonia.Headless.XUnit` für echte Views mit Theme und Bindings einsetzen; Interaktionen mit `[AvaloniaFact]` testen. Reale Eingaben für Tab, Enter, Escape, F1, Strg+S und Kontextmenü-Taste verwenden. Fokusreihenfolge, Fehlermeldung am Feld, aktivierte/deaktivierte Aktionen, Dialogergebnis und Datenbindung prüfen. Async-Vorgänge über Dispatcher/Signale synchronisieren statt pauschaler Sleeps. Native Dialoge und PDF-Flächen durch klar markierte Testdoubles ersetzen. Siehe [xUnit-Integration](https://docs.avaloniaui.net/docs/testing/headless-xunit).

Gezielte Layoutassertionen ergänzen: Hauptaktionen liegen im sichtbaren Fenster, Fußleiste überlagert keine Positionen, Fehlertext bekommt Platz, Scrollbereiche erreichen das letzte Feld, gewünschte Mindestgrößen werden eingehalten. Tests sollen konkrete Bedienprobleme abfangen, nicht die Control-Hierarchie nachbauen.

### Visuelle Regressionstests

Headless-Tests erzeugen standardmäßig keine echten Pixel. Für Bildvergleiche Skia aktivieren, `UseHeadlessDrawing = false` setzen und über `CaptureRenderedFrame()` Referenzbilder erzeugen. Siehe [Avalonia-Renderingtests](https://docs.avaloniaui.net/docs/testing/setting-up-the-headless-platform).

- Referenzbilder nach menschlicher Sichtprüfung versionieren, beispielsweise unter `tests/Abgerechnet.Ui.Tests/Baselines`. Ein Bildvergleich erkennt Änderungen; ob ein Entwurf gut aussieht, entscheidet die Designabnahme.
- Mindestsatz: Übersicht leer/gefüllt mit Erinnerung; Editor normal/mit Validierungsfehler/mit vielen Positionen/geschützt; Kunden; Meine Daten; Zahlungseingang; alle Einrichtungsschritte; Hilfe; PDF-Rahmen im Lade-, Fehler- und Bereitschaftszustand.
- Feste synthetische Daten verwenden: lange Firmennamen, Umlaute, mehrzeilige Texte, große Beträge, Logo und kein Logo. Datum/Uhrzeit und IDs fixieren; Animationen und Cursorblinken im Test abschalten.
- Reproduzierbare Windows-Umgebung, feste Schriftdateien mit passender Lizenz, Paketversionen, Theme und Kultur festlegen. Für Baselines möglichst ein festes Runner-Image statt des beweglichen `windows-latest` verwenden; Aktualisierung kontrolliert durchführen.
- Kontrollierte Matrix: Hauptansichten bei 1024×768 und 1440×900 logischen Einheiten, zunächst 100 % und 150 % Render-Skalierung. 1024×768 ist ein vorgeschlagenes Designziel, noch keine bestehende Mindestanforderung. Nicht jede Ansicht braucht jede Kombination.
- Bei Abweichung Soll-, Ist- und Differenzbild als CI-Artefakte ablegen. Kleine Anti-Aliasing-Toleranz anhand der ersten Bilder festlegen; Layoutverschiebungen, fehlende Texte und abgeschnittene Buttons dürfen nicht verschwinden. Referenzen niemals automatisch im CI akzeptieren.

### Echte Windows-/PDF-Integration und manuelle Abnahme

Headless-Bilder erfassen den nativen WebView2-Inhalt nicht zuverlässig. Deshalb echte WebView2-Tests auf Windows mit installierter Runtime: alle drei Vorlagen, eigene Vorlage/CSS, Logo, Umlaute/€, Steuer/Kleinunternehmer und 30+ Positionen. PDF-Seitengröße, Textinhalt, Umbrüche und gerasterte Seiten vergleichen; keine Bytegleichheit zwischen unterschiedlichen Chromium-Versionen verlangen. Version der Runtime dokumentieren. Die gespeicherte Datei muss innerhalb desselben Ablaufs bytegleich zur gezeigten temporären PDF-Datei sein.

Fehlerprüfungen umfassen fehlende Runtime, nicht lesbare Vorlage, gesperrte PDF-Datei, nicht beschreibbaren Ordner, abgebrochene/fehlgeschlagene Navigation, Schließen während Rendern und wiederholte Vorschau. Timeout, Cancellation und Freigabe nativer Ressourcen ausdrücklich berücksichtigen.

Wenige Desktop-Ende-zu-Ende-Smokes mit einem im Prototyp erprobten Windows-Automationstool, etwa Appium: Einrichtung → Kunde → Rechnung → Vorschau → PDF speichern → Zahlungseingang → Neustart. Diese benötigen eine interaktive Desktop-Session und sind getrennt von schnellen PR-Tests auszuführen. Automatisierbarkeit von Avalonia und nativer WebView-Fläche zuerst nachweisen.

Manuell auf Windows 10 und 11 abnehmen: 100/125/150/200 % Skalierung, kleiner Bildschirm, Monitorwechsel mit unterschiedlicher Skalierung, abgesteckter Monitor, Tastaturbedienung, Screenreader-Namen/Fokus, Kontrast und gesperrte Dateien. Dabei die tatsächlich veröffentlichte exe auf einem Rechner ohne installiertes .NET starten, auch offline und aus einem Ordner ohne Schreibrechte neben der exe. Selbstkopie, Verknüpfungen, Settings-Fallback und Startargument testen. Single-File bedeutet weiterhin: die externe WebView2 Runtime bleibt Voraussetzung.

### CI-Aufteilung

Jeder PR: Core-/ViewModel-Tests, Headless-Interaktion/Layout, ausgewählte Bildvergleiche und Release-Publish. WebView2-Integration als eigener Windows-Job mit expliziter Runtime-Voraussetzung; interaktive Desktop-Smokes in einer dafür geeigneten Session vor Freigabe. Tests dürfen bei fehlenden Voraussetzungen nicht stillschweigend als erfolgreiche Integration gelten. Screenshots, Diff-Bilder und Testberichte nur mit erfundenen Daten hochladen.

## 6. Wesentliche Risiken und Entscheidungen

| Risiko | Behandlung / Entscheidungspunkt |
| --- | --- |
| WebView2 lässt sich nicht stabil einbetten oder Zusatzcontrol ist lizenzlich ungeeignet | Paket B muss Host, PDF-Optionen, Ressourcen und Weitergabe nachweisen; vor weiteren UI-Paketen entscheiden. |
| Einzeldatei-Auslieferung verliert native Bibliotheken oder Ressourcen | Früh veröffentlichte exe testen; später Releasejob und lokale Kopierfunktion mit genau diesem Artefakt prüfen. Nicht nur Build-Ausgabe testen. |
| Logikänderungen durch neue Bindings | Bearbeitungskopien, bestehende Berechnung und Speicherdienste verwenden; Abbruch-/Fehlerfälle charakterisieren. |
| Styles sehen modern aus, sind aber unpraktisch | Drei zentrale Ansichten vorab abstimmen; echte lange Texte, Tastatur und kleine Fenster mitprüfen. |
| Screenshot-Tests schwanken | Schrift, Renderer, Kultur und Testumgebung kontrollieren; Skia-Bilder und native Windows-Abnahme getrennt bewerten. |
| Alte Settings vergrößern/verstecken das Fenster nach DPI-Wechsel | Bestehende Pixelwerte ohne Schemaänderung übernehmen und korrekt in DIPs umrechnen; sichtbare Platzierung sicherstellen. |
| Rückfall auf alte exe beschädigt Daten oder überschreibt PDFs | Keine Formatmigration im UI-Vorhaben; Vorschauversion nur auf Kopien testen, vor Umstellung sichern; alten und neuen Prozess nie parallel am selben Ordner betreiben. |

Vor Umsetzung festzulegen: freigegebene Avalonia-/Paketversionen, PDF-Host und dessen Lizenz, endgültiges helles Design/Mindestgröße und Abnahmeverantwortung für Referenzbilder. Diese Detailentscheidungen verhindern den Plan nicht; sie sind Ergebnisse von A/B. macOS/Linux und Dark Mode bleiben separate Folgevorhaben.

## 7. Vorlage für das spätere GitHub-Issue

**Titel:** Frontend schrittweise von WinForms auf Avalonia umstellen – mit visuellen Regressionstests

**Problem und Ziel**

Abgerechnet verwendet aktuell WinForms. Die Oberfläche soll moderner, konsistent gestaltet und bei unterschiedlichen Fenstergrößen und DPI-Einstellungen zuverlässig bedienbar werden. Dafür soll ein eigenständiges Avalonia-Frontend entstehen. Die vorhandene Rechnungslogik, lokalen Daten und HTML-/PDF-Vorlagen bleiben kompatibel. Zielplattform bleibt zunächst Windows 10/11 x64; die Auslieferung erfolgt weiterhin als selbstständige `Abgerechnet.exe`.

**Ausgangslage**

Core und UI sind bereits weitgehend getrennt, aber im selben Projekt. 200 vorhandene Tests bestehen am untersuchten Commit `05f1f12`. PDF-Rendering und Vorschau hängen direkt an WinForms-WebView2; Speicherabläufe befinden sich teilweise in Forms. Automatische Tests für das visuelle Layout fehlen. Die vollständige Bestandsaufnahme und Testmatrix stehen in `docs/AVALONIA-MIGRATION.md`.

**Geplante Arbeitspakete**

- [ ] A: Funktionsinventar und Referenzentwürfe für Übersicht, Rechnungseditor und PDF-Ansicht abstimmen.
- [ ] B: Avalonia/WebView2-Prototyp einschließlich PDF, Logo, Fokus, DPI und Single-File-Publish; Host/Paketversion/Lizenz dokumentieren.
- [ ] C: Gemeinsame Fach- und Anwendungslogik extrahieren; Windows-Adapter, Ressourcen und Versionsanzeige absichern.
- [ ] D: Avalonia-Shell mit MVVM, zentralen Styles/Texten, Rechnungsliste und UI-Testharness erstellen.
- [ ] E: Kunden, Meine Daten, Rechnungs-/Positionseditor und Zahlungseingang migrieren.
- [ ] F: PDF-Ablauf, Einrichtung, Hilfe, Vorlagen und Ordner-/Shell-Funktionen vervollständigen.
- [ ] G: CI, visuelle Abnahme, Preview-Release und Auslieferung umstellen; WinForms nach Abnahme entfernen.

**Abnahmekriterien**

- [ ] Alle elf Bereiche des bestehenden manuellen Testplans sind im Avalonia-Frontend abgedeckt.
- [ ] Vorhandene Rechnungsordner funktionieren ohne Formatmigration; IDs, Nummern, Summen, Status, Backups, Vorlagen und Empfänger-Snapshots bleiben korrekt.
- [ ] Bisherige Tests bestehen weiter; ViewModel-, Headless-Interaktions-, Layout- und visuelle Regressionstests laufen in CI.
- [ ] Abgestimmte Referenzbilder sind versioniert; Abweichungen liefern Soll-/Ist-/Diff-Artefakte und erfordern Review.
- [ ] Deutsche lange Texte und Fehlermeldungen sind bei den festgelegten Fenstergrößen bedienbar; DPI-Matrix bis 200 % und Tastaturbedienung wurden geprüft.
- [ ] Alle drei PDF-Vorlagen, eigenes Logo und mehrseitige Rechnungen sind mit echter WebView2 Runtime getestet; gespeichert wird die tatsächlich gezeigte PDF-Datei.
- [ ] Abbruch, doppelte Aktionen, Renderer-/Dateifehler und fehlerhafte Metadaten-Speicherung erzeugen keine irreführenden Erfolgsmeldungen oder verlorenen Änderungen.
- [ ] Eine veröffentlichte einzelne exe startet ohne installiertes .NET; Version, lokale Kopie, Verknüpfungen, Settings und Offlinebetrieb sind geprüft.
- [ ] README, Hilfe, manueller Testplan und Build-/Releaseworkflows passen zur neuen Oberfläche.

**Nicht im Umfang:** neue PDF-Engine, macOS/Linux-Auslieferung, Dark Mode, neue Rechnungsfunktionen oder Änderung des JSON-Formats.

**Umsetzung:** mehrere PRs gegen `develop`, Reihenfolge A/B → C → D → E → F → G. Aufwand grob 22–36 Personentage, nach dem PDF-Prototyp neu bewerten. Das Issue dient als übergreifender Plan; die Pakete können bei Bedarf als verknüpfte Teil-Issues verfolgt werden.

## Quellen

Repository: `README.md`, `CONTRIBUTING.md`, beide Projektdateien, `Program.cs`, `Core/`, `UI/`, vorhandene Tests, `Directory.Build.targets`, Build-/Releaseworkflows und `docs/MANUAL-TESTING.md`.

Offizielle Framework-Dokumentation, geprüft am 17.10.2026:

- [Avalonia unter Windows, WinForms-Integration und native Controls](https://docs.avaloniaui.net/docs/platform-specific-guides/windows)
- [Themes](https://docs.avaloniaui.net/docs/styling/themes) und [DataGrid](https://docs.avaloniaui.net/controls/data-display/structured-data/datagrid)
- [WebView-Einbettung](https://docs.avaloniaui.net/docs/app-development/embedding-web-content), [NativeWebView](https://docs.avaloniaui.net/controls/web/nativewebview) und [Komponentenübersicht](https://docs.avaloniaui.net/tools/)
- [Headless-Tests und visuelle Regression](https://docs.avaloniaui.net/docs/testing/setting-up-the-headless-platform) und [xUnit-Integration](https://docs.avaloniaui.net/docs/testing/headless-xunit)
