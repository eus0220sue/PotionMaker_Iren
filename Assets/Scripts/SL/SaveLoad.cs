// SaveLoad.cs
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveLoad
{
    // KeyDB 인스턴스를 보관
    static KeyDB db = new KeyDB();

    static string PathFor(string name = null)
    {
        var file = string.IsNullOrWhiteSpace(name) ? "save_slot0.json" : name.Trim();
        if (System.IO.Path.GetExtension(file) == string.Empty)
            file += ".json";
        return System.IO.Path.Combine(Application.persistentDataPath, file);
    }

    // ── 기본 타입 ───────────────────────────────────────────────────────────────
    public static void SetString(string k, string v) => db.Set(k, v);
    public static string GetString(string k, string d = "") => db.TryGet(k, out var v) ? v : d;

    public static void SetInt(string k, int v) => db.Set(k, v.ToString());
    public static int GetInt(string k, int d = 0) => db.TryGet(k, out var v) && int.TryParse(v, out var n) ? n : d;

    public static void SetFloat(string k, float v) => db.Set(k, v.ToString("R"));
    public static float GetFloat(string k, float d = 0) => db.TryGet(k, out var v) && float.TryParse(v, out var n) ? n : d;

    public static void SetBool(string k, bool v) => db.Set(k, v ? "1" : "0");
    public static bool GetBool(string k, bool d = false) => db.TryGet(k, out var v) ? v == "1" : d;

    // ── Vector/Quat ─────────────────────────────────────────────────────────────
    [Serializable] struct V3 { public float x, y, z; }
    [Serializable] struct Q4 { public float x, y, z, w; }

    public static void SetVector3(string k, Vector3 v)
        => db.Set(k, JsonUtility.ToJson(new V3 { x = v.x, y = v.y, z = v.z }));
    public static Vector3 GetVector3(string k, Vector3 d)
    {
        if (!db.TryGet(k, out var s) || string.IsNullOrEmpty(s)) return d;
        var o = JsonUtility.FromJson<V3>(s); return new Vector3(o.x, o.y, o.z);
    }

    public static void SetQuaternion(string k, Quaternion q)
        => db.Set(k, JsonUtility.ToJson(new Q4 { x = q.x, y = q.y, z = q.z, w = q.w }));
    public static Quaternion GetQuaternion(string k, Quaternion d)
    {
        if (!db.TryGet(k, out var s) || string.IsNullOrEmpty(s)) return d;
        var o = JsonUtility.FromJson<Q4>(s); return new Quaternion(o.x, o.y, o.z, o.w);
    }

    // ── 리스트/세트(문자열) ─────────────────────────────────────────────────────
    public static void SetStringList(string k, List<string> list)
        => db.Set(k, JsonUtility.ToJson(new _StringList { items = list ?? new List<string>() }));
    public static List<string> GetStringList(string k)
    {
        if (!db.TryGet(k, out var s) || string.IsNullOrEmpty(s)) return new List<string>();
        var o = JsonUtility.FromJson<_StringList>(s) ?? new _StringList();
        return o.items ?? new List<string>();
    }

    public static void AddToStringSet(string k, string id)
    {
        var hs = new HashSet<string>(GetStringList(k));
        if (hs.Add(id)) SetStringList(k, new List<string>(hs));
    }
    public static void RemoveFromStringSet(string k, string id)
    {
        var hs = new HashSet<string>(GetStringList(k));
        if (hs.Remove(id)) SetStringList(k, new List<string>(hs));
    }
    public static bool ContainsInStringSet(string k, string id)
        => new HashSet<string>(GetStringList(k)).Contains(id);

    // ── 관리/파일 I-O ──────────────────────────────────────────────────────────
    public static bool HasKey(string k) => db.TryGet(k, out _);
    public static bool Remove(string k) => db.Remove(k);
    public static void Clear() => db.Clear();
    public static void NewEmpty() => db = new KeyDB();

    public static void Save(string file = null)
    {
        var json = JsonUtility.ToJson(db);
        var path = PathFor(file);
        var dir = System.IO.Path.GetDirectoryName(path);
        if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);

        var tmp = path + ".tmp";
        System.IO.File.WriteAllText(tmp, json);

        try
        {
            System.IO.File.Copy(tmp, path, overwrite: true); // 덮어쓰기
        }
        finally
        {
            if (System.IO.File.Exists(tmp)) System.IO.File.Delete(tmp);
        }
    }

    public static bool Load(string file = null)
    {
        var path = PathFor(file);
        if (!File.Exists(path)) return false;
        var json = File.ReadAllText(path);
        db = JsonUtility.FromJson<KeyDB>(json) ?? new KeyDB();
        return true;
    }

    public static void Delete(string file = null)
    {
        var path = PathFor(file);
        if (File.Exists(path)) File.Delete(path);
    }
    public static bool Exists(string file = null)
    {
        var path = PathFor(file);
        return System.IO.File.Exists(path);
    }

    public static string GetSavePath(string file = null)
    {
        return PathFor(file);
    }
}
