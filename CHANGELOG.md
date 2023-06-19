# Changelog
### **v0.1.0 - IN DEVELOPMENT**

#### *Done features*
- TODO

#### *WIP features*
- Rewrote networking backend to make it more user friendly to other modders
  - Warning: These changes might incur a performance penalty for people with weaker /
  older CPUs, sorry about that, we see it as a fair trade for friendliness and speed.

#### *Planned features*
- Changed how we handle connections to use an "acknowledgement" system. This won't change any part the joining process for the end user except it helps us handle being disconnected a lot better because we wait for a `ConnectionAck` message to determine whether or not we are allowed to join the game we're trying to connect to.
- Fixed "Lobby purgatory" when being disconnected from full lobbies
- Made PVP a lot better
  - Teams
  - Friendly fire toggle

### **v0.0.5 - Current**
- Fixed a problem with lobby capacities

### **v0.0.4**
- Fixed a critical divide by zero error that happened when pausing to the SteamVR menu

### **v0.0.3**
- Optimized the mod for better performance
- Increased stability of physics
