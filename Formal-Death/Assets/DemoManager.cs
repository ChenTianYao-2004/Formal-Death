using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoManager : MonoBehaviour
{
    private bool showWeapon;
    public GameObject weapon;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void Awake()
    {
        showWeapon = false;
        weapon.SetActive(showWeapon);
    }

    // Update is called once per frame
    void Update()
    {
        ControlWeaponActive();
    }

    void ControlWeaponActive()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            showWeapon = !showWeapon;
            weapon.SetActive(showWeapon);
        }
    }
}
