using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Survivor : Character
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Pick Up Items
    protected bool PickUpGun(GameObject currentGameObject)
    {
        Gun gunToPickUp = currentGameObject.GetComponent<Gun>();

        if (gunToPickUp == null) return false; //Not A Gun
        if (gunToPickUp.Owner != null) return false; //Already Has A Owner

        bool pickedUpItem = false;

        //It is a Primary Weapon
        if (gunToPickUp.GetWeaponType == Weapon.WeaponType.Primary)
        {
            gunToPickUp.WeaponPickedUp(this);
            pickedUpItem = true;

            if (primaryGun1 == null) primaryGun1 = gunToPickUp;
            else if (primaryGun2 == null) primaryGun2 = gunToPickUp;
            else if (chosenWeaponSlotNetworked == ChosenWeaponSlot.PrimaryGun1)
            {
                ClearWeaponSlot(ChosenWeaponSlot.PrimaryGun1);
                primaryGun1 = gunToPickUp;
            }
            else if (chosenWeaponSlotNetworked == ChosenWeaponSlot.PrimaryGun2)
            {
                ClearWeaponSlot(ChosenWeaponSlot.PrimaryGun2);
                primaryGun2 = gunToPickUp;
            }
            else
            {
                gunToPickUp.WeaponDropped();
                pickedUpItem = false;
            }
        }

        //It is a SideArm Weapon
        if (gunToPickUp.GetWeaponType == Weapon.WeaponType.SideArm)
        {
            gunToPickUp.WeaponPickedUp(this);
            pickedUpItem = true;

            if (sideArm1 == null) sideArm1 = gunToPickUp;
            else if (sideArm2 == null) sideArm2 = gunToPickUp;
            else if (chosenWeaponSlotNetworked == ChosenWeaponSlot.SideArm1)
            {
                ClearWeaponSlot(ChosenWeaponSlot.SideArm1);
                sideArm1 = gunToPickUp;
            }
            else if (chosenWeaponSlotNetworked == ChosenWeaponSlot.SideArm2)
            {
                ClearWeaponSlot(ChosenWeaponSlot.SideArm2);
                sideArm2 = gunToPickUp;
            }
            else
            {
                gunToPickUp.WeaponDropped();
                pickedUpItem = false;
            }
        }

        return pickedUpItem;
    }

    protected bool PickUpMelee(GameObject currentGameObject)
    {
        Melee meleeToPickUp = currentGameObject.GetComponent<Melee>();

        if (meleeToPickUp == null) return false; //Not A Melee
        if (meleeToPickUp.Owner != null) return false; //Already Has A Owner

        bool pickedUpItem = true;
        meleeToPickUp.WeaponPickedUp(this);

        if (melee == null) melee = meleeToPickUp;
        else if (chosenWeaponSlotNetworked == ChosenWeaponSlot.Melee)
        {
            ClearWeaponSlot(ChosenWeaponSlot.Melee);
            melee = meleeToPickUp;
        }
        else
        {
            meleeToPickUp.WeaponDropped();
            pickedUpItem = false;
        }

        return pickedUpItem;
    }

    protected bool PickUpThrowable(GameObject currentGameObject)
    {
        Throwable throwableToPickUp = currentGameObject.GetComponent<Throwable>();

        if (throwableToPickUp == null) return false; //Not A Thriwable
        if (throwableToPickUp.Owner != null) return false; //Already Has A Owner

        bool pickedUpWeapon = true;
        throwableToPickUp.WeaponPickedUp(this);

        if (throwable == null) throwable = throwableToPickUp;
        else if (throwable.gameObject.name == throwableToPickUp.gameObject.name)
        {
            Throwable currentThrowable = throwable.gameObject.GetComponent<Throwable>();
            currentThrowable.CurrentAmount += throwableToPickUp.CurrentAmount;

            throwableToPickUp.WeaponDropped();
            throwableToPickUp.HandleDestruction();
        }
        else if (chosenWeaponSlotNetworked == ChosenWeaponSlot.Throwable)
        {
            ClearWeaponSlot(ChosenWeaponSlot.Throwable);
            throwable = throwableToPickUp;
        }
        else
        {
            throwableToPickUp.WeaponDropped();
            pickedUpWeapon = false;
        }

        return pickedUpWeapon;
    }

    protected override void PickUpItem()
    {
        if (!Runner.IsServer) return;
        if (!IsPickingUp) return;
        if (Vehicle.CanUse(currentVehicle)) return; //Can't pick up while in a vehicle

        Collider2D[] itemColliders = Physics2D.OverlapCircleAll(PickUpPosition, PickUpRadius, LayerMask.GetMask("Item"));
        if (itemColliders.Length == 0) return;

        // Sort itemColliders by distance to PickUpPosition
        Array.Sort(itemColliders, (a, b) =>
        {
            float distanceA = Vector2.Distance(PickUpPosition, a.transform.position);
            float distanceB = Vector2.Distance(PickUpPosition, b.transform.position);
            return distanceA.CompareTo(distanceB);
        });

        for (int i = 0; i < itemColliders.Length; i++)
        {
            GameObject currentGameObject = itemColliders[i].gameObject;
            if (PickUpGun(currentGameObject) ||
                PickUpMelee(currentGameObject) ||
                PickUpThrowable(currentGameObject))
            {
                alreadyPickedUp = true;
                return;
            }
        }
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    protected override void DropChosenWeapon() 
    {
        if (!Runner.IsServer) return;
        if (!IsDroppingChosenWeapon) return;
        if (Vehicle.CanUse(currentVehicle)) return; //Can't drop while in a vehicle

        ClearWeaponSlot(chosenWeaponSlotNetworked);
        alreadyDroppedChosenWeapon = true;
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    protected override void EnterVehicle() 
    {
        if (!Runner.IsServer) return;
        if (!IsPickingUp) return;
        if (Vehicle.CanUse(currentVehicle)) return; //Already has a vehicle

        Collider2D[] vehicleCollider = Physics2D.OverlapCircleAll(PickUpPosition, PickUpRadius, LayerMask.GetMask("Vehicle"));
        if (vehicleCollider.Length == 0) return;

        // Sort itemColliders by distance to PickUpPosition
        Array.Sort(vehicleCollider, (a, b) =>
        {
            float distanceA = Vector2.Distance(PickUpPosition, a.transform.position);
            float distanceB = Vector2.Distance(PickUpPosition, b.transform.position);
            return distanceA.CompareTo(distanceB);
        });

        for (int i = 0; i < vehicleCollider.Length; i++)
        {
            Vehicle vehicleToEnter = vehicleCollider[i].GetComponent<Vehicle>();
            if (!Vehicle.CanUse(vehicleToEnter)) continue; //Not a Vehicle
            if (vehicleToEnter.EnterCar(this)) //Successfully Entered
            {
                currentVehicle = vehicleToEnter;
                ColliderIsTrigger = true;

                alreadyPickedUp = true;
                return;
            }
        }
    }
    protected override void ExitVehicle() 
    {
        if (!Runner.IsServer) return;
        if (!IsDroppingChosenWeapon) return;
        if (!Vehicle.CanUse(currentVehicle)) return; //Doesn't have a vehicle

        currentVehicle.ExitCar(this);
        currentVehicle = null;
        ColliderIsTrigger = false;

        alreadyDroppedChosenWeapon = true;
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
}
