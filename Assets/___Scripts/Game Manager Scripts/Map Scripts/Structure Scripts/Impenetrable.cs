using CITC.GameManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CITC.GameManager.SingleAnimationClipManager;

public class Impenetrable : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Collisions
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Collided With Bullet
        Bullet bullet = collision.gameObject.GetComponent<Bullet>();
        if (bullet != null && !bullet.Finished)
        {
            LocalSpawnerManager.CreateVisualEffectInstance(SingleAnimationType.GetPunchedVFX, bullet.transform.position, parent: this.transform);
            bullet.HitSomethingImpenetrable(); //Hitting Building
        }
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
}
