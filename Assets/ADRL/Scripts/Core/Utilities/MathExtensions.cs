namespace ADRL.Core.Utilities
{
    using UnityEngine;

    public static class MathExtensions
    {
        public static float Remap(this float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            var t = Mathf.InverseLerp(fromMin, fromMax, value);
            return Mathf.Lerp(toMin, toMax, t);
        }

        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360f)
                angle += 360f;

            if (angle > 360f)
                angle -= 360f;

            return Mathf.Clamp(angle, min, max);
        }

        public static Vector3 WithX(this Vector3 vector, float x)
        {
            return new Vector3(x, vector.y, vector.z);
        }

        public static Vector3 WithY(this Vector3 vector, float y)
        {
            return new Vector3(vector.x, y, vector.z);
        }

        public static Vector3 WithZ(this Vector3 vector, float z)
        {
            return new Vector3(vector.x, vector.y, z);
        }

        public static Vector3 AddX(this Vector3 vector, float x)
        {
            return new Vector3(vector.x + x, vector.y, vector.z);
        }

        public static Vector3 AddY(this Vector3 vector, float y)
        {
            return new Vector3(vector.x, vector.y + y, vector.z);
        }

        public static Vector3 AddZ(this Vector3 vector, float z)
        {
            return new Vector3(vector.x, vector.y, vector.z + z);
        }

        public static float InverseLerpUnclamped(float a, float b, float value)
        {
            if (Mathf.Approximately(a, b))
                return 0f;

            return (value - a) / (b - a);
        }

        public static float MoveTo(float current, float target, float maxDelta)
        {
            if (Mathf.Abs(target - current) <= maxDelta)
                return target;

            return current + Mathf.Sign(target - current) * maxDelta;
        }

        public static Vector3 MoveTo(Vector3 current, Vector3 target, float maxDelta)
        {
            var delta = target - current;
            var distance = delta.magnitude;

            if (distance <= maxDelta)
                return target;

            return current + delta.normalized * maxDelta;
        }
    }
}
