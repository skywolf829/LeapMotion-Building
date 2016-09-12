using UnityEngine;
using System.Collections;

public class HandListener : MonoBehaviour {

    private const float MAX_ANGLE_DIFFERENCE_FOR_INTERFACE = 40.0f;
    private const float MIN_ERASE_VELOCITY = 1.25f;
    private const float ERASE_SCALE = 8.0f;
    private const float DRAW_WIDTH = 0.02f;
    private const float MIN_PINCH_DISTANCE = 0.01f;
    private const float MAX_PINCH_DISTANCE = 0.08f;
    private const float PINCH_START = 0.8f;
    private const float PINCH_TO_CONTINUE = 0.6f;
    private const float SLIDER_HEIGHT = 1.27f;

    public GameObject LeapHandController, mainCamera;
    public GameObject cubePrefab, cylinderPrefab, spherePrefab, huePrefab, selectionBarPrefab;

    private GameObject lastObjectHit, objectHoldingInRightHand, objectHoldingInLeftHand;
    private GameObject leftHand, rightHand;
    private GameObject hue, selectionBar, currentObject;

    private HandModel leftHandModel, rightHandModel;

    private Color savedColor = Color.white;

    private Vector3 distanceBetweenLeftHandAndObject, distanceBetweenRightHandAndObject, setLeftDistance, setRightDistance;

    private float lastTimeHit;

    private float hueSliderHeightPercentage, selectionSliderHeightPercentage;

    private bool rightHandIndexPinching, leftHandIndexPinching, leftHandInterfaceActive, rightHandInterfaceActive;
    private bool rightHandMiddlePinching, rightHandRingPinching, rightHandPinkyPinching;
    private bool leftHandMiddlePinching, leftHandRingPinching, leftHandPinkyPinching;
    private bool rightHandFacingUp, leftHandFacingUp;
    private bool cubePicked, cylinderPicked, spherePicked, creatingObject;
    private bool leftHandTouchingObject, rightHandTouchingObject;
    private bool holdingObjectWithLeftHand, holdingObjectWithRightHand;
    
    void Start () {
        Physics.gravity = new Vector3(0, -2.0f, 0);
        if (LeapHandController)
        {
            leftHandModel = LeapHandController.transform.GetChild(2).GetComponent<HandModel>();
            rightHandModel = LeapHandController.transform.GetChild(3).GetComponent<HandModel>();
            leftHand = LeapHandController.transform.GetChild(2).gameObject;
            rightHand = LeapHandController.transform.GetChild(3).gameObject;
        }
        cubePicked = true;
    }	

	void Update () {
        if (leftHandModel)
        {
            updateLeftHand();
        }
        if (rightHandModel)
        {
            updateRightHand();
        }
        if(leftHand && rightHand)
        {
            build();   
        }
    }

