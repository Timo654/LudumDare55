using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonfireCharacterMove : MonoBehaviour {
    
    public GameObject backgroundPrefab; 

    void Start()
    {
        GameObject background = Instantiate(backgroundPrefab) as GameObject;
        SetAsBackground(background, Camera.main);
    }

    void Update()
    {   
      
    }

    void SetAsBackground(GameObject backgroundObject, Camera mainCamera)
    {
        // Set the position of the GameObject to the camera's position
        backgroundObject.transform.position = mainCamera.transform.position;

        // Calculate the height and width that the background needs to be
        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        // Assuming the GameObject has a SpriteRenderer component and the sprite is oriented in the XY plane
        SpriteRenderer spriteRenderer = backgroundObject.GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            float spriteHeight = spriteRenderer.sprite.bounds.size.y;
            float spriteWidth = spriteRenderer.sprite.bounds.size.x;

            // Calculate the scale in each direction
            float scaleY = cameraHeight / spriteHeight;
            float scaleX = cameraWidth / spriteWidth;

            // Set the scale of the GameObject
            backgroundObject.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }
        else
        {
            Debug.LogError("The GameObject does not have a SpriteRenderer component.");
        }
    }
}
