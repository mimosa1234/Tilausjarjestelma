# MSIX-paketointi (Visual Studio)

Tama projekti on WPF (.NET 8), joten kayta Windows Application Packaging Project -mallia Visual Studiossa.

## Esivaatimukset
1. Visual Studio 2022
2. Workloadit:
   - .NET desktop development
   - Universal Windows Platform development (tarvitaan Packaging Project -templateen)
3. Windows 10/11 SDK asennettuna

## Vaihe 1: Lisaa packaging-projekti
1. Avaa solution Visual Studiossa.
2. Right click solution -> Add -> New Project.
3. Valitse `Windows Application Packaging Project`.
4. Nimea esimerkiksi `Tilausjarjestelma.Package`.
5. Valitse Target/Min version samaksi tai yhteensopivaksi kuin kehityskoneessa.

## Vaihe 2: Viittaa WPF-projektiin
1. Packaging-projektissa Dependencies -> Add Reference.
2. Valitse `Tilausjarjestelma` (WPF-projekti).

## Vaihe 3: Maarita paketin tiedot
1. Avaa `Package.appxmanifest`.
2. Maarita:
   - Display name
   - Publisher
   - Version
   - Logo (halutessa)

## Vaihe 4: Luo allekirjoitussertifikaatti
1. Right click packaging-projekti -> Publish -> Create App Packages.
2. Valitse sideloading (ei Store).
3. Luo uusi testisertifikaatti (Create...).
4. Muista salasana ja talleta sertifikaatti.

## Vaihe 5: Rakenna MSIX
1. Build configuration: `Release`, platform esim. `x64`.
2. Right click packaging-projekti -> Publish -> Create App Packages.
3. Buildin jalkeen paketti loytyy kansiosta:
   - `Tilausjarjestelma.Package\AppPackages\...\*.msix`

## Vaihe 6: Testaa asennus
1. Avaa `.msix` kohdekoneessa.
2. Asenna tarvittaessa sertifikaatti Trusted People -storeen.
3. Kaynnista sovellus ja testaa CRUD + tilaus + varastosaldo.

## Huomio tietokannasta
Sovellus kayttaa LocalDB-yhteytta. Varmista kohdekoneella:
1. SQL Server LocalDB on asennettu.
2. Tietokanta alustetaan SQL-skripteilla:
   - `create_database.sql`
   - `testidata.sql`

## Lahde
- Microsoft Learn: Setup your desktop app for MSIX packaging in Visual Studio
  https://learn.microsoft.com/en-us/windows/msix/desktop/vs-package-overview
- Microsoft Learn: (linkkaamasi ohje)
  https://learn.microsoft.com/fi-fi/windows/msix/desktop/desktop-to-uwp-packaging-dot-net
