using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    const int WEAPON_PUNCH = 0, WEAPON_PISTOL = 1, WEAPON_RIFLE = 2, WEAPON_ROCKETLAUNCHER = 3, WEAPON_GRENADE = 4, WEAPON_BASEBALLBAT = 5;
    const int LAYER_PUNCH = 1, LAYER_AIMPISTOL = 2, LAYER_FIREPISTOL = 3, LAYER_AIMRIFLE = 4, LAYER_FIRERIFLE = 5, LAYER_THROW = 6, LAYER_STRIKE = 7;

    [SerializeField] private GameObject aimPoint;
    [SerializeField] ParticleSystem particleSystemBlood;
    [SerializeField] private LayerMask aimColliderLayerMask;
    [SerializeField] private Transform vfxFireGun;
    [SerializeField] private Transform vfxRocketSmoke;
    [SerializeField] private GameObject rightHandPosition;
    [SerializeField] private CinemachineCamera vcamAim;
    [SerializeField] private CinemachineCamera vcamPlayerFollow;
    [SerializeField] private CinemachineCamera vcamRagdoll;
    [SerializeField] private GameObject bloodSpawnPosition;
    [SerializeField] private GameObject pfBullet;
    [SerializeField] private GameObject gunFirePistol;
    [SerializeField] private GameObject gunFireRifle;
    [SerializeField] private Image aimCursor;
    [SerializeField] private GameObject imageIconWeapon;
    [SerializeField] private GameObject pistol;
    [SerializeField] private GameObject rifle;
    [SerializeField] private GameObject grenade;
    [SerializeField] private GameObject rocketLauncher;
    [SerializeField] private GameObject baseballBat;
    [SerializeField] private GameObject pfGrenade;
    [SerializeField] private GameObject playerSpawnPoint;
    [SerializeField] private GameObject meshRoot;
    [SerializeField] private GameObject pfRocket;
    [SerializeField] private Transform rocketSpawnPosition;
    [SerializeField] private Transform pfShell;
    [SerializeField] private new Rigidbody rigidbody;
    [SerializeField] private Transform hips;
    [SerializeField] Slider sliderHealthBar;
    /*
    [SerializeField] private GameObject hitIndicator;
    [SerializeField] private GameObject hitIndicator2;
    [SerializeField] private GameObject hitIndicator3;
    [SerializeField] private GameObject hitIndicator4;
    [SerializeField] private GameObject hitIndicator5;
    [SerializeField] private GameObject hitIndicator6;
    [SerializeField] private Material matIndicator1;
    [SerializeField] private Material matIndicator2;
    */

    private Driveable driveable;
    private GameObject objectInHand;
    private UI UIScript;
    private AudioSource soundPistolShot;
    private AudioSource soundRifleShot;
    private AudioSource soundWoosh;
    private AudioSource soundPunch;
    private AudioSource soundThrow;
    private AudioSource soundSwitchWeapon;
    private AudioSource soundRocketLauncher;
    private bool smokeCreated;
    private bool isAiming;
    private Animator animator;
    private float timeLeftShooting;
    private float timeLeftPunching;
    private float timeLeftBlood;
    private float timeLeftExploded;
    private float timeLastWeaponThrow;
    private float timeLeftDying;
    private float timeLeftAimToShoot;
    private float timeLeftAutoFire;
    private bool throwingWeapon = false;
    private bool thrownWeapon = false;
    private bool isAutoFiring = false;
    private Vector3 hitPosition;
    private RigBuilder rigBuilder;
    private Transform hitTransForm;
    private Vector3 aimDirection;
    private CharacterController characterController;
    private NPC hitNPC;
    private bool hitEnemy;
    private int activeWeapon;
    private int health = 100;
    private int timesStuck;
    private LayerMask layerMaskVehicle;
    private List<AudioClip> clipsScreamMale = new List<AudioClip>();

    [Header("Character Input Values")]
    public Vector2 move;
    public Vector2 look;
    public bool jump;
    public bool sprint;

    [Header("Movement Settings")]
    public bool analogMovement;

    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;

    public float TimeLeftPunching { get => timeLeftPunching; set => timeLeftPunching = value; }
    public GameObject ObjectInHand { get => objectInHand; set => objectInHand = value; }

    private void Awake()
    {
        rigBuilder = gameObject.GetComponent<RigBuilder>();
        layerMaskVehicle = LayerMask.GetMask("Vehicle");
    }

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<CarController>().enabled = false;
        GetComponent<MotorbikeController>().enabled = false;
        vcamRagdoll.enabled = false;
        vcamAim.enabled = false;
        animator = GetComponent<Animator>();
        gunFirePistol.SetActive(false);
        gunFireRifle.SetActive(false);
        aimCursor.enabled = false;
        soundPistolShot = GameObject.Find("/Sound/Gunshot").GetComponent<AudioSource>();
        soundRifleShot = GameObject.Find("/Sound/Gunshot2").GetComponent<AudioSource>();
        soundWoosh = GameObject.Find("/Sound/Woosh").GetComponent<AudioSource>();
        soundPunch = GameObject.Find("/Sound/Punch").GetComponent<AudioSource>();
        soundThrow = GameObject.Find("/Sound/Throw").GetComponent<AudioSource>();
        soundSwitchWeapon = GameObject.Find("/Sound/SwitchWeapon").GetComponent<AudioSource>();
        soundRocketLauncher = GameObject.Find("/Sound/RocketLauncher").GetComponent<AudioSource>();
        Transform soundsRoot = GameObject.Find("/Sound/MaleScreams").transform;
        foreach (Transform item in soundsRoot)
        {
            AudioClip clip = item.gameObject.GetComponent<AudioSource>().clip;
            clipsScreamMale.Add(clip);
        }
        characterController = gameObject.GetComponent<CharacterController>();
        baseballBat.SetActive(false);
        rigBuilder.layers[0].active = false;
        rigBuilder.layers[1].active = false;
        rigBuilder.layers[2].active = false;
        particleSystemBlood.Stop();
        activeWeapon = WEAPON_PUNCH;
        imageIconWeapon.GetComponent<Image>().sprite = Resources.Load<Sprite>("icon_fist");
        UIScript = GameObject.Find("/Scripts/UI").GetComponent<UI>();
        sliderHealthBar.value = 1 - (health / 100f);
    }

    public void Hit()
    {
        if (timeLeftDying > 0)
        {
            return;
        }
        particleSystemBlood.transform.position = bloodSpawnPosition.transform.position;
        particleSystemBlood.Play();
        timeLeftBlood = 0.5f;
        health -= 5;
        sliderHealthBar.value = 1 - (health / 100f);
        Scream();
        if (health <= 0)
        {
            Explode(0);
            timeLeftDying = 4;
        }
    }
    private void Scream()
    {
        AudioSource.PlayClipAtPoint(clipsScreamMale[Random.Range(0, clipsScreamMale.Count)], transform.position);
    }

    public void PutObjectInHand(GameObject objectInHand)
    {
        this.objectInHand = objectInHand;
        Game.Instance.ShowMessage("Press [R] to release item");
        if (objectInHand.name == "safe" && Game.Instance.Missions.Find(o => o.Name == "bank").IsActive)
        {
            Game.Instance.Missions.Find(o => o.Name == "bank").NextDestination();
            Game.Instance.DisplayMission("Good! Now bring the safe to the Saloon on the hill above St Dutch.");
        }
    }

    private void HandlePistolFiring()
    {
        if (timeLeftShooting < 0.6 && !smokeCreated)
        {
            smokeCreated = true;
            Transform newEffect = Instantiate(vfxFireGun, rightHandPosition.transform.position, Quaternion.identity);
            newEffect.parent = Game.Instance.EffectsParent.transform;
            gunFirePistol.SetActive(true);
        }
        if (timeLeftShooting < 0.55)
        {
            gunFirePistol.SetActive(false);
        }
    }

    private void HandleRifleFiring()
    {
        if (timeLeftShooting < 0)
        {
            animator.SetLayerWeight(LAYER_FIRERIFLE, 0);
        }
        else
        {
            animator.SetLayerWeight(LAYER_FIRERIFLE, 1);
        }
        if (timeLeftShooting < 0.6 && !smokeCreated)
        {
            smokeCreated = true;
            Transform newEffect = Instantiate(vfxFireGun, rightHandPosition.transform.position, Quaternion.identity);
            newEffect.parent = Game.Instance.EffectsParent.transform;
            Instantiate(pfShell, rifle.transform.position, Quaternion.LookRotation(transform.forward, Vector3.up));
            gunFireRifle.SetActive(true);
        }
        if (timeLeftShooting < 0.55)
        {
            gunFireRifle.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (timeLeftDying > 0)
        {
            timeLeftDying -= Time.deltaTime;
            if (timeLeftDying <= 0)
            {
                SceneManager.UnloadSceneAsync("MainScene");
                SceneManager.LoadSceneAsync("MenuScene");
                Cursor.visible = true;
            }
        }
        if (timeLeftExploded > 0)
        {
            timeLeftExploded -= Time.deltaTime;
            if (timeLeftExploded < 0)
            {
                float y = Game.Instance.MainTerrain.SampleHeight(hips.position);
                if (timesStuck < 10 && hips.position.y - y > 1)
                {
                    // not on the ground yet
                    timeLeftExploded = 2f;
                    timesStuck++;     // prevent getting stuck when there is something wrong with sampling the terrain height
                }
                else
                {
                    timesStuck = 0;
                    transform.position = new Vector3(hips.position.x, y + 1, hips.position.z);
                    characterController.enabled = true;
                    animator.enabled = true;
                    vcamRagdoll.enabled = false;
                }
            }
        }
        if (timeLeftBlood > 0)
        {
            timeLeftBlood -= Time.deltaTime;
            if (timeLeftBlood < 0)
            {
                particleSystemBlood.Stop();
            }
        }
        if (timeLeftPunching > 0)
        {
            timeLeftPunching -= Time.deltaTime;
            if (timeLeftPunching < 0)
            {
                animator.SetLayerWeight(LAYER_PUNCH, 0);
                animator.SetLayerWeight(LAYER_STRIKE, 0);
            }
            if (hitEnemy==false && timeLeftPunching < 0.8f)
            {
                hitEnemy = true;
                float hitDistance = (transform.position - hitPosition).sqrMagnitude;
                if (hitDistance < 16f && hitTransForm!=null)
                {
                    soundPunch.Play();
                    CheckHit();
                }
            }
        }
        if (timeLeftShooting > 0)
        {
            animator.SetLayerWeight(LAYER_FIREPISTOL, Mathf.Lerp(animator.GetLayerWeight(LAYER_FIREPISTOL), 1f, Time.deltaTime * 20f));
            timeLeftShooting -= Time.deltaTime;

            if (activeWeapon == WEAPON_PISTOL)
            {
                HandlePistolFiring();
            }
            if (activeWeapon == WEAPON_RIFLE)
            {
                HandleRifleFiring();
            }
        }
        else
        {
            animator.SetLayerWeight(LAYER_FIREPISTOL, Mathf.Lerp(animator.GetLayerWeight(LAYER_FIREPISTOL), 0f, Time.deltaTime * 5f));
        }
        if (throwingWeapon)
        {
            if (thrownWeapon == false && Time.time >= timeLastWeaponThrow + 0.7f)
            {
                thrownWeapon = true;
                if (activeWeapon == WEAPON_GRENADE)
                {
                    soundThrow.Play();
                    Instantiate(pfGrenade, rightHandPosition.transform.position, Quaternion.LookRotation(transform.forward, Vector3.up));
                }
            }
            if (Time.time >= timeLastWeaponThrow + 0.7f)
            {
                animator.SetLayerWeight(LAYER_THROW, Mathf.Lerp(animator.GetLayerWeight(LAYER_THROW), 0f, Time.deltaTime * 10f));
            }
            if (Time.time >= timeLastWeaponThrow + 1.2f)
            {
                FinishThrowing();
            }
        }
        HandleAiming();
        SetRigLayers();
    }

    public void FinishThrowing()
    {
        if (activeWeapon == WEAPON_BASEBALLBAT)
        {
            baseballBat.SetActive(true);
        }
        throwingWeapon = false;
        thrownWeapon = false;
    }

    public void FixedUpdate()
    {
        if (transform.position.y < -50)
        {
            transform.position = playerSpawnPoint.transform.position;
        }
        driveable = null;
        Collider[] colliders = Physics.OverlapSphere(transform.position, 2, layerMaskVehicle);
        foreach (var collider in colliders)
        {
            if (collider.transform.GetComponent<Driveable>() != null && 
                GetComponent<CarController>().Car == null && GetComponent<MotorbikeController>().Motorbike == null && 
                GetComponent<AirplaneController>().Airplane == null)
            {
                driveable = collider.transform.GetComponent<Driveable>();
                Game.Instance.ShowMessage("Press [F] to enter");
            }
        }
        if (isAutoFiring)
        {
            timeLeftAutoFire -= Time.deltaTime;
            if (timeLeftAutoFire < 0)
            {
                timeLeftAutoFire = 0.15f;
                FireRifle();
            }
        }
    }

    public void IncreaseHealth(int amount)
    {
        health += amount;
        if (health > 100)
        {
            health = 100;
        }
        sliderHealthBar.value = 1 - (health / 100f);
    }

    public void HandleNPCHit()
    {
        if (hitNPC != null)
        {
            hitNPC.Hit(transform.position);
        }
        particleSystemBlood.transform.position = hitPosition;
        particleSystemBlood.Play();
        timeLeftBlood = 0.5f;
    }

    private void CalculateAimPoint()
    {
        Ray ray;
        if (isAiming)
        {
            Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        }
        else
        {
            // if not aiming, the player fires in the direction he is currently facing
            Vector3 raycastDir = transform.forward;
            ray = new Ray(rightHandPosition.transform.position, raycastDir);
        }

        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
        {
            /*
            hitIndicator.GetComponent<Renderer>().material = matIndicator1;
            hitIndicator2.GetComponent<Renderer>().material = matIndicator1;
            hitIndicator3.GetComponent<Renderer>().material = matIndicator1;
            hitIndicator4.GetComponent<Renderer>().material = matIndicator1;
            hitIndicator5.GetComponent<Renderer>().material = matIndicator1;
            hitIndicator6.GetComponent<Renderer>().material = matIndicator1;
            */
            hitTransForm = raycastHit.transform;
            hitPosition = raycastHit.point;
        }
        else
        {
            /*
            hitIndicator.GetComponent<Renderer>().material = matIndicator2;
            hitIndicator2.GetComponent<Renderer>().material = matIndicator2;
            hitIndicator3.GetComponent<Renderer>().material = matIndicator2;
            hitIndicator4.GetComponent<Renderer>().material = matIndicator2;
            hitIndicator5.GetComponent<Renderer>().material = matIndicator2;
            hitIndicator6.GetComponent<Renderer>().material = matIndicator2;
            */

            // we didn't hit anything, so take a point in the direction of the ray
            hitTransForm = null;
            hitPosition = ray.GetPoint(100);
        }

        /*
        hitIndicator.transform.position = Vector3.Lerp(hitPosition, transform.position, 0);
        hitIndicator2.transform.position = Vector3.Lerp(hitPosition, transform.position, 0.2f);
        hitIndicator3.transform.position = Vector3.Lerp(hitPosition, transform.position, 0.4f);
        hitIndicator4.transform.position = Vector3.Lerp(hitPosition, transform.position, 0.6f);
        hitIndicator5.transform.position = Vector3.Lerp(hitPosition, transform.position, 0.8f);
        hitIndicator6.transform.position = Vector3.Lerp(hitPosition, transform.position, 1);
        */

        // Move the aim target
        aimPoint.transform.position = hitPosition;
        aimDirection = (hitPosition - rightHandPosition.transform.position).normalized;
    }

    private void CheckHit(GameObject gunFire = null)
    {
        if (hitTransForm != null)
        {
            hitNPC = hitTransForm.GetComponent<NPC>();
            if (hitNPC != null)
            {
                HandleNPCHit();
                return;
            }
            Target target = hitTransForm.GetComponent<Target>();
            if (target != null)
            {
                target.Hit(hitPosition);
                return;
            }
        }
        else
        {
            hitNPC = null;
        }
    }

    private void SetRigLayers()
    {
        if (isAiming || timeLeftShooting > 0)
        {
            if (activeWeapon == WEAPON_PISTOL)
            {
                rigBuilder.layers[0].active = true;
            }
            if (activeWeapon == WEAPON_RIFLE)
            {
                rigBuilder.layers[1].active = true;
            }
            if (activeWeapon == WEAPON_ROCKETLAUNCHER)
            {
                rigBuilder.layers[2].active = true;
            }
        }
        else
        {
            rigBuilder.layers[0].active = false;
            rigBuilder.layers[1].active = false;
            rigBuilder.layers[2].active = false;
        }
    }

    private void HandleAiming()
    {
        //isAiming = true;
        CalculateAimPoint();

        if (isAiming || timeLeftAimToShoot > 0)
        {
            if (timeLeftAimToShoot > 0)
            {
                timeLeftAimToShoot -= Time.deltaTime;
                if (timeLeftAimToShoot < 0)
                {
                    FireRocket();
                }
            }

            Vector3 aimLocationXZ = new(hitPosition.x, hitPosition.y, hitPosition.z);
            aimLocationXZ.y = transform.position.y;
            // Turn player towards aim point (only x and z axis)
            transform.forward = Vector3.Lerp(transform.forward, (aimLocationXZ - transform.position).normalized, Time.deltaTime * 10f);
            if (activeWeapon == WEAPON_RIFLE || activeWeapon == WEAPON_ROCKETLAUNCHER)
            {
                animator.SetLayerWeight(LAYER_AIMRIFLE, Mathf.Lerp(animator.GetLayerWeight(LAYER_AIMRIFLE), 1f, Time.deltaTime * 10f));
            }
            if (activeWeapon == WEAPON_PISTOL)
            {
                animator.SetLayerWeight(LAYER_AIMPISTOL, Mathf.Lerp(animator.GetLayerWeight(LAYER_AIMPISTOL), 1f, Time.deltaTime * 10f));
            }
        }
        else
        {
            animator.SetLayerWeight(LAYER_AIMPISTOL, Mathf.Lerp(animator.GetLayerWeight(LAYER_AIMPISTOL), 0f, Time.deltaTime * 10f));
            animator.SetLayerWeight(LAYER_AIMRIFLE, Mathf.Lerp(animator.GetLayerWeight(LAYER_AIMRIFLE), 0f, Time.deltaTime * 10f));
        }

        if (isAiming && vcamRagdoll.enabled == false)
        {
            vcamAim.enabled = true;
            aimCursor.enabled = true;

            Vector3 aimLocationXZ = new(hitPosition.x, hitPosition.y, hitPosition.z);
            aimLocationXZ .y = transform.position.y;

            // Turn player towards aim point (only x and z axis)
            transform.forward = Vector3.Lerp(transform.forward, (aimLocationXZ - transform.position).normalized, Time.deltaTime * 10f);
        }
        else
        {
            vcamAim.enabled = false;
            aimCursor.enabled = false;
        }
    }

    private void OnRelease()
    {
        if (objectInHand != null)
        {
            objectInHand.GetComponent<MoveableItem>().TimeLeftThrowing = 2;
            objectInHand.transform.parent = null;
            objectInHand.GetComponent<Rigidbody>().isKinematic = false;
            objectInHand.GetComponent<Rigidbody>().AddForce(new Vector3(Random.value * 8, 10, Random.value * 8), ForceMode.VelocityChange);
            objectInHand.GetComponent<Rigidbody>().AddTorque(Random.insideUnitSphere * 5, ForceMode.VelocityChange);
            objectInHand = null;
        }
    }

    private void OnShoot(InputValue value)
    {
        if (value.isPressed)
        {
            if (activeWeapon == WEAPON_PUNCH && timeLeftPunching <= 0)
            {
                timeLeftPunching = 1.2f;
                animator.Play("Punch", LAYER_PUNCH, 0f);
                animator.SetLayerWeight(LAYER_PUNCH, 1f);
                soundWoosh.Play();
                hitEnemy = false;
            }
            if (activeWeapon == WEAPON_PISTOL)
            {
                animator.Play("FirePistol", LAYER_FIREPISTOL, 0);
                soundPistolShot.Play();
                ShootGun(gunFirePistol);
            }
            if (activeWeapon == WEAPON_RIFLE)
            {
                isAutoFiring = true;
                timeLeftAutoFire = 0;
            }
            if (activeWeapon == WEAPON_ROCKETLAUNCHER)
            {
                timeLeftShooting = 0.7f;
                if (!isAiming)
                {
                    // If player is not aiming, this is done here to have a firing animation
                    timeLeftAimToShoot = 0.5f;
                }
                else
                {
                    FireRocket();
                }
            }
            if (activeWeapon == WEAPON_GRENADE)
            {
                timeLastWeaponThrow = Time.time;
                throwingWeapon = true;
                animator.Play("Throw", LAYER_THROW, 0f);
                animator.SetLayerWeight(LAYER_THROW, 1f);
            }
            if (activeWeapon == WEAPON_BASEBALLBAT)
            {
                timeLeftPunching = 1.2f;
                soundWoosh.Play();
                animator.Play("Strike", LAYER_STRIKE, 0.05f);
                animator.SetLayerWeight(LAYER_STRIKE, 1f);
                hitEnemy = false;
            }
        }
        else
        {
            isAutoFiring = false;
        }
    }

    private void FireRifle()
    {
        animator.Play("FireRifle", LAYER_FIRERIFLE, 0);
        animator.SetLayerWeight(LAYER_FIRERIFLE, 1);
        soundRifleShot.Play();
        ShootGun(gunFireRifle);
    }

    private void FireRocket()
    {
        animator.Play("FireRifle", LAYER_FIRERIFLE, 0);
        animator.SetLayerWeight(LAYER_FIRERIFLE, 1);
        soundRocketLauncher.Play();
        Instantiate(pfRocket, rocketSpawnPosition.position, Quaternion.LookRotation(aimDirection, Vector3.up));
        Instantiate(vfxRocketSmoke, transform.position, Quaternion.identity);
    }

    private void ShootGun(GameObject gunFire)
    {
        timeLeftShooting = 0.7f;
        smokeCreated = false;
        CheckHit(gunFire);
    }

    public void Explode(int strength = 80)
    {
        vcamRagdoll.enabled = true;
        characterController.enabled = false;
        animator.enabled = false;
        rigidbody.AddForce(new Vector3(0, strength, 0), ForceMode.VelocityChange);
        rigidbody.AddTorque(Random.insideUnitSphere, ForceMode.VelocityChange);
        timeLeftExploded = 4;
    }

    private void OnWeaponSelect()
    {
        soundSwitchWeapon.Play();
        activeWeapon++;
        animator.SetLayerWeight(LAYER_PUNCH, 0f);
        animator.SetLayerWeight(LAYER_AIMPISTOL, 0f);
        animator.SetLayerWeight(LAYER_AIMRIFLE, 0f);
        animator.SetLayerWeight(LAYER_FIREPISTOL, 0f);
        animator.SetLayerWeight(LAYER_FIRERIFLE, 0f);
        if (activeWeapon > 5)
        {
            activeWeapon = 0;
        }
        if (activeWeapon == WEAPON_PUNCH)
        {
            baseballBat.SetActive(false);
            imageIconWeapon.GetComponent<Image>().sprite = Resources.Load<Sprite>("icon_fist");
        }
        if (activeWeapon == WEAPON_PISTOL)
        {
            pistol.SetActive(true);
            imageIconWeapon.GetComponent<Image>().sprite = Resources.Load<Sprite>("icon_pistol");
        }
        if (activeWeapon == WEAPON_RIFLE)
        {
            rigBuilder.layers[0].active = false;
            pistol.SetActive(false);
            rifle.SetActive(true);
            imageIconWeapon.GetComponent<Image>().sprite = Resources.Load<Sprite>("icon_akm");
        }
        if (activeWeapon == WEAPON_ROCKETLAUNCHER)
        {
            rigBuilder.layers[1].active = false;
            rifle.SetActive(false);
            rocketLauncher.SetActive(true);
            imageIconWeapon.GetComponent<Image>().sprite = Resources.Load<Sprite>("icon_rocketlauncher");
        }
        if (activeWeapon == WEAPON_GRENADE)
        {
            rigBuilder.layers[2].active = false;
            rocketLauncher.SetActive(false);
            grenade.SetActive(true);
            imageIconWeapon.GetComponent<Image>().sprite = Resources.Load<Sprite>("icon_grenade");
        }
        if (activeWeapon == WEAPON_BASEBALLBAT)
        {
            grenade.SetActive(false);
            baseballBat.SetActive(true);
            imageIconWeapon.GetComponent<Image>().sprite = Resources.Load<Sprite>("icon_baseballbat");
        }
    }

    private void OnAim(InputValue value)
    {
        isAiming = value.isPressed;
        if (isAiming)
        {
            if (activeWeapon == WEAPON_PISTOL)
            {
                animator.Play("AimPistol", LAYER_AIMPISTOL, 0);
            }
            if (activeWeapon == WEAPON_RIFLE || activeWeapon == WEAPON_ROCKETLAUNCHER)
            {
                animator.Play("AimRifle", LAYER_AIMRIFLE, 0);
            }
        }
    }

    public void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        look = value.Get<Vector2>();
    }

    public void OnEnterVehicle()
    {
        if ( GetComponent<CarController>().Car == null && 
            GetComponent<AirplaneController>().Airplane == null &&
             GetComponent<MotorbikeController>().Motorbike == null)
        {
            if (driveable != null)
            {
                if (driveable.gameObject.GetComponent<Car>() != null)
                {
                    GetComponent<CarController>().SetCar(driveable.gameObject.GetComponent<Car>());
                    GetComponent<CarController>().enabled = true;
                    GetComponent<AirplaneController>().enabled = false;
                    GetComponent<MotorbikeController>().enabled = false;
                    Game.Instance.ShowMessage("");
                }
                else if (driveable.gameObject.GetComponent<Motorbike>() != null)
                {
                    GetComponent<MotorbikeController>().SetMotorbike(driveable.gameObject.GetComponent<Motorbike>());
                    GetComponent<MotorbikeController>().enabled = true;
                    GetComponent<AirplaneController>().enabled = false;
                    GetComponent<CarController>().enabled = false;
                    Game.Instance.ShowMessage("");
                }
                else if (driveable.gameObject.GetComponent<Airplane>() != null)
                {
                    GetComponent<AirplaneController>().SetAirplane(driveable.gameObject.GetComponent<Airplane>());
                    GetComponent<AirplaneController>().enabled = true;
                    GetComponent<MotorbikeController>().enabled = false;
                    GetComponent<CarController>().enabled = false;
                    Game.Instance.ShowMessage("Accelerate: +   Decelerate: -   Yaw Left: <   Yaw Right: >", 20);
                }
                animator.enabled = false;
                GetComponent<ThirdPersonController>().enabled = false;
                GetComponent<CapsuleCollider>().enabled = false;
                GetComponent<CharacterController>().enabled = false;
                meshRoot.SetActive(false);
                UpdateView();
            }
        }
        else
        {
            ExitVehicle();
        }
    }

    public void ExitVehicle()
    {
        if (GetComponent<CarController>().Car != null)
        {
            transform.position = GetComponent<CarController>().Car.transform.position + new Vector3(2, 0, 0);
            GetComponent<CarController>().ExitCar();
            GetComponent<CarController>().enabled = false;
        }
        else if (GetComponent<MotorbikeController>().Motorbike != null)
        {
            transform.position = GetComponent<MotorbikeController>().Motorbike.transform.position + new Vector3(2, 0, 0);
            GetComponent<MotorbikeController>().enabled = false;
        }
        else if (GetComponent<AirplaneController>().Airplane != null)
        {
            transform.position = GetComponent<AirplaneController>().Airplane.transform.position + new Vector3(2, 0, 0);
            GetComponent<AirplaneController>().ExitAirplane();
            Game.Instance.SetFollowCameraToPlayer();
        }
        animator.enabled = true;
        GetComponent<ThirdPersonController>().enabled = true;
        GetComponent<CapsuleCollider>().enabled = true;
        GetComponent<CharacterController>().enabled = true;
        meshRoot.SetActive(true);
        UpdateView();
    }

    public void OnToggleMiniMap()
    {
        Game.Instance.ShowMiniMap = !Game.Instance.ShowMiniMap;
        Game.Instance.ShowMessage("Minimap: " + (Game.Instance.ShowMiniMap ? "On" : "Off"));
    }

    public void OnChangeView()
    {
        if (Game.Instance.ViewDistance == 1)
        {
            Game.Instance.ViewDistance = 2;
        }
        else if (Game.Instance.ViewDistance == 2)
        {
            Game.Instance.ViewDistance = 5;
        }
        else if(Game.Instance.ViewDistance == 5)
        {
            Game.Instance.ViewDistance = 10;
        }
        else
        {
            Game.Instance.ViewDistance = 1;
        }
        Game.Instance.ShowMessage("View Distance: " + Game.Instance.ViewDistance);
        UpdateView();
    }

    public void UpdateView()
    {
        float distanceFactor = 1;
        if (GetComponent<CarController>().Car != null)
        {
            distanceFactor = 2f;
        }
        if (GetComponent<AirplaneController>().Airplane != null)
        {
            distanceFactor = 2f;
        }
        vcamPlayerFollow.GetComponent<CinemachineThirdPersonFollow>().CameraDistance = Game.Instance.ViewDistance * distanceFactor;
    }

    public void OnToggleSuperSpeed()
    {
        if (gameObject.GetComponent<ThirdPersonController>().SprintSpeed < 300)
        {
            gameObject.GetComponent<ThirdPersonController>().SprintSpeed = 300;
        }
        else
        {
            gameObject.GetComponent<ThirdPersonController>().SprintSpeed = 16;
        }
        Game.Instance.ShowMessage("Sprint Speed: " + gameObject.GetComponent<ThirdPersonController>().SprintSpeed);
    }

    public void OnJump(InputValue value)
    {
        jump = value.isPressed;
    }

    public void OnSprint(InputValue value)
    {
        sprint = value.isPressed;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        SetCursorState(cursorLocked);
    }

    private void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }

    private void OnMap(InputValue value)
    {
        UIScript.ShowMap = value.isPressed;
    }

    private void OnProgressScreen(InputValue value)
    {
        UIScript.ToggleAchievementsScreen();
    }

    private void OnHelp()
    {
        UIScript.ToggleHelpScreen();
    }

    private void OnToggleFreeCamera()
    {
        Game.Instance.ToggleFreeCamera();
    }
}
