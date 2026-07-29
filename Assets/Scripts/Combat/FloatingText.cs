using UnityEngine;

namespace DungeonOdyssey.Combat
{
    public enum FloatTextKind
    {
        Damage,
        Crit,
        Heal,
        Gold,
        Info
    }

    /// <summary>데미지·골드·회복 등 공통 플로팅 텍스트.</summary>
    public class FloatingText : MonoBehaviour
    {
        private TextMesh _text;
        private float _life;
        private float _maxLife;
        private Vector3 _velocity;
        private float _baseSize;

        public static void Spawn(Vector3 position, string message, FloatTextKind kind = FloatTextKind.Info)
        {
            var go = new GameObject("FloatingText");
            go.transform.position = position + Vector3.up * 0.65f;
            go.AddComponent<FloatingText>().Build(message, kind);
        }

        public static void Damage(Vector3 position, int amount, bool critical) =>
            Spawn(position, critical ? $"{amount}!" : amount.ToString(),
                critical ? FloatTextKind.Crit : FloatTextKind.Damage);

        public static void Gold(Vector3 position, int amount) =>
            Spawn(position, $"+{amount}G", FloatTextKind.Gold);

        public static void Heal(Vector3 position, int amount) =>
            Spawn(position, $"+{amount}", FloatTextKind.Heal);

        private void Build(string message, FloatTextKind kind)
        {
            _text = gameObject.AddComponent<TextMesh>();
            _text.text = message;
            _text.anchor = TextAnchor.MiddleCenter;
            _text.alignment = TextAlignment.Center;
            _text.fontStyle = FontStyle.Bold;
            _text.fontSize = kind == FloatTextKind.Crit ? 54 : 44;
            _text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                         ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            switch (kind)
            {
                case FloatTextKind.Crit:
                    _text.color = new Color(1f, 0.88f, 0.35f);
                    _baseSize = 0.15f;
                    _maxLife = 0.85f;
                    _velocity = new Vector3(Random.Range(-0.2f, 0.2f), 2f, Random.Range(-0.2f, 0.2f));
                    break;
                case FloatTextKind.Damage:
                    _text.color = new Color(1f, 0.55f, 0.45f);
                    _baseSize = 0.11f;
                    _maxLife = 0.65f;
                    _velocity = new Vector3(Random.Range(-0.25f, 0.25f), 1.5f, Random.Range(-0.25f, 0.25f));
                    break;
                case FloatTextKind.Heal:
                    _text.color = new Color(0.45f, 0.95f, 0.6f);
                    _baseSize = 0.12f;
                    _maxLife = 0.75f;
                    _velocity = new Vector3(0f, 1.4f, 0f);
                    break;
                case FloatTextKind.Gold:
                    _text.color = new Color(1f, 0.85f, 0.4f);
                    _baseSize = 0.11f;
                    _maxLife = 0.8f;
                    _velocity = new Vector3(Random.Range(-0.15f, 0.15f), 1.3f, Random.Range(-0.15f, 0.15f));
                    break;
                default:
                    _text.color = new Color(0.9f, 0.88f, 0.82f);
                    _baseSize = 0.1f;
                    _maxLife = 1.1f;
                    _velocity = new Vector3(0f, 1.1f, 0f);
                    break;
            }

            _text.characterSize = _baseSize * 0.45f;
            _life = _maxLife;
            var mr = GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sortingOrder = 85;
            }
        }

        private void Update()
        {
            _life -= Time.deltaTime;
            var age = 1f - Mathf.Clamp01(_life / _maxLife);
            transform.position += _velocity * Time.deltaTime;
            _velocity.y -= 2.8f * Time.deltaTime;

            if (_text != null)
            {
                var pop = age < 0.12f ? Mathf.Lerp(0.5f, 1.12f, age / 0.12f) : 1f;
                _text.characterSize = _baseSize * pop;
                var c = _text.color;
                c.a = Mathf.Clamp01(_life / (_maxLife * 0.4f));
                _text.color = c;
            }

            var cam = Camera.main;
            if (cam != null)
            {
                transform.rotation = cam.transform.rotation;
            }

            if (_life <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }
}
