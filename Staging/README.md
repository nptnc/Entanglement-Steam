# Entanglement Pre-Release

### Warning: This mod is still WIP and is not final yet!

An alternative multiplayer mod for BONEWORKS written with performance and scalability in mind!

This mod uses discord networking and therefore is compatible with both Steam and Oculus clients, enjoy having cross platform multiplayer!

Join our discord server! https://discord.gg/FmRGBgbc3N

Check out the git repo for the source code: https://gitlab.com/zCubed3/entanglement-public

# Changelog
### **v0.2.0 - Current**
- Added CustomMaps 2.0 and r2modman support
- Added playermodel r2modman support (hasn't been tested so might not work, everything else has though)
- Notifications! (Player leave, join, disconnect, join server, leave server)
- AUTOMATIC NPC SYNCING! (You no longer have to grab them! Might not always work but thats what you have to accept with a multiplayer mod)
- Custom maps objects sync more reliably
- Updated to MelonLoader 0.5.2 and updated logging to LoggerInstance (better performance when logging)
- Custom weapons no longer spawn magazines at the map spawn that lag your game
- Holstered objects disable collision so objects with broken holster points don't destroy a bunch of objects
- Non-broken pieces no longer remain after destroying an object
- Having discord closed when starting the game no longer breaks gunshots

### **v0.1.1**
- Added heartbeat system (if the client has not received a message from the server in over 40 seconds they will disconnect)
- NOTICE: If someone with 0.1.1 joins someone with 0.1.0, they will be disconnected after 40 seconds since they won't receive the heartbeats.
- Fixed eyebrows
- Fixed rich presence not showing the correct player count for clients

### **v0.1.0**
- Dying while in a server will not reload the scene in scenes that usually do (means if you die in Zombie Warehouse or Arena you wont have to restart)
- Falling out of the map/other players entering a reload trigger while in a server will not reload the scene
- Added check if Discord isn't open by disabling the mod
- Actually fixed SteamVR pause menu if it still existed (has been tested thoroughly)
- Improved performance SOOO much (Rigidbodies sleep properly now)
- Improved Rich Presence (Shows level icon and name as well as version number)
- Added local name tag toggle (BoneMenu - Entanglement - Client - Client Settings - NameTags)
- Improved physics joints on synced objects
- Removed a byte from player ids (pretty much every message)
- Internal changes (Cleaner to make new messages)

### **v0.0.5**
- Fixed a problem with lobby capacities

### **v0.0.4**
- Fixed a critical divide by zero error that happened when pausing to the SteamVR menu

### **v0.0.3**
- Optimized the mod for better performance
- Increased stability of physics
