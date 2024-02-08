using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


// Dynamically creates UI directional keys that point in accordance with the necessary reflect sequence.
// Rename script later.
public class FindPointDirection : MonoBehaviour
{
    [SerializeField] Transform canvasParent;
    [SerializeField] GameObject arrowPrefab;
    [SerializeField] GameObject dividerBetweenHeldKeys;
    [SerializeField] TextMeshProUGUI heldDurationText;
    [SerializeField] float distanceBetweenTiles;
    [SerializeField] [Range(0.4f, 0.9f)] float scaleAmountForMultiKey;

    PlayerController player;
    List<Image> arrowCanvases;

    Dictionary<PlayerController.KeySequenceItem.AllowedKeys, Quaternion> rotationMappings;

    bool canCreateDividers = true;



    void Start()
    {
        player = PlayerController.Instance;
        player.currentKeyData.AddListener(HideCurrentKeyUI);

        // var keys = PlayerController.KeySequenceItem.AllowedKeys.Up;

        arrowCanvases = new List<Image>();
        
        rotationMappings = new Dictionary<PlayerController.KeySequenceItem.AllowedKeys, Quaternion>
        {
            { PlayerController.KeySequenceItem.AllowedKeys.Up, Quaternion.Euler(0f, 0f, -180f) },
            { PlayerController.KeySequenceItem.AllowedKeys.Down, Quaternion.Euler(0f, 0, 0f) },
            { PlayerController.KeySequenceItem.AllowedKeys.Left, Quaternion.Euler(0, 0f, -90f) },
            { PlayerController.KeySequenceItem.AllowedKeys.Right, Quaternion.Euler(0, 0f, 90f) }
        };

        CreateUITiles(player.reflectSequence.Count);
    }


    void Update() =>
        UpdateHoldDurationText();


    
    
    void CreateUITiles(int numberOfImageArrows)
    {
        for (int i = 0; i < numberOfImageArrows; i++)
        {
            GameObject newArrow = Instantiate(arrowPrefab, canvasParent);
            Vector3 newArrowPos = arrowPrefab.transform.localPosition + new Vector3(0, i * distanceBetweenTiles, 0);
            newArrow.transform.localPosition = newArrowPos;

            // Get the corresponding KeyCode from the reflectSequence.
            var keyItem = player.reflectSequence[i];

            if (keyItem.KeyNeedsHeldDown())
                newArrow.transform.localScale *= scaleAmountForMultiKey;

            CreateDividersBetweenHeldKeys(newArrowPos, keyItem);


            // Use a dictionary to find where to point arrow.
            PlayerController.KeySequenceItem.AllowedKeys keyCode = keyItem.key;
            if (rotationMappings.ContainsKey(keyCode))
                newArrow.transform.localRotation = rotationMappings[keyCode];


            Image newArrowCanvas = newArrow.GetComponent<Image>();
            arrowCanvases.Add(newArrowCanvas);
            // ScaleDownArrowImageForKeysThatNeedHeld(newArrow.GetComponentInChildren<Image>(), i);
        }
    }

    void CreateDividersBetweenHeldKeys(Vector3 newArrowPos, PlayerController.KeySequenceItem keyItem)
    {
        if (keyItem.KeyNeedsHeldDown() && canCreateDividers)
        {
            GameObject newDividerRight = Instantiate(dividerBetweenHeldKeys, canvasParent);
            GameObject newDividerLeft = Instantiate(dividerBetweenHeldKeys, canvasParent);

            // Added extra (0.1f) offset, because perfectly exact positioned dividers sometimes weren't visible?
            float divider_Y_Offset = (distanceBetweenTiles * 0.5f) + 0.1f;


            newDividerLeft.transform.localPosition = new Vector3(
                newArrowPos.x,
                newArrowPos.y - divider_Y_Offset,
                newArrowPos.z);

            if (keyItem.alsoRequiresNextKey)
                divider_Y_Offset += distanceBetweenTiles;

            newDividerRight.transform.localPosition = new Vector3(
                newArrowPos.x,
                newArrowPos.y + divider_Y_Offset,
                newArrowPos.z);
        }

        // Turn off for next frame. Helps prevent excess dividers and overlaps.
        canCreateDividers = !keyItem.alsoRequiresNextKey;
    }

