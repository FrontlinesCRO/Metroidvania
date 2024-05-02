using System.Collections.Generic;
using UnityEngine;

namespace Fpx.Utilities
{
    public static class PhysicsExtensions
    {
        /// <summary>
        /// Basically a filtered Sphere Cast based on an angle.
        /// </summary>
        /// <param name="physics">The physics class for which we provide this extension.</param>
        /// <param name="origin">The starting point of the cast.</param>
        /// <param name="maxRadius">The maximum radius of the cone (sphere).</param>
        /// <param name="direction">The direction in which to cast.</param>
        /// <param name="maxDistance">How far to check.</param>
        /// <param name="hitInfo">The raycast hit information.</param>
        /// <param name="coneAngle">The angle of the cone.</param>
        /// <param name="layerMask">The physics layers that we will cast against.</param>
        /// <param name="queryTriggerInteraction">Should volumes marked as triggers be taken into account.</param>
        /// <returns>RaycastHit array about the colliders we hit.</returns>
        public static bool ConeCast(this PhysicsScene physics, Vector3 origin, float maxRadius, Vector3 direction,
            out RaycastHit hitInfo,
            float coneAngle, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            var hasHit = physics.SphereCast(origin - direction * maxRadius, maxRadius,
                direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);

            if (hasHit)
            {
                Vector3 hitPoint = hitInfo.point;
                Vector3 directionToHit = hitPoint - origin;
                float angleToHit = Vector3.Angle(direction, directionToHit);

                if (angleToHit < coneAngle)
                {
                    return true;
                }
            }

            hitInfo = new RaycastHit();
            return false;
        }

        /// <summary>
        /// Basically a filtered Sphere Cast based on an angle.
        /// </summary>
        /// <param name="physics">The physics class for which we provide this extension.</param>
        /// <param name="origin">The starting point of the cast.</param>
        /// <param name="maxRadius">The maximum radius of the cone (sphere).</param>
        /// <param name="direction">The direction in which to cast.</param>
        /// <param name="maxDistance">How far to check.</param>
        /// <param name="coneAngle">The angle of the cone.</param>
        /// <param name="layerMask">The physics layers that we will cast against.</param>
        /// <param name="queryTriggerInteraction">Should volumes marked as triggers be taken into account.</param>
        /// <returns>RaycastHit array about the colliders we hit.</returns>
        public static RaycastHit[] ConeCastAll(Vector3 origin, float maxRadius, Vector3 direction,
            float coneAngle, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            RaycastHit[] sphereCastHits = Physics.SphereCastAll(origin - direction * maxRadius, maxRadius,
                direction, maxDistance, layerMask, queryTriggerInteraction);

            List<RaycastHit> coneCastHitList = new List<RaycastHit>();

            if (sphereCastHits.Length > 0)
            {
                for (int i = 0; i < sphereCastHits.Length; i++)
                {
                    Vector3 hitPoint = sphereCastHits[i].point;
                    Vector3 directionToHit = hitPoint - origin;
                    float angleToHit = Vector3.Angle(direction, directionToHit);

                    if (angleToHit < coneAngle)
                    {
                        coneCastHitList.Add(sphereCastHits[i]);
                    }
                }
            }

            return coneCastHitList.ToArray();
        }

        /// <summary>
        /// Basically a filtered Sphere Cast based on an angle but with a little less allocation.
        /// </summary>
        /// <param name="physics">The physics class for which we provide this extension.</param>
        /// <param name="origin">The starting point of the cast.</param>
        /// <param name="maxRadius">The maximum radius of the cone (sphere).</param>
        /// <param name="direction">The direction in which to cast.</param>
        /// <param name="maxDistance">How far to check.</param>
        /// <param name="results">The array that will store the raycast hits.</param>
        /// <param name="coneAngle">The angle of the cone.</param>
        /// <param name="layerMask">The physics layers that we will cast against.</param>
        /// <param name="queryTriggerInteraction">Should volumes marked as triggers be taken into account.</param>
        /// <returns>RaycastHit array about the colliders we hit.</returns>
        public static int ConeCastNonAlloc(this PhysicsScene physics, Vector3 origin, float maxRadius, Vector3 direction,
            ref RaycastHit[] results, float coneAngle, float maxDistance = Mathf.Infinity,
            int layerMask = Physics.DefaultRaycastLayers,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            var hitCount = physics.SphereCast(origin - direction * maxRadius, maxRadius, direction,
                results, maxDistance, layerMask, queryTriggerInteraction);

            List<RaycastHit> coneCastHitList = new List<RaycastHit>(hitCount);
            var coneHitCount = 0;

            if (hitCount > 0)
            {
                for (int i = 0; i < hitCount; i++)
                {
                    Vector3 hitPoint = results[i].point;
                    Vector3 directionToHit = hitPoint - origin;
                    float angleToHit = Vector3.Angle(direction, directionToHit);

                    if (angleToHit < coneAngle)
                    {
                        coneCastHitList.Add(results[i]);
                        coneHitCount++;
                    }
                }
            }

            results = coneCastHitList.ToArray();

            return coneHitCount;
        }

