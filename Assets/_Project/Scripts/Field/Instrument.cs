﻿using System.Collections;
using _Project.Scripts.Systems;
using DrumRhythmGame.Data;
using DrumRhythmGame.Systems;
using UnityEngine;

namespace DrumRhythmGame.Field
{
    /// <summary>
    /// 
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(Rigidbody))]
    public class Instrument : MonoBehaviour
    {
        [SerializeField] private InstrumentType type;
        [SerializeField] public AudioClip sound = null;
        [SerializeField] public float volume = 0.8f;
        [SerializeField] public float timeToPlayStart = 0;
        [SerializeField] private float chatteringInterval = 0.1f;

        
        [HideInInspector] public AudioSource audioSource;
        private bool _isPlayable = true;

        [SerializeField] private InsideCollider wrongSide;
        
        private void Awake()
        {
            //　AudioSource setting
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
            
            // Collider setting
            var meshCollider = GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                meshCollider.convex = true;
                meshCollider.isTrigger = true;
            }
            else
            {
                var col = GetComponent<Collider>();
                col.isTrigger = true;
            }
            
            // Rigidbody setting
            var rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if(wrongSide == null || wrongSide.currentTriggerStatus() == false)
            {
                if (_isPlayable && other.transform.TryGetComponent<DrumStick>(out var stick))
                {
                    audioSource.PlayOneShot(sound, volume);
                    StartCoroutine(PreventChatteringCoroutine(chatteringInterval));

                    EventManager.InvokeDrumHitEvent(stick.owner, type, stick.node);
                }
            }
        }

        private IEnumerator PreventChatteringCoroutine(float time)
        {
            _isPlayable = false;
            
            yield return new WaitForSeconds(time);

            _isPlayable = true;
        }
    }
}
