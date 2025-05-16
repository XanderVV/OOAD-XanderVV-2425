# Product Requirements Document: BenchmarkTool

## 1. Introductie & Overzicht

* **Product:** BenchmarkTool is een C#/WPF desktopapplicatie bestaande uit twee delen: een portaal voor bedrijven en een beheerportaal voor administrators.
* **Doel:** Bedrijven in staat stellen om anoniem hun operationele kosten en belangrijke prestatie-indicatoren (KPI's) te vergelijken met sectorgenoten. Dit biedt inzicht in hun prestaties en potentiële verbeterpunten.
* **Probleem:** Bedrijven missen vaak een objectieve maatstaf om hun interne prestaties (kosten, efficiëntie) te vergelijken met de markt of concurrenten.
* **Oplossing:** Een beveiligd platform waar bedrijven jaarlijks data kunnen invoeren en geanonimiseerde benchmarkrapportages kunnen genereren met statistieken en visualisaties.

## 2. Doelstellingen

* Een functionele, robuuste en professioneel ogende C#/WPF-applicatie ontwikkelen conform de specificaties.
* Voldoen aan strikte architecturale en codeerstandaarden (OO, SQL-isolatie, naamgeving).
* Demonstreren van het vermogen om nieuwe .NET-technieken zelfstandig te verwerven en toe te passen (DB-interactie, hashing, dynamische UI, NuGet packages).
* Effectief gebruik maken van AI (ChatGPT) gedurende het gehele ontwikkelproces en dit documenteren.
* Simuleren van klantinteractie om requirements te verzamelen en te verfijnen.

## 3. Doelgroep

* **Bedrijfsgebruikers:** Medewerkers van deelnemende bedrijven die verantwoordelijk zijn voor het invoeren van jaarlijkse data en het analyseren van benchmarkresultaten.
* **Administrators:** Beheerders van het BenchmarkTool-platform, verantwoordelijk voor het beheren van bedrijfsaccounts en registratieverzoeken.

## 4. Functionele Vereisten

### 4.1. Class Library (`BenchmarkTool.ClassLibrary`)

* **Architectuur:** Dient als de enige laag voor data-access en centrale logica. Geen directe database-aanroepen vanuit de WPF-applicaties.
* **Data Klassen:** Definieert C# klassen (`Bedrijf`, `Jaarrapport`, `Kost`, `KostType`, `Vraag`, `Antwoord`, `Categorie`, `Nacecode`) die de databasetabellen (zie ERD/Databstructuur PDF's) representeren. Volgt OO-principes en naamgevingsconventies.
* **Database Connectiviteit:** Bevat logica voor het verbinden met de SQL Server database via de opgegeven connectiestring: `Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=BenchmarkDB;Integrated Security=True;Encrypt=False`.
* **Authenticatie & Autorisatie:**
    * Biedt methoden voor het valideren van bedrijfs- en admin-logins.
    * Admin login wordt gevalideerd tegen credentials opgeslagen in de `App.config` van de Admin App.
    * Bedrijfslogin wordt gevalideerd tegen de `Companies` tabel.
    * Implementeert paswoord hashing en verificatie met `Microsoft.AspNetCore.Identity.PasswordHasher<T>`.
* **Bedrijfsbeheer (voor Admin App):**
    * CRUD-operaties voor `Companies`.
    * Ophalen van registratieverzoeken (`status = 'Pending'`).
    * Bijwerken van bedrijfsstatus naar `'Active'` of `'Rejected'`. Bij goedkeuring wordt het door de admin ingestelde wachtwoord gehasht en opgeslagen.
    * Lezen en schrijven van bedrijfslogo's (`byte[]` voor `VARBINARY(MAX)`).
* **Jaarrapportbeheer (voor Company App):**
    * CRUD-operaties voor `Yearreports`, `Costs`, en `Answers`.
    * Zorgt ervoor dat enkel data gerelateerd aan `Questions` met `type != 'info'` wordt verwerkt.
* **Benchmarking Data (voor Company App):**
    * Biedt een methode om geanonimiseerde data (`fte`, `Costs.value`, `Answers.value` voor relevante `Questions`) van *andere* bedrijven op te halen.
    * Ondersteunt filtering op Jaar en NACE-code.
    * Ondersteunt groepering van NACE-codes op 2-, 3-, of 4/5-cijferig niveau.
    * Accepteert een lijst van geselecteerde indicatoren (`Questions.id`, `Costtypes.type`, `fte`) om enkel die data op te halen.
* **Stamgegevens:** Biedt methoden om lijsten op te halen van `Nacecodes`, `Categories`, `Questions`, `Costtypes`.
* **Foutafhandeling:** Robuuste `try-catch` blokken rond database-operaties.

### 4.2. Admin Applicatie (`BenchmarkTool.AdminApp`)

* **UI:** Professionele WPF-interface, volledig in het Nederlands. Navigatie via `Frame`/`Page`.
* **Authenticatie:**
    * Login-pagina (`AdminLoginPage.xaml`) met gebruikersnaam en `PasswordBox`.
    * Valideert via Class Library tegen vaste credentials in `App.config`.
* **Hoofdvenster/Dashboard (`AdminMainWindow.xaml`, `AdminDashboardPage.xaml`):**
    * Bevat `Frame` voor paginaweergave.
    * Menu (knoppen of `Menu`-control) voor navigatie naar beheersecties.
* **Bedrijvenbeheer (`BedrijvenBeheerPage.xaml`):**
    * Overzicht van geregistreerde bedrijven (tonen m.b.v. basiscontrols zoals `TextBlock` in een `Grid` of `StackPanel`).
    * Functionaliteit voor Toevoegen, Wijzigen, Verwijderen van bedrijven.
    * Formulier voor invoer/wijzigen bedrijfsgegevens, inclusief uploaden/koppelen van een logo (`.png`).
* **Registratiebeheer (`RegistratieBeheerPage.xaml`):**
    * Toont lijst van bedrijven met status `'Pending'`.
    * Knop "Goedkeuren": Stelt admin in staat initieel wachtwoord in te voeren, roept Class Library aan om wachtwoord te hashen, op te slaan en status naar `'Active'` te zetten.
    * Knop "Weigeren": Roept Class Library aan om status naar `'Rejected'` te zetten.

### 4.3. Company Applicatie (`BenchmarkTool.CompanyApp`)

* **UI:** Professionele WPF-interface, volledig in het Nederlands. Navigatie via `Frame`/`Page`. Toont bedrijfslogo na login.
* **Authenticatie & Registratie:**
    * Login-pagina (`CompanyLoginPage.xaml`) met gebruikersnaam, `PasswordBox`. Valideert via Class Library tegen `Companies` tabel.
    * Knop "Registreer nieuw bedrijf" leidt naar `RegistratiePage.xaml`.
    * Registratiepagina (`RegistratiePage.xaml`): Formulier voor invoer bedrijfsgegevens. Roept Class Library aan om record met `status = 'pending'` aan te maken (wachtwoord wordt later door admin ingesteld en gehasht).
* **Hoofdvenster/Dashboard (`CompanyMainWindow.xaml`, `CompanyDashboardPage.xaml`):**
    * Bevat `Frame` en menu.
    * Toont logo van ingelogd bedrijf.
* **Jaarrapportbeheer (`JaarrapportBeheerPage.xaml`):**
    * Overzicht van eigen jaarrapporten.
    * Functionaliteit voor aanmaken/wijzigen van jaarrapporten (`year`, `fte`).
    * Dynamisch gegenereerde invoervelden voor `Costs` en `Answers` (enkel voor `Questions.type != 'info'`) per `Category`. Gebruikt basiscontrols (`TextBox`, `ComboBox`, `DatePicker`/`Calendar` indien nodig).
* **Benchmarking (`BenchmarkPage.xaml`):**
    * **Selectie & Filters:** UI elementen om benchmark-indicatoren te selecteren, filterjaar te kiezen, en sector (NACE) te filteren. Bevat switch voor NACE-groepering (2, 3, 4/5 cijfers).
    * **Berekeningen:** Haalt data op via Class Library en berekent lokaal: Gemiddelde, Mediaan (verplicht); Percentielen (25-50-75), Standaardafwijking (optioneel/geavanceerd).
    * **Visualisaties (met LiveCharts):**
        * Kolom-/Staafdiagram: Eigen waarde vs. Gemiddelde.
        * Box-and-whisker plot: Percentielen en spreiding.
        * (Optioneel) Lijn-/Areagrafiek: Trends over tijd.
    * **Rapport Sterke/Zwakke Punten (Verplicht):** Tekstuele weergave die aangeeft waar het bedrijf significant afwijkt van de benchmark (gemiddelde/mediaan).
    * **(Optioneel) Extra Features:** Tijdlijnweergave, instellen van streefwaarden, PDF-export van het benchmarkrapport.

## 5. Non-Functional Requirements

* **Architectuur:** Strikte 3-lagen scheiding (WPF Apps - Class Library - Database). SQL-code enkel in Class Library.
* **Technologie:** C# (.NET Framework of .NET Core/5+), WPF, SQL Server LocalDB.
* **Bibliotheken:** Verplicht gebruik van `Microsoft.AspNetCore.Identity.PasswordHasher<T>` voor hashing en `LiveCharts` (of opvolger) voor visualisaties. Andere NuGet packages enkel indien nodig en toegestaan.
* **Code Kwaliteit:**
    * Volledig Object-Georiënteerd.
    * Volgt C# naamgevingsconventies (PascalCase, camelCase) en Hongaarse notatie voor UI controls.
    * Gebruik van StyleCop wordt aangeraden/vereist.
    * Code moet begrijpelijk, onderhoudbaar en efficiënt zijn. Elke regel code moet uitgelegd kunnen worden.
    * Geen gebruik van verboden technieken (databinding, `DataGrid`, `ListView`, `DataTable`, transacties, `UserControl`, `ExpandoObject`, `EventHandler`/`Action`, tenzij expliciet toegestaan).
* **UI/UX:**
    * Professionele en consistente look-and-feel voor beide applicaties.
    * Alle UI-teksten, labels, tooltips in het Nederlands.
    * Navigatie uitsluitend via `Frame`/`Page`.
    * Correct gebruik van WPF layout panels (`Grid`, `StackPanel`, etc.).
* **Security:**
    * Paswoorden worden veilig opgeslagen met BCrypt hashing en salt. Geen clear text paswoorden.
    * Admin credentials niet hardcoded in code, maar in `App.config` (idealiter gehasht).
    * Data voor benchmarks is geanonimiseerd.
* **Data:**
    * Database schema zoals voorzien in `ERD.pdf` en `databstructuur.pdf`.
    * Logo's worden als `VARBINARY(MAX)` opgeslagen (vooraf gegenereerde PNGs).
    * Connectiestring is vastgelegd.

## 6. AI Gebruik Vereisten

bekijk deze cursor rule voor meer informatie .cursor\rules\ai-gebruik.mdc

## 7. Data Model

* De databasestructuur wordt gedefinieerd door de aangeleverde documenten: `ERD BenchmarkTool.png` en `databsetructuur1.png, databasestructuur2.png en databastructuur3.png`.
* De Class Library zal klassen bevatten die deze structuur reflecteren. Logo's worden opgeslagen in `Companies.logo` als `VARBINARY(MAX)`.

## 8. Release Criteria & Deliverables

* Volledige, werkende C# code (Solution + 3 Projecten) die build zonder fouten.
* Alle functionele en non-functionele eisen geïmplementeerd zoals beschreven.
* Correcte connectiestring geconfigureerd.
* Volledige set PDF-exports van alle ChatGPT-sessies.
* Kort verslag over AI-gebruik.
* Indiening als één ZIP-bestand voor de deadline (vrijdag 30 mei 2025, 20u).
* Succesvolle mondelinge verdediging waarbij alle code en keuzes verklaard kunnen worden.

## 9. Openstaande Punten / Beslissingen tijdens Ontwikkeling

* Exacte afhandeling van geweigerde registraties (status 'Rejected' behouden vs. verwijderen). *Aanname nu: status 'Rejected'.*
* Precieze manier waarop admin initieel wachtwoord instelt bij goedkeuring registratie. *Aanname nu: invoerveld op adminpagina.*
* Keuze van specifieke LiveCharts grafiektypes en configuratie voor optimale weergave.