# Mitmachen bei Abgerechnet

Danke für Ihr Interesse an Abgerechnet! Fehlermeldungen, Ideen, Vorlagen und Code sind willkommen.
Abgerechnet ist ein kleines Projekt, das in der Freizeit gepflegt wird – bitte lesen Sie vorher diese
wenigen Regeln.

## Ein Problem oder eine Idee melden

Legen Sie ein [Issue](https://github.com/Schelawski/Abgerechnet/issues) an. Hilfreich bei Problemen:

- was Sie getan haben, was Sie erwartet haben und was stattdessen passiert ist,
- die Version von Abgerechnet (Fenstertitel) und Ihre Windows-Version,
- die Fehlermeldung im Wortlaut, falls es eine gibt.

Bitte hängen Sie **niemals echte Rechnungen, Kundendaten oder Ihre Daten-Dateien** (`rechnungen.json`,
`kunden.json`, `abgerechnet.json`) an. Wenn ein Beispiel nötig ist, ersetzen Sie Namen, Adressen, IBAN und
Beträge durch erfundene Werte.

## Bevor Sie Code schreiben

**Bitte legen Sie zuerst ein Issue an und warten Sie auf eine kurze Antwort**, bevor Sie größere Änderungen
beginnen (neue Funktionen, neue Abhängigkeiten, Änderungen an der Oberfläche oder am Dateiformat). So
vermeiden wir Arbeit, die nicht übernommen werden kann. Kleine Korrekturen (Tippfehler, offensichtliche
Fehler) können direkt als Pull Request kommen.

## Pull Requests

1. Forken Sie das Repository und erstellen Sie einen Branch von `develop` (nicht `master`).
2. Ein Pull Request behandelt genau ein Thema.
3. Lokal bauen und testen (siehe [Build](README.md#build)):
   ```powershell
   dotnet build
   dotnet test
   ```
   Der automatische Build auf GitHub führt dieselben Tests aus; bei Erstbeiträgen startet er, nachdem der
   Maintainer ihn freigegeben hat.
4. Beschreiben Sie, was Sie geändert haben und warum, und verlinken Sie das Issue (`Closes #123`).

Der Maintainer prüft jeden Pull Request und entscheidet, ob und wann er übernommen wird. Nichts wird
automatisch übernommen.

## Richtlinien

- **Code und Kommentare auf Englisch**, Dokumentation für Nutzer auf Deutsch. Halten Sie sich an den Stil
  des umgebenden Codes.
- **Die Oberfläche ist deutsch.** Jeder sichtbare Text steht zentral in `src/Abgerechnet/UI/UiText.cs`.
- **Einfache Sprache.** Abgerechnet ist für Menschen ohne Technik- oder Buchhaltungswissen gemacht.
  Lieber „Rechnungsordner“ als „Datenverzeichnis“, und immer sagen, was der Nutzer tun kann.
- **Hilfetexte** stehen in `src/Abgerechnet/Help/help.de.md`. Namen von Schaltflächen müssen mit der
  Oberfläche übereinstimmen.
- **Datenschutz zuerst.** Rechnungs- und Kundendaten verlassen den Computer nicht. Keine Telemetrie, keine
  Cloud-Anbindung.
- **Daten des Nutzers sind heilig.** Dateien werden sicher geschrieben (erst temporär, dann ersetzen),
  beschädigte Dateien werden nie überschrieben, gestellte Rechnungen werden nie gelöscht.
- Ergänzen oder aktualisieren Sie Unit-Tests in `tests/Abgerechnet.Tests` bei Änderungen in
  `src/Abgerechnet/Core`.

## Lizenz der Beiträge

Abgerechnet steht unter der [GNU General Public License v3.0](LICENSE). Mit einem Pull Request stimmen Sie
zu, dass Ihr Beitrag unter derselben Lizenz veröffentlicht wird. Das Urheberrecht an Ihrem Beitrag bleibt
bei Ihnen.
