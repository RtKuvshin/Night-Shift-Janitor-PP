using System;
using SatProductions;
using UnityEngine;
using UnityEngine.UI;

public class DoorAnimation : MonoBehaviour
{
    private bool? isDoorOpen = false;
    private float holdTime = 0.5f;
    private float holdTimer = 0f;
    private bool isHolding = false;

    [SerializeField] private Slider doorProgressSlider;
    [SerializeField] private GameObject doorProgressPanel;
    [SerializeField] private float interactionDistance = 5f;
    [SerializeField] private LayerMask doorLayer;
    [SerializeField] private Transform playerHead;
    
    private GameObject currentDoor = null;
    private bool isBack;

    private void Awake()
    {
        if (doorProgressSlider != null)
            doorProgressPanel.gameObject.SetActive(false);
    }

    private void Update()
    {
        RaycastHit hit;
        Ray ray = new Ray(playerHead.position, playerHead.forward);

        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.red);

        if (Physics.Raycast(ray, out hit, interactionDistance, doorLayer))
        {
            if (hit.collider.CompareTag("Door"))
            {
                GameObject detectedDoor = hit.collider.gameObject;

                if (detectedDoor != currentDoor)
                {
                    currentDoor = detectedDoor;
                    isDoorOpen = null;
                    ResetSlider();
                }

                Vector3 doorForward = currentDoor.transform.forward;
                Vector3 toPlayer = (playerHead.position - hit.point).normalized;
                float dot = Vector3.Dot(doorForward, toPlayer);

                isBack = dot < 0;
            }
        }
        else
        {
            currentDoor = null;
            ResetSlider();
        }

        if (currentDoor != null)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                isHolding = true;
                holdTimer = 0f;
                if (doorProgressSlider != null)
                    doorProgressPanel.gameObject.SetActive(true);
            }

            if (Input.GetKey(KeyCode.E) && isHolding)
            {
                holdTimer += Time.deltaTime;
                if (doorProgressSlider != null)
                    doorProgressSlider.value = holdTimer / holdTime;

                if (holdTimer >= holdTime)
                {
                    EasyDoor door = currentDoor.GetComponentInParent<EasyDoor>();
                    if (door != null)
                    {
                        if (!door.IsOpen)
                            door.OpenDoor(isBack);
                        else
                            door.CloseDoor();


                        Debug.Log("Door toggled: " + currentDoor.name);
                    }
                    else
                    {
                        Debug.LogWarning("EasyDoor script not found on detected object.");
                    }

                    isHolding = false;
                    ResetSlider();
                }
            }

            if (Input.GetKeyUp(KeyCode.E))
            {
                isHolding = false;
                ResetSlider();
            }
        }
    }

    private void ResetSlider()
    {
        if (doorProgressSlider != null)
        {
            doorProgressSlider.value = 0;
            doorProgressPanel.gameObject.SetActive(false);
        }
    }
}
