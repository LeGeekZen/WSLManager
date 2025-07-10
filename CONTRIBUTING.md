# Guide de Contribution

Merci de votre intérêt pour contribuer à WSLManager ! Ce document vous guidera dans le processus de contribution.

## 🚀 Comment Contribuer

### Signaler un Bug

1. Vérifiez que le bug n'a pas déjà été signalé dans les [Issues](../../issues)
2. Créez une nouvelle issue avec le label `bug`
3. Utilisez le template de bug report et fournissez :
   - Description détaillée du problème
   - Étapes pour reproduire le bug
   - Version de Windows et .NET
   - Captures d'écran si applicable

### Proposer une Fonctionnalité

1. Vérifiez que la fonctionnalité n'a pas déjà été proposée
2. Créez une issue avec le label `enhancement` ou `feature`
3. Décrivez clairement la fonctionnalité souhaitée
4. Expliquez pourquoi cette fonctionnalité serait utile

### Contribuer au Code

1. **Fork** le repository
2. Créez une **branche** pour votre fonctionnalité :
   ```bash
   git checkout -b feature/ma-nouvelle-fonctionnalite
   ```
3. **Développez** votre fonctionnalité
4. **Testez** votre code sur Windows 10 et 11
5. **Committez** vos changements :
   ```bash
   git commit -m "feat: ajoute une nouvelle fonctionnalité"
   ```
6. **Poussez** vers votre fork :
   ```bash
   git push origin feature/ma-nouvelle-fonctionnalite
   ```
7. Créez une **Pull Request**

## 📋 Standards de Code

### C# Guidelines

- Utilisez **PascalCase** pour les noms de classes, méthodes et propriétés
- Utilisez **camelCase** pour les variables locales
- Ajoutez des **commentaires XML** pour les méthodes publiques
- Respectez l'indentation de 4 espaces
- Utilisez des noms de variables explicites

### XAML Guidelines

- Utilisez des noms explicites pour les contrôles
- Organisez les propriétés dans un ordre logique
- Utilisez des styles et ressources pour la cohérence
- Respectez l'accessibilité avec des `AutomationProperties`

### Messages de Commit

Utilisez le format [Conventional Commits](https://www.conventionalcommits.org/) :

```
type(scope): description

feat(ui): ajoute un bouton de rafraîchissement
fix(wsl): corrige la détection des distributions
docs(readme): met à jour la documentation d'installation
```

Types disponibles :
- `feat` : Nouvelle fonctionnalité
- `fix` : Correction de bug
- `docs` : Documentation
- `style` : Formatage du code
- `refactor` : Refactoring
- `test` : Tests
- `chore` : Maintenance

## 🧪 Tests

### Tests Manuels

Avant de soumettre une PR, testez votre code sur :
- ✅ Windows 10 (dernière version)
- ✅ Windows 11 (dernière version)
- ✅ Avec et sans privilèges administrateur
- ✅ Avec différentes distributions WSL installées

### Tests Automatiques

Si vous ajoutez des tests unitaires :
```bash
dotnet test
```

## 📝 Documentation

### Mise à Jour de la Documentation

- Mettez à jour le README.md si nécessaire
- Ajoutez des commentaires dans le code
- Documentez les nouvelles fonctionnalités
- Mettez à jour les captures d'écran si l'interface change

### Ajout de Captures d'Écran

- Utilisez un format PNG ou JPG
- Optimisez la taille des images
- Placez-les dans un dossier `docs/images/`
- Utilisez des noms descriptifs

## 🔍 Processus de Review

1. **Code Review** : Toutes les PR sont revues par les mainteneurs
2. **Tests** : Vérification que les tests passent
3. **Documentation** : Vérification de la mise à jour de la documentation
4. **Merge** : Une fois approuvée, la PR est mergée

### Critères d'Acceptation

- ✅ Code fonctionnel et testé
- ✅ Respect des standards de code
- ✅ Documentation mise à jour
- ✅ Pas de régression
- ✅ Tests passent

## 🐛 Débogage

### Outils Recommandés

- **Visual Studio Community** : IDE principal
- **GitHub Desktop** : Interface Git graphique
- **PowerShell** : Tests de commandes WSL
- **Process Monitor** : Débogage des permissions

### Logs et Diagnostic

- Activez les logs détaillés dans l'application
- Utilisez l'Observateur d'événements Windows
- Vérifiez les logs PowerShell pour les commandes WSL

## 📞 Support

### Questions et Discussions

- **Issues** : Pour les bugs et fonctionnalités
- **Discussions** : Pour les questions générales
- **Wiki** : Documentation détaillée

### Communication

- Soyez respectueux et constructif
- Utilisez le français ou l'anglais
- Fournissez des détails suffisants
- Répondez aux questions des autres contributeurs

## 🎯 Premiers Pas

### Issues pour Débutants

Cherchez les issues avec le label `good first issue` ou `help wanted` pour commencer.

### Setup de l'Environnement

1. Clonez le repository
2. Ouvrez la solution dans Visual Studio
3. Restaurez les packages NuGet
4. Compilez et lancez l'application

### Ressources Utiles

- [Documentation .NET](https://docs.microsoft.com/dotnet/)
- [Guide WPF](https://docs.microsoft.com/dotnet/desktop/wpf/)
- [Documentation WSL](https://docs.microsoft.com/windows/wsl/)

---

**Merci de contribuer à WSLManager !** 🚀

Votre contribution aide à améliorer l'expérience WSL pour tous les utilisateurs Windows. 