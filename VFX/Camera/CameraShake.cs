/*  References */
/* https://roystan.net/articles/camera-shake.html */
/* https://www.youtube.com/watch?v=7noMEjDJ-_Q&t=12s */

using System;
using UnityEngine;

namespace PKTools.VFX
{
    public class CameraShake : MonoBehaviour
    {
        [SerializeField]
        private float smoothFactor = 2.0f;

        private float wave = 0.0f;

        private Transform cameraTransform;

        private float recoverySpeed = 0.0f;

        private float moveSpeed = 0.0f;

        private Vector3 maximumTranslation = Vector3.one * 0.5f;

        private Vector3 maximumAngular = Vector3.one * 2.0f;

        private Vector3 cameraPosition = Vector3.zero;

        private Vector3 cameraAngular = Vector3.zero;

        private float seed = float.NaN;

        private void Start() {
            seed = UnityEngine.Random.value;
            
            cameraTransform = transform;
        }

        void LateUpdate()
        {
            if (cameraTransform == null) {
                return;
            }

            float waveStr = Mathf.Pow(wave, smoothFactor);

            cameraTransform.localPosition = new Vector3(
                maximumTranslation.x * (Mathf.PerlinNoise(seed, Time.time * moveSpeed) * 2 - 1),
                maximumTranslation.y * (Mathf.PerlinNoise(seed + 1, Time.time * moveSpeed) * 2 - 1),
                maximumTranslation.z * (Mathf.PerlinNoise(seed + 2, Time.time * moveSpeed) * 2 - 1)
            ) * waveStr;

            cameraTransform.localRotation = Quaternion.Euler(new Vector3(
                maximumAngular.x * (Mathf.PerlinNoise(seed + 3, Time.time * moveSpeed) * 2 - 1),
                maximumAngular.y * (Mathf.PerlinNoise(seed + 4, Time.time * moveSpeed) * 2 - 1),
                maximumAngular.z * (Mathf.PerlinNoise(seed + 5, Time.time * moveSpeed) * 2 - 1)
            ) * waveStr);

            
            wave = Mathf.Clamp01(wave - recoverySpeed * Time.deltaTime);
        }

        public void Setup(Camera camera)
        {
            cameraTransform = camera.transform;
        }

        public void Shake(float strength = 0.6f, float frequency = 20.0f, float duration = 1.5f) 
        {   
            cameraTransform.localPosition = Vector3.zero;
            cameraTransform.localRotation = Quaternion.identity;

            recoverySpeed = 1.0f / duration;
            moveSpeed = frequency;
            wave = Mathf.Clamp01(wave + strength);
        }
    }
}
