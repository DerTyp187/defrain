using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class WeaponManager : NetworkBehaviour
{
    public int currentWeaponIndex = 2; // Hand
    private int lastWeaponIndex = 0;
    public List<GameObject> activeWeapons = new List<GameObject>();
    private ProcedualAnimationController procedualAnimationController;
    private Weapon weaponData;

    [SerializeField] Shoot shoot;
    [SerializeField] GameObject gunHolster;
    [SerializeField] Camera cam;


    private void Awake()
    {
        procedualAnimationController = GetComponent<ProcedualAnimationController>();
        currentWeaponIndex = 2; // Hand
        weaponData = activeWeapons[currentWeaponIndex].GetComponent<Weapon>(); // Hand
    }

    void Update() {
        if (isLocalPlayer) {
            if (Input.GetAxis("Mouse ScrollWheel") > 0f) { // Scroll up
                lastWeaponIndex = currentWeaponIndex;
                activeWeapons[currentWeaponIndex].SetActive(false);
                switchWeapon(-1);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f) { // Scroll down
                lastWeaponIndex = currentWeaponIndex;
                activeWeapons[currentWeaponIndex].SetActive(false);
                switchWeapon(1);
            }
            if (Input.GetButtonDown("Interact")) { // e 
                    PickupWeapon();
            }else if (Input.GetButtonDown("Drop")) { // q Droping weapon 
                if (activeWeapons[currentWeaponIndex] != null) {
                    dropWeapon(activeWeapons[currentWeaponIndex].GetComponent<Weapon>().DropForce, currentWeaponIndex); // Throws weapon away
                    switchWeapon(1);
                }
            }
        }
    }

    private void FixedUpdate() {
        /*if(currentWeaponIndex != 2) {
            if (weaponData.ToCloseToWall) {
                procedualAnimationController.weaponToCloseToWall(true);
            } else {
                procedualAnimationController.weaponToCloseToWall(false);
            }
        }*/
    }

    public bool switchWeapon(int direction) {
        // Get next active weapon index
        int nextActive = searchForNext(activeWeapons, lastWeaponIndex, direction);
        currentWeaponIndex = nextActive;
        procedualAnimationController.OnSwitchWeapon(activeWeapons[currentWeaponIndex]);
        shoot.setWeapon(activeWeapons[currentWeaponIndex]);
        weaponData = activeWeapons[currentWeaponIndex].GetComponent<Weapon>();
        procedualAnimationController.GunRightHandREF = weaponData.GunRightREF;
        procedualAnimationController.GunLeftHandREF = weaponData.GunLeftREF;
        // Play weapon switch animation
        switchAnimation(weaponData.WeaponKind.ToString()); 

        activeWeapons[currentWeaponIndex].SetActive(true);
        return false;
    }
    private void switchAnimation(string weaponType) {
        switch (weaponType) {
            case "Rifle": procedualAnimationController.changeRifle(true); break;
            case "Pistole": procedualAnimationController.changePistole(true); break;
            case "Knife": ; procedualAnimationController.changePistole(true); break;
            case "Grenade": ; procedualAnimationController.changePistole(true); break;
        }
    }
    private int searchForNext(List<GameObject> l, int lastActive = 0, int direction = 1)
    {
        int current = lastActive + direction;

        for (int trys = 0; trys < l.Count; trys++)
        {
            //Check if you are at the start or end of the list and loop around accordingly
            if (current < 0) current = l.Count - 1; 
            else if (current >= l.Count) current = 0;
            
            //Check if in the current position is a gun or not 
            if (l[current] != null) 
            {
                return current;
            }
            //if there is no gun go to the next
            current = current + direction;
        }
        //If no next gun can be found return the index of the gun that was already selected
        return lastActive;
    }


    public GameObject getCurrentWeapon() {
        return activeWeapons[currentWeaponIndex].gameObject;
    }

    private void PickupWeapon() {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit)) 
        {
            // If Object is a weapon and the weapon is not in the current active weapons
            if (hit.transform.tag == "Weapon") 
            { 
                // Disable all weapons
                foreach (GameObject obj in activeWeapons) {
                    if (obj != null) { obj.SetActive(false); }
                }
                // Parent weapon to gunHolster
                hit.transform.parent = gunHolster.transform; 
                hit.rigidbody.isKinematic = true;
                hit.rigidbody.useGravity = false;
                // Disable all Collider
                SetAllColliderStatus(hit.transform.gameObject, false);
                // Adding weapon to correct inventory slot
                switch (hit.transform.GetComponent<Weapon>().WeaponKind.ToString()) { 
                    case "Rifle": putWeaponInArray(0, hit); break;
                    case "Pistole": putWeaponInArray(1, hit); break;
                    case "Knife": putWeaponInArray(2, hit); break;
                    case "Grenade": putWeaponInArray(3, hit); break;
                    default: break;
                }
            }
        }
    }

    private bool putWeaponInArray(int index, RaycastHit hit) {
        if (activeWeapons[index] != null) {
            // Throws weapon away
            dropWeapon(activeWeapons[index].GetComponent<Weapon>().DropForce, index); 
        }
        activeWeapons[index] = hit.transform.gameObject;
        activeWeapons[index].SetActive(true);
        // \/ Same as in switchWeapon()
        currentWeaponIndex = index;
        procedualAnimationController.OnSwitchWeapon(activeWeapons[currentWeaponIndex]);
        shoot.setWeapon(activeWeapons[currentWeaponIndex]);
        Weapon weaponData = activeWeapons[currentWeaponIndex].GetComponent<Weapon>();
        procedualAnimationController.GunRightHandREF = weaponData.GunRightREF;
        procedualAnimationController.GunLeftHandREF = weaponData.GunLeftREF;
        return true;
    }

    public bool dropWeapon(float dropForce, int index) {
        if(index != 2) {
            GameObject currentWeapon = activeWeapons[index];
            currentWeapon.SetActive(true);
            Rigidbody rigid = currentWeapon.GetComponent<Rigidbody>();
            rigid.useGravity = true;
            rigid.isKinematic = false;
            rigid.velocity = cam.transform.forward * dropForce + cam.transform.up * 2;
            // Activate all Collider
            SetAllColliderStatus(currentWeapon, true); 
            currentWeapon.gameObject.transform.SetParent(null);
            activeWeapons[index] = null;
            return true;
        }
        else {
            return false;
        }
    }
    
    // Set status to all colliders on a gameobject
    private void SetAllColliderStatus(GameObject obj, bool newStatus) {
        // For every collider on gameobject 
        foreach(Collider col in obj.GetComponents<Collider>()) {
            // set newStatus (enable, disable)
            col.enabled = newStatus;
        }
    }

}
