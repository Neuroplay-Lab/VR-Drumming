using System.Collections.Generic;
using _Project.Scripts.Data;
using _Project.Scripts.Systems;
using DrumRhythmGame.Data;
using DrumRhythmGame.Systems;
using UnityEngine;
using UnityEngine.XR;

namespace _Project.Scripts.Field
{
    [DefaultExecutionOrder(2)]
    public class SpreadPromptController : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private SpreadPrompt[] notes;
        [SerializeField] private float duration;
        [SerializeField] private MusicSequence sequence;

        #endregion

        private readonly InstrumentType[] _instrumentTypes =
        {
            InstrumentType.CrashCymbal,
            InstrumentType.LeftHighTom,
            InstrumentType.RightMiddleTom,
            InstrumentType.SnareDrum
        };

        private readonly Dictionary<InstrumentType, SpreadPrompt> _notes =
            new Dictionary<InstrumentType, SpreadPrompt>();

        private readonly Dictionary<HitStatus, int> _points = new Dictionary<HitStatus, int>
        {
            {HitStatus.None, 0},
            {HitStatus.Bad, 1},
            {HitStatus.Good, 5},
            {HitStatus.Perfect, 10}
        };

        private int _beatCounter;
        private bool _loopScore;

        private MusicScore _musicScore;
        private float _noteMarginTime;

        #region Event Functions

        private void OnEnable()
        {
            EventManager.DrumHitEvent += OnDrumHit;
            EventManager.MusicSettingChangeEvent += UpdateScore;
            EventManager.MusicResetEvent += ResetBeatCounter;
        }

        private void OnDisable()
        {
            EventManager.DrumHitEvent -= OnDrumHit;
            EventManager.MusicSettingChangeEvent -= UpdateScore;
            EventManager.MusicResetEvent -= ResetBeatCounter;
        }

        #endregion

        /// <summary>
        ///     Updates the Spread Prompt's score based on the passed in MusicSetting.
        /// </summary>
        /// <param name="setting">New new MusicSetting to use</param>
        private void UpdateScore(MusicSetting setting)
        {
            Debug.Log("UpdateScore - SpreadPromptController\n");

            // TODO: This doesn't correctly clear the score
            if (_notes != null)
            {
                foreach (var note in notes)
                    note.OnSpreadEnd -= OnNoteEnd;
                _notes.Clear();
            }

            _musicScore = MusicScoreLoader.Load(setting.scoreAsset, setting.instrumentCount);
            _noteMarginTime = setting.noteMarginTime;
            _loopScore = setting.loopScore;

            foreach (var note in notes)
            {
                _notes?.Add(note.instrumentType, note);
                note.OnSpreadEnd += OnNoteEnd;
            }

            MusicSequence.Instance.SetTrigger(-duration, OnTime);
        }

        private void ResetBeatCounter()
        {
            _beatCounter = 0;
        }

        private void OnDrumHit(ActorType actor, InstrumentType type, XRNode node)
        {
            if (actor != ActorType.Player) return;

            if (_notes.TryGetValue(type, out var note))
            {
                var errorRate = note.CurrentErrorRate();
                ErrorRateController.Instance.AddErrorRate(errorRate);
                var status = note.Hit();
                GameData.Instance.AddScorePoint(_points[status]);
            }

            // NOTE: This will disable the prompt dispersing even if we hit wrong
            //_notes[type].CancelSpread();
        }

        private static void OnNoteEnd(InstrumentType type)
        {
            GameData.Instance.AddScorePoint(0);
        }

        /// <summary>
        ///     Called {reachTime} seconds before the beat
        /// </summary>
        /// <param name="delay"></param>
        private void OnTime(float delay)
        {
            if (_beatCounter >= _musicScore.rowCount)
            {
                // Do nothing if the music score has been ended and loop flag is false
                if (_loopScore == false) return;

                // Reset counter
                ResetBeatCounter();
            }

            for (var i = 0; i < _musicScore.columnCount; i++)
                if (_musicScore.score[_beatCounter, i])
                {
                    _notes[_instrumentTypes[i]].StartSpread(duration);

                    EventManager.InvokeMusicScoreNoteSetEvent(_instrumentTypes[i], _noteMarginTime);
                }

            _beatCounter++;
        }
    }
}