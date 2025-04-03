
using UnityEngine;

public class BuildManager : MonoBehaviour
{

    public static BuildManager instance;

    void Awake ()
    {
        if (instance !=null)
        {
            Debug.LogError("More Than One BuildManager!");
        }
        instance = this;
    }

    public GameObject standardTurretPrefab;

    // Start is called before the first frame update
    void Start()
    {
        turretToBuild = standardTurretPrefab;
    }

    private GameObject turretToBuild;

    public GameObject GetTurretToBuild()
    {
        return turretToBuild;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
