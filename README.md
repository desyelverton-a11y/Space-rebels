# Space-rebels
Space Rebels is a C# / Unity software project built around real-time procedural generation and reusable gameplay systems.
The project began as an experiment in recreating the fast-moving environments of classic on-rails space games, but evolved into an exploration of software architecture, procedural algorithms, runtime resource management, and performance-conscious system design.

Technical Highlights:

C# application development using Unity’s component-based architecture

Procedural algorithms for generating continuous 3D environments at runtime

Runtime mesh construction using calculated vertices, triangles, normals, and UV coordinates

Object and mesh pooling to reduce repeated allocation and destruction

Parameterized system design allowing behavior to be configured without modifying core generation logic

Multiple trajectory algorithms, including linear, sinusoidal, spiral, and experimental Bézier paths

Dynamic object spawning and lifecycle management

Collision and visibility management for procedurally generated environments

Procedural Generation System:

A central component of the project is InfiniteTunnelGenerator, a C# system that continuously constructs and manages sections of a 3D environment as the player progresses.

Rather than storing an entire level in memory, the system generates sections as needed and recycles previously created resources.

The generator is responsible for:

Calculating the path of the environment.
Constructing runtime mesh geometry.
Maintaining continuity between generated sections.
Recycling tunnel chunks.
Pooling reusable objects and meshes.
Dynamically spawning environmental objects.
Generating collision geometry.
Managing active and inactive sections.

Supported Trajectories
Straight • Sine Wave • Spiral • Bézier (Experimental)


Language: C# Engine / Framework: 

Unity Development Areas: Object-Oriented Programming, Procedural Generation, Algorithms, 3D Mathematics, Resource Management, Performance Optimization


Project Status:

Space Rebels is an ongoing technical prototype rather than a finished commercial game.
The repository is maintained as a demonstration of hands-on C# development, software problem solving, and the design of real-time systems.


