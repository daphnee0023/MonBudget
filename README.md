# MonBudget

MonBudget est une application de gestion de budget développée avec .NET MAUI. Elle permet de suivre ses dépenses, de les classer par catégorie et de consulter un résumé de son budget mensuel. Les données sont enregistrées localement dans une base SQLite.

## Fonctionnalités

- Création de compte, connexion et restauration de session.
- Catégories de dépenses par défaut et catégories personnalisées.
- Ajout, consultation et suppression de dépenses.
- Définition d'un budget mensuel et suivi du montant dépensé et restant.
- Statistiques mensuelles et évolution des dépenses.
- Notifications système et génération/partage de rapports PDF.

## Prérequis

- SDK .NET 10.
- Charge de travail .NET MAUI.
- Pour Windows : Windows 10 version 1809 (build 17763) ou ultérieure, avec les outils de développement Windows nécessaires.
- Pour Android : Android SDK et un émulateur ou appareil connecté.
- Pour iOS et Mac Catalyst : un Mac avec les outils Apple requis.

Installer la charge de travail MAUI si elle n'est pas déjà disponible :

```powershell
dotnet workload install maui
```

## Démarrer sous Windows

Depuis le dossier du projet :

```powershell
dotnet restore
dotnet run -f net10.0-windows10.0.19041.0
```

Pour compiler sans lancer l'application :

```powershell
dotnet build -f net10.0-windows10.0.19041.0
```

Fermez une instance déjà ouverte de MonBudget avant de relancer la commande si Windows indique que les fichiers de sortie sont utilisés.

## Démarrer sous Android

Après avoir installé Android SDK et démarré un émulateur ou connecté un appareil :

```powershell
dotnet build -t:Run -f net10.0-android
```

La compilation Android seule peut être lancée avec :

```powershell
dotnet build -f net10.0-android
```

## Données et ressources

- La base `monbudget.db3` est créée dans le répertoire de données privé de l'application. Les tables et catégories par défaut sont initialisées au premier démarrage.
- Les images de l'interface se trouvent dans `Resources/Images` et sont incluses comme ressources MAUI par `MonBudget.csproj`. Référencez-les avec leur nom et leur extension, par exemple `ic_wallet_indigo.png`.
- Les catégories initiales sont définies dans `Data/DonneesExemple.cs`.

## Organisation du code

- `Models/` : modèles de données et objets de résumé.
- `Data/` : accès à SQLite et données initiales.
- `Services/` : authentification, catégories, dépenses, budget, notifications et export.
- `Views/` : pages XAML de l'application.
- `Resources/` : images, polices, styles et écran de démarrage.
- `Platforms/` : configuration propre à Android, iOS, Mac Catalyst et Windows.

## Technologies

- C# et .NET MAUI 10.
- XAML pour les interfaces.
- SQLite avec `sqlite-net-pcl`.
- Injections de dépendances intégrées à .NET.