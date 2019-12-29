using System;
using UnityEngine;

namespace BlueDove.InputUtils
{
    public static class ObjSelector
    {
        public static T GetObjectFromHits<T>(RaycastHit[] hits, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var hit = hits[i];
                var value = hit.transform.GetComponent<T>();
                if (!value.Equals(default))
                    return value;
            }
            return default;
        }
    }
}