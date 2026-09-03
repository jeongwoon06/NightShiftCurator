using UnityEngine;
using NightShiftCurator.Loot;
using NightShiftCurator.Cart;

namespace NightShiftCurator.Player
{
    public class PlayerInteraction : MonoBehaviour
    {
        public Camera eye;
        public float interactRange = 2.5f;
        public LayerMask interactMask = ~0;
        public Transform holdPoint;

        [Header("카트를 끄는 동안 이동속도 배율")]
        [Range(0.1f, 1f)] public float pushingSpeedMultiplier = 0.5f;

        private LootItem _heldItem;
        private CartController _grabbedCart; // 지금 손잡이를 잡고 있는 카트
        private PlayerController _controller;

        void Awake()
        {
            _controller = GetComponent<PlayerController>();
        }

        void Update()
        {
            if (eye != null)
            {
                Ray ray = new Ray(eye.transform.position, eye.transform.forward);
                if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactMask))
                {
                    var loot = hit.collider.GetComponentInParent<LootItem>();
                    var lookedCart = hit.collider.GetComponentInParent<CartController>();

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        if (_heldItem != null && lookedCart != null)
                        {
                            // 유물을 든 상태로 카트를 보고 E → 적재 (손잡이 잡기보다 우선)
                            if (lookedCart.TryLoadItem(_heldItem))
                                _heldItem = null;
                        }
                        else if (_heldItem == null && loot != null && !loot.IsHeld)
                        {
                            PickUp(loot);
                        }
                        else if (_heldItem == null && lookedCart != null && _grabbedCart == null)
                        {
                            _grabbedCart = lookedCart;
                            _grabbedCart.Grab(transform);
                        }
                        else if (_grabbedCart != null && lookedCart == _grabbedCart)
                        {
                            // 잡고 있는 카트를 다시 보고 E → 놓기
                            _grabbedCart.Release();
                            _grabbedCart = null;
                        }
                    }
                }
                else if (Input.GetKeyDown(KeyCode.E) && _grabbedCart != null)
                {
                    // 아무것도 안 보고 있어도 E로 놓을 수 있게
                    _grabbedCart.Release();
                    _grabbedCart = null;
                }
            }

            // 카트를 끄는 동안은 플레이어 이동속도 자체도 느려짐 (무거운 걸 끄는 느낌)
            if (_controller != null)
                _controller.speedMultiplier = _grabbedCart != null ? pushingSpeedMultiplier : 1f;
        }

        void PickUp(LootItem item)
        {
            _heldItem = item;
            item.SetHeld(true, holdPoint != null ? holdPoint : eye.transform);
        }
    }
}
