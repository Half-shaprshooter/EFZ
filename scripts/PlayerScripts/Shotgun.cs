namespace EscapeFromZone.scripts.PlayerScripts;

public partial class Shotgun : Gun
{
    public override void _Ready()
    {
        spread = 15;
        isAuto = true;
        bulletsPerSecond = 10f;
        bulletSpeed = 12000f;
        bulletDamage = 25f;
        bulletsPerShoot = 1;
		
        // Вызываем базовый метод Ready
        base._Ready();
    }
}