    void ScaleDownArrowImageForKeysThatNeedHeld(Image newArrowCanvas, int keyIndex)
    // void ScaleDownArrowImageForKeysThatNeedHeld(Image newArrowCanvas, bool keyNeedsHeldDown, bool alsoRequiresNextKey)
    {
        var key = player.reflectSequence[keyIndex];

        if (key.KeyNeedsHeldDown())
        {
            UnityEngine.Debug.Log("arrow canvas length = " + arrowCanvases.Count);
            newArrowCanvas.transform.localScale *= scaleAmountForMultiKey;

            if (key.alsoRequiresNextKey)
            {
                int nextCanvasIndex = arrowCanvases.IndexOf(newArrowCanvas) + 1;
                Image nextCanvas = arrowCanvases[nextCanvasIndex + 1];

                nextCanvas.transform.localScale *= scaleAmountForMultiKey;
            }
        }
    }


    // Called every frame via Update() in PlayerController.cs
    public void HideCurrentKeyUI(int keyIndex, bool alsoNeedsNextKey)
    {
        // Pass in -1 to when resetting the keySequence.
        if (keyIndex == -1)
        {
            ResetFillAmounts();
            return;
        }

        HoldingKeyLowersFillAmounts(keyIndex, alsoNeedsNextKey);
    }


    void UpdateHoldDurationText()
    {
        if (player.currentKey.KeyNeedsHeldDown())
        {
            float holdDurationRemainder = Mathf.Max((player.currentKey.holdDurationNeeded -
                                                    player.currentKey.timeHeld), 0);

            if (holdDurationRemainder > 0)
                heldDurationText.text = holdDurationRemainder.ToString("F2");
            else
                heldDurationText.text = " ";


            Vector3 newTextLocation = arrowCanvases[player.currentKeyIndex].transform.position;


            // Find the average position between corresponding canvas, and the next one (if there is one).
            if (player.currentKey.alsoRequiresNextKey)
            {
                if (player.currentKeyIndex < player.reflectSequence.Count - 1)
                {
                    Vector3 nextCanvasLocation = arrowCanvases[player.currentKeyIndex + 1].transform.position;

                    if (nextCanvasLocation != null)
                        newTextLocation = (newTextLocation + nextCanvasLocation) / 2;
                }
            }            


            heldDurationText.transform.position = new Vector3
                                                    (newTextLocation.x,
                                                     newTextLocation.y + 0.25f,
                                                     newTextLocation.z);
        }

        else
            heldDurationText.text = " ";
    }



    void HoldingKeyLowersFillAmounts(int keyIndex, bool alsoNeedsNextKey)
    {
        if (player.currentKey.holdDurationNeeded <= 0)
        {
            arrowCanvases[keyIndex].fillAmount = 0;

            if (alsoNeedsNextKey)
                arrowCanvases[keyIndex + 1].fillAmount = 0;
        }

        else
        {
            float newFillAmount = 1 - (player.currentKey.timeHeld /
                                        player.currentKey.holdDurationNeeded);
            
            arrowCanvases[keyIndex].fillAmount = newFillAmount;

            
            if (alsoNeedsNextKey)
            {
                if (arrowCanvases.Count - 1 > keyIndex)
                    arrowCanvases[keyIndex + 1].fillAmount = newFillAmount;
            }
        }
    }



    void ResetFillAmounts()
    {
        for (int i = 0; i < arrowCanvases.Count; i++)
            arrowCanvases[i].fillAmount = 1;
    }
}

