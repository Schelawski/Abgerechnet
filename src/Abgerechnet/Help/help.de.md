Hilfe von Abgerechnet. Format: siehe src/Abgerechnet/Core/Help/HelpDocument.cs.
Einfache Sprache: kurze Sätze, „Sie“, Fachwörter erklären. Namen von Schaltflächen genau wie in der Oberfläche.

# about | Was ist Abgerechnet?

Mit Abgerechnet schreiben Sie Rechnungen für Ihre selbstständige Arbeit. Sie tragen ein, was Sie geleistet haben, und Abgerechnet macht daraus eine fertige Rechnung als PDF-Datei. Eine Liste zeigt Ihnen jederzeit, welche Rechnungen noch offen und welche schon bezahlt sind. **Abgerechnet ist kostenlos.**

## Ihre Daten bleiben bei Ihnen

Abgerechnet arbeitet nur auf Ihrem Computer. **Ihre Rechnungen, Kunden und Bankdaten werden nirgendwohin übertragen.** Abgerechnet stellt keine Verbindung zum Internet her. Sie brauchen kein Benutzerkonto, kein Abo und keine Cloud.

Alles liegt in einem Ordner, den Sie selbst auswählen: als ganz normale Dateien, die Sie sehen, sichern und mitnehmen können. Wenn Sie Abgerechnet nicht mehr benutzen, bleiben Ihre Rechnungen als PDF-Dateien erhalten.

## Bewusst einfach

Abgerechnet kann nur das, was man für Rechnungen wirklich braucht: Absender, Kunden, Positionen, Summen, PDF. Es gibt keine Buchhaltung, keine Anbindung an Banken, Steuerprogramme oder andere Systeme und keine komplizierten Einstellungen. Sie sollen in wenigen Minuten Ihre erste Rechnung schreiben können – und am Monatsende mit ein paar Klicks die nächste.

## Für wen ist Abgerechnet?

Für Selbstständige und Freiberufler in Deutschland, die regelmäßig Rechnungen schreiben – zum Beispiel jeden Monat an dieselben Kunden. Kleinunternehmer nach § 19 UStG werden unterstützt (siehe „Meine Daten“).

# start | Erste Schritte

1. **Rechnungsordner wählen:** Beim ersten Start fragt Abgerechnet nach einem Ordner. Legen Sie am besten einen neuen Ordner an, zum Beispiel „Rechnungen“ in Ihren Dokumenten. Mehr dazu unter „Ihr Rechnungsordner“.
2. **Ihre Daten eintragen:** Klicken Sie auf **Meine Daten eintragen…** (oder **Datei → Meine Daten…**). Tragen Sie Ihren Namen, Ihre Anschrift, Ihre Steuernummer und Ihre Bankverbindung ein. Das steht später auf jeder Rechnung.
3. **Kunden anlegen:** Unter **Datei → Kunden…** tragen Sie die Anschriften Ihrer Kunden ein.
4. **Rechnung schreiben:** Klicken Sie auf **+ Neue Rechnung**. Wählen Sie den Kunden und tragen Sie ein, was Sie geleistet haben.
5. **PDF erzeugen:** Klicken Sie auf **PDF erzeugen…** – Sie sehen die fertige Rechnung. Mit **PDF speichern** legen Sie sie im Ordner `PDF` ab.
6. **Verschicken:** Hängen Sie die PDF-Datei an eine E-Mail an Ihren Kunden. Abgerechnet verschickt nichts selbst.

## Jeden Monat dieselbe Rechnung?

Klicken Sie in der Liste mit der rechten Maustaste auf die Rechnung vom Vormonat und wählen Sie **Als neue Rechnung kopieren**. Kunde, Projekt und Positionen sind übernommen, Nummer, Datum und Zeitraum sind schon neu. Sie ändern nur noch die Stunden.

## Tastenkürzel

- **F1** öffnet diese Hilfe.
- **Strg+N** erstellt eine neue Rechnung.
- **Strg+S** speichert die geöffnete Rechnung.
- **Entf** löscht einen markierten Entwurf.

