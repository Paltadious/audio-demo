# demo-project
Demonstration of project setup, application launching, implementation of audio (using FMOD), UI elements.

This project demonstrates the following systems and practices:

1. Application initialization: application is launched from a single entry point using the sequence of steps - Chain. This makes the process of launching of all the application systems and game session logics consistent and predictable.
2. Chain: it is an easy manageable sequence of Steps. This system allows not only to create and successively conduct sequences of Steps, but also to pause, to resume, to interrupt and to change the order of Steps during Chain running. Control over the Chain could also be made from inside of the Step.
Project shows the most simple use of the Chain. However the ability to orchestrate its content and behavior gives possibility to implement much more complex systems, e.g. AI.
3. Audio: implementation of audio system based on FMOD. Alongside with base functionality concerned with control over sounds, volumes and VCAs, some more advanced technics are shown. Primarily these are 1) music fading while specific sounds are being played and 2) managing of FMOD engine behavior by changing its parameters.
4. UI elements: approach to implementation of UI basic elements. It is shown using the example of TransitionButton and TransitionHandler. Two points have to be highlighted here: 
    1. separation of UI from game logics. Logics and data must work independently from UI. UI should only display the data and give the user the required points of interaction with  the game logics. This is partly demonstrated in SettingsScreen.
    2. possibility to set up and change the behavior of the UI elements without changing the code. This is achieved by implementing simple functionality in MonoBehaviours. Such approach gives VFX designers, artists and game designers more tools for quick experimenting and partly frees the programmer from the duty of implementing such experiments. This approach can and should be implemented not only in UI but in many fields of the game logics.

Please note: this is a small demo project aiming to show only the above mentioned systems and practices. Other systems by all means required for commercial protect, e.g. architecture solution, UI system, etc. are not implemented.

Important: in order to reduce the repository size FMOD libs for iOS (~200 mb, cannot be imported through the package manager) were not imported into the project. This means that builds on iOS are currently cannot be made. If you want to make such a build you have to import those libs into the project. You can download and unpack libs from FMOD package for Unity v2.02.04 which is used in the project on the following link: [https://fmod.com/download#fmodforunity](https://fmod.com/download#fmodforunity)
