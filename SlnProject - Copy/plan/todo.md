# Todo Lijst: BenchmarkTool Project (Hiërarchisch)

## Fase 0: Setup & Voorbereiding

0.1 [ ] **Database Opzetten:**
    -   [ ] Definieer de juiste datatypes, primary keys, foreign keys en constraints. Let specifiek op `Companies.logo` als `VARBINARY(MAX)`.
    -   [ ] Vul de stamgegevens tabellen (`Nacecodes`, `Categories`, `Questions`, `Costtypes`) met initiële data indien nodig/mogelijk.

0.2 [ ] **Visual Studio Solution Opzetten:**
    -   [ ] Maak een nieuwe Solution aan.
    -   [ ] Voeg drie C# projecten toe:
        * `BenchmarkTool.ClassLibrary` (Class Library)
        * `BenchmarkTool.AdminApp` (WPF Applicatie)
        * `BenchmarkTool.CompanyApp` (WPF Applicatie)
    -   [ ] Stel projectreferenties in: AdminApp -> ClassLibrary, CompanyApp -> ClassLibrary.

0.3 [ ] **NuGet Packages Installeren:**
    -   [ ] Installeer `Microsoft.AspNetCore.Identity.PasswordHasher` in `BenchmarkTool.ClassLibrary`.
    -   [ ] Installeer `LiveCharts.Wpf` (of opvolger) in `BenchmarkTool.CompanyApp`.
    -   [ ] Installeer eventueel `System.Data.SqlClient` of `Microsoft.Data.SqlClient` in `BenchmarkTool.ClassLibrary`.

0.4 [ ] **Configuratie:**
    -   [ ] Sla de connectiestring op (`App.config` of andere methode). `Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=BenchmarkDB;Integrated Security=True;Encrypt=False`
    -   [ ] Configureer admin credentials in `BenchmarkTool.AdminApp`'s `App.config` (idealiter gehasht).

## Fase 1: BenchmarkTool.ClassLibrary Ontwikkeling

1.1 [ x ] **Data Klassen Definiëren:**
    -   [ ] Maak C# klasse `Bedrijf.cs`.
    -   [ ] Maak C# klasse `Jaarrapport.cs`.
    -   [ ] Maak C# klasse `Kost.cs`.
    -   [ ] Maak C# klasse `KostType.cs`.
    -   [ ] Maak C# klasse `Vraag.cs`.
    -   [ ] Maak C# klasse `Antwoord.cs`.
    -   [ ] Maak C# klasse `Categorie.cs`.
    -   [ ] Maak C# klasse `Nacecode.cs`.
    -   [ ] Implementeer properties conform databasekolommen voor alle klassen.
    -   [ ] Pas OO-principes en C# naamgevingsconventies (PascalCase) toe.

1.2 [ x ] **Database Connectiviteit Implementeren:**
    -   [ ] Creëer een centrale (helper) klasse/methode voor `SqlConnection` en commando executie.
    -   [ ] Implementeer een methode voor SELECT queries (bv. teruggeven van `DataTable` of lijsten van objecten).
    -   [ ] Implementeer een methode voor INSERT/UPDATE/DELETE queries (bv. teruggeven van aantal beïnvloede rijen).
    -   [ ] Implementeer robuuste `try-catch` blokken rond alle database interacties in de helper methodes.

1.3 [ x ] **Authenticatie & Autorisatie Logica:**
    -   [ ] Implementeer methode `ValidateAdminCredentials(string username, string password)` (leest uit config).
    -   [ ] Implementeer methode `ValidateCompanyCredentials(string username, string password)` (query `Companies`).
    -   [ ] Implementeer methode `HashPassword(string password)` met `PasswordHasher`.
    -   [ ] Implementeer methode `VerifyPassword(string hashedPassword, string providedPassword)` met `PasswordHasher`.

1.4 [ x ] **Bedrijfsbeheer Logica (voor Admin App):**
    -   [ ] Implementeer methode `CreateCompany(Bedrijf bedrijf)`.
    -   [ ] Implementeer methode `GetCompanyById(int id)`.
    -   [ ] Implementeer methode `GetCompanyByUsername(string username)`.
    -   [ ] Implementeer methode `GetAllCompanies()`.
    -   [ ] Implementeer methode `UpdateCompany(Bedrijf bedrijf)` (inclusief logo update).
    -   [ ] Implementeer methode `DeleteCompany(int id)`.
    -   [ ] Implementeer methode `GetPendingRegistrations()` (filter `Companies` op `status = 'Pending'`).
    -   [ ] Implementeer methode `ApproveRegistration(int companyId, string initialPassword)` (hash & update status).
    -   [ ] Implementeer methode `RejectRegistration(int companyId)` (update status).
    -   [ ] Implementeer methode `GetCompanyLogo(int companyId)` -> `byte[]`.
    -   [ ] Implementeer methode `UpdateCompanyLogo(int companyId, byte[] logo)`.

