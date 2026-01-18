# **Veil of Colours**

## **Mitwirkende**

- **Moritz Binneweiß** - Networking and Designing Developer
- **Sebastian Schuster** - Animating Developer
- **Vanessa Schoger** - Level and Tilemapping Developer

Unity Version: 6000.2.6f2

### Figma Board
https://www.figma.com/board/kBoRnHPDNGJm4fzHZrUeEu/Veil-of-Colours?node-id=0-1&t=oa8m78GmibciiSQd-1![FigmaBoard](image.png)

### GitHub Repo
https://github.com/Moritz-Binneweiss/Veil-of-Colours

### Link zum Video
- https://youtu.be/kQ3A_jH6J6Y

## Start-Up Guide
1. Projekt auf GitHub (z.B. als Zip) herunterladen
2. Zip Entpacken
3. Projekt in Unity (Version: 6000.2.6f2) starten/öffnen
4. Falls gewünscht mit eigenem Relay Service verbinden (Edit > Project Settings > Services > evtl. Unlink Project > selbst mit Unity Account verknüpfen)
5. MainMenu Scene öffnen > Spiel starten > "Host Game" > und dann losspielen
6. Falls 2. Player gewünscht muss dieser einfach den Code wenn man Pause drückt in das textfeld im MainMenu eingeben > Join Game > und schon sind beide verbunden

## Beschreibung des Projektes

Veil of Colours ist ein kooperativer 2D-Platformer für zwei Spieler, der über Unity's Relay Service vernetzt ist. Das Spiel kombiniert die klassischen Jump-and-Run-Mechaniken mit der Farbwechsel-Mechanic von dem Indie Spiel "Hue". 
Spieler können zwischen verschiedenen Farblayern wechseln, wodurch bestimmte Objekte und Hindernisse sichtbar oder unsichtbar werden. Jeder Spieler durchläuft ein eigenes Level, das speziell auf die Farbmechanik zugeschnitten ist. Das Projekt nutzt Tilemaps für die Levelgestaltung, selbst erstellte 2D-Animationen im Unity Animator und zusätzliche 2D-Assets erstellt in Aseprite.

## Verwendete Technologien

- **Unity 6000.2.6f2** als Game Engine mit Universal Render Pipeline (URP)
- **Unity Netcode for GameObjects** in Kombination mit Unity's **Relay Service** für die Multiplayer-Synchronisation
- **Unity Tilemap System** für das Level-Design mit Rules und automatischer Tile-Platzierung
- **Unity's neues Input System** für flexible Controller- und Keyboard-Unterstützung
- **Aseprite** für die Erstellung von Sprites und 2D-Assets
- **Unity Animator** für Character-Animationen und State Changes

## Besondere Herausforderungen / Lessions Learned

- **Tilemap Flickering Problem**: Bei der Implementierung der Tilempaps und der Farblayers trat ein Flickering/Tethering-Problem auf. 
Wir haben das Problem analysiert und sehr viele verschiedene Fixes versucht.
Schließlich ist es ein recht bekanntes Problem und durch einen TileMapAtlas kann man das Problem auf jeden Fall beheben und verbessern.

- **Networking-Synchronisation**: Die Synchronisation von zwei Spielern über Unity's Relay Service funktioniert einwandfrei. Zunächst hatten wir Probleme, da wir es über ein lokales gemeinsames Netzwerk versucht hatten, was aber Probleme mit den Firewalls und Ports dartstellte