        /// <summary>
        /// Gets the friction/bounce/stickiness factor between two interacting objects based on given combination enums.
        /// </summary>
        /// <param name="factor">The first friction/bounce/stickiness factor.</param>
        /// <param name="otherFactor">The other friction/bounce/stickiness factor.</param>
        /// <param name="combinationMethod">The desired combination method for the first factor.</param>
        /// <param name="otherCombinationMethod">The desired combination method for the other factor.</param>
        /// <returns>The combined friction/bounce/stickiness factor.</returns>
        public static float GetCombinationFactor(float factor, float otherFactor,
            PhysicMaterialCombine combinationMethod, PhysicMaterialCombine otherCombinationMethod)
        {
            // the combination method that has a larger value is used for combining passed factors
            PhysicMaterialCombine combination = (int)combinationMethod >= (int)otherCombinationMethod ?
                combinationMethod : otherCombinationMethod;
            
            switch (combination)
            {
                case PhysicMaterialCombine.Average:
                    return (otherFactor + factor) * 0.5f;
                case PhysicMaterialCombine.Minimum:
                    return Mathf.Min(otherFactor, factor);
                case PhysicMaterialCombine.Multiply:
                    return otherFactor * factor;
                case PhysicMaterialCombine.Maximum:
                    return Mathf.Max(otherFactor, factor);
                default:
                    return otherFactor * factor;
            }
        }

        /// <summary>
        /// Calculates the linear velocity for a given point in world space relative to a rotating object.
        /// </summary>
        /// <param name="worldPoint">The point in world space we are sampling for the linear velocity.</param>
        /// <param name="rotatingObject">The transform of the object that is rotating.</param>
        /// <param name="angularVelocity">The angular velocity of the rotating object.</param>
        /// <param name="worldCenterOfMass">The center of mass in world space of the rotating object.</param>
        /// <returns>The linear velocity at the given point in space.</returns>
        public static Vector3 CalculateLinearVelocity(Vector3 worldPoint,
            Transform rotatingObject, Vector3 angularVelocity, Vector3? worldCenterOfMass = null)
        {
            if (!rotatingObject) return Vector3.zero;

            var centerOfMass = worldCenterOfMass.HasValue ? worldCenterOfMass.Value : rotatingObject.position;

            var centerToPoint = rotatingObject.InverseTransformDirection(worldPoint - centerOfMass);

            var linearVelocity = Vector3.Cross(angularVelocity, centerToPoint);

            return rotatingObject.TransformDirection(linearVelocity);
        }

        /// <summary>
        /// Calculates the velocity for a given point in world space relative to a another object moving at velocity and rotating at angular velocity.
        /// </summary>
        /// <param name="transform">The transform of the object that is moving through space.</param>
        /// <param name="worldCenterOfMass">The center of mass of the moving object in world space.</param>
        /// <param name="velocity">The velocity of the moving object.</param>
        /// <param name="angularVelocity">The angular velocity of the moving object.</param>
        /// <param name="worldPoint">A point in space for which we want to know the velocity.</param>
        /// <returns>The velocity at the given point in space.</returns>
        public static Vector3 CalculatePointVelocity(Transform transform, Vector3 worldCenterOfMass,
            Vector3 velocity, Vector3 angularVelocity, Vector3 worldPoint)
        {
            var p = transform.InverseTransformDirection(worldPoint - worldCenterOfMass);
            var v = Vector3.Cross(angularVelocity, p);
            v = transform.TransformDirection(v);
            v += velocity;
            return v;
        }

        /// <summary>
        /// Calculates the velocity for the point that will move along with another physics body.
        /// </summary>
        /// <param name="point">The point in space close to the follow body.</param>
        /// <param name="followBody">The physics body we want to stick close to.</param>
        /// <returns>The velocity the point will have to move at for it to follow the target body..</returns>
        public static Vector3 CalculateFollowVelocity(Vector3 point, Rigidbody followBody)
        {
            var directionToGroundedObject = point - followBody.position;

            directionToGroundedObject = Quaternion.Euler(followBody.angularVelocity) * directionToGroundedObject;
            
            var nextPosition = followBody.position + directionToGroundedObject;

            var rotationVelocity = (nextPosition - point) / Time.fixedDeltaTime;

            return followBody.velocity + rotationVelocity;
        }

