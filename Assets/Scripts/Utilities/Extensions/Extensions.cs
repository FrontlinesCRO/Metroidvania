using UnityEngine;
using Unity.Mathematics;
using Bounds = UnityEngine.Bounds;

namespace Assets.Scripts.Utilities
{
    public static class Extensions
    {
        /// <summary>
        /// Gets the layer mask for the given physics layer.
        /// </summary>
        /// <remarks>
        /// Cache the result because this method can be quite expense if used in loops.
        /// </remarks>
        /// <param name="layer">The physics layer id.</param>
        /// <returns>The layer mask for the given physics layer.</returns>
        public static LayerMask GetPhysicsLayerMask(int layer)
        {
            int mask = 0;

            for (int i = 0; i < 32; i++)
            {
                if (!Physics.GetIgnoreLayerCollision(layer, i))
                {
                    mask |= (1 << i);
                }
            }

            return mask;
        }

        /// <summary>
        /// Gets the component of type T from the given component or any of its ancestors.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <param name="component">The component from which to start getting the target component of type T.</param>
        /// <returns>The component of type T if it was found, else returns null.</returns>
        public static T GetComponentUpwards<T>(this Component component) where T : class
        {
            if (!component) return null;

            var transform = component.transform;
            Transform currentTransform = null;
            
            do
            {
                currentTransform = !currentTransform ? transform : currentTransform.parent;

                if (currentTransform.TryGetComponent(out T targetComponent))
                    return targetComponent;

            } while (currentTransform.parent);

            return null;
        }

        /// <summary>
        /// Tries to get the component of type T from the given component or any of its ancestors.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <param name="component">The component from which to start getting the target component of type T.</param>
        /// <param name="targetComponent">The component of type T to be found.</param>
        /// <returns>True if the component was found, else False.</returns>
        public static bool TryGetComponentUpwards<T>(this Component component, out T targetComponent) where T : class
        {
            if (!component)
            {
                targetComponent = null;
                return false;
            }

            var transform = component.transform;
            Transform currentTransform = null;

            do
            {
                currentTransform = !currentTransform ? transform : currentTransform.parent;

                if (currentTransform.TryGetComponent(out targetComponent))
                    return true;

            } while (currentTransform.parent);

            targetComponent = null;
            return false;
        }

        /// <summary>
        /// Gets the size of the object based on the given collider.
        /// </summary>
        /// <param name="collider">The collider on which the size is based on.</param>
        /// <returns>The size of the collider in width, height and length.</returns>
        public static Vector3 GetObjectSize(this Collider collider)
        {
            var transform = collider.transform;

            var scale = collider.GetColliderSize();

            return transform.lossyScale.Multiply(scale);
        }

        /// <summary>
        /// Gets the collider scale in a nice Vector3 format, depending on the type of Collider it is defined as.
        /// </summary>
        /// <param name="collider">The collider whose scale we want to get.</param>
        /// <returns>The scale of the collider in every dimension given as a Vector3.</returns>
        public static Vector3 GetColliderSize(this Collider collider)
        {
            if (collider is BoxCollider boxCollider)
            {
                return boxCollider.size;
            }
            else if (collider is SphereCollider sphereCollider)
            {
                var radiusScale = 1f + (sphereCollider.radius - 0.5f) * 2f;

                return radiusScale * Vector3.one;
            }
            else if (collider is CapsuleCollider capsuleCollider)
            {
                var radiusScale = 1f + (capsuleCollider.radius - 0.5f) * 2f;
                
                if (capsuleCollider.direction == 0)
                {
                    return new Vector3(capsuleCollider.height, radiusScale, radiusScale);
                }
                else if (capsuleCollider.direction == 1)
                {
                    return new Vector3(radiusScale, capsuleCollider.height, radiusScale);
                }
                else
                {
                    return new Vector3(radiusScale, radiusScale, capsuleCollider.height);
                }
                
            }
            else if (collider is MeshCollider meshCollider)
            {
                return meshCollider.sharedMesh.bounds.size;
            }

            return Vector3.one;
        }

        /// <summary>
        /// Transforms a position from local space to world space, but ignoring the scale of the transform.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="position"></param>
        /// <returns>The world space position.</returns>
        public static Vector3 TransformPointUnscaled(this Transform transform, Vector3 position)
        {
            var localToWorldMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

            return localToWorldMatrix.MultiplyPoint(position);
        }

        /// <summary>
        /// Transforms a position from world space to local space, but ignoring the scale of the transform.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Vector3 InverseTransformPointUnscaled(this Transform transform, Vector3 position)
        {
            var worldToLocalMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).inverse;

            return worldToLocalMatrix.MultiplyPoint3x4(position);
        }

        /// <summary>
        /// Gets the component of type T on the collider or the rigidbody the collider is attached to.
        /// </summary>
        /// <typeparam name="T">The component of type t we are looking for.</typeparam>
        /// <param name="col">The collider on which we are looking for a certain component of type T.</param>
        /// <param name="component"></param>
        /// <returns>The component if found, else null.</returns>
        public static bool TryGetComponentOnCollider<T>(this Collider col, out T component) where T : class
        {
            component = null;

            if (!col) return false;

            if (col.TryGetComponent<T>(out component))
                return true;

            var rigidBody = col.attachedRigidbody;
            
            if (!rigidBody) return false;

            return rigidBody.TryGetComponent<T>(out component);
        }

        /// <summary>
        /// Converts bounds from local space to world space.
        /// </summary>
        /// <param name="transform">The transform of the bounds.</param>
        /// <param name="bounds">The bounds in local space.</param>
        /// <returns>Bounds in world space.</returns>
        public static Bounds TransformBounds(Transform transform, Bounds bounds)
        {
            var min = transform.TransformPoint(bounds.min);
            var max = transform.TransformPoint(bounds.max);
            var center = transform.TransformPoint(bounds.center);

            return new Bounds(center, (max - min).Abs()); ;
        }

        /// <summary>
        /// Converts bounds from world space to local space.
        /// </summary>
        /// <param name="collider">The collider bounds to convert.</param>
        /// <returns>Bounds in local space.</returns>
        public static Bounds InverseTransformBounds(this Collider collider)
        {
            var transform = collider.transform;
            var worldBounds = collider.bounds;

            var localMin = transform.InverseTransformPoint(worldBounds.min);
            var localMax = transform.InverseTransformPoint(worldBounds.max);
            var localCenter = transform.InverseTransformPoint(worldBounds.center);

            var localBounds = new Bounds(localCenter, (localMax - localMin).Abs());

            return localBounds;
        }

        public static Color32 ConvertUintToColor(uint colorValue)
        {
            Color32 c = default;
            c.b = (byte)((colorValue) & 0xFF);
            c.g = (byte)((colorValue >> 8) & 0xFF);
            c.r = (byte)((colorValue >> 16) & 0xFF);
            c.a = (byte)((colorValue >> 24) & 0xFF);
            return c;
        }
    }
}
