using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityAtoms;
using UnityAtoms.BaseAtoms;

namespace Mole.LevelElements
{
    public abstract class ThirdPartyEvent : MonoBehaviour
    {
        protected enum StartMode
        {
            OnStart,
            OnEvent
        }
        
        protected enum IntervalMode
        {
            Fixed,
            Range,
            Curve
        }

        #region Config (Serialized Fields)

        [SerializeField]
        protected IntervalMode _intervalMode = IntervalMode.Fixed;
        [SerializeField]
        [ShowIf("_intervalMode", IntervalMode.Fixed)]
        protected float _intervalFixed = 1f;
        [SerializeField]
        [ShowIf("_intervalMode", IntervalMode.Range)]
        protected float _intervalRangeMin = 1f;
        [SerializeField]
        [ShowIf("_intervalMode", IntervalMode.Range)]
        protected float _intervalRangeMax = 2f;
        [SerializeField]
        [ShowIf("_intervalMode", IntervalMode.Curve)]
        protected AnimationCurve _intervalCurve = AnimationCurve.Constant(0, 1, 0.5f);
        [SerializeField]
        [ShowIf("_intervalMode", IntervalMode.Curve)]
        protected float _frequency = 10f;

        [SerializeField]
        protected StartMode _startMode = StartMode.OnStart;
        [SerializeField]
        [ShowIf("_startMode", StartMode.OnEvent)]
        protected AtomEventBase _startEvent = null;
        [SerializeField]
        protected AtomEventBase _stopEvent = null;

        #endregion

        private Coroutine _coroutine;
        private WaitForSeconds _interval;
        protected bool _running;
        protected float _timer;

        #region MonoBehaviour

        protected virtual void Start()
        {
            _running = false;
            _timer = 0f;

            if (_startMode == StartMode.OnStart)
                StartLoop();
        }

        protected virtual void Update()
        {
            if(_running)
                _timer += Time.deltaTime;
        }

        protected virtual void OnEnable()
        {
            if (_startMode == StartMode.OnEvent)
                _startEvent.Register(StartLoop);

            _stopEvent.Register(StopLoop);
        }

        protected virtual void OnDisable()
        {
            if (_startMode == StartMode.OnEvent)
                _startEvent.Unregister(StartLoop);

            _stopEvent.Unregister(StopLoop);
        }

        #endregion

        private void StartLoop()
        {
            _running = true;

            if(_intervalMode == IntervalMode.Fixed)
                _interval = new WaitForSeconds(_intervalFixed);
            StartInterval();
        }

        private void StopLoop()
        {
            _running = false;
            _timer = 0f;
            StopCoroutine(_coroutine);
        }

        private IEnumerator WaitInterval()
        {
            yield return _interval;
            OnIntervalComplete();
            StartInterval();
        }

        private void StartInterval()
        {
            if (_intervalMode == IntervalMode.Range)
            {
                float t = Random.Range(_intervalRangeMin, _intervalRangeMax);
                _interval = new WaitForSeconds(t);
            }
            else if(_intervalMode == IntervalMode.Curve)
            {
                float step = Random.Range(0f, 1f);
                float t = _intervalCurve.Evaluate(step) * _frequency;
                _interval = new WaitForSeconds(t);
            }

            _coroutine = StartCoroutine(WaitInterval());
        }

        protected abstract void OnIntervalComplete();
    }
}