1.5 [ x ] **Jaarrapportbeheer Logica (voor Company App):**
    -   [x] Implementeer methode `CreateYearreport(Jaarrapport report)`.
    -   [x] Implementeer methode `GetYearreportsByCompany(int companyId)` -> `List<Jaarrapport>`.
    -   [x] Implementeer methode `GetYearreportDetails(int reportId)` (om Costs/Answers op te halen).
    -   [x] Implementeer methode `UpdateYearreport(Jaarrapport report)`.
    -   [x] Implementeer methode `DeleteYearreport(int reportId)`.
    -   [x] Implementeer methode `SaveCostsForReport(int reportId, List<Kost> costs)`.
    -   [x] Implementeer methode `SaveAnswersForReport(int reportId, List<Antwoord> answers)`.
    -   [x] Implementeer filter in relevante methodes: Verwerk enkel data voor `Questions` met `type != 'info'`.

1.6 [ x ] **Benchmarking Data Logica (voor Company App):**
    -   [ ] Implementeer methode `GetBenchmarkData(int currentCompanyId, int year, string naceFilter, NaceGroupingLevel groupingLevel, List<string> selectedIndicators)`:
        * Haal data op (`fte`, `Costs.value`, `Answers.value`) van ANDERE bedrijven.
        * Filter op Jaar (`Yearreports.year`).
        * Filter op NACE-code (`Companies.nacecode`).
        * Implementeer NACE-groepering (substring op 2, 3 of 4/5 cijfers).
        * Filter op geselecteerde indicatoren (`Questions.id`, `Costtypes.type`, `'fte'`).
        * Zorg voor anonimisering (geen bedrijfsnamen/ID's).

1.7 [ x ] **Stamgegevens Ophalen:**
    -   [ ] Implementeer methode `GetAllNacecodes()`.
    -   [ ] Implementeer methode `GetAllCategories()`.
    -   [ ] Implementeer methode `GetAllQuestions()`.
    -   [ ] Implementeer methode `GetAllCosttypes()`.

1.8 [ ] **Code Kwaliteit & Architectuur:**
    -   [ ] Verifieer continu dat alle SQL-code binnen de Class Library blijft.
    -   [ ] Refactor code regelmatig voor duidelijkheid, onderhoudbaarheid en efficiëntie.
    -   [ ] Pas StyleCop toe (indien vereist/gewenst) en los waarschuwingen op.

## Fase 2: BenchmarkTool.AdminApp Ontwikkeling

2.1 [ ] **Basis UI Structuur:**
    -   [ ] Maak `AdminMainWindow.xaml` (met `Frame` control `x:Name="MainFrame"`).
    -   [ ] Maak `AdminLoginPage.xaml` (Username `TextBox`, Password `PasswordBox`, Login knop).
    -   [ ] Maak `AdminDashboardPage.xaml` (Menu/knoppen voor navigatie).
    -   [ ] Maak `BedrijvenBeheerPage.xaml` (Layout voor lijst, CRUD knoppen).
    -   [ ] Maak `RegistratieBeheerPage.xaml` (Layout voor lijst, Goedkeur/Weiger knoppen).

2.2 [ ] **Navigatie Implementeren:**
    -   [ ] Implementeer `Frame.Navigate()` logica vanuit `AdminMainWindow` of `AdminDashboardPage`.

2.3 [ ] **Authenticatie Implementeren:**
    -   [ ] Koppel Login knop aan Class Library `ValidateAdminCredentials`.
    -   [ ] Bij succesvolle login, navigeer naar `AdminDashboardPage`.
    -   [ ] Toon foutmelding bij mislukte login.

2.4 [ ] **Bedrijvenbeheer UI & Logica:**
    -   [ ] Toon lijst van bedrijven (gebruik `ItemsControl` met `DataTemplate`, haal data op via `GetAllCompanies`).
    -   [ ] Implementeer "Toevoegen": Open venster/formulier, verzamel data, roep `CreateCompany` aan.
    -   [ ] Implementeer "Wijzigen": Selecteer bedrijf, open venster/formulier met data, roep `UpdateCompany` aan.
    -   [ ] Implementeer "Verwijderen": Selecteer bedrijf, bevestig, roep `DeleteCompany` aan.
    -   [ ] Implementeer Logo Upload/Weergave (`OpenFileDialog`, `Image` control, `GetCompanyLogo`, `UpdateCompanyLogo`).
    -   [ ] Refresh de lijst na CUD operaties.

2.5 [ ] **Registratiebeheer UI & Logica:**
    -   [ ] Toon lijst van bedrijven met `status = 'Pending'` (roep `GetPendingRegistrations` aan).
    -   [ ] Implementeer "Goedkeuren":
        * Selecteer bedrijf.
        * Toon invoerveld/dialoog voor initieel wachtwoord.
        * Roep Class Library `ApproveRegistration` aan.
        * Refresh de lijst.
    -   [ ] Implementeer "Weigeren":
        * Selecteer bedrijf.
        * Roep Class Library `RejectRegistration` aan.
        * Refresh de lijst.

2.6 [ ] **UI Afwerking:**
    -   [ ] Zorg voor een professionele look-and-feel (styles, resources).
    -   [ ] Gebruik correcte WPF layout panels (`Grid`, `StackPanel`, etc.).
    -   [ ] Alle UI-tekst in het Nederlands.
    -   [ ] Pas Hongaarse notatie toe voor UI controls (`btnSubmit`, `txtUsername`, `lstCompanies`).

## Fase 3: BenchmarkTool.CompanyApp Ontwikkeling

3.1 [ ] **Basis UI Structuur:**
    -   [ ] Maak `CompanyMainWindow.xaml` (met `Frame` `MainFrame` en `Image` voor logo).
    -   [ ] Maak `CompanyLoginPage.xaml` (Username, Password, Login knop, Registreer knop).
    -   [ ] Maak `RegistratiePage.xaml` (Formulier voor bedrijfsgegevens).
    -   [ ] Maak `CompanyDashboardPage.xaml` (Menu/knoppen voor navigatie).
    -   [ ] Maak `JaarrapportBeheerPage.xaml` (Lijst rapporten, CRUD knoppen, gebied voor dynamische velden).
    -   [ ] Maak `BenchmarkPage.xaml` (Filters, selecties, grafieken, rapport gebied).

3.2 [ ] **Navigatie Implementeren:**
    -   [ ] Implementeer `Frame.Navigate()` vanuit `CompanyMainWindow` of `CompanyDashboardPage`.

3.3 [ ] **Authenticatie & Registratie Implementeren:**
    -   [ ] Koppel Login knop aan Class Library `ValidateCompanyCredentials`.
    -   [ ] Bij succes: Haal bedrijfslogo op (`GetCompanyLogo`), toon in `CompanyMainWindow`, sla ingelogd bedrijf op, navigeer naar `CompanyDashboardPage`.
    -   [ ] Bij falen: Toon foutmelding.
    -   [ ] Koppel "Registreer nieuw bedrijf" knop aan navigatie naar `RegistratiePage.xaml`.
    -   [ ] Implementeer `RegistratiePage` logica: Verzamel data, roep `CreateCompany` aan (status='Pending'), navigeer terug/toon bericht.

3.4 [ ] **Jaarrapportbeheer UI & Logica:**
    -   [ ] Toon lijst van eigen jaarrapporten (`GetYearreportsByCompany`).
    -   [ ] Implementeer Toevoegen/Wijzigen van `Jaarrapport` (`year`, `fte`).
    -   [ ] **Dynamische Invoervelden:**
        * Haal `Categories`, `Questions` (`type != 'info'`), `Costtypes` op.
        * Genereer UI elementen (bv. `TextBlock`/`TextBox`) per `Cost`/`Answer`, gegroepeerd per `Category`.
        * Laad bestaande `Costs`/`Answers` data in de velden bij wijzigen (`GetYearreportDetails`).
        * Lees waarden uit de dynamische velden bij opslaan.
        * Roep `SaveCostsForReport` en `SaveAnswersForReport` aan bij opslaan.

3.5 [ ] **Benchmarking UI & Logica:**
    -   3.5.1[ ] **Filters & Selecties UI:**
        * Implementeer Indicatoren selectie (`ListBox` met `CheckBoxes`).
        * Implementeer Jaar selectie (`ComboBox`).
        * Implementeer NACE-code filter (`ComboBox` met `Nacecodes`).
        * Implementeer NACE-groepering (`RadioButtons`).
    -   3.5.2[ ] **Data Ophalen & Berekenen:**
        * Koppel "Genereer" knop aan logica:
            * Verzamel filter/selectie waarden.
            * Roep `GetBenchmarkData` aan.
            * Haal eigen data op voor vergelijking.
            * Bereken lokaal Gemiddelde, Mediaan.
            * (Optioneel) Bereken Percentielen (P25, P75), Standaardafwijking.
    -   3.5.3[ ] **Visualisaties (LiveCharts):**
        * Implementeer Kolom-/Staafdiagram (Eigen vs. Gemiddelde).
        * Implementeer Box-and-whisker plot (Spreiding benchmark data).
        * (Optioneel) Lijn-/Areagrafiek voor trends.
    -  3.5.4 [ ] **Rapport Sterke/Zwakke Punten:**
        * Genereer tekstuele output (`TextBlock`/`RichTextBox`).
        * Vergelijk eigen waarde met benchmark (gemiddelde/mediaan).
        * Markeer significante afwijkingen.
    -   3.5.5[ ] **(Optioneel) Extra Features:**
        * Implementeer tijdlijnweergave.
        * Implementeer instellen streefwaarden.
        * Implementeer PDF-export van benchmarkrapport.

3.6 [ ] **UI Afwerking:**
    -   [ ] Zorg voor een professionele look-and-feel.
    -   [ ] Gebruik correcte WPF layout panels.
    -   [ ] Alle UI-tekst in het Nederlands.
    -   [ ] Pas Hongaarse notatie toe.

## Fase 4: Testen, Refinement & Documentatie

4.1 [ ] **Testen:**
    -   [ ] Test alle functionaliteiten in AdminApp en CompanyApp.
    -   [ ] Test edge cases (geen data, foute invoer, lege lijsten).
    -   [ ] Test login, registratie, goedkeuring, weigering flows.
    -   [ ] Test database CRUD operaties en data integriteit.
    -   [ ] Test NACE-filtering en groepering op alle niveaus.
    -   [ ] Verifieer correctheid benchmark berekeningen en visualisaties.
    -   [ ] Test met verschillende schermresoluties (indien van toepassing).

4.2 [ ] **Refinement:**
    -   [ ] Verbeter UI/UX op basis van testbevindingen (duidelijkheid, gebruiksgemak).
    -   [ ] Optimaliseer eventuele trage database queries of berekeningen.
    -   [ ] Los alle geïdentificeerde bugs op.
    -   [ ] Controleer of aan alle Non-Functional Requirements is voldaan (architectuur, code kwaliteit, security, geen verboden technieken).

4.3 [ ] **AI Gebruik Documentatie:**
    -   [ ] **Gedurende het hele proces:** Houd AI interacties bij.
    -   [ ] Exporteer alle relevante ChatGPT (of andere AI) sessies naar PDF.
    -   [ ] Schrijf het korte verslag over AI-gebruik conform de richtlijnen.

4.4 [ ] **Code Review & Cleanup:**
    -   [ ] Verwijder ongebruikte using statements, variabelen, methodes.
    -   [ ] Verwijder test/debug code (zoals `Console.WriteLine`).
    -   [ ] Zorg dat de code voldoet aan naamgevings- en stijlconventies (StyleCop).
    -   [ ] Voeg commentaar toe waar nodig voor complexe logica.
    -   [ ] Zorg dat elke regel code uitgelegd kan worden.

## Fase 5: Oplevering

5.1 [ ] **Final Build:**
    -   [ ] Zorg dat de volledige solution build zonder fouten of waarschuwingen (indien mogelijk).
    -   [ ] Verifieer dat de correcte, werkende connectiestring is geconfigureerd.

5.2 [ ] **Packaging:**
    -   [ ] Bundel de volledige Visual Studio Solution map.
    -   [ ] Voeg de map met PDF-exports van ChatGPT sessies toe.
    -   [ ] Voeg het AI-gebruik verslag (.md of .pdf) toe.
    -   [ ] Maak één ZIP-bestand van de gehele bundel.

5.3 [ ] **Indienen:**
    -   [ ] Dien het ZIP-bestand in via het opgegeven kanaal voor de deadline (**vrijdag 3 mei 2025**, 20u - *Let op: PRD noemt 30 mei, maar vandaag is 3 mei. Controleer de juiste deadline!*).

5.4 [ ] **Voorbereiding Mondelinge Verdediging:**
    -   [ ] Wees voorbereid om de applicaties te demonstreren.
    -   [ ] Wees voorbereid om de code structuur, belangrijke algoritmes, architecturale keuzes en AI-gebruik te verklaren en te verdedigen.

**Tip:** Werk de taken binnen elke genummerde hoofdtaak (bv. 1.1, 1.2) af. Vink zowel de subtaken als de hoofdtaak af wanneer deze compleet is.