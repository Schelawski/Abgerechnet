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
| `Abgerechnet.exe` starten. | Das Hauptfenster öffnet sich mittig. Fenstertitel `Abgerechnet 1.0.0` (bzw. die Version des Tags). |
| **Hilfe → Über Abgerechnet…** | Version, Hinweis „kostenlos“ und die offizielle Quelle werden angezeigt. |
| Fenster verkleinern. | Es lässt sich nicht kleiner als die Mindestgröße ziehen, nichts wird abgeschnitten. |
| **Datei → Beenden** | Die App schließt sich. |
| Auf einem Monitor mit 150 % Skalierung wiederholen. | Texte scharf, nichts abgeschnitten. |
