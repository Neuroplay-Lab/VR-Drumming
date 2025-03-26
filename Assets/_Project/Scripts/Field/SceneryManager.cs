using System.Collections.Generic;
using _Project.Scripts.Data;
using _Project.Scripts.Systems;
using DrumRhythmGame.Data;
using DrumRhythmGame.Systems;
using UnityEditor;
using UnityEngine;

namespace _Project.Scripts.Field
{
    public class SceneryManager : SingletonMonoBehaviour<SceneryManager>
    {
        private static readonly string Prefix = "[<b>SceneryManager</b>]";

        #region Serialized Fields

        [SerializeField] private List<GameObject> scenes;

        [SerializeField] private Transform instantiationPosition;

        #endregion

        private GameObject _currentScenery;

        [SerializeField] private EyeFocus eyeTracker;

        #region Event Functions

        private void OnEnable()
        {
            _currentScenery = scenes[0];  // choose an initial scene from index 0
            EventManager.SceneSelected += InstantiateScene;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var position = transform.position;
            Handles.DrawDottedLine(position, instantiationPosition.position, 4f);
        }
#endif

        #endregion

        /// <summary>
        ///     Instantiates the prefab of the selected scene.
        /// </summary>
        /// <param name="agent"></param>
        private void InstantiateScene(SceneSO scene)
        {
            // If we already have a partner, and its the same one, we double clicked the same agent so remove it
            if (_currentScenery != null && _currentScenery == scene.prefab)
            {
                //Destroy(_currentScenery);
                // TODO: Remove this if not needed - there should never be no scene
                return;
            }

            // If we selected a different agent, destroy the old one and instantiate the new one
            if (_currentScenery != null) Destroy(_currentScenery);
            SaveData.Instance.sceneryData.sceneryIndex = scene.index;
            _currentScenery = Instantiate(scene.prefab, instantiationPosition);
            eyeTracker.sceneChange(scene.name);
            Debug.Log($"{Prefix} Instantiated scene <color=green>{scene.index}</color>");
        }


        private void SelectScene(int sceneIndex, int altSceneIndex)
        {
            if (sceneIndex < 0) sceneIndex = 0;

            if (sceneIndex > scenes.Count && altSceneIndex == 0)
                sceneIndex = 0;

            // If we already have a partner
            if (_currentScenery != null)
                Destroy(_currentScenery);

            // SET SKIN INDEX
            SaveData.Instance.sceneryData.sceneryIndex = sceneIndex;

            // INSTANTIATE THE MODEL
            _currentScenery = Instantiate(scenes[sceneIndex], instantiationPosition);

            // GET PARTNER SCRIPT ON OBJECT
            //var partnerBehaviour = CurrentPartnerOne.GetComponentInChildren<Partner>
            // SET PARTNER AS ACTIVE

            Debug.Log($"{Prefix} Updated the prefab of scene: <color=green>{sceneIndex}</color>");
        }

        public void DestroyScene()
        {
            if (_currentScenery == null) return;
            Destroy(_currentScenery);
            _currentScenery = null;
        }

        public void SelectNextScene(int altSceneIndex)
        {
            var sceneIndex = SaveData.Instance.sceneryData.sceneryIndex;

            SelectScene((sceneIndex + 1) % scenes.Count, altSceneIndex);
        }

        public void SelectPreviousScene(int altSceneIndex)
        {
            var sceneIndex = SaveData.Instance.sceneryData.sceneryIndex;
            SelectScene((scenes.Count + sceneIndex - 1) % scenes.Count, altSceneIndex);
        }

    }
}