# ordner | Ihr Rechnungsordner

Abgerechnet speichert alles in einem einzigen Ordner. Darin finden Sie:

- `abgerechnet.json` – Ihre Daten aus „Meine Daten“
- `kunden.json` – Ihre Kunden
- `rechnungen.json` – alle Rechnungen mit ihren Positionen
- `Vorlagen` – das Aussehen Ihrer Rechnungen (siehe „Vorlagen“)
- `PDF` – die fertigen Rechnungen

Die `.json`-Dateien sind einfache Textdateien. Sie müssen sie nicht öffnen – Abgerechnet erledigt das für Sie.

## Sichern

Legen Sie Ihren Rechnungsordner in einen Ordner, der automatisch gesichert wird, zum Beispiel OneDrive oder Dropbox. Oder kopieren Sie den ganzen Ordner regelmäßig auf einen USB-Stick. Mehr ist nicht nötig. Wichtig: Rechnungen müssen Sie in Deutschland in der Regel zehn Jahre aufbewahren.

Vor jedem Speichern legt Abgerechnet eine Kopie der vorigen Fassung an, zum Beispiel `rechnungen.bak.json`. Geht etwas schief, ist der letzte Stand noch da.

## Auf einen anderen Computer umziehen

Kopieren Sie den ganzen Rechnungsordner und `Abgerechnet.exe` auf den neuen Computer. Starten Sie Abgerechnet und wählen Sie unter **Datei → Rechnungsordner wechseln…** den kopierten Ordner.

## Den Ordner wechseln oder öffnen

**Datei → Rechnungsordner wechseln…** öffnet einen anderen Ordner. **Datei → Rechnungsordner im Explorer öffnen** (oder ein Klick auf den Pfad unten im Fenster) zeigt Ihnen den Ordner.

# meinedaten | Meine Daten

Unter **Datei → Meine Daten…** tragen Sie ein, was auf jeder Rechnung stehen soll.

## Absender

Ihre Firma oder Ihr Name und Ihre Anschrift sind Pflicht. Telefon, E-Mail und Website sind freiwillig. Die **Unterzeile** steht unter Ihrem Firmennamen, zum Beispiel Ihr Tätigkeitsfeld.

## Steuer und Bank

- **Steuernummer oder USt-IdNr.:** Eine davon muss auf jeder Rechnung stehen. Die Steuernummer bekommen Sie vom Finanzamt, die USt-IdNr. vom Bundeszentralamt für Steuern.
- **Kleinunternehmer:** Wenn Sie die Kleinunternehmer-Regelung nach § 19 UStG nutzen, setzen Sie hier das Häkchen. Dann berechnet Abgerechnet keine Umsatzsteuer und druckt stattdessen einen Hinweis auf die Rechnung.
- **Umsatzsteuersatz:** In der Regel 19 %.
- **IBAN:** Abgerechnet prüft, ob die IBAN stimmen kann. Neben dem Feld steht dann „gültig“ oder „ungültig“.

## Rechnungen

- **Zahlungsziel:** So viele Tage nach dem Rechnungsdatum soll Ihr Kunde bezahlen. Daraus wird das Datum „Zahlbar bis“. 0 bedeutet: kein Zahlungsziel.
- **Nächste Rechnungsnummer:** Lassen Sie das Feld leer, zählt Abgerechnet einfach weiter (höchste Nummer plus eins). Tragen Sie eine Nummer ein, um einen neuen Nummernkreis zu beginnen. Rechnungsnummern müssen fortlaufend und eindeutig sein – Abgerechnet verhindert doppelte Nummern.
- **PDF-Dateiname:** So heißen die PDF-Dateien. Darunter sehen Sie ein Beispiel. Platzhalter wie `{nummer}` oder `{kunde_kurzname}` setzt Abgerechnet ein.
- **Vorlage:** Das Aussehen neuer Rechnungen (siehe „Vorlagen“).
- **Vorgaben für neue Positionen:** Beschreibung, Einheit und Preis, mit denen jede neue Position beginnt. Das spart Tipparbeit, wenn Sie meist dasselbe abrechnen.

