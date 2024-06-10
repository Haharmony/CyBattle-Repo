using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Cinemachine;
using Photon.Pun;
using UnityEngine.UI;
using ExitGames.Client.Photon.StructWrapping;

public class WeaponChangePro : MonoBehaviour
{
    public TwoBoneIKConstraint leftHand;
    public TwoBoneIKConstraint rightHand;

    private CinemachineVirtualCamera cam;
    private GameObject camObject;
    public MultiAimConstraint[] aimObjects;
    private Transform aimTarget;

    public RigBuilder rig;
    public Transform[] leftTargets;
    public Transform[] rightTargets;
    public GameObject[] weapons;
    private int weaponNumber = 0;
    private GameObject testForWeapons;
    private Image weaponIcon;
    private Text ammoAmtText;
    public Sprite[] weaponIcons;
    public int[] ammoAmts;
    public GameObject[] muzzleFlash;
    private string shooterName;
    private string gotShotName;
    public float[] damageAmts;
    public bool isDead = false;
    private GameObject choosePanel;


    // Start is called before the first frame update
    void Start()
    {
        choosePanel = GameObject.Find("ChoosePanel");

        weaponIcon = GameObject.Find("WeaponUI").GetComponent<Image>();
        ammoAmtText = GameObject.Find("AmmoAmt").GetComponent<Text>();
        camObject = GameObject.Find("PlayerCam");
        ammoAmts[0] = 60;
        ammoAmts[1] = 0;
        ammoAmts[2] = 0;
        ammoAmtText.text = ammoAmts[0].ToString();
        //aimTarget = GameObject.Find("AimRef").transform;
        if (this.gameObject.GetComponent<PhotonView>().IsMine == true)
        {
            cam = camObject.GetComponent<CinemachineVirtualCamera>();
            cam.Follow = this.gameObject.transform;
            cam.LookAt = this.gameObject.transform;
            //Invoke("SetLookAt", 0.1f);
        }
        else
        {
            this.gameObject.GetComponent<PlayerMovement>().enabled = false;
        }

        testForWeapons = GameObject.Find("Weapon1Pickup(Clone)");
        if (testForWeapons == null)
        {
            if (this.gameObject.GetComponent<PhotonView>().Owner.IsMasterClient == true)
            {
                var spawner = GameObject.Find("SpawnScript");
                spawner.GetComponent<SpawnCharacters>().SpawnWeaponsStart();
            }

        }
    }
    /*
        void SetLookAt()
        {
            if(aimTarget != null)
            {
                for(int i = 0; i < aimObjects.Length; i++)
                {
                    var target = aimObjects[i].data.sourceObjects;
                    target.SetTransform(0, aimTarget.transform);
                    aimObjects[i].data.sourceObjects = target;
                }
                rig.Build();
            }
        }*/

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && isDead == false && choosePanel.activeInHierarchy == false)
        {
            if (this.GetComponent<PhotonView>().IsMine == true && ammoAmts[weaponNumber] > 0)
            {
                ammoAmts[weaponNumber]--;
                ammoAmtText.text = ammoAmts[weaponNumber].ToString();
                GetComponent<DisplayColor>().PlayGunShot(GetComponent<PhotonView>().Owner.NickName, weaponNumber);
                this.GetComponent<PhotonView>().RPC("GunMuzzleFlash", RpcTarget.All);
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                this.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                if (Physics.Raycast(ray, out hit, 500))
                {
                    if (hit.transform.gameObject.GetComponent<PhotonView>() != null)
                    {
                        gotShotName = hit.transform.gameObject.GetComponent<PhotonView>().Owner.NickName;
                    }
                    if (hit.transform.gameObject.GetComponent<DisplayColor>() != null)
                    {
                        hit.transform.gameObject.GetComponent<DisplayColor>().DeliverDamage(this.GetComponent<PhotonView>().Owner.NickName, hit.transform.gameObject.GetComponent<PhotonView>().Owner.NickName, damageAmts[weaponNumber]);
                    }

                    shooterName = GetComponent<PhotonView>().Owner.NickName;
                }
            }
            this.gameObject.layer = LayerMask.NameToLayer("Default");
        }
        if (Input.GetMouseButtonDown(1) && this.gameObject.GetComponent<PhotonView>().IsMine == true && isDead == false)
        {
            this.GetComponent<PhotonView>().RPC("Change", RpcTarget.AllBuffered);
            if (weaponNumber > weapons.Length - 1)
            {
                weaponIcon.GetComponent<Image>().sprite = weaponIcons[0];
                ammoAmtText.text = ammoAmts[0].ToString();
                weaponNumber = 0;
            }
            for (int i = 0; i < weapons.Length; i++)
            {
                weapons[i].SetActive(false);
            }
            weapons[weaponNumber].SetActive(true);
            weaponIcon.GetComponent<Image>().sprite = weaponIcons[weaponNumber];
            ammoAmtText.text = ammoAmts[weaponNumber].ToString();
            leftHand.data.target = leftTargets[weaponNumber];
            rightHand.data.target = rightTargets[weaponNumber];
            rig.Build();
        }
    }

    public void UpdatePickup()
    {
        ammoAmtText.text = ammoAmts[weaponNumber].ToString();
    }

    [PunRPC]
    void GunMuzzleFlash()
    {
        muzzleFlash[weaponNumber].SetActive(true);
        StartCoroutine(MuzzleOff());
    }

    [PunRPC]
    public void Change()
    {
        weaponNumber++;
        if (weaponNumber > weapons.Length - 1)
        {
            weaponNumber = 0;
        }
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].SetActive(false);
        }
        weapons[weaponNumber].SetActive(true);
        leftHand.data.target = leftTargets[weaponNumber];
        rightHand.data.target = rightTargets[weaponNumber];
        rig.Build();
    }


    IEnumerator MuzzleOff()
    {
        yield return new WaitForSeconds(0.03f);
        this.GetComponent<PhotonView>().RPC("MuzzleFlashOff", RpcTarget.All);
    }

    [PunRPC]
    void MuzzleFlashOff()
    {
        muzzleFlash[weaponNumber].SetActive(false);
    }
}
