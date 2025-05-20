using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System.Globalization;

public class LivesUI : MonoBehaviour
{
    public Text livesText;
   
    void Update()
    {
        livesText.text = PlayerStats.Lives.ToString() + " Lives";  
    }
}
