using UnityEngine;
using UnityEngine.Events;
using NightShiftCurator.Cart;

namespace NightShiftCurator.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class ExtractionZone : MonoBehaviour
    {
        public UnityEvent onExtractionSuccess;
        private bool _triggered;

        void OnTriggerEnter(Collider other)
        {
            if (_triggered) return;

            var cart = other.GetComponentInParent<CartController>();
            if (cart == null) return;

            _triggered = true;
            Debug.Log($"[탈출 성공] 확보한 유물 {cart.LoadedCount}개, 총 가치 {cart.TotalValue}");
            onExtractionSuccess?.Invoke();
        }
    }
}
