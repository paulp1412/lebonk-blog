using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Mole.LevelElements
{
    public class IceSpikeSpawner : ThirdPartyEvent
    {
        [System.Serializable]
        private struct SpawnArea
        {
            public Transform TopLeft;
            public float Width;
            public float Height;
        }

        #region Config

        [SerializeField]
        private GameObject _prefab;
        [SerializeField]
        private SpawnArea _spawnArea;
        [SerializeField]
        private List<SpawnArea> _excludeAreas = new();
        [FoldoutGroup("Spawn Count")]
        [SerializeField]
        private int _minSpawnCount = 2;
        [FoldoutGroup("Spawn Count")]
        [SerializeField]
        private int _maxSpawnCountWithoutAdditional = 3;
        [FoldoutGroup("Spawn Count")]
        [SerializeField]
        private int _maxAdditionalCount = 7;
        [FoldoutGroup("Spawn Count")]
        [SerializeField]
        private float _additionalSpawnInterval = 10f;
        [FoldoutGroup("Spawn Delay")]
        [SerializeField]
        private float _minDelayBetweenSameSpawns = 0.15f;
        [FoldoutGroup("Spawn Delay")]
        [SerializeField]
        private float _maxDelayBetweenSameSpawns = 0.5f;

        #endregion

        protected override void OnIntervalComplete()
        {
            int randomCount = Random.Range(_minSpawnCount, _maxSpawnCountWithoutAdditional + 1);
            int additionalCount = Mathf.Min ((int) (_timer / _additionalSpawnInterval), _maxAdditionalCount);
            float delay = 0f;

            for(int i = 0; i < randomCount + additionalCount; i++)
            {
                if(i > 0)
                    delay += Random.Range(_minDelayBetweenSameSpawns, _maxDelayBetweenSameSpawns);
                StartCoroutine(SpawnWithDelay(delay));
            }
        }

        private IEnumerator SpawnWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
                Instantiate(_prefab, GetRandomSpawnPos(), Quaternion.identity);
        }

        private Vector3 GetRandomSpawnPos()
        {
            float posX;
            float posZ;
            Vector3 tl = _spawnArea.TopLeft.position;
            do
            {
                posX = tl.x + Random.Range(0f, _spawnArea.Width);
                posZ = tl.z - Random.Range(0f, _spawnArea.Height);
            } while (IsInvalidPosition(posX, posZ));

            return new Vector3(posX, tl.y, posZ);
        }

        private bool IsInvalidPosition(float x, float z)
        {
            Vector2 pos = new Vector2(x, z);
            foreach (var area in _excludeAreas)
                if (AreaToRect(area).Contains(pos))
                    return true;
            return false;
        }

        private Rect AreaToRect(SpawnArea area)
        {
            return new Rect(new Vector2(area.TopLeft.position.x, area.TopLeft.position.z - area.Height), new Vector2(area.Width, area.Height));
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            DrawArea(_spawnArea);
            Gizmos.color = Color.red;
            foreach (var area in _excludeAreas)
                DrawArea(area);
        }

        private void DrawArea(SpawnArea area)
        {
            var w = area.Width;
            var h = area.Height;
            var tl = area.TopLeft.position;
            var tr = tl + new Vector3(w, 0f, 0f);
            var bl = tl - new Vector3(0f, 0f, h);
            var br = tr - new Vector3(0f, 0f, h);
            Gizmos.DrawLine(tl, tr);
            Gizmos.DrawLine(tl, bl);
            Gizmos.DrawLine(bl, br);
            Gizmos.DrawLine(tr, br);
        }
    }
}