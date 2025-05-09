using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FC_CutsceneSystem
{
    public class FactDB : ScriptableObject
    {
        public Dictionary<string, bool> conditions = new Dictionary<string, bool>();
        public List<string> _key = new List<string>();
        public List<bool> _value = new List<bool>();

        public List<string> Key { get { return _key; } set { _key = value; } }
        public List<bool> Value { get { return _value; } set { _value = value; } }
    }
}