        public static float ConvertToForce(float force, float mass, ForceMode mode)
        {
            switch(mode)
            {
                case ForceMode.Acceleration:
                    return force * mass;
                case ForceMode.Impulse:
                    return force / Time.fixedDeltaTime;
                case ForceMode.VelocityChange:
                    return mass * (force / Time.fixedDeltaTime);
                default:
                    return force;
            }
        }

        /// <summary>
        /// Calculates the mass scalars.
        /// </summary>
        /// <param name="mass">The first mass.</param>
        /// <param name="otherMass">The other mass.</param>
        /// <param name="scalar">The mass scalar for the first mass.</param>
        /// <param name="otherScalar">The mass scalar for the other mass.</param>
        public static void GetMassScalars(Rigidbody rb, Rigidbody otherRb, out float scalar, out float otherScalar)
        {
            if (!rb && !otherRb)
            {
                scalar = 0f;
                otherScalar = 0f;
                return;
            }
            else if (!rb)
            {
                scalar = 0f;
                otherScalar = 1f;
                return;
            }
            else if (!otherRb)
            {
                scalar = 1f;
                otherScalar = 0f;
                return;
            }

            if (rb.isKinematic && otherRb.isKinematic)
            {
                scalar = 0f;
                otherScalar = 0f;
                return;
            }
            else if (rb.isKinematic)
            {
                scalar = 0f;
                otherScalar = 1f;
                return;
            }
            else if (otherRb.isKinematic)
            {
                otherScalar = 0f;
                scalar = 1f; return;

            }

            // inverse the mass quantities
            float im1 = 1f / rb.mass;
            float im2 = 1f / otherRb.mass;
            scalar = (im1 / (im1 + im2));
            otherScalar = 1f - scalar;
        }

        /// <summary>
        /// Calculates the mass scalars.
        /// </summary>
        /// <param name="mass">The first mass.</param>
        /// <param name="otherMass">The other mass.</param>
        /// <param name="scalar">The mass scalar for the first mass.</param>
        /// <param name="otherScalar">The mass scalar for the other mass.</param>
        public static void GetMassScalars(float mass, float otherMass, out float scalar, out float otherScalar)
        {
            // inverse the mass quantities
            float im1 = 1f / mass;
            float im2 = 1f / otherMass;
            scalar = (im1 / (im1 + im2));
            otherScalar = 1f - scalar;
        }

        /// <summary>
        /// Calculates the maximum swing velocity can achieve while hanging from a rope.
        /// </summary>
        /// <param name="g">The gravitational acceleration.</param>
        /// <param name="ropeLength">The length of the rope.</param>
        /// <returns></returns>
        public static float CalculateMaxSwingVelocity(float g, float ropeLength)
        {
            return Mathf.Sqrt(2f * g * ropeLength);
        }

        /// <summary>
        /// Clamps the given vector within min and max magnitude.
        /// </summary>
        /// <param name="v">The vector to clamp.</param>
        /// <param name="min">The minimum magnitude.</param>
        /// <param name="max">The maximum magnitude.</param>
        /// <returns>The clamped vector.</returns>
        public static Vector3 ClampMagnitude(Vector3 v, float min, float max)
        {
            var mag = v.magnitude;

            if (max < min)
                max = min;

            if (min > max)
                min = max;

            if (mag < min)
            {
                return v.normalized * min;
            }
            else if (mag > max)
            {
                return v.normalized * max;
            }
            
            return v;
        }

        /// <summary>
        /// Calculates the buoyancy force of a submerged volume in a liquid body.
        /// </summary>
        /// <param name="density">The density of the liquid.</param>
        /// <param name="gravity">The gravitational force that affects the body.</param>
        /// <param name="submergedVolume">The amount of submerged volume of a body.</param>
        /// <returns>Buoyancy force vector.</returns>
        public static Vector3 BuoyancyForce(float density, Vector3 gravity, float submergedVolume)
        {
            // negative - because buoyancy works in the opposite direction of the gravitational force
            return -(density * gravity * submergedVolume);
        }

