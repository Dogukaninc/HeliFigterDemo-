using System;
using UnityEngine;

/// <summary>
/// Menzil içindeki en yakın hedefi bulur. Movement ve Gun sistemleri
/// bu component'ten CurrentTarget'ı okur.
/// </summary>
public class HelicopterTargeting : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float aimRange = 15f;
    [SerializeField] private LayerMask enemyLayers = ~0;

    [Tooltip("Hedef taramasının kaç saniyede bir yapılacağı (performans).")]
    [SerializeField] private float scanInterval = 0.15f;

    private readonly Collider[] _hits = new Collider[32];
    private float _scanTimer;
    [SerializeField] private Transform origin;
    
    public Transform CurrentTarget { get; private set; }
    public bool HasTarget => CurrentTarget != null;
    public float AimRange => aimRange;

    private void Update()
    {
        _scanTimer -= Time.deltaTime;
        if (_scanTimer <= 0f)
        {
            _scanTimer = scanInterval;
            ScanForTarget();
        }

        // Mevcut hedef menzilden çıktıysa ya da yok olduysa bırak.
        if (CurrentTarget != null)
        {
            if (!CurrentTarget.gameObject.activeInHierarchy ||
                FlatDistance(CurrentTarget.position) > aimRange)
            {
                CurrentTarget = null;
            }
        }
    }

    private void ScanForTarget()
    {
        int count = Physics.OverlapSphereNonAlloc(origin.transform.position, aimRange, _hits, enemyLayers);

        Transform nearest = null;
        float nearestSqr = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Transform t = _hits[i].transform;
            float sqr = FlatSqrDistance(t.position);
            if (sqr < nearestSqr)
            {
                nearestSqr = sqr;
                nearest = t;
            }
        }

        CurrentTarget = nearest;
    }

    // Yatay düzlemde mesafe (yükseklik farkını yok say).
    private float FlatDistance(Vector3 point) => Mathf.Sqrt(FlatSqrDistance(point));

    private float FlatSqrDistance(Vector3 point)
    {
        float dx = point.x - transform.position.x;
        float dz = point.z - transform.position.z;
        return dx * dx + dz * dz;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin.transform.position, aimRange);
    }
}
