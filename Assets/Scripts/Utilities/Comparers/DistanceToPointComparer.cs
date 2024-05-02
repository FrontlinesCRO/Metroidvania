using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Utilities.Comparers
{
    public readonly struct DistanceToPointComparer : IComparer, IComparer<Vector3>, IComparer<Transform>, IComparer<Collider>, IComparer<RaycastHit>
    {
        private readonly Vector3 _targetPoint;
        private readonly bool _ascending;

        public DistanceToPointComparer(Vector3 targetPoint, bool ascending = true)
        {
            _targetPoint = targetPoint;
            _ascending = ascending;
        }

        public int Compare(object x, object y)
        {
            if (x is Vector3 vector && y is Vector3 otherVector)
                return Compare(vector, otherVector);
            else if (x is Transform transform && y is Transform otherTransform)
                return Compare(transform, otherTransform);
            else if (x is Collider collider && y is Collider otherCollider)
                return Compare(collider, otherCollider);
            else if (x is RaycastHit hit && y is RaycastHit otherHit)
                return Compare(hit, otherHit);

            return Comparer.Default.Compare(x, y);
        }

        public int Compare(Vector3 x, Vector3 y)
        {
            var sqrDistanceX = (_targetPoint - x).sqrMagnitude;
            var sqrDistanceY = (_targetPoint - y).sqrMagnitude;

            var comparison = sqrDistanceX.CompareTo(sqrDistanceY);

            return _ascending ? comparison : -comparison;
        }

        public int Compare(Transform x, Transform y)
        {
            if (!x && !y)
                return 0;
            else if (!x)
                return 1;
            else if (!y)
                return -1;

            return Compare(x.position, y.position);
        }

        public int Compare(Collider x, Collider y)
        {
            if (!x && !y)
                return 0;
            else if (!x)
                return 1;
            else if (!y)
                return -1;

            return Compare(x.transform, y.transform);
        }

        public int Compare(RaycastHit x, RaycastHit y)
        {
            var xCollider = x.collider;
            var yCollider = y.collider;

            if (!xCollider && !yCollider)
                return 0;
            else if (!xCollider)
                return 1;
            else if (!yCollider)
                return -1;

            return Compare(x.point, y.point);
        }
    }
}
