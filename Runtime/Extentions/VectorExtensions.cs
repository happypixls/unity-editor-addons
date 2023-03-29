using UnityEngine;

namespace EditorAddons
{
    public static class VectorExtensions
    {
        public static bool DoesExistInTriangle(this Vector3 point, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            Vector3 v0 = p2 - p0;
            Vector3 v1 = p1 - p0;
            Vector3 v2 = point - p0;

            float dot00 = Vector3.Dot(v0, v0);
            float dot01 = Vector3.Dot(v0, v1);
            float dot02 = Vector3.Dot(v0, v2);
            float dot11 = Vector3.Dot(v1, v1);
            float dot12 = Vector3.Dot(v1, v2);

            float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            return (u >= 0) && (v >= 0) && (u + v < 1);
        }
        
        public static bool DoesExistInTriangle(this Vector2 point, Vector2 p0, Vector2 p1, Vector2 p2)
        {
            float invDenom = 1.0f / ((p1.y - p2.y) * (p0.x - p2.x) + (p2.x - p1.x) * (p0.y - p2.y));
            float u = ((p1.y - p2.y) * (point.x - p2.x) + (p2.x - p1.x) * (point.y - p2.y)) * invDenom;
            float v = ((p2.y - p0.y) * (point.x - p2.x) + (p0.x - p2.x) * (point.y - p2.y)) * invDenom;

            return (u >= 0) && (v >= 0) && (u + v < 1);
        }
    }
}
