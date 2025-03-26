using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace _Project.Scripts.UI
{
    public class AvatarPositionOffset : MonoBehaviour
    {

        [SerializeField] private GameObject partnerContainer;
        [SerializeField] private Vector3 initialAvatarPosition;

        [SerializeField] private Slider leftRightSlider;
        [SerializeField] private Slider frontBackSlider;
        [SerializeField] private TextMeshProUGUI leftRightSliderLabel;
        [SerializeField] private TextMeshProUGUI frontBackSliderLabel;

        private void Awake()
        {
            partnerContainer = GameObject.Find("Partner One");
            initialAvatarPosition = partnerContainer.transform.localPosition;
        }

        public void Reset()
        {
            leftRightSlider.value = 0;
            frontBackSlider.value = 0;
        }

        public void OnLeftRightOffset(float offset)
        {
            if (offset > 0)
            {
                leftRightSliderLabel.SetText("R+" + offset.ToString("F2"));
            }
            else if (offset < 0)
            {
                leftRightSliderLabel.SetText("L+" + Mathf.Abs(offset).ToString("F2"));
            }
            else
            {
                leftRightSliderLabel.SetText("0");
            }

            Vector3 newPosition = partnerContainer.transform.localPosition;
            newPosition.x = initialAvatarPosition.x + offset;
            partnerContainer.transform.localPosition = newPosition;
        }

        public void OnFrontBackOffset(float offset)
        {
            if (offset > 0)
            {
                frontBackSliderLabel.SetText("B+" + offset.ToString("F2"));
            }
            else if (offset < 0)
            {
                frontBackSliderLabel.SetText("F+" + Mathf.Abs(offset).ToString("F2"));
            }
            else
            {
                frontBackSliderLabel.SetText("0");
            }

            Vector3 newPosition = partnerContainer.transform.localPosition;
            newPosition.z = initialAvatarPosition.z + offset;
            partnerContainer.transform.localPosition = newPosition;
        }
    }
}