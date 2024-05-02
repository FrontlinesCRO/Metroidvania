using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Utilities.Comparers
{
    public readonly struct RaycastHitDistanceComparer : IComparer, IComparer<RaycastHit>
    {
        private readonly bool _ascending;

        public RaycastHitDistanceComparer(bool ascending = true)
        {
            _ascending = ascending;
        }

        public int Compare(object x, object y)
        {
            if (x is RaycastHit hit && y is RaycastHit otherHit)
                return Compare(hit, otherHit);

            return Comparer.Default.Compare(x, y);
        }

        public int Compare(RaycastHit x, RaycastHit y)
        {
            var comparison = x.distance.CompareTo(y.distance);

            return _ascending ? comparison : -comparison;
        }
    }
}
