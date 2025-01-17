using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class DebugCanvas : MonoBehaviour
{
    public TextMeshProUGUI DebugTextGrounded;
    public TextMeshProUGUI DebugTextClientServer;
    public TextMeshProUGUI DebugAmmunition;
    public GameObject Player;
    public GameObject GameManager;
    public TextMeshProUGUI fpsText;
    public float deltaTime;

    private Shoot shoot;

    private void Start()
    {
        GameManager = GameObject.Find("GameManager");

    }
    private void Update()
    {
        if(Player == null) {
            try {
                Player = GameObject.FindGameObjectWithTag("Player").gameObject;
                shoot = Player.GetComponent<Shoot>();
                Debug.Log("Player Found");
            }
            catch {
                //Debug.Log("DEBUG CANVAS PLAYER NOT YET FOUND");
            }
        } else {
            DebugTextGrounded.text = "isGrounded: " + Player.GetComponent<PlayerController>().isGrounded.ToString();
            if (Player) {
                DebugAmmunition.text = shoot.CurAmmo.ToString() + " / " + shoot.TotalAmmo.ToString();
            }
            deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / deltaTime;
            fpsText.text = Mathf.Ceil(fps).ToString() + "FPS";
        }
        
    }

}