    void updateLeftHand()
    {
        updateLeftIndexPinching();
        updateLeftMiddlePinching();
        updateLeftRingPinching();
        updateLeftPinkyPinching();
        updateLeftHandFacingUp();
        leftHandActions();
    }
    void updateLeftIndexPinching()
    {
        if (!leftHand.activeSelf)
        {
            leftHandIndexPinching = false;
        }
        else
        {
            Vector3 indexPosition = leftHandModel.fingers[1].GetBoneCenter(3);
            Vector3 thumbPosition = leftHandModel.fingers[0].GetBoneCenter(3);

            float distance = (indexPosition - thumbPosition).magnitude;
            float normalizedDistance = (distance - MIN_PINCH_DISTANCE) / (MAX_PINCH_DISTANCE - MIN_PINCH_DISTANCE);
            float pinch = 1.0f - Mathf.Clamp01(normalizedDistance);

            if (!leftHandIndexPinching && pinch > PINCH_START)
            {
                leftHandIndexPinching = true;
            }
            else if (leftHandIndexPinching && pinch < PINCH_TO_CONTINUE)
            {
                leftHandIndexPinching = false;
            }
        }
    }
    void updateLeftMiddlePinching()
    {
        if (!leftHand.activeSelf)
        {
            leftHandMiddlePinching = false;
        }
        else
        {
            Vector3 middlePosition = leftHandModel.fingers[2].GetBoneCenter(3);
            Vector3 thumbPosition = leftHandModel.fingers[0].GetBoneCenter(3);

            float distance = (middlePosition - thumbPosition).magnitude;
            float normalizedDistance = (distance - MIN_PINCH_DISTANCE) / (MAX_PINCH_DISTANCE - MIN_PINCH_DISTANCE);
            float pinch = 1.0f - Mathf.Clamp01(normalizedDistance);

            if (!leftHandMiddlePinching && pinch > PINCH_START)
            {
                leftHandMiddlePinching = true;
            }
            else if (leftHandMiddlePinching && pinch < PINCH_TO_CONTINUE)
            {
                leftHandMiddlePinching = false;
            }
        }
    }
    void updateLeftRingPinching()
    {
        if (!leftHand.activeSelf)
        {
            leftHandRingPinching = false;
        }
        else
        {
            Vector3 ringPosition = leftHandModel.fingers[3].GetBoneCenter(3);
            Vector3 thumbPosition = leftHandModel.fingers[0].GetBoneCenter(3);

            float distance = (ringPosition - thumbPosition).magnitude;
            float normalizedDistance = (distance - MIN_PINCH_DISTANCE) / (MAX_PINCH_DISTANCE - MIN_PINCH_DISTANCE);
            float pinch = 1.0f - Mathf.Clamp01(normalizedDistance);

            if (!leftHandRingPinching && pinch > PINCH_START)
            {
                leftHandRingPinching = true;
            }
            else if (leftHandRingPinching && pinch < PINCH_TO_CONTINUE)
            {
                leftHandRingPinching = false;
            }
        }
    }
    void updateLeftPinkyPinching()
    {
        if (!leftHand.activeSelf)
        {
            leftHandPinkyPinching = false;
        }
        else
        {
            Vector3 pinkyPosition = leftHandModel.fingers[4].GetBoneCenter(3);
            Vector3 thumbPosition = leftHandModel.fingers[0].GetBoneCenter(3);

            float distance = (pinkyPosition - thumbPosition).magnitude;
            float normalizedDistance = (distance - MIN_PINCH_DISTANCE) / (MAX_PINCH_DISTANCE - MIN_PINCH_DISTANCE);
            float pinch = 1.0f - Mathf.Clamp01(normalizedDistance);

            if (!leftHandPinkyPinching && pinch > PINCH_START)
            {
                leftHandPinkyPinching = true;
            }
            else if (leftHandPinkyPinching && pinch < PINCH_TO_CONTINUE)
            {
                leftHandPinkyPinching = false;
            }
        }
    }
    bool anyLeftFingerPinching()
    {
        return leftHandIndexPinching || leftHandMiddlePinching || leftHandRingPinching || leftHandPinkyPinching;
    }
    void updateLeftHandFacingUp()
    {
        Vector3 upCam = mainCamera.transform.up;
        float difference = Vector3.Angle(leftHandModel.GetPalmNormal(), upCam);
        leftHandFacingUp = difference < MAX_ANGLE_DIFFERENCE_FOR_INTERFACE && leftHand.activeSelf;
    }    
    void leftHandActions()
    {
        if (anyLeftFingerPinching() && leftHandTouchingObject && Time.time - lastTimeHit < 0.25f && lastObjectHit && !holdingObjectWithLeftHand)
        {
            grabObjectWithLeftHand();
        }
        else if (!anyLeftFingerPinching() && holdingObjectWithLeftHand)
        {
            dropObjectHeldInLeftHand();
        }
        else if (rightHandInterfaceActive && leftHandIndexPinching)
        {
            moveObjectSlider();
            updateObjectSelected();
        }

        if (leftHandInterfaceActive && !leftHandFacingUp)
        {
            deactivateColorSelectionBar();
        }
        else if (leftHandInterfaceActive && leftHandFacingUp)
        {
            updateColorSelectionBar();
        }
        else if (!leftHandInterfaceActive && leftHandFacingUp && !holdingObjectWithLeftHand)
        {
            activateColorSelectionBar();
        }
    }