## Gilt eine Änderung auch für alte Rechnungen?

Nein. Jede Rechnung merkt sich den Steuersatz und die Kleinunternehmer-Regelung, die beim Anlegen galten. Ihre Bankverbindung und Anschrift werden beim Erzeugen des PDFs eingesetzt – ein bereits gespeichertes PDF ändert sich nie.

# kunden | Kunden

Unter **Datei → Kunden…** oder im Rechnungsformular mit **Kunden verwalten…** pflegen Sie Ihre Kunden.

- Mit **Neu** legen Sie einen Kunden an. Firma und Anschrift reichen aus.
- Der **Kurzname** erscheint im Dateinamen des PDFs. Abgerechnet schlägt ihn aus der Firma vor.
- Darunter sehen Sie, wie die Anschrift auf der Rechnung steht.
- **Speichern** übernimmt alle Änderungen, **Abbrechen** verwirft sie.

## Kunden löschen

Ein Kunde lässt sich nur löschen, wenn es keine Rechnung an ihn gibt. Ändert sich die Anschrift eines Kunden, ändern Sie sie einfach. Bereits erzeugte Rechnungen behalten die Anschrift, mit der sie gestellt wurden.

# rechnungen | Rechnungen schreiben

Klicken Sie auf **+ Neue Rechnung**. Nummer, Datum und Leistungszeitraum (der Vormonat) sind schon ausgefüllt. Sie können alles ändern.

## Positionen

Jede Position ist eine Zeile auf der Rechnung: was Sie geleistet haben, wie viel davon und zu welchem Preis. Der Betrag und die Summen rechnen sich bei jeder Eingabe neu.

- **Zeitraum:** Bleibt das Feld leer, gilt der Leistungszeitraum der Rechnung.
- **Zusatz:** Eine zweite, kleinere Zeile unter der Beschreibung, zum Beispiel eine Ticketnummer.
- **+ Position hinzufügen** fügt eine Zeile an. Mit den Pfeilen ändern Sie die Reihenfolge, mit dem Kreuz entfernen Sie eine Zeile.

## Der Status einer Rechnung

- **Entwurf:** Die Rechnung ist noch in Arbeit. Entwürfe können Sie jederzeit ändern oder löschen.
- **Offen:** Das PDF wurde erzeugt. Abgerechnet setzt diesen Status automatisch.
- **Bezahlt:** Ihr Kunde hat bezahlt. Klicken Sie mit der rechten Maustaste auf die Rechnung und wählen Sie **Status ändern → Bezahlt**.
- **Storniert:** Die Rechnung gilt nicht mehr.

## Warum kann ich gestellte Rechnungen nicht löschen?

Rechnungsnummern müssen lückenlos sein. Eine verschickte Rechnung wird deshalb nicht gelöscht, sondern storniert: rechte Maustaste, **Status ändern → Storniert**. Sie bleibt grau in der Liste und zählt nicht mehr mit. Schicken Sie Ihrem Kunden danach eine korrigierte neue Rechnung.

Eine gestellte Rechnung öffnet sich schreibgeschützt. Müssen Sie doch etwas ändern, klicken Sie auf **Bearbeiten…** und erzeugen danach ein neues PDF.

## Die Liste

Oben sehen Sie, wie viel im gewählten Jahr offen und bezahlt ist. „Gesamt“ zählt nur offene und bezahlte Rechnungen, keine Entwürfe. Mit einem Klick auf eine Spaltenüberschrift sortieren Sie die Liste. Ein Doppelklick öffnet eine Rechnung.

# pdf | PDF erzeugen und verschicken

Klicken Sie im Rechnungsformular auf **PDF erzeugen…** – oder in der Liste mit der rechten Maustaste auf eine Rechnung. Abgerechnet zeigt Ihnen das fertige PDF so, wie es Ihr Kunde sehen wird.

## Hinweise vor dem Versand

