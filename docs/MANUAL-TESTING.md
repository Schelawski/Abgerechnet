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
| `Abgerechnet.exe` starten. | Der Einrichtungsassistent öffnet sich („Schritt 1 von 5“). |
| Assistent mit **Abbrechen** schließen. | Die App beendet sich ohne Fehlermeldung. `Abgerechnet.settings.json` existiert nicht. |
| Erneut starten, im Assistenten den Ordner `C:\Temp\Meine Rechnungen` eintragen, die Daten mit **Später eintragen** überspringen, Vorlage lassen, **Fertig** (ohne Haken bei „kopieren“). | Das Hauptfenster öffnet sich mittig. Fenstertitel `Abgerechnet 1.0.0` (bzw. die Version des Tags). Die Statusleiste zeigt den Rechnungsordner. |
| Klick auf den Ordner in der Statusleiste. | Der Explorer öffnet `C:\Temp\Meine Rechnungen` mit `abgerechnet.json`, `kunden.json`, `rechnungen.json`, `Vorlagen` und `PDF`. |
| `rechnungen.json` im Editor öffnen. | Eingerücktes JSON mit `"version": 1` und `"rechnungen": []`. |
| **Hilfe → Über Abgerechnet…** | Version, Hinweis „kostenlos“ und die offizielle Quelle werden angezeigt. |
| Fenster verkleinern. | Es lässt sich nicht kleiner als die Mindestgröße ziehen, nichts wird abgeschnitten. |
| Fenster verschieben und vergrößern, **Datei → Beenden**. | Die App schließt sich. `Abgerechnet.settings.json` liegt neben der exe. |
| Erneut starten. | Kein Assistent: Das Fenster öffnet sich mit demselben Ordner an derselben Stelle in derselben Größe. |
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
| **Neu**, Firma `Grünwald & Söhne GmbH` tippen. | Der Cursor steht in **Firma**. Die Liste zeigt den Namen schon beim Tippen; **Kurzname** füllt sich mit `Gruenwald-Soehne`. |
| Kurzname auf `Gruenwald` ändern, dann Firma weiter ändern. | Der Kurzname bleibt `Gruenwald` (eigene Eingabe wird nicht überschrieben). |
| Ansprechpartner, Straße, PLZ, Ort eintragen. | Die Vorschau „So steht die Anschrift auf der Rechnung“ zeigt die Zeilen, `&` wird korrekt angezeigt. |
| **Neu**, Firma leer lassen, **Speichern**. | Meldung, dass jeder Kunde eine Firma braucht; der leere Kunde ist ausgewählt. Firma `Beispiel AG` und Anschrift eintragen, **Speichern**. |
| Dialog erneut öffnen. | Kunden alphabetisch sortiert; `kunden.json` enthält beide lesbar, daneben `kunden.bak.json`. |
| Kunden auswählen, **Löschen** → **Ja**, dann **Abbrechen** → **Ja**. | Nach dem erneuten Öffnen ist der Kunde noch da – Abbrechen verwirft alle Änderungen. |
| App schließen, in `rechnungen.json` eine Rechnung mit `"kundeId"` eines Kunden eintragen, App starten, diesen Kunden löschen. | Hinweis, dass der Kunde Rechnungen hat und nicht gelöscht werden kann. |

## 5. Rechnungsliste

