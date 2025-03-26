using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleGazeRay : MonoBehaviour
{
    [SerializeField] private LineRenderer gazeRay;
    [SerializeField] private bool gazeRayShown;
    [SerializeField] private GameObject showText;
    [SerializeField] private GameObject hideText;

    private void Reset()
    {
        gazeRay = GameObject.FindObjectOfType<EyeFocus>().GetComponent<LineRenderer>();
        gazeRayShown = gazeRay.enabled;
        showText = transform.Find("Show Text").gameObject;
        hideText = transform.Find("Hide Text").gameObject;
        UpdateText();
    }

    private void UpdateText()
    {
        if (gazeRayShown)
        {
            showText.SetActive(false);
            hideText.SetActive(true);
        }
        else
        {
            showText.SetActive(true);
            hideText.SetActive(false);
        }
    }

    public void ToggleGazeRayVisibility()
    {
        if (gazeRayShown)
        {
            gazeRay.enabled = false;
            gazeRayShown = false;
        }
        else
        {
            gazeRay.enabled = true;
            gazeRayShown = true;
        }

        UpdateText();
    }
}