Fehlt etwas, das auf eine Rechnung gehört – zum Beispiel Ihre Steuernummer oder die Anschrift des Kunden –, steht oben ein gelber Hinweis. Sie können das PDF trotzdem speichern, sollten die Angaben aber ergänzen.

## Speichern

**PDF speichern** legt die Datei im Ordner `PDF` Ihres Rechnungsordners ab. Danach können Sie sie mit **PDF öffnen** ansehen oder mit **Im Ordner zeigen** im Explorer finden. Aus dem Entwurf wird eine offene Rechnung.

## Verschicken

Schreiben Sie Ihrem Kunden eine E-Mail und hängen Sie die PDF-Datei an. Abgerechnet verschickt bewusst nichts selbst – so bleiben Ihre Daten bei Ihnen.

# vorlagen | Vorlagen

Wie Ihre Rechnung aussieht, bestimmt eine Vorlage. Abgerechnet bringt drei mit:

- **Klassisch** – Absender rechts oben, grauer Titelbalken, Tabelle mit Rahmen.
- **Modern** – viel Weißraum und eine Akzentfarbe.
- **Schlicht** – schwarz-weiß, ohne Rahmen, gut zum Ausdrucken.

Die Vorlage für neue Rechnungen wählen Sie unter **Meine Daten** auf der Seite „Rechnungen“. Für eine einzelne Rechnung können Sie im Rechnungsformular eine andere Vorlage wählen.

## Ihr Logo

Speichern Sie Ihr Logo als Bild mit dem Namen `logo.png` im Ordner `Vorlagen`. Alle drei Vorlagen zeigen es dann automatisch an. Ohne diese Datei erscheint kein Logo.

## Wo liegen die Vorlagen?

Im Ordner `Vorlagen` Ihres Rechnungsordners. **Vorlagen → Vorlagen-Ordner öffnen** zeigt ihn Ihnen. Jede `.html`-Datei darin ist eine Vorlage. Legen Sie eine eigene hinein, erscheint sie automatisch in der Auswahl.

## Etwas kaputt gemacht?

**Vorlagen → Original wiederherstellen** holt die ursprüngliche Vorlage zurück. Ihre geänderte Fassung bleibt als Sicherung erhalten, zum Beispiel als `klassisch.bak.html`.

# anpassen | Vorlage anpassen

Eine Vorlage ist eine HTML-Datei – das ist das Format von Webseiten. Sie können sie mit einem einfachen Texteditor wie dem Windows-Editor öffnen und ändern. Wenn Ihnen das zu technisch ist: Lassen Sie eine KI die Arbeit machen (siehe „Vorlage mit KI anpassen“).

## So geht es

1. **Kopie anlegen:** Öffnen Sie mit **Vorlagen → Vorlagen-Ordner öffnen** den Ordner. Kopieren Sie eine Vorlage und geben Sie der Kopie einen eigenen Namen, zum Beispiel `meine-vorlage.html`.
2. **Öffnen:** Klicken Sie mit der rechten Maustaste auf die Datei und wählen Sie „Öffnen mit“ → „Editor“.
3. **Farben und Schrift ändern:** Ganz oben in der Datei, unter `:root`, stehen Farben, Schrift und Größen. Ändern Sie dort die Werte, zum Beispiel `#2563eb` in eine andere Farbe.
4. **Speichern:** Speichern Sie die Datei. Achten Sie darauf, dass sie die Endung `.html` behält und als „UTF-8“ gespeichert wird (der Windows-Editor macht das von selbst).
5. **Auswählen und prüfen:** Wählen Sie Ihre Vorlage unter **Meine Daten** und sehen Sie sich mit **PDF erzeugen…** das Ergebnis an.

## Was Sie nicht ändern sollten

- Die **Platzhalter** in doppelten geschweiften Klammern, zum Beispiel `{{rechnung_nummer}}`. Dort setzt Abgerechnet Ihre Daten ein. Sie dürfen sie aber verschieben oder weglassen.
- Die Namen der **CSS-Klassen** der Tabellen (unten aufgeführt). Ihr Aussehen dürfen Sie ändern.

