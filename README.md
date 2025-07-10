# WSLManager

[![License: CC BY-NC 4.0](https://img.shields.io/badge/License-CC%20BY--NC%204.0-lightgrey.svg)](https://creativecommons.org/licenses/by-nc/4.0/)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Windows](https://img.shields.io/badge/Windows-10%2B-blue.svg)](https://www.microsoft.com/windows)

**WSLManager** est une application de gestion graphique pour Windows Subsystem for Linux (WSL). Elle permet de gérer facilement vos distributions Linux, installer de nouvelles distributions et surveiller l'état de votre système WSL.

## 🚀 Fonctionnalités

### 📋 Gestion des distributions
- **Lister** toutes les distributions WSL installées
- **Lancer** une distribution avec un clic
- **Arrêter** une distribution en cours d'exécution
- **Définir** une distribution par défaut
- **Supprimer** une distribution (avec confirmation)
- **Mettre à jour** les distributions
- **Rafraîchir** la liste des distributions

### 🔧 Installation de distributions
- **Sélection** de distributions disponibles sur le Microsoft Store
- **Personnalisation** du nom de la distribution
- **Choix** de l'emplacement d'installation
- **Option** de lancement automatique après installation
- **Barre de progression** pendant l'installation

### 💻 Informations système
- **Version Windows** avec numéro de build
- **État des fonctionnalités** WSL et VirtualMachinePlatform
- **Statut du service** WSLService
- **Version WSL** installée
- **Gestion des services** WSL (démarrer/arrêter)

### 🛡️ Sécurité et privilèges
- **Détection automatique** des privilèges administrateur
- **Boutons grisés** quand les privilèges sont insuffisants
- **Indicateurs visuels** pour les actions nécessitant des droits admin
- **Messages informatifs** pour guider l'utilisateur

## 📋 Prérequis

- **Windows 10** version 2004 ou plus récente / **Windows 11**
- **.NET 8.0 Runtime** ou plus récent
- **Privilèges administrateur** pour certaines fonctionnalités
- **WSL 2** recommandé (mais WSL 1 supporté)

## 🛠️ Installation

### Méthode 1 : Exécutable précompilé
1. Téléchargez la dernière version depuis les [Releases](../../releases)
2. Extrayez l'archive ZIP
3. Exécutez `WSLManager.exe` en tant qu'administrateur

### Méthode 2 : Compilation depuis les sources
```bash
# Cloner le repository
git clone https://github.com/votre-username/WSLManager.git
cd WSLManager

# Restaurer les dépendances
dotnet restore

# Compiler l'application
dotnet build --configuration Release

# Lancer l'application
dotnet run --project WSLManagerApp
```

## 🎯 Utilisation

### Interface principale
L'application se compose de trois onglets principaux :

#### 1. **Gérer les distributions**
- Affiche la liste de toutes vos distributions WSL
- Boutons d'action pour chaque distribution :
  - ▶️ **Lancer** : Démarre la distribution
  - ■ **Arrêter** : Arrête la distribution
  - ★ **Définir par défaut** : Définit comme distribution par défaut
  - 🗑️ **Supprimer** : Supprime la distribution (avec confirmation)
  - ↑ **Mettre à jour** : Met à jour la distribution
- Bouton ↻ **Rafraîchir** pour actualiser la liste

#### 2. **Installer une distribution**
- Liste déroulante des distributions disponibles
- Champs de personnalisation :
  - **Nom personnalisé** : Nom pour votre instance
  - **Emplacement** : Dossier d'installation
- Option pour lancer automatiquement après installation
- Barre de progression pendant l'installation

#### 3. **Informations système**
- **Version Windows** : Affiche la version et le build
- **Fonctionnalité WSL** : État d'activation avec boutons d'installation/suppression
- **Plateforme VM** : État d'activation avec boutons d'installation/suppression
- **Service WSL** : Statut du service avec boutons de démarrage/arrêt
- **Version WSL** : Version installée avec bouton de mise à jour

### Gestion des privilèges
- Les boutons nécessitant des privilèges administrateur sont automatiquement grisés
- Des indicateurs visuels (⚠️) apparaissent quand les droits sont insuffisants
- Des info-bulles expliquent les actions nécessitant des privilèges

## 🔧 Configuration

### Lancement automatique en mode administrateur
L'application est configurée pour demander automatiquement les privilèges administrateur via le fichier `app.manifest`.

### Personnalisation
- **Emplacement par défaut** : Modifiable dans l'interface d'installation
- **Noms personnalisés** : Caractères autorisés : lettres, chiffres, tirets, underscores
- **Lancement automatique** : Option activée par défaut

## 🐛 Dépannage

### Problèmes courants

#### "Privilèges administrateur requis"
- **Solution** : Relancez l'application en tant qu'administrateur
- **Cause** : Certaines fonctionnalités nécessitent des droits élevés

#### "Service WSL non trouvé"
- **Solution** : Activez WSL via `wsl --install` dans PowerShell
- **Cause** : WSL n'est pas installé sur le système

#### "Distribution non installable"
- **Solution** : Vérifiez votre connexion internet et l'accès au Microsoft Store
- **Cause** : Problème de réseau ou de permissions Store

#### "Erreur lors de l'installation"
- **Solution** : Vérifiez l'espace disque disponible et les permissions
- **Cause** : Espace insuffisant ou permissions insuffisantes

### Logs et diagnostic
L'application affiche des messages d'erreur détaillés pour faciliter le diagnostic des problèmes.

## 🤝 Contribution

Les contributions sont les bienvenues ! Voici comment contribuer :

1. **Fork** le projet
2. Créez une **branche** pour votre fonctionnalité (`git checkout -b feature/AmazingFeature`)
3. **Commit** vos changements (`git commit -m 'Add some AmazingFeature'`)
4. **Push** vers la branche (`git push origin feature/AmazingFeature`)
5. Ouvrez une **Pull Request**

### Guidelines de contribution
- Respectez le style de code existant
- Ajoutez des commentaires pour les nouvelles fonctionnalités
- Testez vos modifications sur Windows 10 et 11
- Mettez à jour la documentation si nécessaire

## 📝 Licence

Ce projet est distribué sous licence **Creative Commons Attribution-NonCommercial 4.0 International (CC BY-NC 4.0)**. Voir le fichier [![License: CC BY-NC 4.0](https://img.shields.io/badge/License-CC%20BY--NC%204.0-lightgrey.svg)](https://creativecommons.org/licenses/by-nc/4.0/) pour plus de détails.

### Ce que vous pouvez faire :
- ✅ **Partager** : Copier et redistribuer le matériel sur tout support
- ✅ **Adapter** : Remixer, transformer et créer à partir du matériel

### Ce que vous devez faire :
- 📝 **Attribution** : Donner les crédits appropriés et indiquer les modifications
- 🚫 **Usage non commercial** : Vous ne pouvez pas utiliser le matériel à des fins commerciales

## 🙏 Remerciements

- **Microsoft** pour WSL et le framework .NET
- **La communauté WSL** pour les retours et suggestions
- **Les contributeurs** qui participent au développement

## 📞 Support

- **Issues** : [GitHub Issues](../../issues)
- **Discussions** : [GitHub Discussions](../../discussions)
- **Wiki** : [Documentation détaillée](../../wiki)

## 📊 Statut du projet

- ✅ **Stable** : Prêt pour la production
- 🔄 **Maintenu** : Mises à jour régulières
- 📈 **En développement** : Nouvelles fonctionnalités en cours

---

**Développé avec ❤️ par Le Geek Zen**

*Une application moderne pour gérer WSL simplement et efficacement.* 