    void updateRightHand()
    {
        updateRightIndexPinching();
        updateRightMiddlePinching();
        updateRightRingPinching();
        updateRightPinkyPinching();
        updateRightHandFacingUp();
        rightHandActions();
    }
    void updateRightIndexPinching()
    {
        if (!rightHand.activeSelf)
        {
            rightHandIndexPinching = false;
        }
        else
        {
            Vector3 indexPosition = rightHandModel.fingers[1].GetBoneCenter(3);
            Vector3 thumbPosition = rightHandModel.fingers[0].GetBoneCenter(3);

            float distance = (indexPosition - thumbPosition).magnitude;
            float normalizedDistance = (distance - MIN_PINCH_DISTANCE) / (MAX_PINCH_DISTANCE - MIN_PINCH_DISTANCE);
            float pinch = 1.0f - Mathf.Clamp01(normalizedDistance);

            if (!rightHandIndexPinching && pinch > PINCH_START)
            {
                rightHandIndexPinching = true;
            }
            else if (rightHandIndexPinching && pinch < PINCH_TO_CONTINUE)
            {
                rightHandIndexPinching = false;
            }
        }
    }
    void updateRightMiddlePinching()
    {
        if (!rightHand.activeSelf)
        {
            rightHandMiddlePinching = false;
        }
        else
        {
            Vector3 middlePosition = rightHandModel.fingers[2].GetBoneCenter(3);
            Vector3 thumbPosition = rightHandModel.fingers[0].GetBoneCenter(3);

            float distance = (middlePosition - thumbPosition).magnitude;
            float normalizedDistance = (distance - MIN_PINCH_DISTANCE) / (MAX_PINCH_DISTANCE - MIN_PINCH_DISTANCE);
            float pinch = 1.0f - Mathf.Clamp01(normalizedDistance);

            if (!rightHandMiddlePinching && pinch > PINCH_START)
            {
                rightHandMiddlePinching = true;
            }
            else if (rightHandMiddlePinching && pinch < PINCH_TO_CONTINUE)
            {
                rightHandMiddlePinching = false;
            }
        }
    }
    void updateRightRingPinching()
    {
        if (!rightHand.activeSelf)
        {
            rightHandRingPinching = false;
        }
        else
        {
            Vector3 ringPosition = rightHandModel.fingers[3].GetBoneCenter(3);
            Vector3 thumbPosition = rightHandModel.fingers[0].GetBoneCenter(3);

            float distance = (ringPosition - thumbPosition).magnitude;
            float normalizedDistance = (distance - MIN_PINCH_DISTANCE) / (MAX_PINCH_DISTANCE - MIN_PINCH_DISTANCE);
            float pinch = 1.0f - Mathf.Clamp01(normalizedDistance);

            if (!rightHandRingPinching && pinch > PINCH_START)
            {
                rightHandRingPinching = true;
            }
            else if (rightHandRingPinching && pinch < PINCH_TO_CONTINUE)
            {
                rightHandRingPinching = false;
            }
        }
    }
    void updateRightPinkyPinching()
    {
        if (!rightHand.activeSelf)
        {
            rightHandPinkyPinching = false;
        }
        else
        {
            Vector3 pinkyPosition = rightHandModel.fingers[4].GetBoneCenter(3);
            Vector3 thumbPosition = rightHandModel.fingers[0].GetBoneCenter(3);

            float distance = (pinkyPosition - thumbPosition).magnitude;
            float normalizedDistance = (distance - MIN_PINCH_DISTANCE) / (MAX_PINCH_DISTANCE - MIN_PINCH_DISTANCE);
            float pinch = 1.0f - Mathf.Clamp01(normalizedDistance);

            if (!rightHandPinkyPinching && pinch > PINCH_START)
            {
                rightHandPinkyPinching = true;
            }
            else if (rightHandPinkyPinching && pinch < PINCH_TO_CONTINUE)
            {
                rightHandPinkyPinching = false;
            }
        }
    }
    bool anyRightFingerPinching()
    {
        return rightHandIndexPinching || rightHandMiddlePinching || rightHandRingPinching || rightHandPinkyPinching;
    }
    void updateRightHandFacingUp()
    {
        Vector3 upCam = mainCamera.transform.up;
        float difference = Vector3.Angle(rightHandModel.GetPalmNormal(), upCam);
        rightHandFacingUp = difference < MAX_ANGLE_DIFFERENCE_FOR_INTERFACE && rightHand.activeSelf;
    }
    void rightHandActions()
    {
        if(anyRightFingerPinching() && rightHandTouchingObject && Time.time - lastTimeHit < 0.25f && lastObjectHit && !holdingObjectWithRightHand)
        {
            grabObjectWithRightHand();
        }
        else if(!anyRightFingerPinching() && holdingObjectWithRightHand)
        {
            dropObjectHeldInRightHand();
        }
        else if(leftHandInterfaceActive && rightHandIndexPinching)
        {
            moveHueSlider();
            updateColorSelected();
        }

        if(rightHandInterfaceActive && !rightHandFacingUp)
        {
            deactivateObjectSelectionBar();
        }
        else if(rightHandInterfaceActive && rightHandFacingUp)
        {
            updateObjectSelectionBar();
        }
        else if(!rightHandInterfaceActive && rightHandFacingUp && !holdingObjectWithRightHand)
        {
            activateObjectSelectionBar();
        }
    }