Vorbereitung: Einige Rechnungen mit dem Rechnungsformular anlegen (Abschnitt 6) oder von Hand in `rechnungen.json`,
z. B. je eine mit Status `entwurf`, `offen`, `bezahlt` und `storniert`, eine davon im Vorjahr, Positionen mit
`menge`, `einheit` und `einzelpreis`, `umsatzsteuersatz: 19`. Für eine Rechnung eine Datei in `PDF\` ablegen und
ihren Namen als `pdfDatei` eintragen.

| Schritt | Erwartet |
|---------|----------|
| App starten. | Jahr = aktuelles Jahr. Kacheln Offen / Bezahlt / Gesamt mit Summe (brutto) und Anzahl; Gesamt = Offen + Bezahlt. Rechts daneben „2 Entwürfe“ o. ä. Stornierte Rechnungen sind grau. Neueste oben. |
| Beträge nachrechnen. | Menge × Einzelpreis je Position auf Cent gerundet, plus 19 % USt (gerundet). |
| Bei 150 % Skalierung. | Alle Spalten lesbar, Datum nicht abgeschnitten; die Spalte Kunde füllt die Breite. |
| Jahr auf das Vorjahr bzw. **Alle Jahre**. | Liste und Kacheln passen sich an. |
| Status-Filter **Offen**. | Nur offene Rechnungen; Kacheln bleiben für das ganze Jahr. Filter ohne Treffer: „Keine Rechnungen für diese Auswahl.“ |
| Spaltenköpfe Nr., Kunde, Betrag anklicken, jeweils zweimal. | Sortierung wechselt; Nr. natürlich sortiert (9 vor 10). |
| Rechtsklick auf eine offene Rechnung. | Menü: PDF öffnen (nur aktiv, wenn es ein PDF gibt), Im Ordner zeigen, Status ändern ▸ (Offen ausgegraut), Löschen. |
| **Status ändern → Bezahlt**. | Status grün, Kacheln aktualisiert; in `rechnungen.json` `"bezahltAm"` mit heutigem Datum. Zurück auf **Offen** entfernt `bezahltAm`. |
| **Status ändern → Storniert**. | Rückfrage mit Erklärung; danach grau, zählt nicht mehr in den Kacheln. |
| Offene Rechnung markieren, **Entf**. | Hinweis „Nur Entwürfe können gelöscht werden …“. |
| Entwurf markieren, **Entf** → **Ja**. | Entwurf verschwindet, Entwurfsanzahl sinkt. |
| Mehrere Rechnungen mit Strg+Klick markieren, **Status ändern → Bezahlt**. | Alle, bei denen das erlaubt ist, werden bezahlt. |
| Rechnung mit PDF: **PDF öffnen** / **Im Ordner zeigen**. | PDF öffnet sich im Standardprogramm bzw. Explorer mit markierter Datei. |
| Unter **Datei → Kunden…** einen Kunden umbenennen und speichern. | Entwürfe zeigen den neuen Namen sofort. |

## 6. Rechnung erstellen und bearbeiten

| Schritt | Erwartet |
|---------|----------|
| In „Meine Daten“ als Vorgabe Beschreibung `Softwareentwicklung`, Einheit `Std.`, Einzelpreis `88` eintragen. **+ Neue Rechnung** (oder Strg+N). | Formular „Rechnung …“: Nummer = höchste vorhandene + 1 (bzw. „Nächste Rechnungsnummer“ aus den Einstellungen, in einem leeren Ordner `JJJJ-001`), Datum heute, Leistungszeitraum Vormonat (z. B. „September 2026“), Status „Entwurf“. Eine Position mit den Vorgaben. |
| Kunde wählen. | Anschrift erscheint darunter. |
| **Kunden verwalten…**, neuen Kunden anlegen, **Speichern**. | Der neue Kunde ist in der Rechnung ausgewählt. |
| Mit Tab in **Menge** springen, `12,5` tippen. | Der Inhalt wird ersetzt (nicht „12,50,00“). Betrag der Position und Summen ändern sich bei jedem Tastendruck. |
| **+ Position hinzufügen** zweimal. | Neue Positionen mit den Vorgaben, gleich groß wie die erste (auch bei 150 % Skalierung); der Cursor steht in der Beschreibung. Zeitraum leer zeigt grau den Leistungszeitraum der Rechnung. |
| ▲ / ▼ / ✕ an den Positionen. | Reihenfolge ändert sich; die letzte Position lässt sich nicht entfernen (▲ der ersten und ▼ der letzten ausgegraut). |
| Summen prüfen: 12,5 × 95 €. | Netto 1.187,50 €, USt 19 % 225,63 €, Rechnungsbetrag 1.413,13 €. Bei Kleinunternehmer (Rechnung neu anlegen, nachdem der Schalter in „Meine Daten“ gesetzt wurde): nur Rechnungsbetrag und Hinweis „Keine Umsatzsteuer …“. |
| Nummer auf eine vorhandene Nummer ändern, **Speichern**. | Meldung „… ist bereits vergeben“. |
| Nummer zurück, Strg+S. | Formular schließt, die Rechnung ist in der Liste markiert; Jahr-/Statusfilter wechseln, falls sie sonst nicht sichtbar wäre. |
| Rechnung erneut öffnen (Doppelklick), etwas ändern, **Abbrechen**. | Rückfrage „Speichern?“ (Ja / Nein / Abbrechen). Ohne Änderung schließt das Formular ohne Rückfrage. |
| Offene oder bezahlte Rechnung öffnen. | Gelber Hinweis „bereits gestellt … schreibgeschützt“, alle Felder gesperrt, Kunde mit Namen, **Schließen** statt Speichern. **Bearbeiten…** fragt nach; danach editierbar. |
| Rechtsklick auf eine Rechnung → **Als neue Rechnung kopieren**. | Formular mit neuer Nummer, heutigem Datum, Vormonat, Entwurf; Kunde, Projekt und Positionen übernommen (ohne eigene Positions-Zeiträume). Erst **Speichern** legt sie an. |

## 7. PDF erzeugen

Mit der veröffentlichten exe testen (`dotnet publish src/Abgerechnet -c Release`), damit auch die in die exe
eingebettete WebView2-Bibliothek geprüft wird.

| Schritt | Erwartet |
|---------|----------|
| Entwurf öffnen, **PDF erzeugen…** (Alt+P). | Die Rechnung wird gespeichert, die Vorschau öffnet sich und passt auf den Bildschirm. Nach wenigen Sekunden erscheint das PDF (Seitenzahl im Viewer „1 von 1“). Fehlen Angaben (z. B. Steuernummer), steht oben ein gelber Hinweis. |
| Unten links. | „Vorlage: klassisch.html aus dem Vorlagen-Ordner“. |
| PDF ansehen. | DIN A4 mit Rändern, Umlaute und € korrekt, Anschrift des Kunden links, Absender rechts, Fußzeile mit Bank und Steuernummer unten. |
| `logo.png` in den Ordner `Vorlagen` legen, Vorschau erneut öffnen. | Das Logo erscheint oben rechts. |
| **PDF speichern** (Alt+S). | Unten „Gespeichert: …pdf“, **PDF öffnen** und **Im Ordner zeigen** erscheinen. Die Datei liegt im Ordner `PDF` mit dem Namen nach Muster. |
| Vorschau schließen. | Das Formular schließt; in der Liste steht die Rechnung auf **Offen**. In `rechnungen.json` stehen `pdfDatei` und `empfaenger` (Kopie der Anschrift). |
| Kundenanschrift ändern, gestellte Rechnung öffnen. | Die Rechnung zeigt weiterhin die alte Anschrift. |
| Rechtsklick → **PDF erzeugen…** auf dieselbe Rechnung, **PDF speichern**. | Rückfrage „gibt es bereits … Überschreiben?“. Der Status bleibt (z. B. Bezahlt). |
| PDF in einem PDF-Programm geöffnet lassen, erneut speichern und überschreiben. | Verständliche Meldung, falls das Programm die Datei sperrt; nichts halb Geschriebenes. |
| Eine Rechnung mit 30+ Positionen erzeugen. | Mehrere Seiten; Tabellenkopf und Fußzeile auf jeder Seite, keine Überlappung, keine Position wird zerrissen. |
| `%LOCALAPPDATA%\Abgerechnet` ansehen. | Ordner `WebView2` (Browserdaten); im Ordner `Vorschau` bleiben nach dem Schließen keine PDFs liegen. Neben der exe liegen keine neuen Dateien außer `Abgerechnet.settings.json`. |

## 8. Vorlagen

| Schritt | Erwartet |
|---------|----------|
| Neuen leeren Rechnungsordner anlegen und öffnen. | Im Ordner `Vorlagen` liegen `klassisch.html`, `modern.html`, `schlicht.html`, aber kein Logo. |
| Für eine Rechnung mit drei Positionen (eine mit Zusatz) nacheinander alle drei Vorlagen wählen und **PDF erzeugen…**. | Jeweils eine saubere A4-Seite; „Klassisch“ mit grauem Titelbalken, Rahmen und Zebrastreifen; „Modern“ mit blauem Rechnungsbetrag; „Schlicht“ ohne Farben. Leere Angaben (z. B. ohne Telefon) hinterlassen keine leeren Beschriftungen. |
| Dasselbe als Kleinunternehmer (Schalter in „Meine Daten“, dann neue Rechnung). | Nur „Rechnungsbetrag“, darunter bzw. daneben der § 19-Hinweis. |
| **Meine Daten → Rechnungen → Vorlage** auf „Modern“, neue Rechnung. | Im Rechnungsformular steht „Standard (Modern)“; die Vorschau nutzt Modern. |
| `klassisch.html` im Editor ändern (z. B. `--titelbalken: #ffd700;`), Vorschau öffnen. | Die Änderung ist sichtbar; unten links „klassisch.html aus dem Vorlagen-Ordner“. |
| Datei `meine.html` (Kopie einer Vorlage) in den Ordner legen. | „meine“ erscheint in beiden Auswahlfeldern. |
| `meine.html` wählen, Datei löschen, Vorschau öffnen. | Hinweis „gibt es nicht mehr; verwendet wird Klassisch“; das PDF entsteht trotzdem. |
| **Vorlagen → Original wiederherstellen → Klassisch** → **Ja**. | Meldung mit „klassisch.bak.html“; die eigene Fassung liegt als `klassisch.bak.html` daneben, `klassisch.html` ist wieder das Original. `.bak.html` erscheint nicht in der Auswahl. |
| **Vorlagen → Vorlagen-Ordner öffnen**. | Explorer mit dem Ordner `Vorlagen`. |
| Jahresfilter in der Liste aufklappen. | Letzter Eintrag „Alle Jahre“. |

