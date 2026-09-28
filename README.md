# CS2-FixRandomSpawn
Fixes ConVar `mp_randomspawn` for any game mode

## Requirments
- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp/)

## Installation
- Download the newest release from [Releases](https://github.com/qstage/CS2-FixRandomSpawn/releases)
- Make a folder in /plugins named /FixRandomSpawn.
- Put the plugin files (including the /gamedata folder) in to the new folder.
- Restart your server.

The plugin reads `plugins/FixRandomSpawn/gamedata/FixRandomSpawn.json` first. If that file is missing, it falls back to `counterstrikesharp/gamedata/FixRandomSpawn.json` (the old install location).

## Configuration
`css_randomspawn_reload` - Reload configuration
```json
{
    "warmup_mode": {
        "enable": false, // Enable the plugin only for warmup time
        "buy_anywhere": false, // Allow purchase anywhere on the map
        "alert_for_players": false // Notification for players about the enabled random spawns
    },
    "ConfigVersion": 1
}
```
