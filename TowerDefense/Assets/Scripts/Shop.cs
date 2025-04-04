
using UnityEngine;

public class Shop : MonoBehaviour
{
    BuildManager buildManager;

    void Start ()
    {
        buildManager = BuildManager.instance;
    }
   public void SelectStandardTurret ()
    {
        Debug.Log("Standard Turret Selected");
        buildManager.SetTurretToBuild(buildManager.standardTurretPrefab);
    }

    public void SelectSecondTurret()
    {
        Debug.Log("Standard Turret Selected");
        buildManager.SetTurretToBuild(buildManager.secondTurretPrefab);
    }

    /* Copy and paste to create again for other turret variants 
      public void PurchaseStandardTurret ()
        {
            Debug.Log("Standard Turret Purchased");
        }
        Click on shop item add a onclick add shop to it and then select proper function
    */


}
