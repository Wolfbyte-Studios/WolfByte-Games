using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace WaffleWare.Examples
{
    public class ObjectSpawner : MonoBehaviour
    {
        [Range(.01f, 10f)]
        public float spawnRadius = 5f;
        [Range(.01f, 10f)]
        public float spawnInterval = 1f;
        public bool spawnEnabled = true;

        private float timeUntilNextSpawn = 0f;

        void Update()
        {
            if (spawnEnabled)
            {
                timeUntilNextSpawn -= Time.deltaTime;
                if (timeUntilNextSpawn <= 0f)
                {
                    SpawnObject();
                    timeUntilNextSpawn = spawnInterval;
                }
            }
            else
                timeUntilNextSpawn = 0f;
        }

        void SpawnObject()
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = transform.position + Random.insideUnitSphere * spawnRadius;
            float sphereRadius = Random.Range(.1f, .35f);
            sphere.transform.localScale = Vector3.one * sphereRadius;
            sphere.transform.parent = this.gameObject.transform;
            sphere.AddComponent<Rigidbody>();

            Destroy(sphere, 7);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }
    }
}