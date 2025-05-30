# Node.js MSSQL Example

Dieses Beispiel zeigt, wie man mit Node.js eine Verbindung zu einem MS-SQL Server aufbaut und eine Stored Procedure ausführt. Bei erfolgreicher Ausführung wartet der Server auf die Beendigung der Stored Procedure und liefert das Ergebnis zurück.

## Voraussetzungen

- Node.js (mindestens Version 14)
- Zugriffsdaten für den MS-SQL Server

## Installation

1. Abhängigkeiten installieren (Internetverbindung erforderlich):
   ```bash
   npm install
   ```
2. Die Zugangsdaten in `index.js` im `config`-Objekt anpassen.
3. Optional: Den Namen der Stored Procedure im Aufruf `execute('MyProcedure')` anpassen.

## Starten

```bash
npm start
```

Der Server lauscht anschließend standardmäßig auf Port `3000`. Beim Aufruf der Startseite `/` wird die Stored Procedure ausgeführt.
