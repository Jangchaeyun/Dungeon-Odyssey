using System.Collections;
using UnityEngine;

namespace DungeonOdyssey.Art
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteAnimator : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float frameRate = 6f;

        private SpriteRenderer _renderer;
        private float _timer;
        private int _index;
        private bool _playing = true;
        private bool _wasMoving;
        private bool _locked;
        private Coroutine _oneShot;

        public bool IsLocked => _locked;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (_locked || !_playing || frames == null || frames.Length == 0)
            {
                return;
            }

            _timer += Time.deltaTime;
            if (_timer < 1f / Mathf.Max(0.1f, frameRate))
            {
                return;
            }

            _timer = 0f;
            _index = (_index + 1) % frames.Length;
            _renderer.sprite = frames[_index];
        }

        public void SetFrames(Sprite[] newFrames, float rate = 6f)
        {
            frames = newFrames;
            frameRate = rate;
            _index = 0;
            _timer = 0f;
            if (_renderer == null)
            {
                _renderer = GetComponent<SpriteRenderer>();
            }

            if (frames != null && frames.Length > 0 && _renderer != null)
            {
                _renderer.sprite = frames[0];
            }
        }

        public void SetMoving(bool moving, Sprite idle, Sprite[] walkFrames)
        {
            if (_locked)
            {
                return;
            }

            if (moving)
            {
                if (!_wasMoving)
                {
                    SetFrames(walkFrames, 9f);
                }

                _playing = true;
            }
            else
            {
                _playing = false;
                if (_renderer != null)
                {
                    _renderer.sprite = idle;
                }
            }

            _wasMoving = moving;
        }

        public void PlayOnce(Sprite[] oneshot, float rate, System.Action onComplete = null)
        {
            if (oneshot == null || oneshot.Length == 0)
            {
                return;
            }

            if (_oneShot != null)
            {
                StopCoroutine(_oneShot);
            }

            _oneShot = StartCoroutine(PlayOnceRoutine(oneshot, rate, onComplete));
        }

        private IEnumerator PlayOnceRoutine(Sprite[] oneshot, float rate, System.Action onComplete)
        {
            _locked = true;
            _playing = false;
            _wasMoving = false;
            var delay = 1f / Mathf.Max(0.1f, rate);

            for (var i = 0; i < oneshot.Length; i++)
            {
                if (_renderer != null && oneshot[i] != null)
                {
                    _renderer.sprite = oneshot[i];
                }

                yield return new WaitForSeconds(delay);
            }

            _locked = false;
            _oneShot = null;
            onComplete?.Invoke();
        }
    }
}
