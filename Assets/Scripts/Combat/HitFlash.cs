using System.Collections;
using UnityEngine;

namespace DungeonOdyssey.Combat
{
    public class HitFlash : MonoBehaviour
    {
        public static void Play(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            var flash = target.GetComponent<HitFlash>();
            if (flash == null)
            {
                flash = target.AddComponent<HitFlash>();
            }

            flash.Trigger();
        }

        private SpriteRenderer _renderer;
        private Color _original;
        private Coroutine _routine;

        private void Awake()
        {
            _renderer = GetComponentInChildren<SpriteRenderer>();
            if (_renderer != null)
            {
                _original = _renderer.color;
            }
        }

        public void Trigger()
        {
            if (_renderer == null)
            {
                _renderer = GetComponentInChildren<SpriteRenderer>();
                if (_renderer == null)
                {
                    return;
                }

                _original = _renderer.color;
            }

            if (_routine != null)
            {
                StopCoroutine(_routine);
            }

            _routine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            _renderer.color = Color.white;
            yield return new WaitForSeconds(0.07f);
            _renderer.color = _original;
            _routine = null;
        }
    }
}
