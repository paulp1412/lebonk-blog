using System;
using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;
using Mole.Controller;
using Mole.Scripts.Util;
using UnityAtoms.BaseAtoms;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

namespace Mole.LevelElements
{
    [RequireComponent(typeof(Rigidbody))]
    public class IceSpike : MonoBehaviour
    {
        [SerializeField]
        private float _strength = 10f;
        [SerializeField]
        private float _impactRadius = 1f;
        [SerializeField]
        private int _minScale = 1;
        [SerializeField]
        private int _maxScale = 3;
        [SerializeField]
        private LayerMask _layerMask = 0;
        [SerializeField]
        private bool _useCustomGravity = false;
        [SerializeField]
        [ShowIf("_useCustomGravity")]
        private float _customGravity = 9.81f;
        [SerializeField]
        private float _disappearDuration = 5f;
        [SerializeField]
        private bool _showGizmo = false;
        [SerializeField]
        private ParticleSystem _snowFlakesEffect;
        [SerializeField]
        private ParticleSystem _snowExplosionEffect;
        [SerializeField]
        private GameObject _model;
        [SerializeField]
        private GameObject[] _shards;
        [SerializeField]
        private AudioClip _impactSound;
        [SerializeField]
        private AudioClip _explosionSound;
        [SerializeField]
        private FloatReference _masterVolume;
        [SerializeField]
        private FloatReference _effectVolume;

        private Rigidbody _rb;
        private Vector3 _velocity;
        private bool _hitGround;
        private bool _preHitGround;
        private float _stuckInGroundY;
        private float _multiplier;
        private Vector3 _posOnGround;
        private Vector3 _posOnStart;
        private DecalProjector _projector = null;
        private AudioSource _audioSource;
        private Material _decalMaterialInstance;
        private static readonly int _emission = Shader.PropertyToID("_Emission");
        private static readonly int _alpha = Shader.PropertyToID("_Alpha");
        private float _fallTimer;
        private Vector3 _projectorInitialSize;
        
        private const float IMPACT_SOUND_VOLUME_MULTIPLIER = 1f;
        private const float EXPLOSION_SOUND_VOLUME_MULTIPLIER = 0.25f;
        private const float PRE_HIT_GROUND_OFFSET = 0.4f;
        private const float FALL_DELAY = 2.5f;
        private const float DECAL_SCALE_ANIMATION_SPEED = 3.3f;
        private const float DECAL_SCALE_ANIMATION_AMPLITUDE = 0.125f;
        private const float DECAL_EMISSION_ANIMATION_FREQUENCY_INCREASE = 4f;
        private const float DECAL_EMISSION_ANIMATION_FALL_DELAY_INFLUENCE = 5f;
        
        private void Awake()
        {
            transform.localScale = transform.localScale * (1 + 0.5f * Random.Range(_minScale, _maxScale + 1)); // 1.5f - 2.5f
            _rb = GetComponent<Rigidbody>();
            _projector = GetComponentInChildren<DecalProjector>();

            _decalMaterialInstance = new Material(_projector.material);
            _projector.material = _decalMaterialInstance;

            _decalMaterialInstance.SetFloat(_emission, 0);
            _decalMaterialInstance.SetFloat(_alpha, 0);
            
            _snowExplosionEffect.gameObject.SetActive(false);
            
            Debug.Assert(_shards != null);

            _audioSource = new GameObject("Audio Source").AddComponent<AudioSource>();
            _audioSource.transform.SetParent(transform);
        }

        // Start is called before the first frame update
        void Start()
        {
            _rb.useGravity = !_useCustomGravity;
            _hitGround = false;
            _stuckInGroundY = -transform.localScale.y * 2f / 3f;
            _multiplier = transform.localScale.x;
            _posOnGround = new Vector3(transform.position.x, 0f, transform.position.z);
            _posOnStart = transform.position;
            if(_projector)
            {
                _projector.size = new Vector3(transform.localScale.x, transform.localScale.z, transform.position.y);
                _projectorInitialSize = _projector.size;
                _projector.pivot = new Vector3(0f, 0f, _projector.size.z / 2);
            }
            
            _model.transform.rotation *= Quaternion.Euler(0, Random.Range(0, 360), 0);
        }

