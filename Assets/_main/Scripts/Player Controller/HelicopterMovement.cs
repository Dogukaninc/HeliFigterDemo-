using UnityEngine;

/// <summary>
/// Top-down helikopter hareketi. Joystick ile yatay (XZ) düzlemde her yöne gider.
///
/// Hierarchy:
///   PlayerController (bu script + pozisyon/yaw)  ->  Renderer (lean tilt)  ->  helicoptermodel
///
/// Davranış:
///   - Hedef YOKSA: gövde hareket yönüne doğru smooth döner (yaw), ve o yöne lean olur.
///   - Hedef VARSA: gövde forward'ı hedefe aim alır; joystick ile sağa/sola strafe atılabilir,
///     helikopter yine de hareket yönüne göre lean (eğim) alır.
/// </summary>
[RequireComponent(typeof(HelicopterTargeting))]
public class HelicopterMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Joystick joystick;
    [Tooltip("Lean/tilt uygulanacak child (helikopter modelinin parent'ı olan Renderer).")]
    [SerializeField] private Transform rendererTransform;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [Tooltip("Hız değişiminin yumuşatılması (SmoothDamp süresi). 0 = anında.")]
    [SerializeField] private float moveSmoothTime = 0.1f;

    [Header("Rotation (Yaw)")]
    [Tooltip("Gövdenin dönüş yumuşaklığı (derece/saniye).")]
    [SerializeField] private float turnSpeed = 540f;

    [Header("Lean / Tilt")]
    [Tooltip("Helikopterin hareket yönüne doğru eğilebileceği maksimum açı.")]
    [SerializeField] private float maxLeanAngle = 25f;
    [Tooltip("Lean açısına ulaşma yumuşaklığı.")]
    [SerializeField] private float leanSmoothTime = 0.12f;
    [Tooltip("Model ters kuruluysa (örn. 180° dönük) buradan düzelt.")]
    [SerializeField] private float modelYawOffset = 180f;
    [Tooltip("Lean yönü ters geliyorsa işaretle.")]
    [SerializeField] private bool invertLean = true;

    private HelicopterTargeting _targeting;
    private Vector3 _currentVelocity;      // aktif hareket hızı (SmoothDamp için)
    private Vector3 _velocityRef;          // SmoothDamp referansı

    private Vector2 _leanAngles;           // (pitch, roll) mevcut lean
    private Vector2 _leanVelRef;           // lean SmoothDamp referansı

    private void Awake()
    {
        _targeting = GetComponent<HelicopterTargeting>();
        if (rendererTransform == null && transform.childCount > 0)
            rendererTransform = transform.GetChild(0);
    }

    private void Update()
    {
        Vector2 input = joystick != null ? joystick.Direction : Vector2.zero;
        Vector3 moveDir = new Vector3(input.x, 0f, input.y);
        float inputMag = Mathf.Clamp01(moveDir.magnitude);

        HandleMovement(moveDir, inputMag);
        HandleYaw(moveDir, inputMag);
        HandleLean();
    }

    private void HandleMovement(Vector3 moveDir, float inputMag)
    {
        Vector3 targetVel = moveDir.normalized * (moveSpeed * inputMag);
        _currentVelocity = Vector3.SmoothDamp(_currentVelocity, targetVel, ref _velocityRef, moveSmoothTime);
        transform.position += _currentVelocity * Time.deltaTime;
    }

    private void HandleYaw(Vector3 moveDir, float inputMag)
    {
        Vector3 faceDir;

        if (_targeting.HasTarget)
        {
            // Hedefe aim al: forward hedefe baksın (yatay düzlemde).
            Vector3 toTarget = _targeting.CurrentTarget.position - transform.position;
            toTarget.y = 0f;
            faceDir = toTarget;
        }
        else if (inputMag > 0.01f)
        {
            // Hedef yok: hareket yönüne dön.
            faceDir = moveDir;
        }
        else
        {
            return; // input yok, hedef yok -> mevcut yönü koru.
        }

        if (faceDir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(faceDir.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, targetRot, turnSpeed * Time.deltaTime);
    }

    private void HandleLean()
    {
        if (rendererTransform == null) return;

        // Hareketi gövdenin local uzayına çevir: forward bileşeni pitch, right bileşeni roll üretir.
        Vector3 localVel = transform.InverseTransformDirection(_currentVelocity);
        float normalized = moveSpeed > 0.01f ? 1f / moveSpeed : 0f;

        float sign = invertLean ? -1f : 1f;
        float targetPitch = Mathf.Clamp(localVel.z * normalized, -1f, 1f) * maxLeanAngle * sign;   // ileri = burun aşağı
        float targetRoll = Mathf.Clamp(-localVel.x * normalized, -1f, 1f) * maxLeanAngle * sign;   // sağa git = sağa yat

        _leanAngles = Vector2.SmoothDamp(
            _leanAngles, new Vector2(targetPitch, targetRoll), ref _leanVelRef, leanSmoothTime);

        // Model yaw offset'i (ters kurulum) uygulanır, lean bunun üzerine gövde uzayında biner.
        rendererTransform.localRotation =
            Quaternion.Euler(0f, modelYawOffset, 0f) * Quaternion.Euler(_leanAngles.x, 0f, _leanAngles.y);
    }
}
