using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FC_CutsceneSystem
{
    public static class Extentions
    {
        public static bool OverlapRect(this Rect rect1, Rect rect)
        {
            Vector2 p1 = new Vector2(rect.position.x + rect.width, rect.position.y);
            Vector2 p2 = new Vector2(rect.position.x, rect.position.y + rect.height);
            Vector2 p3 = new Vector2(rect.position.x + rect.width, rect.position.y + rect.height);

            if (rect1.Contains(p1) || rect1.Contains(p2) || rect1.Contains(p3) || rect1.Contains(rect.position))
                return true;
            return false;
        }

        public static Vector2 TopLeft(this Rect rect)
        {
            return new Vector2(rect.xMin, rect.yMin);
        }
        public static Rect ScaleSizeBy(this Rect rect, float scale)
        {
            return rect.ScaleSizeBy(scale, rect.center);
        }
        public static Rect ScaleSizeBy(this Rect rect, float scale, Vector2 pivotPoint)
        {
            Rect result = rect;
            result.x -= pivotPoint.x;
            result.y -= pivotPoint.y;
            result.xMin *= scale;
            result.xMax *= scale;
            result.yMin *= scale;
            result.yMax *= scale;
            result.x += pivotPoint.x;
            result.y += pivotPoint.y;
            return result;
        }
        public static Rect ScaleSizeBy(this Rect rect, Vector2 scale)
        {
            return rect.ScaleSizeBy(scale, rect.center);
        }
        public static Rect ScaleSizeBy(this Rect rect, Vector2 scale, Vector2 pivotPoint)
        {
            Rect result = rect;
            result.x -= pivotPoint.x;
            result.y -= pivotPoint.y;
            result.xMin *= scale.x;
            result.xMax *= scale.x;
            result.yMin *= scale.y;
            result.yMax *= scale.y;
            result.x += pivotPoint.x;
            result.y += pivotPoint.y;
            return result;
        }
    }
}