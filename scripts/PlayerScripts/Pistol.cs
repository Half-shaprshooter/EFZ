namespace EscapeFromZone.scripts.PlayerScripts;

public partial class Pistol : Gun
{
    public override void _Ready()
    {
        spread = 5;
        isAuto = false;
        bulletsPerSecond = 2f;
        bulletSpeed = 8000f;
        bulletDamage = 40f;
        bulletsPerShoot = 1;
        
        base._Ready();
    }

}