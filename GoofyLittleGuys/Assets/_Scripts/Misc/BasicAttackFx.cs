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
        Debug.Log($"convertRotation: {convertRotation} & _rotation: {_rotation}");
        switch (convertRotation)
        {
            // Upwards animation
            case > 25:
                upwardsAttack.gameObject.SetActive(true);
                upwardsAttack.ResetTrigger(Attack);
                upwardsAttack.SetTrigger(Attack);
                break;
            // Downwards animation
            case < -25:
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
        _flip = Mathf.Abs(_rotation) > 90 || Mathf.Abs(_rotation) < -90;

        var convertRotation = ConvertRotation(_rotation);
        switch (convertRotation)
        {
            // Upwards animation
            case > 25:
                upwardsAttack.gameObject.transform.rotation = Quaternion.Euler(0, _flip ? 0 : 180, 0);
                break;
            // Downwards animation
            case < -25:
                downwardsAttack.gameObject.transform.rotation = Quaternion.Euler(0, _flip ? 0 : 180, 0);
                break;
            // Center animation
            default:
                centerAttack.gameObject.transform.rotation = Quaternion.Euler(0, _flip ? 0 : 180, 0);
                break;
        }
    }

    /// <summary>
    /// Convert the rotation to constantly be between 90 and -90 based on the attack orbit of a player
    /// </summary>
    /// <param name="rotation">Rotation to convert</param>
    /// <returns>Converted rotation value</returns>
    private float ConvertRotation(float rotation)
    {
        // had this line for some reason it doesnt work properly even tho the math seems fine... idk come back later :3
        //return rotation > 90 ? (rotation - 180) * -1 : (rotation < -90 ? (rotation + 180) * -1 : rotation);

        if (rotation > 90)
        {
            return (rotation - 180) * -1;
        }
        if (rotation < -90)
        {
            return (rotation + 180) * -1;
        }
        return rotation;
    }
}