        /// <summary>
        /// Calculates the kinetic energy based on mass and velocity.
        /// </summary>
        /// <param name="mass">The mass of the object.</param>
        /// <param name="velocity">The velocity of the object.</param>
        /// <returns>The kinetic energy of the object.</returns>
        public static Vector3 KineticEnergy(float mass, Vector3 velocity)
        {
            var keX = (mass * Mathf.Pow(velocity.x, 2f)) / 2f;
            var keY = (mass * Mathf.Pow(velocity.y, 2f)) / 2f;
            var keZ = (mass * Mathf.Pow(velocity.z, 2f)) / 2f;

            return new Vector3(keX, keY, keZ);
        }

        /// <summary>
        /// Calculates the gravitational force of a body of mass affected by gravity against a surface.
        /// </summary>
        /// <param name="mass">The mass of the body.</param>
        /// <param name="gravity">The gravitational force that affects the body.</param>
        /// <param name="surfaceNormal">The normal of the surface.</param>
        /// <returns>The gravitational force of the body against the surface.</returns>
        public static Vector3 CalculateGravityForceAgainstSurface(float mass, Vector3 gravity, Vector3 surfaceNormal)
        {
            var groundAngle = Vector3.Angle(surfaceNormal, Vector3.up);
            var gravityForce = gravity * mass;

            return CalculateGravityForceAgainstSurface(gravityForce, surfaceNormal, groundAngle);
        }

        /// <summary>
        /// Calculates the gravitational force of a body of mass affected by gravity against a surface.
        /// </summary>
        /// <param name="mass">The mass of the body.</param>
        /// <param name="gravity">The gravitational force that affects the body.</param>
        /// <param name="surfaceNormal">The normal of the surface.</param>
        /// <param name="surfaceAngle">The angle the surface is at. (in degrees)</param>
        /// <returns>The gravitational force of the body against the surface.</returns>
        public static Vector3 CalculateGravityForceAgainstSurface(float mass, Vector3 gravity, Vector3 surfaceNormal, float surfaceAngle)
        {
            var gravityForce = gravity * mass;

            return CalculateGravityForceAgainstSurface(gravityForce, surfaceNormal, surfaceAngle);
        }

        /// <summary>
        /// Calculates the gravitational force of a body of mass affected by gravity against a surface.
        /// </summary>
        /// <param name="gravityForce">The total gravitational force that affects the body.</param>
        /// <param name="surfaceNormal">The normal of the surface.</param>
        /// <param name="surfaceAngle">The angle the surface is at. (in degrees)</param>
        /// <returns>The gravitational force of the body against the surface.</returns>
        public static Vector3 CalculateGravityForceAgainstSurface(Vector3 gravityForce, Vector3 surfaceNormal, float surfaceAngle)
        {
            return gravityForce.magnitude * Mathf.Cos(surfaceAngle * Mathf.Deg2Rad) * -surfaceNormal;
        }

        /// <summary>
        /// Calculates the gravitational forces on a body of mass affected by gravity.
        /// </summary>
        /// <param name="mass">The mass of the body.</param>
        /// <param name="gravity">The gravitational force that affects the body.</param>
        /// <param name="surfaceNormal">The normal of the surface.</param>
        /// <param name="surfaceForce">The force against the surface.</param>
        /// <param name="slideForce">The force down the surface.</param>
        /// <returns>The gravitational force that pulls the body in the direction of the gravity vector.</returns>
        public static void CalculateGravitationalForcesOnSurface(float mass, Vector3 gravity, Vector3 surfaceNormal, out Vector3 surfaceForce, out Vector3 slideForce)
        {
            var groundAngle = Vector3.Angle(surfaceNormal, Vector3.up);
            var gravityForce = mass * gravity;

            CalculateGravitationalForcesOnSurface(gravityForce, surfaceNormal, groundAngle, out surfaceForce, out slideForce);
        }

        /// <summary>
        /// Calculates the gravitational forces on a body of mass affected by gravity.
        /// </summary>
        /// <param name="gravityForce">The total gravitational force that affects the body.</param>
        /// <param name="surfaceNormal">The normal of the surface.</param>
        /// <param name="surfaceAngle">The angle the surface is at. (in degrees)</param>
        /// <param name="surfaceForce">The force against the surface.</param>
        /// <param name="slideForce">The force down the surface.</param>
        /// <returns>The gravitational force that pulls the body in the direction of the gravity vector.</returns>
        public static void CalculateGravitationalForcesOnSurface(Vector3 gravityForce, Vector3 surfaceNormal, float surfaceAngle, out Vector3 surfaceForce, out Vector3 slideForce)
        {
            surfaceForce = CalculateGravityForceAgainstSurface(gravityForce, surfaceNormal, surfaceAngle);
            slideForce = Vector3.ClampMagnitude(gravityForce - surfaceForce, gravityForce.magnitude - surfaceForce.magnitude);
        }

    }
}
