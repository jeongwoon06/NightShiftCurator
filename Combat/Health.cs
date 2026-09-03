using UnityEngine;
using UnityEngine.Events;

namespace NightShiftCurator.Combat
{
    public class Health : MonoBehaviour
    {
        public float maxHealth = 100f;
        public float currentHealth;

        public UnityEvent onDamaged;
        public UnityEvent onDeath;

        void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (currentHealth <= 0f) return;

            currentHealth -= amount;
            onDamaged?.Invoke();

            if (currentHealth <= 0f)
            {
                currentHealth = 0f;
                onDeath?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        }

        public bool IsDead => currentHealth <= 0f;
    }
}
