# Tilausjärjestelmä

## GitHub-linkki
- Lisää tähän: `<<https://github.com/mimosa1234/Tilausjarjestelma>>

## Sovelluksen käyttötarkoitus
Tilausjärjestelmä on WPF-sovellus tilausten, asiakkaiden, tuotteiden, kategorioiden ja varastosaldon hallintaan.
Sovelluksella voidaan lisätä, poistaa ja tarkastella tietoja sekä tehdä tilauksia niin, että varastosaldo päivittyy.

## Tietokannan luonti (SQL)
Tietokannan skriptit löytyvät kansiosta:
- `Tilausjarjestelma/TietokantaKoodit/create_database.sql`
- `Tilausjarjestelma/TietokantaKoodit/testidata.sql`

Ajo LocalDB:hen (sqlcmd):
```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i .\Tilausjarjestelma\TietokantaKoodit\create_database.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i .\Tilausjarjestelma\TietokantaKoodit\testidata.sql
```

## Tietokannan rakenne (lyhyt)
Taulut:
1. `Customers` (PK: `id`)
2. `Categories` (PK: `id`)
3. `Products` (PK: `id`, FK: `category_id -> Categories.id`)
4. `Orders` (PK: `id`, FK: `customer_id -> Customers.id`)
5. `OrderItems` (PK: `id`, FK: `order_id -> Orders.id`, FK: `product_id -> Products.id`)

## Sovelluksen toiminnan kuvaus
- Asiakkaat: lisäys, listaus, poisto (poisto estetty jos asiakkaalla tilauksia)
- Kategoriat: lisäys, listaus, poisto (poisto estetty jos kategoriassa tuotteita)
- Tuotteet: lisäys, listaus, poisto (poisto estetty jos tuotteella tilausrivejä)
- Tilaukset:
  - tilaukselle lisätään rivejä valitulle asiakkaalle
  - varastosaldo tarkistetaan ennen rivin hyväksyntää
  - tilauksen tallennus tehdään transaktiona
  - poisto palauttaa varastosaldot
- Varastosaldo: aseta/lisää/vähennä tuotekohtaisesti

## Käyttöliittymä
- Teemana käytössä MahApps.Metro (MetroWindow)
- Välilehtipohjainen käyttöliittymä eri toimintoihin

