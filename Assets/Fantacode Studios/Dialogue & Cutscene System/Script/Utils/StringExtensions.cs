using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FC_CutsceneSystem
{

    public static class StringExtensions
    {
        public static string CleanForUI(this string s)
        {
            return string.Concat(s.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
        }
    }

}
