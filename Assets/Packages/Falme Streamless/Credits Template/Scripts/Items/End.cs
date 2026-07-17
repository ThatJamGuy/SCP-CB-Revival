using UnityEngine;
using System;

namespace FalmeStreamless.Credits
{
    public class End : MonoBehaviour
    {
        public static event Action<float> onCreditEndReached;

        private RectTransform rectTransform;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        void Update()
        {
            if (hasReachedTopBorder())
                onCreditEndReached?.Invoke(rectTransform.position.y - Screen.height);
        }

        public bool hasReachedTopBorder() => rectTransform.position.y > Screen.height;
    }
}