## 9. Hilfe

| Schritt | Erwartet |
|---------|----------|
| Im Hauptfenster **F1** (oder **Hilfe → Hilfe**). | Hilfefenster mit 13 Themen links; „Erste Schritte“ ist ausgewählt. Das Fenster passt auf den Bildschirm. |
| Thema „Was ist Abgerechnet?“. | Abschnitte „Ihre Daten bleiben bei Ihnen“ und „Bewusst einfach“. |
| In **Meine Daten**, **Kunden**, im Rechnungsformular **F1** drücken. | Die Hilfe springt zum passenden Thema; es öffnet sich kein zweites Hilfefenster. |
| Thema „Vorlage anpassen“, nach unten scrollen. | Liste aller Platzhalter nach Gruppen, jeweils mit Erklärung. |
| **Vorlagen → Mit KI anpassen…** | Thema „Vorlage mit KI anpassen“; unten der Text für die KI im grauen Kasten und die Schaltfläche **Prompt mit Vorlage kopieren**. |
| **Prompt mit Vorlage kopieren**, dann in den Editor einfügen. | Meldung nennt die Standardvorlage. Eingefügt wird der Prompt, am Ende die vollständige HTML-Vorlage (keine persönlichen Daten). |
| Hilfe mit **Esc** schließen. | Das Fenster schließt sich. |

