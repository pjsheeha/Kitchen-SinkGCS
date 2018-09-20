using UnityEngine;
using UnityEngine.Networking;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] int maxHealth = 3;

    [SyncVar(hook = "OnHealthChanged")] int health; //if we need the client to update their health they use this, 
    //whenever server updates this, do function

    Player player;


    void Awake()
    {
        player = GetComponent<Player>();
    }

    [ServerCallback]
    void OnEnable()
    {
        health = maxHealth;
    }

    [Server]
    public bool TakeDamage()
    {
        bool died = false;

        if (health <= 0)
            return died;

        health--;
        died = health <= 0;

        RpcTakeDamage(died);

        return died;
    }

    [ClientRpc]
    void RpcTakeDamage(bool died)
    {
        if (isLocalPlayer)
            PlayerCanvas.canvas.FlashDamageEffect(); // causes ui element to flash

        if (died)
            player.Die();
    }

    void OnHealthChanged(int value) // a callback - define rules for when server sends new value
    {
        health = value;
        if (isLocalPlayer)
            PlayerCanvas.canvas.SetHealth(value); // when health changes, call this method
    }
}