- **Camera Follow und Movement System**: Die Entwicklung des Camera-Follow und Movement-Systems ist stark inspiriert von Celeste und dem sehr interessantem und empfehlenswerten Video (https://www.youtube.com/watch?v=yorTG9at90g von Game Maker's Toolkit)

- **Animationen**: Bei den Animation war ein learning, dass man falls möglich die 2D Animationen über Sprite Sheets machen soll, da dass viel besser und schneller umzusetzen ist.
Das rigging mit Bones funktioniert zwar auch gut aber ist wesentlich besser für 3D geeignet

## Besondere Leistungen

- **Vollständig selbst erstellte Assets**: Alle visuellen Assets, Sprites und Animationen wurden eigenständig in Aseprite und im Animator erstellt.

- **Funktionales Multiplayer-System**: Erfolgreiche Implementation eines stabilen 2-Spieler-Networking-Systems mit Unity's Relay Service.

- **Komplexe Puzzle-Level**: Entwicklung von zwei individuellen Puzzle-Leveln, die die Farbwechsel-Mechanic kreativ nutzen und kooperatives Gameplay fördern.

- **Poliertes Gameplay**: Integration von erweiterten Bewegungsmechaniken (Dash, Klettern, Festhalten), einem Checkpoint-System, Door- und Pressure Plate System, Key-System und einem funktionalen UI mit Pause-Menü und Farbrad-Interface.

## Verwendete Assets

- Alle Assets sind selbst erstellt und selbst desingt (Aseprite, Unity Animator)
- Standard Unity 2D Packages + Relay Service von Unity Registry
- Scene Switcher Pro von Ajay Uthaman (https://assetstore.unity.com/packages/tools/gui/scene-switcher-pro-313355) für schnelleres Scene Switching

## Steuerung

|           Taste / Button (Gamepad)           |             Funktion             |
| :-----------------------: | :------------------------------: |
|           **W / Left Stick (Up)**           |         Vorwärts bewegen         |
|           **S / Left Stick (Down)**           |        Rückwärts bewegen         |
|           **A / Left Stick (Left)**           |        Nach links bewegen        |
|           **D / Left Stick (Right)**           |       Nach rechts bewegen        |
|           **Space / Button South**           |            Springen             |
| **Shift / Button West** |             Dash             |
|           **Left Control / Left Trigger**           |            Klettern             |
|           **Pfeiltasten / Right Stick**           |            Farbrad             |
|           **Escape / Start Button**           |            Pause             |

## Protokolle

#### **03.11.2025**

Besprechung:

- Projektidee vorgestellt
- Erweiterung angestoßen
- Herausforderungen besprochen

Ziel:

- erster Networking Protoyp
- Projekt genauer ausarbeiten

#### **10.11.2025**

Besprechung:

- Basic Networking Test Prototype gezeigt

Ziel:

- Networking Prototype ausarbeiten
- Basic Player Movement und Animation erweitern
- Tilesets anschauen und einarbeiten

#### **17.11.2025**

Besprechung:

- Test Animations gezeigt
- Tilesets, Tilemaps und Rules
- Networking erweitert

Ziel:

- Networking fertigstellen
- Maps erweitern

#### **01.12.2025**

Besprechung:

- fertiges Movement gezeigt
- fertiges Networking gezeigt
- Levels erweitert
- Key Items test gezeigt

Ziel:

- Animations erweitern
- Farblayer Mechanic starten
- Camera Follow verbessern

#### **08.12.2025**

Besprechung:

- Tilemap flickering Problem dargestellt
- ColorLayer Anfänge gezeigt

Ziel:

- Camera Follow verbessern
- Animations erweitern

#### **15.12.2025**

Besprechung:

- LevelA erweiterungen gezeigt
- Animations angefangen
- Präsentation fragen

Ziel:

- Präsentation vorbereiten
- LevelA und LevelB fertigstellen
- Animations verbessern und erweitern
- Puzzle Elemente einbauen
- Polishing

Präsentation:

- 15 min
- 2D und Networking vorallem Präsentieren wie das funktioniert
- Technologien erklären
- Flickering Problem erklären

#### **12.01.2026**

Abschluss:

- Präsentation gehalten
- Basic UI
- Neue Designs für alles
- Checkpoint und Door System
- Animationen
- alles erweitert und verbessert