## 10. Einrichtungsassistent

Vorher `Abgerechnet.settings.json` neben der exe löschen (oder die exe in einen neuen Ordner kopieren).

| Schritt | Erwartet |
|---------|----------|
| `Abgerechnet.exe` aus dem Ordner `Downloads` starten. | „Schritt 1 von 5“, Willkommen, grüner Kasten „Ihre Daten bleiben auf Ihrem Computer…“. Kein Zurück-Knopf. |
| F1 auf jeder Seite. | Die Hilfe öffnet ein passendes Thema (Was ist Abgerechnet?, Rechnungsordner, Meine Daten, Vorlagen, Erste Schritte). |
| **Weiter**. | Vorschlag `Dokumente\Rechnungen` mit „Dieser Ordner wird neu angelegt.“ (bzw. „Rechnungen (Abgerechnet)“, wenn `Rechnungen` schon andere Dateien enthält). Tipp zur Sicherung. |
| Pfad leeren, **Weiter**. | Hinweis „Bitte wählen Sie einen Ordner.“ |
| **Ändern…** → einen Ordner mit anderen Dateien wählen. | Orangefarbener Hinweis, dass Abgerechnet seine Dateien dazulegt. |
| Neuen Ordner eintragen, **Weiter**. | Der Ordner ist angelegt, „Schritt 3 von 5“: Ihre Daten mit den Seiten „Absender“ und „Steuer und Bank“ (ohne „Rechnungen“), Cursor im Feld Firma. |
| Nur Firma eintragen, **Weiter**. | Hinweis auf die fehlende Anschrift, die Seite bleibt. |
| Anschrift ergänzen, Kleinunternehmer ankreuzen, **Weiter** (Rückfrage zur Steuernummer mit Ja). | „Schritt 4 von 5“: drei Vorlagen, rechts nach kurzer Zeit eine ganze A4-Seite mit Ihrer Firma und „Nordlicht GmbH“, ohne Werkzeugleiste. Kleinunternehmer: keine Umsatzsteuer, Hinweis auf § 19 UStG. |
| Vorlage „Modern“ und „Schlicht“ wählen, schnell hin und her klicken. | Die Vorschau zeigt am Ende die gewählte Vorlage. |
| **Logo wählen…** → ein JPG wählen. | „logo.png“, **Logo entfernen** erscheint, die Vorschau zeigt das Logo. Im Ordner `Vorlagen` liegt `logo.png`. |
| **Logo entfernen**. | Logo verschwindet aus der Vorschau und dem Ordner. |
| **Zurück**, **Zurück**. | Der Ordner wird mit „Der Ordner ist leer – gut geeignet.“ angezeigt (nicht als vorhandener Rechnungsordner); **Weiter** zeigt die eingetragenen Daten wieder. |
| Bis „Fertig!“ weiter. | „Schritt 5 von 5“, **Erste Rechnung erstellen** und **Fertig**, kein Abbrechen. Ankreuzfeld „Abgerechnet in meinen Benutzerordner kopieren…“ ist gesetzt. |
| **Erste Rechnung erstellen**. | Der Assistent schließt sich, Abgerechnet startet aus `%LOCALAPPDATA%\Abgerechnet` mit demselben Rechnungsordner und öffnet sofort eine neue Rechnung. Auf dem Desktop und im Startmenü gibt es „Abgerechnet“. Die gewählte Vorlage steht in „Meine Daten“ → „Rechnungen“. |
| Abgerechnet aus einem Ordner außerhalb von Downloads/Desktop erneut einrichten. | Das Ankreuzfeld ist nicht gesetzt; ohne Haken öffnet sich das Hauptfenster aus diesem Ordner. |
| Mit leeren Einstellungen starten und einen vorhandenen Rechnungsordner wählen. | „In diesem Ordner liegen schon Daten…“, **Weiter** springt zu „Schritt 3 von 3“; **Fertig** (ohne „Erste Rechnung erstellen“, wenn es schon Rechnungen gibt). |
| Assistent nach der Ordnerseite schließen (X). | Das Hauptfenster öffnet sich mit dem gewählten Ordner; ohne Daten erscheint der gelbe Hinweis. |
| **Datei → Einrichtungsassistent…** im Hauptfenster. | Der Assistent startet mit dem offenen Ordner („vorhandene Daten“). Ein anderer Ordner wird nach **Fertig** im Hauptfenster geöffnet. |
| Ohne WebView2 Runtime (falls testbar). | Die Vorlagen-Seite erklärt, dass die Vorschau nicht möglich ist; Weiter funktioniert. |

