using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace com.chs.final
{
    public class SimulationBounceManager : MonoBehaviour
    {
        [Header("Simulation Settings")]
        [SerializeField] bool SimulateOnPlay = true;

        CapsuleBounce[] CapsuleBouncers;
        int BouncerCount;
        bool _simulate = false;

        public void StartSimulation() { if (!_simulate) { _simulate = true; } }
        public void StopSimulation() { if (_simulate) { _simulate = false; } }
        public void ResetSimulation() { for (int i = BouncerCount - 1; i >= 0; i--) { CapsuleBouncers[i].ResetBouncer(); } }

        void Start()
        {
            CapsuleBouncers = FindObjectsOfType<CapsuleBounce>();
            BouncerCount = CapsuleBouncers.Length;
            CacheBouncerPositions();

            if (SimulateOnPlay)
            {
                StartSimulation();
            }
        }
        void FixedUpdate()
        {
            if (!_simulate)
                return;

            float _cachedFixedTimestep = Time.fixedDeltaTime;

            for (int i = BouncerCount - 1; i >= 0; i--)
                CapsuleBouncers[i].Simulate(_cachedFixedTimestep);
        }

        public void CacheBouncerPositions()
        {
            for (int i = BouncerCount - 1; i >= 0; i--)
                CapsuleBouncers[i].Initialize();
        }

    }
}