    void updateObjectSelectionBar()
    {
        if (selectionBar && selectionBar.activeSelf)
        {
            selectionBar.transform.position = rightHandModel.GetPalmPosition() + rightHandModel.GetPalmNormal() * 0.15f;
            selectionBar.transform.rotation = rightHandModel.GetPalmRotation();
        }
    }
    void activateObjectSelectionBar()
    {
        if (selectionBar)
        {
            selectionBar.transform.position = rightHandModel.GetPalmPosition() + rightHandModel.GetPalmNormal() * 0.15f;
            selectionBar.transform.rotation = rightHandModel.GetPalmRotation();
            selectionBar.SetActive(true);
        }

        else if (selectionBarPrefab)
        {
            selectionBar = (GameObject)Instantiate(selectionBarPrefab,
                rightHandModel.GetPalmPosition() + rightHandModel.GetPalmNormal() * 0.15f,
                rightHandModel.GetPalmRotation());
            selectionSliderHeightPercentage = 0.5f;
            updateObjectSelected();
        }
        rightHandInterfaceActive = true;
    }
    void deactivateObjectSelectionBar()
    {
        if (selectionBar)
        {
            selectionBar.SetActive(false);
        }
        rightHandInterfaceActive = false;
    }
    void moveObjectSlider()
    {
        Vector3 averagePinchPoint = (mainCamera.GetComponent<Camera>().WorldToScreenPoint(leftHandModel.fingers[1].bones[3].transform.position) +
            mainCamera.GetComponent<Camera>().WorldToScreenPoint(leftHandModel.fingers[0].bones[3].transform.position)) / 2.0f;

        int screenHeight = Screen.height;
        float percentageOfHeight = Mathf.Clamp01(averagePinchPoint.y / screenHeight);
        if (selectionBar)
        {
            selectionBar.transform.GetChild(0).transform.localPosition =
                new Vector3(0, -(SLIDER_HEIGHT * 2.0f * percentageOfHeight) + SLIDER_HEIGHT, 0);
        }
        selectionSliderHeightPercentage = percentageOfHeight;
    }
    void updateObjectSelected()
    {
        if (selectionSliderHeightPercentage >= 0 && selectionSliderHeightPercentage < 0.33f)
        {
            cubePicked = true;
            cylinderPicked = false;
            spherePicked = false;
        }
        else if (selectionSliderHeightPercentage < 0.66f)
        {
            cubePicked = false;
            cylinderPicked = true;
            spherePicked = false;
        }
        else
        {
            cubePicked = false;
            cylinderPicked = false;
            spherePicked = true;
        }
    }