Unbekannte Platzhalter – etwa durch einen Tippfehler – meldet die Vorschau. Platzhalter in Kommentaren (zwischen `<!--` und `-->`) werden nicht ersetzt.

## Die fertigen Tabellen

`{{positionen_tabelle}}` setzt alle Positionen als Tabelle ein. Ihre Spalten haben diese Klassen: `pos-nr` (Nummer), `pos-zeitraum`, `pos-beschreibung` mit `pos-detail` (Zusatz) darin, `pos-menge`, `pos-einheit`, `pos-preis` und `pos-betrag`. Eine Spalte blenden Sie mit `display: none` aus.

`{{summen_tabelle}}` setzt die Summen ein: die Zeilen `netto`, `ust` und `brutto`. Bei Kleinunternehmern gibt es nur `brutto`.

## Leere Angaben ausblenden

Eine Zeile mit der Klasse `wenn-gefuellt` verschwindet, wenn ihr Wert (Klasse `wert`) leer ist. So steht ohne Telefonnummer auch kein „Tel.:“ auf der Rechnung.

## Alle Platzhalter

[[platzhalter]]

# ki | Vorlage mit KI anpassen

Sie kennen sich mit HTML nicht aus? Dann beschreiben Sie einer KI wie ChatGPT, Claude, Gemini oder Copilot einfach in Ihren Worten, was Sie ändern möchten. Abgerechnet gibt Ihnen dafür einen fertigen Text mit allen wichtigen Regeln.

## Schritt für Schritt

1. Klicken Sie unten auf **Prompt mit Vorlage kopieren**. Abgerechnet kopiert den Text zusammen mit Ihrer Standardvorlage in die Zwischenablage.
2. Öffnen Sie Ihre KI im Browser und fügen Sie den Text mit **Strg+V** ein.
3. Ersetzen Sie `[HIER IHREN WUNSCH EINTRAGEN]` durch Ihren Wunsch, zum Beispiel „Akzentfarbe Dunkelgrün und das Logo links oben“. Schicken Sie die Nachricht ab.
4. Kopieren Sie die Antwort der KI – die vollständige HTML-Datei.
5. Öffnen Sie mit **Vorlagen → Vorlagen-Ordner öffnen** den Ordner. Legen Sie eine neue Textdatei an, fügen Sie die Antwort ein und speichern Sie sie zum Beispiel als `meine-vorlage.html`.
6. Wählen Sie die neue Vorlage unter **Meine Daten** aus und prüfen Sie sie mit **PDF erzeugen…** – gefällt Ihnen etwas nicht, bitten Sie die KI um eine weitere Änderung.

## Ihre Daten

Die Vorlage enthält **keine persönlichen Daten** – nur Platzhalter wie `{{absender_firma}}`. Ihre echten Rechnungen, Kunden und Bankdaten geben Sie der KI nicht. Bitte fügen Sie auch nie eine fertige Rechnung in eine KI ein.

## Der Text für die KI

[[ki-prompt]]

# probleme | Probleme und Lösungen

## Windows warnt beim ersten Start

Erscheint „Der Computer wurde durch Windows geschützt“, klicken Sie auf **„Weitere Informationen“** und dann auf **„Trotzdem ausführen“**. Windows zeigt diese Warnung bei Programmen, die noch wenig verbreitet sind. Laden Sie Abgerechnet nur von der offiziellen Projektseite herunter (siehe „Lizenzen“).

## „Die Datei … ist beschädigt“

Eine Datei im Rechnungsordner lässt sich nicht lesen, zum Beispiel weil sie von Hand geändert wurde. Abgerechnet verändert sie nicht. Im selben Ordner liegt die vorige Fassung, zum Beispiel `rechnungen.bak.json`. Schließen Sie Abgerechnet, benennen Sie die beschädigte Datei um und geben Sie der Sicherung ihren Namen (also `rechnungen.json`).

## „Der Rechnungsordner wurde nicht gefunden“