## 11. Erinnerung an offene Rechnungen

Vorbereitung: In `rechnungen.json` (App geschlossen) bei zwei offenen Rechnungen das `datum` auf ein Datum vor mehr als
30 Tagen setzen, bei einer dritten auf genau 30 Tage vor heute, einen Entwurf auf 8 Tage vor heute.

| Schritt | Erwartet |
|---------|----------|
| App starten. | Keine Rückfrage. Über der Liste ein gelber Hinweis „2 Rechnungen sind seit über 30 Tagen offen. Schon bezahlt?“ mit **Zahlungseingang erfassen…** und „1 Entwurf ist älter als 7 Tage …“ mit **Öffnen**. |
| Liste ansehen. | Die zwei Rechnungen stehen als „Offen – überfällig“ in Rot; die Rechnung von genau vor 30 Tagen nur als „Offen“. Tooltip über einer überfälligen Zeile nennt die Tage. Spalte „Bezahlt am“ vorhanden. |
| **Öffnen** im Hinweis. | Der Entwurf öffnet sich im Formular. |
| ✕ im Hinweis. | Der Hinweis verschwindet. Nach einem Neustart ist er wieder da. |
| **Zahlungseingang erfassen…** | Liste der zwei Rechnungen mit Nr., Kunde, Zeitraum, Betrag, „Offen seit“ und Datum (heute). **Als bezahlt speichern** ist ausgegraut. |
| Bei einer Rechnung das Datum ändern. | Der Haken wird automatisch gesetzt, der Knopf wird aktiv. |
| **Alle markieren**, **Als bezahlt speichern**. | Beide Rechnungen sind „Bezahlt“, „Bezahlt am“ zeigt die gewählten Daten, der erste Satz im Hinweis verschwindet. Die Kachel „Bezahlt“ stimmt. |
| Zwei offene Rechnungen markieren, Rechtsklick → **Als bezahlt markieren…**, gestriges Datum. | Kurzer Dialog nur mit Datum (kein Datum in der Zukunft wählbar). Danach beide „Bezahlt“ mit gestrigem Datum. |
| Rechtsklick → **Status ändern → Bezahlt**. | Derselbe Datumsdialog. |
| **Status ändern → Offen** bei einer bezahlten Rechnung. | „Bezahlt am“ ist wieder leer. |
| In „Meine Daten“ das Zahlungsziel auf 14 Tage stellen, speichern. | Hinweis und Rotfärbung passen sich sofort an. |
| Nach „Bezahlt am“ sortieren. | Neueste Zahlung oben, unbezahlte unten. |
