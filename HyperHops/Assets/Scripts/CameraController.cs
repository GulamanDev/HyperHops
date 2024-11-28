using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CameraController : MonoBehaviour
{
    public Vector3 offset = new Vector3(0, 2, -10); // Adjust as needed
    public float smoothSpeed = 5f; // Smoothing speed

    private Transform target;

    void Start()
    {
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            PhotonView photonView = player.GetComponent<PhotonView>();
            if (photonView != null && photonView.IsMine)
            {
                target = player.transform;
                Debug.Log("Camera attached to: " + player.name);
                break;
            }
        }
    }

    void Update()
    {
        if (target == null)
        {
            foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
            {
                PhotonView photonView = player.GetComponent<PhotonView>();
                if (photonView != null && photonView.IsMine)
                {
                    target = player.transform;
                    break;
                }
            }
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Smoothly move the camera towards the target position
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}