        // Update is called once per frame
        void Update()
        {
            if (_hitGround)
                return;

            float projectorScaleOffset = Mathf.Sin(Time.time * DECAL_SCALE_ANIMATION_SPEED) * DECAL_SCALE_ANIMATION_AMPLITUDE;
            _projector.size = new Vector3(_projectorInitialSize.x + projectorScaleOffset, _projectorInitialSize.y + projectorScaleOffset, _projectorInitialSize.z);
            
            float iceSpikeFallPercentage = MathHelper.Map(transform.position.y + (FALL_DELAY - _fallTimer) * DECAL_EMISSION_ANIMATION_FALL_DELAY_INFLUENCE, _posOnGround.y, _posOnStart.y + FALL_DELAY * DECAL_EMISSION_ANIMATION_FALL_DELAY_INFLUENCE, 1, 0);
            float emissionTime = iceSpikeFallPercentage * DECAL_EMISSION_ANIMATION_FREQUENCY_INCREASE;
            _decalMaterialInstance.SetFloat(_emission, (Mathf.Cos(emissionTime * emissionTime) - 1) / -2);

            if (_fallTimer < FALL_DELAY)
            {
                _fallTimer += Time.deltaTime;
                _decalMaterialInstance.SetFloat(_alpha, MathHelper.Map(_fallTimer, 0, FALL_DELAY, 0, 1));
                return;
            }

            if (transform.position.y <= _stuckInGroundY + PRE_HIT_GROUND_OFFSET && !_preHitGround)
            {
                float volume = _masterVolume.Value * _effectVolume.Value;
                _audioSource.pitch = Random.Range(0.9f, 1.1f);
                _audioSource.PlayOneShot(_impactSound, volume * IMPACT_SOUND_VOLUME_MULTIPLIER);
                
                _preHitGround = true;
            }
            
            if (transform.position.y <= _stuckInGroundY)
            {
                _hitGround = true;
                OnHitGround();
                return;
            }
            
            if (_useCustomGravity && !_hitGround)
            {
                _velocity += Vector3.down * _customGravity * Time.deltaTime;
                transform.position += _velocity * Time.deltaTime;
            }
        }

        private void OnHitGround()
        {
            StartCoroutine(DisappearAfterSeconds());
            var colliders = Physics.OverlapSphere(_posOnGround, _impactRadius * _multiplier, _layerMask);
            foreach(var c in colliders)
            {
                var forceReceiver = c.gameObject.GetComponentInChildren<ForceReceiver>();
                if(forceReceiver)
                {
                    var playerPos = forceReceiver.Controller.transform.position;
                    var dir = playerPos - transform.position;
                    dir.y = 0f;
                    dir.Normalize();
                    var forceToApply = dir * _strength * _multiplier;
                    forceReceiver.AddForce(forceToApply);
                }
            }
        }

        private IEnumerator DisappearAfterSeconds()
        {
            yield return new WaitForSeconds(_disappearDuration);

            var shardsParent = new GameObject("Shards Temporary Parent");
            Destroy(shardsParent, 15);
            
            _audioSource.transform.SetParent(shardsParent.transform);
            
            float volume = _masterVolume.Value * _effectVolume.Value;
            _audioSource.pitch = Random.Range(0.8f, 1.2f);
            _audioSource.PlayOneShot(_explosionSound, volume * EXPLOSION_SOUND_VOLUME_MULTIPLIER);

            _snowFlakesEffect.transform.SetParent(shardsParent.transform, true);
            _snowFlakesEffect.Stop();
            
            _snowExplosionEffect.transform.SetParent(shardsParent.transform, true);
            _snowExplosionEffect.gameObject.SetActive(true);
            
            foreach (GameObject shard in _shards)
            {
                if (shard.transform.position.y <= _posOnGround.y) // Exclude shards that are already under the ground
                    continue;

                shard.AddComponent<IceShard>().Initialize(shardsParent.transform, _posOnGround + Vector3.up * 5);
            }

            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            Destroy(_decalMaterialInstance);
        }

        private void OnDrawGizmos()
        {
            if (!_showGizmo)
                return;

            if (!_hitGround)
                Gizmos.color = Color.yellow;
            else
                Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_posOnGround, _impactRadius * _multiplier);
        }
    }
}
