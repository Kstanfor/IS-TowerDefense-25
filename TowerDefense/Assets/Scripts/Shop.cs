using UnityEngine;

public class Shop : MonoBehaviour
{
    public TurretBlueprint standardTurret;
    public TurretBlueprint fireTurretStandard;
    public TurretBlueprint iceTurretStandard;
    public TurretBlueprint waterTurretStandard;

    public TurretBlueprint missleLauncher;
    public TurretBlueprint fireMissleLauncher;
    public TurretBlueprint iceMissleLauncher;
    public TurretBlueprint waterMissleLauncher;

    public TurretBlueprint laserBeamer;

    BuildManager buildManager;

    void Start ()
    {
        buildManager = BuildManager.instance;
    }
   public void SelectStandardTurret ()
    {
        Debug.Log("Standard Turret Selected");
        buildManager.SelectTurretToBuild(standardTurret);
    }

    public void SelectFireStandardTurret()
    {
        Debug.Log("Standard Fire Turret Selected");
        buildManager.SelectTurretToBuild(fireTurretStandard);
    }

    public void SelectIceStandardTurret()
    {
        Debug.Log("Standard Ice Turret Selected");
        buildManager.SelectTurretToBuild(iceTurretStandard);
    }

    public void SelectWaterStandardTurret()
    {
        Debug.Log("Standard Water Turret Selected");
        buildManager.SelectTurretToBuild(waterTurretStandard);
    }

    public void SelectMissleLauncher()
    {
        Debug.Log("Missle Launcher Selected");
        buildManager.SelectTurretToBuild(missleLauncher);
    }

    public void SelectFireMissleLauncher()
    {
        Debug.Log("Fire Missle Launcher Selected");
        buildManager.SelectTurretToBuild(fireMissleLauncher);
    }

    public void SelectIceMissleLauncher()
    {
        Debug.Log("Ice Missle Launcher Selected");
        buildManager.SelectTurretToBuild(iceMissleLauncher);
    }

    public void SelectWaterMissleLauncher()
    {
        Debug.Log("Water Missle Launcher Selected");
        buildManager.SelectTurretToBuild(waterMissleLauncher);
    }

    public void SelectLaserBeamer()
    {
        Debug.Log("Laser Beamer Selected");
        buildManager.SelectTurretToBuild(laserBeamer);
    }

    /* Copy and paste to create again for other turret variants 
      public void PurchaseStandardTurret ()
        {
            Debug.Log("Standard Turret Purchased");
        }
        Click on shop item add a onclick add shop to it and then select proper function
    */


}
