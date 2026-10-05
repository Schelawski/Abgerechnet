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
