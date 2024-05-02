using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    public static class MathExtensions
    {
        public const float SQRT_THREE = 1.7320508075688772f;
        public const float SQRT_TWO = 1.4142135623730951f;

        public static float Sqr(this float value)
        {
            return value * value;
        }

        public static float3 Scale(this float3 f, float3 scale)
        {
            f.x *= scale.x;
            f.y *= scale.y;
            f.z *= scale.z;
            // press f to pay respects
            return f;
        }

        public static float3 MultiplyPoint(this float4x4 matrix, float3 point)
        {
            return math.transform(matrix, point);
        }

        public static float3 MultiplyVector(this float4x4 matrix, float3 vector)
        {
            return math.rotate(matrix, vector);
        }
        
        public static float3 MultiplyDirection(this float4x4 matrix, float3 direction)
        {
            var dir = math.rotate(matrix, direction);
            var len = math.length(direction);
            return ClampMagnitude(dir, len);
        }

        public static bool IsInSameDirection(this Vector3 v1, Vector3 v2, float maxAngle = 45f)
        {
            var vectorAngle = Vector3.Angle(v1, v2);

            return vectorAngle <= maxAngle;
        }

        public static bool IsInSameDirection(this float3 v1, float3 v2, float sensitivity = 0f)
        {
            sensitivity = math.clamp(sensitivity, 0f, 1f);
            return math.dot(v1, v2) > sensitivity;
        }

        public static bool IsInOppositeDirection(this float3 v1, float3 v2, float sensitivity = 0f)
        {
            if (sensitivity > 0)
                sensitivity = -sensitivity;

            sensitivity = math.clamp(sensitivity, -1f, 0f);
            return math.dot(v1, v2) < sensitivity;
        }

        public static Quaternion RotationWorldToLocal(Quaternion parentRotation, Quaternion rotation)
        {
            return Quaternion.Inverse(parentRotation) * rotation;
        }

        public static Quaternion RotationLocalToWorld(Quaternion parentRotation, Quaternion localRotation)
        {
            return parentRotation * localRotation;
        }

        public static Vector3 ProjectVectorOnPlane(Vector3 vector, Vector3 normal)
        {
            var a1 = (Vector3.Dot(vector, normal) / normal.magnitude) * normal;
            return vector - a1;
        }

        public static float3 ProjectVectorOnPlane(float3 vector, float3 normal)
        {
            var a1 = (math.dot(vector, normal) / math.length(normal)) * normal;
            return vector - a1;
        }

        public static Vector3 ProjectPointOnLine(Vector3 point, Vector3 linePoint, Vector3 lineNormal)
        {
            var directionLinePoint = point - linePoint;

            var dot = Vector3.Dot(directionLinePoint, lineNormal.normalized);

            return linePoint + lineNormal * dot;
        }

        public static float3 ProjectPointOnLine(float3 point, float3 linePoint, float3 lineNormal)
        {
            var directionLinePoint = point - linePoint;

            var dot = math.dot(directionLinePoint, math.normalizesafe(lineNormal, float3.zero));

            return linePoint + lineNormal * dot;
        }

        public static Vector3 ProjectPointOnPlane(Vector3 pointToProject, Vector3 planePoint, Vector3 planeNormal)
        {
            var directionToPoint = pointToProject - planePoint;

            var dot = Vector3.Dot(directionToPoint, planeNormal);
            var positionOnPlane = pointToProject - dot * planeNormal;
            
            return positionOnPlane;
        }

        public static float3 ProjectPointOnPlane(float3 pointToProject, float3 planePoint, float3 planeNormal)
        {
            var directionToPoint = pointToProject - planePoint;

            var dot = math.dot(directionToPoint, planeNormal);
            var positionOnPlane = pointToProject - dot * planeNormal;

            return positionOnPlane;
        }

        public static float3 ClampMagnitude(float3 vector, float magnitude)
        {
            if (vector.Magnitude() <= magnitude) return vector;

            return math.normalize(vector) * magnitude;
        }

        public static float NormalizeAngle(float a)
        {
            return a - 180f * Mathf.Floor((a + 180f) / 180f);
        }

        public static Quaternion ShortestRotation(Quaternion a, Quaternion b)
        {
            if (Quaternion.Dot(a, b) < 0)
            {

                return a * Quaternion.Inverse(Multiply(b, -1));

            }

            else return a * Quaternion.Inverse(b);
        }

        public static Quaternion Multiply(Quaternion input, float scalar)
        {
            return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
        }

        public static Vector3 RotateAround(Vector3 position, Vector3 pivotPoint, Quaternion rot)
        {
            return rot * (position - pivotPoint) + pivotPoint;
        }

        public static float Repeat(float t, float length)
        {
            return math.clamp(t - math.floor(t / length) * length, 0, length);
        }

        public static float Pingpong(float t, float length)
        {
            t = Repeat(t, length * 2f);
            return length - math.abs(t - length);
        }

        public static Vector3 Lerp(Vector3 a, Vector3 b, Vector3 t)
        {
            var x = Mathf.Lerp(a.x, b.x, t.x);
            var y = Mathf.Lerp(a.y, b.y, t.y);
            var z = Mathf.Lerp(a.z, b.z, t.z);

            return new Vector3(x, y, z);
        }

        public static WheelFrictionCurve Lerp(WheelFrictionCurve wfc1, WheelFrictionCurve wfc2, float t)
        {
            WheelFrictionCurve lerpCurve = default;

            lerpCurve.asymptoteSlip = Mathf.Lerp(wfc1.asymptoteSlip, wfc2.asymptoteSlip, t);
            lerpCurve.asymptoteValue = Mathf.Lerp(wfc1.asymptoteValue, wfc2.asymptoteValue, t);
            lerpCurve.extremumSlip = Mathf.Lerp(wfc1.extremumSlip, wfc2.extremumSlip, t);
            lerpCurve.extremumValue = Mathf.Lerp(wfc1.extremumValue, wfc2.extremumValue, t);
            lerpCurve.stiffness = Mathf.Lerp(wfc1.stiffness, wfc2.stiffness, t);

            return lerpCurve;
        }

        public static bool Approximately(this Quaternion q1, Quaternion q2, float deltaDegrees = 0.04f)
        {
            return Quaternion.Angle(q1, q2) < deltaDegrees;
        }

        public static bool Approximately(this Vector3 v1, Vector3 v2, float delta = 0.04f)
        {
            return (v1 - v2).sqrMagnitude < delta * delta;
        }

        public static bool Approximately(this Vector2 v1, Vector2 v2, float delta = 0.04f)
        {
            return (v1 - v2).sqrMagnitude < delta * delta;
        }

        public static bool Approximately(this float f1, float f2, float delta = 0.0001f)
        {
            return Mathf.Abs(f1 - f2) <= delta;
        }

        public static bool Approximately(this float3 v1, float3 v2, float delta = 0.0001f)
        {
            return math.lengthsq(v1 - v2) <= delta * delta;
        }

        public static float SqrMagnitude(this float3 v)
        {
            return (v.x * v.x + v.y * v.y + v.z * v.z);
        }

        public static float Magnitude(this float3 v)
        {
            return math.sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
        }

        public static Vector3 Abs(this Vector3 v)
        {
            v.x = Mathf.Abs(v.x);
            v.y = Mathf.Abs(v.y);
            v.z = Mathf.Abs(v.z);
            return v;
        }

        public static Vector3 Divide(this Vector3 v1, Vector3 v2)
        {
            v1.x /= v2.x;
            v1.y /= v2.y;
            v1.z /= v2.z;
            return v1;
        }

        public static float MultiplyAndSum(this Vector3 v1, Vector3 v2)
        {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }

        public static float MultiplyAndSum(this float3 v1, float3 v2)
        {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }

        /// <summary>
        /// Multiplies two vectors component wise.
        /// </summary>
        public static Vector3 Multiply(this Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
        }

        /// <summary>
        /// Multiplies a vector on each axis by a given scalar.
        /// </summary>
        public static Vector3 Multiply(this Vector3 v1, float x, float y, float z)
        {
            return new Vector3(v1.x * x, v1.y * y, v1.z * z);
        }

        public static Vector3 DivideByComponents(this Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z);
        }

        public static bool IsNaN(this Vector3 v)
        {
            return float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z);
        }

        public static bool IsNaN(this float3 v)
        {
            return float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z);
        }

        public static bool IsInfinity(this Vector3 v)
        {
            return float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z);
        }

        public static bool IsInfinity(this float3 v)
        {
            return float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z);
        }

        public static Vector3 NormalizeXZ(this Vector3 v)
        {
            v.Set(v.x, 0f, v.z);
            return v.normalized;
        }

        public static Vector3 NormalizeXY(this Vector3 v)
        {
            v.Set(v.x, v.y, 0f);
            return v.normalized;
        }

        public static Vector3 NormalizeYZ(this Vector3 v)
        {
            v.Set(0f, v.y, v.z);
            return v.normalized;
        }

        public static Vector3 XZ(this Vector3 v1)
        {
            return new Vector3(v1.x, 0f, v1.z);
        }

        public static Vector3 SampleBezier(Vector3 startPoint, Vector3 startDirection, Vector3 endPoint,
            Vector3 endDirection, float t)
        {
            var a = Mathf.Pow(1f - t, 3f) * startPoint;
            var b = 3f * Mathf.Pow(1f - t, 2f) * t * (startPoint + startDirection);
            var c = 3f * (1f - t) * Mathf.Pow(t, 2f) * (endPoint + endDirection);
            var d = Mathf.Pow(t, 3f) * endPoint;

            return a + b + c + d;
        }

        public static Vector3 SampleParabola(Vector3 startPoint, Vector3 endPoint, float parabolaHeight, float t)
        {
            t = Mathf.Clamp01(t);
            float yOffset = parabolaHeight * 4.0f * (t - t * t);
            return Vector3.Lerp(startPoint, endPoint, t) + yOffset * Vector3.up;
        }

        //public static float QuadInOutPingPong(float currentTime, float minValue, float maxValue, float duration)
        //{
        //    // remap values so that we reach max at half step
        //    var time = Mathf.PingPong(0f, duration * 0.5f);

        //    return Fpx.Math.Easing.QuadEaseInOut(time, minValue, maxValue, duration * 0.5f);
        //}

        public static float Remap(float input, Vector2 inMinMax, Vector2 outMinMax)
        {
            return outMinMax.x + (input - inMinMax.x) * (outMinMax.y - outMinMax.x) / (inMinMax.y - inMinMax.x);
        }
    }
}