    void updateColorSelectionBar()
    {
        if (hue && hue.activeSelf)
        {
            hue.transform.position = leftHandModel.GetPalmPosition() + leftHandModel.GetPalmNormal() * 0.15f;
            hue.transform.rotation = leftHandModel.GetPalmRotation();
        }
    }
    void activateColorSelectionBar()
    {
        if (hue)
        {
            hue.transform.position = leftHandModel.GetPalmPosition() + leftHandModel.GetPalmNormal() * 0.15f;
            hue.transform.rotation = leftHandModel.GetPalmRotation();
            hue.SetActive(true);
        }

        else if (huePrefab)
        {
            hue = (GameObject)Instantiate(huePrefab,
                leftHandModel.GetPalmPosition() + leftHandModel.GetPalmNormal() * 0.15f,
                leftHandModel.GetPalmRotation());
            hueSliderHeightPercentage = 0.5f;
            updateColorSelected();
        }
        leftHandInterfaceActive = true;
    }
    void deactivateColorSelectionBar()
    {
        if (!leftHand.activeSelf || !leftHandFacingUp)
        {
            if (hue)
            {
                hue.SetActive(false);
            }
            leftHandInterfaceActive = false;
        }
    }
    void moveHueSlider()
    {
        Vector3 averagePinchPoint = (mainCamera.GetComponent<Camera>().WorldToScreenPoint(rightHandModel.fingers[1].bones[3].transform.position) +
            mainCamera.GetComponent<Camera>().WorldToScreenPoint(rightHandModel.fingers[0].bones[3].transform.position)) / 2.0f;
        int screenHeight = Screen.height;
        float percentageOfHeight = Mathf.Clamp01(averagePinchPoint.y / screenHeight);
        if (hue)
        {
            hue.transform.GetChild(0).transform.localPosition =
                new Vector3(0, -(SLIDER_HEIGHT * 2.0f * percentageOfHeight) + SLIDER_HEIGHT, 0);
        }
        hueSliderHeightPercentage = percentageOfHeight;
    }
    void updateColorSelected()
    {
        Texture2D t;
        if (hue)
        {
            t = hue.GetComponent<SpriteRenderer>().sprite.texture;
            if (t)
            {
                savedColor = t.GetPixel(0, (int)(t.height - (hueSliderHeightPercentage * t.height)));
            }
        }
    }

    void grabObjectWithRightHand()
    {
        objectHoldingInRightHand = lastObjectHit;
        objectHoldingInRightHand.GetComponent<Collider>().enabled = false;       

        objectHoldingInRightHand.AddComponent<FixedJoint>();
        FixedJoint hinge = objectHoldingInRightHand.GetComponent<FixedJoint>();
        hinge.connectedBody = rightHandModel.fingers[1].bones[3].GetComponent<Rigidbody>();
        holdingObjectWithRightHand = true;
    }
    void dropObjectHeldInRightHand()
    {
        objectHoldingInRightHand.GetComponent<Rigidbody>().velocity = rightHandModel.palm.GetComponent<Rigidbody>().velocity;
        objectHoldingInRightHand.GetComponent<Collider>().enabled = true;
        Destroy(objectHoldingInRightHand.GetComponent<FixedJoint>());
        holdingObjectWithRightHand = false;
    }

    void grabObjectWithLeftHand()
    {
        objectHoldingInLeftHand = lastObjectHit;
        objectHoldingInLeftHand.GetComponent<Collider>().enabled = false;

        objectHoldingInLeftHand.AddComponent<FixedJoint>();
        FixedJoint hinge = objectHoldingInLeftHand.GetComponent<FixedJoint>();
        hinge.connectedBody = leftHandModel.fingers[1].bones[3].GetComponent<Rigidbody>();
        holdingObjectWithLeftHand = true;
    }
    void dropObjectHeldInLeftHand()
    {
        objectHoldingInLeftHand.GetComponent<Rigidbody>().velocity = leftHandModel.palm.GetComponent<Rigidbody>().velocity;
        objectHoldingInLeftHand.GetComponent<Collider>().enabled = true;
        Destroy(objectHoldingInLeftHand.GetComponent<FixedJoint>());
        holdingObjectWithLeftHand = false;
    }