Liegt der Ordner auf einem USB-Stick, einem Netzlaufwerk oder in einem Cloud-Ordner, ist er vielleicht gerade nicht verfügbar. Schließen Sie den Stick an oder warten Sie, bis OneDrive bzw. Dropbox fertig ist, und starten Sie Abgerechnet neu.

## Die Vorschau startet nicht

Für die Vorschau und das PDF braucht Abgerechnet die „Microsoft Edge WebView2 Runtime“. Sie ist auf Windows 10 und 11 normalerweise vorinstalliert. Fehlt sie, bietet Abgerechnet den Download bei Microsoft an. Installieren Sie sie und starten Sie Abgerechnet neu.

## Das PDF lässt sich nicht speichern

Meistens ist die Datei gerade in einem anderen Programm geöffnet, zum Beispiel im PDF-Betrachter. Schließen Sie das Programm und versuchen Sie es noch einmal.

## Mein Logo erscheint nicht

Prüfen Sie, ob die Datei genau `logo.png` heißt und im Ordner `Vorlagen` liegt – also neben den `.html`-Dateien. Windows blendet Dateiendungen manchmal aus: Dann heißt die Datei vielleicht in Wirklichkeit `logo.png.png`.

## Nach dem Anpassen sieht die Rechnung falsch aus

Prüfen Sie die Hinweise in der Vorschau, zum Beispiel unbekannte Platzhalter. Stehen statt Umlauten seltsame Zeichen da, wurde die Datei nicht als UTF-8 gespeichert. Hilft nichts mehr: **Vorlagen → Original wiederherstellen**.

# steuern | Steuern und E-Rechnung

## Keine Steuerberatung

Abgerechnet hilft Ihnen, ordentliche Rechnungen zu schreiben, und erinnert an wichtige Angaben. Es ersetzt aber keine Steuerberatung. Ob eine Rechnung alle Vorgaben erfüllt, verantworten Sie selbst. Fragen Sie im Zweifel Ihre Steuerberatung oder Ihr Finanzamt.

## Was auf eine Rechnung gehört

Unter anderem: Ihr Name und Ihre Anschrift, Name und Anschrift des Kunden, Ihre Steuernummer oder USt-IdNr., das Rechnungsdatum, eine fortlaufende Rechnungsnummer, was Sie geleistet haben und wann, der Betrag sowie der Steuersatz und die Steuer – oder bei Kleinunternehmern ein Hinweis, warum keine Steuer berechnet wird. Fehlt etwas davon, weist Abgerechnet Sie in der Vorschau darauf hin.

## E-Rechnung

Zwischen Unternehmen in Deutschland wird schrittweise die elektronische Rechnung (E-Rechnung) Pflicht. Gemeint ist eine Rechnung in einem besonderen, maschinenlesbaren Format – ein einfaches PDF zählt nicht dazu. Stand Herbst 2026 gilt:

- **Empfangen** können müssen alle Unternehmen E-Rechnungen schon seit 2025.
- **Verschicken** müssen ab 2027 Unternehmen mit mehr als 800.000 Euro Umsatz im Vorjahr E-Rechnungen, ab 2028 alle Unternehmen.
- **Ausgenommen** sind Kleinunternehmer, Rechnungen bis 250 Euro und Rechnungen an Privatpersonen.

Abgerechnet erzeugt derzeit PDF-Rechnungen. Die E-Rechnung ist für eine spätere Version geplant.

# lizenzen | Lizenzen

Abgerechnet ist kostenlos und freie Software unter der GNU General Public License v3.0. Sie dürfen es benutzen, weitergeben und verändern.

**Die einzige offizielle Quelle ist** `github.com/Schelawski/Abgerechnet`. Haben Sie für Abgerechnet bezahlt, haben Sie für etwas bezahlt, das es dort kostenlos gibt. Kopien aus anderen Quellen könnten verändert sein.

## Verwendete Bausteine

- **Microsoft Edge WebView2** (© Microsoft, BSD-Lizenz) zeigt die Vorschau an und erzeugt das PDF.
- **.NET** (© Microsoft, MIT-Lizenz) ist die Grundlage des Programms.
