# ClaustrophoVR

> Expérience de réalité virtuelle immersive centrée sur la claustrophobie.  
> Cours **8RVL201 — Réalité virtuelle** | Session Hiver 2026

---

## Description

ClaustrophoVR est une expérience VR qui plonge l'utilisateur dans un ascenseur en panne. L'objectif est de faire ressentir la claustrophobie à une personne qui n'en souffre pas, en simulant la peur de l'enfermement, de la perte de contrôle et de l'invisibilité.

L'expérience se déroule en **5 phases successives** :

| Phase | Nom | Description |
|-------|-----|-------------|
| 1 | Introduction | L'ascenseur fonctionne normalement. Le joueur choisit son étage. |
| 2 | Panne | L'ascenseur tombe en panne. Lumières rouges, vibrations haptiques. |
| 3 | Tension | Les murs se rapprochent lentement avec des sons de grincement. Les boutons ne répondent plus. |
| 4 | Puzzle | Un panneau de maintenance s'ouvre. 3 puzzles de câbles à résoudre en temps limité. |
| 5a | Bonne fin | Puzzle résolu : murs reculent, portes s'ouvrent, message de victoire. |
| 5b | Mauvaise fin | Temps écoulé : les murs écrasent le joueur, écran noir, game over. |

---

## Équipe

| Membre | Rôle |
|--------|------|
| Emma Lesbarrères | Développement VR / Unity |
| Marlon Pegahi | Gestion de projet |
| Alexandre Bret | Documentation |
| Tanguy de Jerphanion | Tests & QA |

---

## Installation

### Prérequis

- **Unity** 6000.3.5f2
- **XR Interaction Toolkit** 2.0+
- **OpenXR Plugin**
- Un casque VR compatible OpenXR (Meta Quest 2/3, HTC Vive, Valve Index...)

### Lancer le projet

1. Cloner le dépôt :
   ```bash
   git clone https://github.com/votre-repo/ClaustrophoVR.git
   ```
2. Ouvrir **Unity Hub** → **Open Project** → sélectionner le dossier cloné
3. Attendre l'import des assets
4. Ouvrir la scène principale : `Assets/Scenes/MainScene.unity`
5. Brancher le casque VR
6. Appuyer sur **Play** dans Unity

### Tester sans casque

Importer le sample **XR Device Simulator** via le Package Manager :
- `Window → Package Manager → XR Interaction Toolkit → Samples → XR Device Simulator → Import`
- Glisser le prefab `XR Device Simulator` dans la Hierarchy
- Utiliser les contrôles clavier 

---

## Contrôles

### Avec casque VR

| Action | Contrôle |
|--------|----------|
| Se déplacer | Joystick gauche |
| Regarder | Mouvement de la tête |
| Appuyer sur un bouton | Toucher avec la main droite ou gauche |
| Attraper un câble | Gâchette |
| Ouvrir/fermer les portes | Bouton dédié sur le panneau intérieur |

---

*Projet réalisé dans le cadre du cours 8RVL201 — Réalité virtuelle*
