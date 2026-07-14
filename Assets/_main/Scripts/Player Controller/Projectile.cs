using UnityEngine;

/// <summary>
/// Basit mermi. HelicopterGun tarafından hız ve hasar ile başlatılır.
/// Rigidbody ile hareket ediyorsa kendi hareketini uygulamaz.
/// </summary>
public class Projectile : MonoBehaviour
{
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private GameObject hitVfx;

    private Vector3 _velocity;
    private float _damage;
    private bool _hasRigidbody;

    public void Init(Vector3 velocity, float damage)
    {
        _velocity = velocity;
        _damage = damage;
        _hasRigidbody = TryGetComponent<Rigidbody>(out _);

        if (_velocity.sqrMagnitude > 0.0001f)
            transform.forward = _velocity.normalized;

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (!_hasRigidbody)
            transform.position += _velocity * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IDamageable damageable))
            damageable.TakeDamage(_damage);

        if (hitVfx != null)
            Instantiate(hitVfx, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
