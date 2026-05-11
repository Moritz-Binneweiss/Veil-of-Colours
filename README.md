# **Veil of Colours**

## Inhaltsverzeichnis

- [Deutsch](#veil-of-colours---deutsch)
- [English](#veil-of-colours---english)

# **Veil of Colours - Deutsch**

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

|    Taste / Button (Gamepad)     |      Funktion       |
| :-----------------------------: | :-----------------: |
|     **W / Left Stick (Up)**     |  Vorwärts bewegen   |
|    **S / Left Stick (Down)**    |  Rückwärts bewegen  |
|    **A / Left Stick (Left)**    | Nach links bewegen  |
|   **D / Left Stick (Right)**    | Nach rechts bewegen |
|    **Space / Button South**     |      Springen       |
|     **Shift / Button West**     |        Dash         |
| **Left Control / Left Trigger** |      Klettern       |
|  **Pfeiltasten / Right Stick**  |       Farbrad       |
|    **Escape / Start Button**    |        Pause        |

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

---

# **Veil of Colours - English**

## **Contributors**

- **Moritz Binneweiß** - Networking and Designing Developer
- **Sebastian Schuster** - Animating Developer
- **Vanessa Schoger** - Level and Tilemapping Developer

Unity Version: 6000.2.6f2

### Figma Board

https://www.figma.com/board/kBoRnHPDNGJm4fzHZrUeEu/Veil-of-Colours?node-id=0-1&t=oa8m78GmibciiSQd-1![FigmaBoard](image.png)

### GitHub Repo

https://github.com/Moritz-Binneweiss/Veil-of-Colours

### Link to the Video

- https://youtu.be/kQ3A_jH6J6Y

## Start-Up Guide

1. Download the project from GitHub (e.g., as a zip file)
2. Extract the zip file
3. Start/open the project in Unity (Version: 6000.2.6f2)
4. If desired, connect to your own Relay Service (Edit > Project Settings > Services > possibly unlink project > link it with your own Unity account)
5. Open the MainMenu scene > start the game > "Host Game" > and start playing
6. If a 2nd player is desired, they simply need to enter the code shown when pressing pause into the text field in the MainMenu > Join Game > and both players are connected

## Project Description

Veil of Colours is a cooperative 2D platformer for two players, networked via Unity's Relay Service. The game combines classic jump-and-run mechanics with the color-switch mechanic from the indie game "Hue".
Players can switch between different color layers, making certain objects and obstacles visible or invisible. Each player progresses through their own level, specifically designed around the color mechanic. The project uses tilemaps for level design, custom 2D animations in the Unity Animator, and additional 2D assets created in Aseprite.

## Technologies Used

- **Unity 6000.2.6f2** as the game engine with Universal Render Pipeline (URP)
- **Unity Netcode for GameObjects** combined with Unity's **Relay Service** for multiplayer synchronization
- **Unity Tilemap System** for level design with rules and automatic tile placement
- **Unity's new Input System** for flexible controller and keyboard support
- **Aseprite** for creating sprites and 2D assets
- **Unity Animator** for character animations and state changes

## Special Challenges / Lessons Learned

- **Tilemap Flickering Problem**: During the implementation of tilemaps and color layers, a flickering/tethering problem occurred.
  We analyzed the issue and tried many different fixes.
  In the end, this is a well-known issue, and using a TileMap Atlas can definitely fix and improve it.

- **Networking Synchronization**: Synchronizing two players via Unity's Relay Service works flawlessly. At first, we had issues because we tried it over a shared local network, which caused problems with firewalls and ports.

- **Camera Follow and Movement System**: The development of the camera-follow and movement system is strongly inspired by Celeste and the very interesting and highly recommended video (https://www.youtube.com/watch?v=yorTG9at90g) by Game Maker's Toolkit.

- **Animations**: One learning was that, whenever possible, 2D animations should be done via sprite sheets, since that is much better and faster to implement.
  Rigging with bones also works well, but it is much more suited for 3D.

## Special Achievements

- **Fully self-created assets**: All visual assets, sprites, and animations were independently created in Aseprite and in the Animator.

- **Functional multiplayer system**: Successful implementation of a stable 2-player networking system with Unity's Relay Service.

- **Complex puzzle levels**: Development of two individual puzzle levels that creatively use the color-switch mechanic and encourage cooperative gameplay.

- **Polished gameplay**: Integration of advanced movement mechanics (dash, climbing, hanging), a checkpoint system, door and pressure plate system, key system, and a functional UI with pause menu and color-wheel interface.

## Assets Used

- All assets are self-created and self-designed (Aseprite, Unity Animator)
- Standard Unity 2D packages + Relay Service from the Unity Registry
- Scene Switcher Pro by Ajay Uthaman (https://assetstore.unity.com/packages/tools/gui/scene-switcher-pro-313355) for faster scene switching

## Controls

|     Key / Button (Gamepad)      |      Function      |
| :-----------------------------: | :----------------: |
|     **W / Left Stick (Up)**     |  Move forward/up   |
|    **S / Left Stick (Down)**    | Move backward/down |
|    **A / Left Stick (Left)**    |     Move left      |
|   **D / Left Stick (Right)**    |     Move right     |
|    **Space / Button South**     |        Jump        |
|     **Shift / Button West**     |        Dash        |
| **Left Control / Left Trigger** |       Climb        |
|  **Arrow Keys / Right Stick**   |    Color wheel     |
|    **Escape / Start Button**    |       Pause        |

## Logs

#### **03.11.2025**

Discussion:

- Presented the project idea
- Initiated expansion
- Discussed challenges

Goal:

- first networking prototype
- further refine the project

#### **10.11.2025**

Discussion:

- Showed basic networking test prototype

Goal:

- Elaborate networking prototype
- Expand basic player movement and animation
- Review and integrate tilesets

#### **17.11.2025**

Discussion:

- Showed test animations
- Tilesets, tilemaps, and rules
- Expanded networking

Goal:

- Complete networking
- Expand maps

#### **01.12.2025**

Discussion:

- Showed finished movement
- Showed finished networking
- Expanded levels
- Showed key item test

Goal:

- Expand animations
- Start color-layer mechanic
- Improve camera follow

#### **08.12.2025**

Discussion:

- Presented tilemap flickering problem
- Showed beginnings of color layers

Goal:

- Improve camera follow
- Expand animations

#### **15.12.2025**

Discussion:

- Showed Level A expansions
- Started animations
- Presentation questions

Goal:

- Prepare presentation
- Complete Level A and Level B
- Improve and expand animations
- Add puzzle elements
- Polishing

Presentation:

- 15 minutes
- Mainly present how 2D and networking works
- Explain technologies
- Explain flickering problem

#### **12.01.2026**

Finalization:

- Gave presentation
- Basic UI
- New designs for everything
- Checkpoint and door system
- Animations
- Expanded and improved everything
