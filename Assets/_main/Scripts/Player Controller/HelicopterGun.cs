using UnityEngine;

/// <summary>
/// Helikopter silah kontrolü. Hedef varken fireRate'e göre otomatik ateş eder.
/// Muzzle noktalarından sırayla mermi (projectile) fırlatır.
/// </summary>
[RequireComponent(typeof(HelicopterTargeting))]
public class HelicopterGun : MonoBehaviour
{
    [Header("Firing")]
    [Tooltip("Saniyede kaç atış yapılacağı.")]
    [SerializeField] private float fireRate = 8f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 40f;
    [SerializeField] private float projectileDamage = 10f;

    [Header("Muzzles")]
    [Tooltip("Merminin çıkacağı namlu noktaları. Birden fazlaysa sırayla kullanılır.")]
    [SerializeField] private Transform[] muzzles;

    [Header("Aim")]
    [Tooltip("Sadece gövde forward'ı hedefe yeterince yakınken ateş et (derece).")]
    [SerializeField] private float fireAngleThreshold = 12f;

    private HelicopterTargeting _targeting;
    private float _fireCooldown;
    private int _muzzleIndex;

    private void Awake()
    {
        _targeting = GetComponent<HelicopterTargeting>();
    }

    private void Update()
    {
        _fireCooldown -= Time.deltaTime;

        if (!_targeting.HasTarget) return;
        if (!IsAimedAtTarget()) return;

        if (_fireCooldown <= 0f && fireRate > 0f)
        {
            Fire();
            _fireCooldown = 1f / fireRate;
        }
    }

    private bool IsAimedAtTarget()
    {
        Vector3 toTarget = _targeting.CurrentTarget.position - transform.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude < 0.0001f) return true;

        float angle = Vector3.Angle(transform.forward, toTarget.normalized);
        return angle <= fireAngleThreshold;
    }

    private void Fire()
    {
        if (projectilePrefab == null || muzzles == null || muzzles.Length == 0) return;

        Transform muzzle = muzzles[_muzzleIndex];
        _muzzleIndex = (_muzzleIndex + 1) % muzzles.Length;

        GameObject proj = Instantiate(projectilePrefab, muzzle.position, muzzle.rotation);

        // Rigidbody varsa hız ver, yoksa varsa Projectile componentine parametreleri ilet.
        if (proj.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = muzzle.forward * projectileSpeed;
        }

        if (proj.TryGetComponent(out Projectile p))
        {
            p.Init(muzzle.forward * projectileSpeed, projectileDamage);
        }
    }
}
