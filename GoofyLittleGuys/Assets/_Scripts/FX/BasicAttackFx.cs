using UnityEngine;

public class BasicAttackFx : MonoBehaviour
{
    private static readonly int Attack = Animator.StringToHash("Attack");
    [SerializeField] private Animator centerAttack;
    [SerializeField] private Animator upwardsAttack;
    [SerializeField] private Animator downwardsAttack;

    private LilGuyBase _owner;
    private float _rotation;
    private bool _flip;
    
    /*
     * just now realizing i switched the logic for flipping the fx,
     * so when flip is true it doesn't flip and when it's false it does.
     * I also flipped the single line if statements so it's fine :3
     */

    public void Init(LilGuyBase owner)
    {
        _owner = owner;
    }
    
    private void Start()
    {
        _rotation = _owner.AttackOrbit.rotation.eulerAngles.y;

        var convertRotation = ConvertRotation(_rotation);
		Managers.DebugManager.Log($"convertRotation: {convertRotation} & _rotation: {_rotation}", Managers.DebugManager.DebugCategory.COMBAT);
        switch (convertRotation)
        {
            // Upwards animation
            case > 25 and < 90:
                upwardsAttack.gameObject.SetActive(true);
                upwardsAttack.ResetTrigger(Attack);
                upwardsAttack.SetTrigger(Attack);
                break;
            // Downwards animation
            case < 335 and > 270:
                downwardsAttack.gameObject.SetActive(true);
                downwardsAttack.ResetTrigger(Attack);
                downwardsAttack.SetTrigger(Attack);
                break;
            // Center animation
            default:
                centerAttack.gameObject.SetActive(true);
                centerAttack.ResetTrigger(Attack);
                centerAttack.SetTrigger(Attack);
                break;
        }
    }

    private void Update()
    {
        // flippin code (flip the attack to proper orientation)
        _rotation = _owner.AttackOrbit.rotation.eulerAngles.y;
        _flip = _rotation is > 90 and < 270;

        var convertRotation = ConvertRotation(_rotation);
        switch (convertRotation)
        {
            // Upwards animation
            case > 25 and < 90:
                upwardsAttack.gameObject.transform.rotation = Quaternion.Euler(0, _flip ? 0 : 180, 0);
                break;
            // Downwards animation
            case < 335 and > 270:
                downwardsAttack.gameObject.transform.rotation = Quaternion.Euler(0, _flip ? 0 : 180, 0);
                break;
            // Center animation
            default:
                centerAttack.gameObject.transform.rotation = Quaternion.Euler(0, _flip ? 0 : 180, 0);
                break;
        }
    }

    /// <summary>
    /// Convert the rotation to constantly be between quadrants 1 and 4 when given a rotation
    /// </summary>
    /// <param name="rotation">Rotation to convert</param>
    /// <returns>Converted rotation value</returns>
    private float ConvertRotation(float rotation)
    {
        return rotation is > 90 and < 270 ? (rotation is < 180 ? (rotation - 180) * -1 : 360 - (rotation - 180)) 
            : rotation;
    }
}
