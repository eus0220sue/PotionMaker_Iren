// KeyDB.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class _StringList  // 리스트 직렬화용 래퍼 (SaveLoad에서 사용)
{
    public List<string> items = new();
}

public static class Keys
{
    public const string Gold = "player.gold";
    public const string Grade = "player.grade";
    public const string Pos = "player.pos";
    public const string Rot = "player.rot";
    public const string OpenedSet = "map.opened";
    public const string DestroyedSet = "map.destroyed";
    public const string PickedSet = "map.picked";
    public const string Scene = "scene.current";
    public const string InvenJson = "inventory.json"; // 새로 기록될 키
    public const string QuestsJson = "quest.all";      // 파일 키와 통일
}

[Serializable]
public class KeyDB
{
    [Serializable] public class Entry { public string key; public string value; }

    // JsonUtility 직렬화를 위한 리스트
    public List<Entry> entries = new();

    [NonSerialized] private Dictionary<string, string> map;

    private void Ensure()
    {
        if (map != null) return;
        map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var e in entries) map[e.key] = e.value;
    }

    public bool TryGet(string key, out string value)
    {
        Ensure();
        return map.TryGetValue(key, out value);
    }

    public void Set(string key, string value)
    {
        Ensure();
        map[key] = value ?? "";
        int i = entries.FindIndex(e => e.key == key);
        if (i >= 0) entries[i].value = value ?? "";
        else entries.Add(new Entry { key = key, value = value ?? "" });
    }

    public bool Remove(string key)
    {
        Ensure();
        bool removed = map.Remove(key);
        if (removed) entries.RemoveAll(e => e.key == key);
        return removed;
    }

    public void Clear()
    {
        entries.Clear();
        map = null;
    }
}
