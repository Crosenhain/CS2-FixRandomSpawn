using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace FixRandomSpawn;

public readonly record struct SpawnPatch(string Signature, string Bytes, int Offset, string Source);

public static class PluginGameData
{
    private const string FileName = "FixRandomSpawn.json";
    private const string SignatureKey = "EntSelectSpawnPoint";
    private const string PatchKey = "EntSelectSpawnPoint_Patch1";

    // CSS only merges top-level counterstrikesharp/gamedata/*.json, so the copy shipped next to
    // the plugin dll is read here first; the global gamedata folder is kept as a fallback.
    public static SpawnPatch Resolve(string moduleDirectory)
    {
        string localPath = Path.Combine(moduleDirectory, "gamedata", FileName);

        if (File.Exists(localPath))
        {
            return FromFile(localPath);
        }

        try
        {
            return new SpawnPatch(
                GameData.GetSignature(SignatureKey),
                GameData.GetSignature(PatchKey),
                GameData.GetOffset(PatchKey),
                "counterstrikesharp/gamedata");
        }
        catch (Exception ex)
        {
            throw new FileNotFoundException(
                $"Gamedata not found: expected {localPath} or '{SignatureKey}'/'{PatchKey}' in counterstrikesharp/gamedata/*.json", ex);
        }
    }

    private static SpawnPatch FromFile(string path)
    {
        var entries = JsonSerializer.Deserialize<Dictionary<string, Entry>>(File.ReadAllText(path))
            ?? throw new InvalidDataException($"{path} is empty");

        bool linux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        string? signature = entries.GetValueOrDefault(SignatureKey)?.Signatures?.For(linux);
        var patch = entries.GetValueOrDefault(PatchKey);
        string? bytes = patch?.Signatures?.For(linux);
        int? offset = patch?.Offsets?.For(linux);

        if (signature is null || bytes is null || offset is null)
        {
            throw new InvalidDataException(
                $"{path} is missing '{SignatureKey}'/'{PatchKey}' for {(linux ? "linux" : "windows")}");
        }

        return new SpawnPatch(signature, bytes, offset.Value, path);
    }

    private sealed class Entry
    {
        [JsonPropertyName("signatures")]
        public PerPlatform<string>? Signatures { get; set; }

        [JsonPropertyName("offsets")]
        public PerPlatform<int?>? Offsets { get; set; }
    }

    private sealed class PerPlatform<T>
    {
        [JsonPropertyName("windows")]
        public T? Windows { get; set; }

        [JsonPropertyName("linux")]
        public T? Linux { get; set; }

        public T? For(bool linux) => linux ? Linux : Windows;
    }
}
