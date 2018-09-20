using UnityEngine;
using UnityEngine.Networking;

public class PlayerShooting : NetworkBehaviour
{

    //goal is to get five kills before player dies

    [SerializeField] float shotCooldown = .3f;
    [SerializeField] Transform firePosition;
    [SerializeField] ShotEffectsManager shotEffects;

    [SyncVar(hook = "OnScoreChanged")] int score; 

    float ellapsedTime;
    bool canShoot;

    void Start()
    {
        shotEffects.Initialize();

        if (isLocalPlayer)
            canShoot = true;
    }

    [ServerCallback]
    void OnEnable()
    {
        score = 0;
    }

    void Update()
    {
        if (!canShoot)
            return;

        ellapsedTime += Time.deltaTime;

        if (Input.GetButtonDown("Fire1") && ellapsedTime > shotCooldown)
        {
            ellapsedTime = 0f;
            CmdFireShot(firePosition.position, firePosition.forward);
        }
    }

    [Command]
    void CmdFireShot(Vector3 origin, Vector3 direction)
    {
        RaycastHit hit;

        Ray ray = new Ray(origin, direction);
        Debug.DrawRay(ray.origin, ray.direction * 3f, Color.red, 1f);

        bool result = Physics.Raycast(ray, out hit, 50f);

        if (result)
        {
            PlayerHealth enemy = hit.transform.GetComponent<PlayerHealth>();

            if (enemy != null)
            {
                bool wasKillShot = enemy.TakeDamage(); //was the enemey killed?

                if (wasKillShot)
                    score++; // add score
            }
        }

        RpcProcessShotEffects(result, hit.point);
    }

    [ClientRpc]
    void RpcProcessShotEffects(bool playImpact, Vector3 point)
    {
        shotEffects.PlayShotEffects();

        if (playImpact)
            shotEffects.PlayImpactEffect(point);
    }

    void OnScoreChanged(int value) //updates score for canvas
    {
        score = value;
        if (isLocalPlayer)
            PlayerCanvas.canvas.SetKills(value);
    }
}