using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InfinityPBR
{
    public class Emissive : MonoBehaviour
    {
        [Header("Required")] 
        public SkinnedMeshRenderer[] renderers;
        public Color color = Color.white;
        
        [Header("Options")] 
        public bool pulse = true;
        public float fadeLength = 1f;
        public float minPower = 0f;
        public float maxPower = 1f;
        public Color[] colors;

        private float _startPower = 1f;
        private float _currentPower = 1f;
        private float _desiredPower = 1f;
        private float _timer = 0f;

        public void SetColor(Color value) => color = value;
        public void SetColor(int index) => color = colors[index];
        public void SetMaxPower(float value) => maxPower = value;
        public void SetLength(float value) => fadeLength = value;
        
        /// <summary>
        /// Sets the specific value on all mateirals
        /// </summary>
        /// <param name="value"></param>
        public void SetEmissive(float value)
        {
            Color finalColor = color * Mathf.LinearToGammaSpace (value);
 
            foreach(var renderer in renderers)
                renderer.material.SetColor ("_EmissionColor", finalColor);
        }
        
        /// <summary>
        /// Sets the desired value on all materials, and resets the timer. start value is optional, if it is < 0 or not assigned,
        ///  it will set to the _currentPower value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="start"></param>
        public void FadeTo(float value, float start = -1)
        {
            _startPower = start < 0 ? _currentPower : start;

            _desiredPower = value;
            _timer = 0;
        }
        
        // Start is called before the first frame update
        void Start()
        {
            if (renderers.Length == 0)
                return;
            
            _startPower = maxPower;
            SetEmissive(_currentPower);
        }
        
        // Update is called once per frame
        void Update()
        {
            if (renderers.Length == 0) Destroy(this); // Remove this script if the material isn't assigned

            if (!pulse) return;
            
            FadeEmissive();
        }

        private void FadeEmissive()
        {
            CheckForSwitch();
            
            _currentPower = Mathf.Lerp(_startPower, _desiredPower, _timer / fadeLength);
            SetEmissive(_currentPower);
            _timer += Time.deltaTime;
        }

        private void CheckForSwitch()
        {
            if (_timer < fadeLength)
                return;
            
            // Set new desired power
            FadeTo(_desiredPower <= minPower ? maxPower : minPower, _desiredPower <= minPower ? minPower : maxPower);
        }
    }
}

