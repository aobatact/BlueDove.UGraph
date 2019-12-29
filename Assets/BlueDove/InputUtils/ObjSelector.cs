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
        
        public static (T1, T2) GetObjectFromHits2<T1, T2>(RaycastHit[] hits, int count)
        {
            T2 newest = default;
            for (var i = 0; i < count; i++)
            {
                var hit = hits[i];
                var value = hit.transform.GetComponent<T1>();
                if (!value.Equals(default))
                    return (value, default);
                var value2 = hit.transform.GetComponent<T2>();
                if (!value2.Equals(default))
                    newest = value2;
            }
            return (default, newest);
        }
    }
}