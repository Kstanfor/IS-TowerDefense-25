using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static int Money;
    public int startMoney = 400;

    public static int Lives = 5;
    //public int startLives = 25;

    public static int Rounds;

    void Start()
    {
        Money = startMoney;
        //Lives = startLives;

        Rounds = 0;
    }



}
