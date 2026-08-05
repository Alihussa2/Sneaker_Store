# Performance-tests (Apache JMeter)

Fire testplaner mod `GET /api/sko` (læser kun data, ændrer intet i databasen):

- `load-test.jmx` — 20 brugere, jævn belastning
- `stress-test.jmx` — 150 brugere, gradvis optrapning over 30 sek.
- `spike-test.jmx` — 150 brugere næsten samtidig (2 sek. opstart)
- `soak-test.jmx` — 5 brugere i 2 minutter, tjekker for langsom degradering

## Forudsætninger

1. [Apache JMeter](https://jmeter.apache.org/) (testet med 5.6.3)
2. To plugins fra [jmeter-plugins.org](https://jmeter-plugins.org/) (bruges til "Ultimate Thread Group" og "Response Times Over Time"):
   - **Custom Thread Groups** (`jpgc-casutg`)
   - **3 Basic Graphs** (`jpgc-graphs-basic`)

   Nemmeste måde at installere dem: åbn JMeter → **Options → Plugins Manager** → find og installér de to pakker → genstart JMeter.

## Sådan køres en test

1. Start appen: `dotnet run --project Sneaker_Store --urls http://localhost:5083`
2. Kør testen fra JMeters `bin`-mappe:
   ```bash
   jmeter -n -t load-test.jmx -l resultater.jtl -e -o rapport-mappe
   ```
3. Åbn `rapport-mappe/index.html` i en browser for den fulde HTML-rapport (grafer, APDEX, fejlrate osv.) — brug den til skærmbilleder i jeres aflevering.

Vil du bygge/redigere en testplan visuelt, kan du åbne `.jmx`-filerne direkte i JMeters GUI (`jmeter -t <fil>.jmx`) — der vises Aggregate Report, Summary Report, View Results Tree og Response Times Over Time som separate faneblade, ligesom i kursusslides.
