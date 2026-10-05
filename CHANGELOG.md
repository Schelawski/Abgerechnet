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
