using UnityEngine;

public class BasicAttackFx : MonoBehaviour
{
    [SerializeField] private Hitbox hitbox;
    [SerializeField] private Animator centerAttack;
    [SerializeField] private Animator upwardsAttack;
    [SerializeField] private Animator downwardsAttack;

    private LilGuyBase _owner;
    private float _rotation;
    private bool _flip;

    private void Awake()
    {
        _owner = hitbox.hitboxOwner.GetComponent<LilGuyBase>();
    }

    private void Update()
    {
        // Flip the sprite to left or right for proper direction
        _rotation = _owner.AttackOrbit.rotation.y;
        _flip = Mathf.Abs(_rotation) < 90;

        _rotation = ConvertRotation(_rotation);
        if (_rotation is > 25 and <= 90) // Upwards animation
        {
            if (_flip)
                upwardsAttack.transform.rotation = Quaternion.Euler(0, 180, 0);
            else
                upwardsAttack.transform.rotation = Quaternion.Euler(0, 0, 0);
            
            // up and check flip
        }
        else if (_rotation is >= -90 and < -25) // Downwards animation
        {
            if (_flip)
                downwardsAttack.transform.rotation = Quaternion.Euler(0, 180, 0);
            else
                downwardsAttack.transform.rotation = Quaternion.Euler(0, 0, 0);
            
            // down and check flip
        }
        else // Center animation
        {
            if (_flip)
                centerAttack.transform.rotation = Quaternion.Euler(0, 180, 0);
            else
                centerAttack.transform.rotation = Quaternion.Euler(0, 0, 0);
            
            //center and check flip
        }
    }

    /// <summary>
    /// Convert the rotation to constantly be between 90 and -90 based on the attack orbit of a player
    /// </summary>
    /// <param name="rotation">Rotation to convert</param>
    /// <returns>Converted rotation value</returns>
    private float ConvertRotation(float rotation)
    {
        return rotation > 90 ? (rotation - 180) * -1 : (rotation < -90 ? (rotation + 180) * -1 : rotation);
    }
    
    
}
