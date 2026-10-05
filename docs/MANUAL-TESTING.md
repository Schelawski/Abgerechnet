# Manueller Testplan

Die automatischen Tests decken die Logik in `Core` ab. Diese manuellen Prüfungen decken die Oberfläche und
die fertige exe ab. Der Plan wächst mit jedem umgesetzten Issue.

Namen von Schaltflächen und Menüs beziehen sich auf die deutsche Oberfläche.

## Vorbereitung

1. App veröffentlichen: `dotnet publish src/Abgerechnet -c Release`.
2. `Abgerechnet.exe` in einen leeren Ordner kopieren, z. B. `C:\Temp\Abgerechnet\`.

## 1. Start

| Schritt | Erwartet |
|---------|----------|
| Ordner `C:\Temp\Abgerechnet\` ansehen. | Nur `Abgerechnet.exe`, keine DLLs oder weiteren Dateien. |
| Rechtsklick auf `Abgerechnet.exe` → Eigenschaften → Details. | Produktname „Abgerechnet“, Copyright mit Hinweis auf GPL-3.0 und `github.com/Schelawski/Abgerechnet`. |
| `Abgerechnet.exe` starten. | Ein Hinweis erklärt den Rechnungsordner, danach öffnet sich die Ordnerauswahl. |
| Ordnerauswahl mit **Abbrechen** schließen. | Die App beendet sich ohne Fehlermeldung. `Abgerechnet.settings.json` existiert nicht. |
| Erneut starten, einen neuen Ordner `C:\Temp\Meine Rechnungen` anlegen und wählen. | Das Hauptfenster öffnet sich mittig. Fenstertitel `Abgerechnet 1.0.0` (bzw. die Version des Tags). Die Statusleiste zeigt den Rechnungsordner. |
| Klick auf den Ordner in der Statusleiste. | Der Explorer öffnet `C:\Temp\Meine Rechnungen` mit `abgerechnet.json`, `kunden.json`, `rechnungen.json`, `Vorlagen` und `PDF`. |
| `rechnungen.json` im Editor öffnen. | Eingerücktes JSON mit `"version": 1` und `"rechnungen": []`. |
| **Hilfe → Über Abgerechnet…** | Version, Hinweis „kostenlos“ und die offizielle Quelle werden angezeigt. |
| Fenster verkleinern. | Es lässt sich nicht kleiner als die Mindestgröße ziehen, nichts wird abgeschnitten. |
| Fenster verschieben und vergrößern, **Datei → Beenden**. | Die App schließt sich. `Abgerechnet.settings.json` liegt neben der exe. |
| Erneut starten. | Kein Hinweis, keine Ordnerauswahl: Das Fenster öffnet sich mit demselben Ordner an derselben Stelle in derselben Größe. |
| Auf einem Monitor mit 150 % Skalierung wiederholen. | Texte scharf, nichts abgeschnitten. |

## 2. Rechnungsordner

| Schritt | Erwartet |
|---------|----------|
| App schließen. In `rechnungen.json` eine Zeile mit `"nummer": "1",` von Hand ergänzen (gültiges JSON), speichern, App starten und schließen. | Keine Meldung. Die Datei bleibt lesbar, Umlaute unverändert. |
| App schließen. In `kunden.json` eine schließende Klammer löschen, App starten. | Meldung: „kunden.json“ ist beschädigt, mit Zeilennummer; die Datei wurde nicht verändert. Frage nach einem anderen Ordner. **Nein** beendet die App. |
| Prüfen, ob `kunden.json` unverändert ist; Klammer wieder einfügen. | Die Datei ist byte-genau wie vor dem Start. |
| In `abgerechnet.json` `"version": 1` in `"version": 99` ändern, App starten. | Meldung: Die Datei stammt von einer neueren Version, bitte die neueste Version herunterladen. Danach Wert zurücksetzen. |
| Den Ordner `C:\Temp\Meine Rechnungen` umbenennen, App starten. | Meldung „Der Rechnungsordner wurde nicht gefunden“ mit Pfad. **Ja** öffnet die Ordnerauswahl. Den umbenannten Ordner wählen. |
| **Datei → Rechnungsordner wechseln…**, einen zweiten leeren Ordner wählen. | Die Statusleiste zeigt den neuen Ordner, die Dateien werden darin angelegt. Nach einem Neustart ist der neue Ordner geöffnet. |

## 3. Meine Daten

| Schritt | Erwartet |
|---------|----------|
| Neuen leeren Rechnungsordner öffnen. | Gelber Hinweis oben: „Tragen Sie zuerst Ihre Daten ein …“ mit **Meine Daten eintragen…**. Fenstertitel ohne Firma. |
| **Meine Daten eintragen…** | Dialog „Meine Daten“ mit den Reitern **Absender**, **Steuer und Bank**, **Rechnungen**. Der Cursor steht im Feld **Firma**. Alle Felder sichtbar, keine Lücken, nichts abgeschnitten. |
| Nur **Firma** eintragen, **Speichern**. | Meldung: vollständige Anschrift fehlt; der Cursor springt ins fehlende Feld. |
| Firma `Schmidt IT`, Name `Jörg Schmidt`, Straße, PLZ, Ort `Köln` eintragen. Reiter **Steuer und Bank**, IBAN `DE89 3704 0044 0532 0130 01` eingeben. | Neben der IBAN „✗ ungültig“ (rot). |
| Letzte Ziffer zu `0` ändern. | „✓ gültig“ (grün). Beim Verlassen des Feldes wird die IBAN in Viererblöcken geschrieben. |
| **Kleinunternehmer** ankreuzen. | Hinweistext wird bearbeitbar, Umsatzsteuersatz ausgegraut. |
| Reiter **Rechnungen**: PDF-Dateiname `{nummer}_{kundenname}.pdf`. | Das Beispiel darunter ändert sich bei jeder Eingabe. |
| **Speichern**. | Meldung: unbekannter Platzhalter `{kundenname}`. Muster zurück auf `{nummer}_{kunde_kurzname}_{datum}.pdf`. |
| **Speichern** (ohne Steuernummer und USt-IdNr.). | Rückfrage nach § 14 UStG. **Nein** lässt den Dialog offen, **Ja** speichert. |
| Nach dem Speichern. | Fenstertitel `Schmidt IT – Abgerechnet …`, gelber Hinweis verschwunden. `abgerechnet.json` enthält die Werte lesbar (Umlaute, IBAN in Blöcken), daneben `abgerechnet.bak.json`. |
| Dialog erneut öffnen, etwas ändern, **Abbrechen**. | Nichts geändert, `abgerechnet.json` unverändert. |
| Dialog auf 125 % und 150 % Skalierung öffnen, Fenster verkleinern. | Bildlaufleiste erscheint, Hinweise brechen um, nichts überlappt. |

## 4. Kunden

| Schritt | Erwartet |
|---------|----------|
| **Datei → Kunden…** in einem neuen Rechnungsordner. | Leere Liste, rechts „Noch keine Kunden …“, **Löschen** ausgegraut. |
| **Neu**, Firma `Müller & Söhne GmbH` tippen. | Der Cursor steht in **Firma**. Die Liste zeigt den Namen schon beim Tippen; **Kurzname** füllt sich mit `Mueller-Soehne`. |
| Kurzname auf `Mueller` ändern, dann Firma weiter ändern. | Der Kurzname bleibt `Mueller` (eigene Eingabe wird nicht überschrieben). |
| Ansprechpartner, Straße, PLZ, Ort eintragen. | Die Vorschau „So steht die Anschrift auf der Rechnung“ zeigt die Zeilen, `&` wird korrekt angezeigt. |
| **Neu**, Firma leer lassen, **Speichern**. | Meldung, dass jeder Kunde eine Firma braucht; der leere Kunde ist ausgewählt. Firma `Beispiel AG` und Anschrift eintragen, **Speichern**. |
| Dialog erneut öffnen. | Kunden alphabetisch sortiert; `kunden.json` enthält beide lesbar, daneben `kunden.bak.json`. |
| Kunden auswählen, **Löschen** → **Ja**, dann **Abbrechen** → **Ja**. | Nach dem erneuten Öffnen ist der Kunde noch da – Abbrechen verwirft alle Änderungen. |
| App schließen, in `rechnungen.json` eine Rechnung mit `"kundeId"` eines Kunden eintragen, App starten, diesen Kunden löschen. | Hinweis, dass der Kunde Rechnungen hat und nicht gelöscht werden kann. |