    void build()
    {
        Vector3 leftHandPos = leftHandModel.fingers[1].bones[3].transform.position;
        Vector3 rightHandPos = rightHandModel.fingers[1].bones[3].transform.position;
        Vector3 forCrossProduct = leftHandPos;
        forCrossProduct.x = rightHandPos.x;
        if (!creatingObject)
        {
            if (leftHandIndexPinching && rightHandIndexPinching && !holdingObjectWithLeftHand && !holdingObjectWithRightHand)
            {
                
                if (cubePicked)
                {
                    currentObject = (GameObject)Instantiate(cubePrefab, (leftHandPos + rightHandPos) / 2.0f, Quaternion.LookRotation(rightHandPos - leftHandPos, forCrossProduct - leftHandPos));
                    currentObject.transform.localScale = new Vector3((leftHandPos - rightHandPos).magnitude / 2.0f, 
                        (leftHandPos - rightHandPos).magnitude / 2.0f, 
                        (leftHandPos - rightHandPos).magnitude / 2.0f);
                    currentObject.GetComponent<MeshRenderer>().material.color = savedColor;
                    
                }
                else if (cylinderPicked)
                {
                    currentObject = (GameObject)Instantiate(cylinderPrefab, (leftHandPos + rightHandPos) / 2.0f, Quaternion.LookRotation(rightHandPos - leftHandPos, forCrossProduct - leftHandPos));
                    currentObject.transform.localScale = new Vector3((leftHandPos - rightHandPos).magnitude / 2.0f,
                        (leftHandPos - rightHandPos).magnitude / 2.0f,
                        (leftHandPos - rightHandPos).magnitude / 2.0f);
                    currentObject.GetComponent<MeshRenderer>().material.color = savedColor;
                }
                else if(spherePicked)
                {
                    currentObject = (GameObject)Instantiate(spherePrefab, (leftHandPos + rightHandPos) / 2.0f, Quaternion.LookRotation(rightHandPos - leftHandPos, forCrossProduct - leftHandPos));
                    currentObject.transform.localScale = new Vector3((leftHandPos - rightHandPos).magnitude / 2.0f,
                        (leftHandPos - rightHandPos).magnitude / 2.0f,
                        (leftHandPos - rightHandPos).magnitude / 2.0f);
                    currentObject.GetComponent<MeshRenderer>().material.color = savedColor;
                }
                currentObject.GetComponent<Rigidbody>().mass = currentObject.transform.localScale.x * 50;
                creatingObject = true; 
            }
        }
        else
        {
            if(!leftHandIndexPinching || !rightHandIndexPinching && currentObject)
            {
                if (cubePicked)
                {
                    currentObject.AddComponent<BoxCollider>();

                }
                else if (spherePicked)
                {
                    currentObject.AddComponent<SphereCollider>();
                }
                else if(cylinderPicked)
                {
                    currentObject.AddComponent<MeshCollider>();
                    currentObject.GetComponent<MeshCollider>().convex = true;
                }
                currentObject.GetComponent<Rigidbody>().velocity = ((leftHandModel.palm.GetComponent<Rigidbody>().velocity + rightHandModel.palm.GetComponent<Rigidbody>().velocity));
                creatingObject = false;
            }
            else
            {
                if (currentObject)
                {
                    currentObject.transform.position = (leftHandPos + rightHandPos) / 2.0f;
                    currentObject.transform.localScale = new Vector3((leftHandPos - rightHandPos).magnitude / 2.0f,
                            (leftHandPos - rightHandPos).magnitude / 2.0f,
                            (leftHandPos - rightHandPos).magnitude / 2.0f);
                    currentObject.transform.rotation = Quaternion.LookRotation(rightHandPos - leftHandPos, forCrossProduct - leftHandPos);
                }
            }
        }
    }
    
    void lastHandTouchingObject(GameObject g)
    {
        if (g.Equals(rightHand))
        {
            leftHandTouchingObject = false;
            rightHandTouchingObject = true;
        }
        else if (g.Equals(leftHand))
        {
            leftHandTouchingObject = true;
            rightHandTouchingObject = false;
        }
    }
    void hittingObject(GameObject g)
    {
        lastObjectHit = g;
        lastTimeHit = Time.time;
        if (leftHandTouchingObject)
        {
            distanceBetweenLeftHandAndObject = g.transform.position - leftHand.transform.position;
        }
        else
        {
            distanceBetweenRightHandAndObject = g.transform.position - rightHand.transform.position;
        }
    }
}