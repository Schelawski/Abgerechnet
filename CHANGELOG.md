# Changelog

Alle wesentlichen Änderungen an Abgerechnet. Der Abschnitt einer Version wird zu ihren Release-Notes auf
GitHub (siehe [Release erstellen](README.md#release-erstellen)). Versionen folgen
[Semantic Versioning](https://semver.org/lang/de/).

## [Unreleased]

### Grundlage
- Projektgerüst: .NET 10, WinForms, eine einzige selbstständige `Abgerechnet.exe`, automatische Builds und
  Releases über GitHub Actions.
- Rechnungsordner: Rechnungen, Kunden und Einstellungen als lesbare JSON-Dateien in einem Ordner Ihrer Wahl,
  mit sicherem Speichern, Sicherung der vorigen Fassung und verständlicher Meldung bei beschädigten Dateien.
  Ordner wechseln über **Datei → Rechnungsordner wechseln…**.
- **Meine Daten:** Absender, Steuernummer bzw. USt-IdNr., Bankverbindung (mit IBAN-Prüfung),
  Kleinunternehmer nach § 19 UStG, Umsatzsteuersatz, Zahlungsziel, Nummernkreis, Muster für den
  PDF-Dateinamen (mit Beispiel) und Vorgaben für neue Positionen. Die Firma erscheint im Fenstertitel; solange
  die Absenderdaten fehlen, erinnert ein Hinweis im Hauptfenster daran.
- **Kunden:** Liste und Bearbeiten in einem Dialog (**Datei → Kunden…**) mit Firma, Kurzname (wird aus der
  Firma vorgeschlagen), Ansprechpartner, Anschrift und USt-IdNr. sowie einer Vorschau der Anschrift. Kunden mit
  Rechnungen lassen sich nicht löschen. Beim Erzeugen des PDFs wird die Anschrift in die Rechnung kopiert, damit
  spätere Änderungen gestellte Rechnungen nicht verändern.
- **Rechnungsliste** im Hauptfenster: Kacheln **Offen**, **Bezahlt** und **Gesamt** (Summe und Anzahl) für das
  gewählte Jahr – Gesamt zählt nur Offen und Bezahlt, Entwürfe werden getrennt gezählt. Tabelle mit Nr., Kunde,
  Zeitraum, Datum, Status und Bruttobetrag, neueste oben, Sortierung per Spaltenkopf, Filter nach Jahr und
  Status. Kontextmenü: PDF öffnen, im Ordner zeigen, Status ändern (auch für mehrere Rechnungen), Löschen.
  Nur Entwürfe lassen sich löschen; gestellte Rechnungen werden storniert, damit der Nummernkreis lückenlos
  bleibt.
- **Rechnung erstellen und bearbeiten:** **+ Neue Rechnung** (Strg+N) bzw. Doppelklick öffnet das Formular mit
  Nummer (automatisch fortlaufend, änderbar, keine Doppelten), Datum, Leistungszeitraum (Standard: Vormonat),
  Projekt, Kunde mit **Kunden verwalten…** daneben und Anschrift darunter, Positionen mit Zeitraum, Beschreibung,
  Zusatz, Menge, Einheit und Einzelpreis (Vorgaben aus „Meine Daten“) sowie live berechneten Summen. Positionen
  lassen sich hinzufügen, verschieben und entfernen (die letzte bleibt). Rückfrage bei ungespeicherten
  Änderungen. Gestellte Rechnungen öffnen schreibgeschützt; Bearbeiten nach Rückfrage.
- **Als neue Rechnung kopieren** (Kontextmenü) für den Monatsabschluss: übernimmt Kunde, Projekt und Positionen,
  setzt neue Nummer, heutiges Datum, Vormonat und Status Entwurf.
- **HTML-Vorlage mit Platzhaltern:** `{{name}}`-Platzhalter für Absender, Bankverbindung, Kunde, Rechnung und
  Beträge sowie die Blöcke `{{positionen_tabelle}}` und `{{summen_tabelle}}` mit sprechenden CSS-Klassen. Werte
  werden HTML-sicher eingesetzt, Platzhalter in HTML-Kommentaren bleiben unberührt, unbekannte Platzhalter werden
  gemeldet, relative Pfade beziehen sich auf den Vorlagen-Ordner.
- Jede Rechnung merkt sich Umsatzsteuersatz und Kleinunternehmer-Regelung, damit spätere Änderungen in „Meine
  Daten“ ihre Beträge nicht verändern. Beträge werden je Position und für die Steuer kaufmännisch gerundet.
