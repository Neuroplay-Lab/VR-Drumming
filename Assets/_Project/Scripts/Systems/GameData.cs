using Data;
using DrumRhythmGame.Data;
using DrumRhythmGame.Systems;
using UnityEngine;
using UnityEngine.XR;

namespace _Project.Scripts.Systems
{
    [DefaultExecutionOrder(-1)]
    public class GameData : SingletonMonoBehaviour<GameData>
    {
        // A way of creating headers in a CSV file
        private const string
            DataHeader =
                "Timestamp,WhichInstrumentWasHit,SyncRateBetweenPlayerAndDrummingAgent,RhythmErrorRate,EyeFocusItem, EyeFocusCoord"; // ,CueOnset

        private DataLogger dataLogger;
        private ErrorRateController errorRateController;
        private SyncRateController syncRateController;

        [SerializeField] private EyeFocus eyeTracker;
        //[SerializeField] private Camera eyeViewCamera;
        private Vector3 lookCoords;

        public int ScorePoint { get; private set; }
        public float SynchronousRate { get; private set; }

        #region Event Functions

        protected override void Awake()
        {
            base.Awake();

            var instrumentTypes = new[]
            {
                InstrumentType.CrashCymbal,
                InstrumentType.SnareDrum,
                InstrumentType.LeftHighTom,
                InstrumentType.RightMiddleTom
            };

            dataLogger = new DataLogger();
            syncRateController = new SyncRateController(
                instrumentTypes,
                ActorType.Partner,
                ActorType.Player,
                20,
                0.2f,
                1.0f);

            syncRateController.OnRateChange += OnSyncRateChange;

            errorRateController = ErrorRateController.Instance;

            EventManager.MusicStartEvent += ResetScorePoint;
            EventManager.MusicStartEvent += StartRecording;
            EventManager.MusicResetEvent += StopRecording;

            EventManager.DrumHitEvent += CaptureDrumHit;
        }

        /*private byte[] SaveCameraView()
        {
            RenderTexture screenTexture = new RenderTexture(Screen.width, Screen.height, 16);
            eyeViewCamera.targetTexture = screenTexture;
            RenderTexture.active = screenTexture;
            eyeViewCamera.Render();
            Texture2D renderedTexture = new Texture2D(Screen.width, Screen.height);
            renderedTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            RenderTexture.active = null;
            return renderedTexture.EncodeToPNG();
        }*/

        private void OnDestroy()
        {
            EventManager.MusicStartEvent -= ResetScorePoint;

            EventManager.MusicStartEvent -= StartRecording;
            EventManager.MusicResetEvent -= StopRecording;
        }

        #endregion

        private static void CaptureDrumHit(ActorType actor, InstrumentType type, XRNode node)
        {
            // Redundant function?
        }

        public void AddScorePoint(int point)
        {
            ScorePoint += point;

            EventManager.InvokePlayerScoreUpdateEvent(point);
        }

        public void ResetScorePoint()
        {
            ScorePoint = 0;
            SynchronousRate = 0;

            syncRateController.Clear();

            EventManager.InvokePlayerScoreUpdateEvent(0);
        }

        private void OnSyncRateChange(float rate, HitUnit latestUnit)
        {
            SynchronousRate = rate;

            if (SaveData.Instance.preferenceData.recordPerUnit)
            {
                // "UnitCenterTime,Instrument,SyncRate" : "HitTime,HitActor,Instrument"
                /*var recordPerUnit =
                    "{(latestUnit.startTime + latestUnit.endTime) / 2f},{latestUnit.instrumentType},{latestUnit.syncRate}";*/

                var lastErrorRate = errorRateController.GetLastErrorRate();

                lookCoords = eyeTracker.GetCurrentFocusCoordinates();

                dataLogger.Enqueue(
                    $"{(latestUnit.startTime + latestUnit.endTime) / 2f},{latestUnit.instrumentType},{latestUnit.syncRate},{lastErrorRate:F7},{eyeTracker.GetCurrentFocusItem()},({lookCoords.x}/{lookCoords.y}/{lookCoords.z})");

                //dataLogger.EnqueueScreenshot(SaveCameraView(), latestUnit.startTime + latestUnit.endTime);

            }
            else
            {
                dataLogger.Enqueue($"{latestUnit.ownerHitTime},{latestUnit.owner},{latestUnit.instrumentType}");
                foreach (var guestHitTime in latestUnit.guestHitTimes)
                    dataLogger.Enqueue($"{guestHitTime},{latestUnit.guest},{latestUnit.instrumentType}");
            }

            EventManager.InvokeSyncRateChangeEvent(rate);
            EventManager.InvokeErrorRateChanged(errorRateController.GetLastErrorRate());
        }

        private void StartRecording()
        {
            if (SaveData.Instance.preferenceData.enableRecording)
            {
                var label = SaveData.Instance.preferenceData.recordPerUnit
                    ? DataHeader
                    : "HitTime,HitActor,Instrument";

                dataLogger.StartRecording(label);
            }
        }

        private void StopRecording()
        {
            if (SaveData.Instance.preferenceData.enableRecording)
                dataLogger.StopRecording();

            ErrorRateController.Instance.ClearErrorRates();
        }
    }
}