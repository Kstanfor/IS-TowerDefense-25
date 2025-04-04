
using UnityEngine;

public class Shop : MonoBehaviour
{
    public TurretBlueprint standardTurret;
    public TurretBlueprint missleLauncher;

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

    public void SelectMissleLauncher()
    {
        Debug.Log("Missle Launcher Selected");
        buildManager.SelectTurretToBuild(missleLauncher);
    }

    /* Copy and paste to create again for other turret variants 
      public void PurchaseStandardTurret ()
        {
            Debug.Log("Standard Turret Purchased");
        }
        Click on shop item add a onclick add shop to it and then select proper function
    */


}
