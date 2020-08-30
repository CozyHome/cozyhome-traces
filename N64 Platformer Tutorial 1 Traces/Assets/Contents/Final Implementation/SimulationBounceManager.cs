using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace com.chs.final
{
    public class SimulationBounceManager : MonoBehaviour
    {
        [Header("Simulation Settings")]
        [SerializeField] bool SimulateOnPlay = true;

        CharacterBouncer[] CapsuleBouncers;
        int BouncerCount;
        bool _simulate = false;

        public void StartSimulation() { if (!_simulate) { _simulate = true; } }
        public void StopSimulation() { if (_simulate) { _simulate = false; } }
        public void ResetSimulation() { for (int i = BouncerCount - 1; i >= 0; i--) { CapsuleBouncers[i].ResetBouncer(); } }

        void Start()
        {
            //very bad code

            //find all references:
            CharacterBouncer[] tmp = FindObjectsOfType<CharacterBouncer>();
            Stack<CharacterBouncer> activeBouncers = new Stack<CharacterBouncer>();

            //filter out inactive ones:
            for (int i = 0; i < tmp.Length; i++)
                if (tmp[i].isActiveAndEnabled)
                    activeBouncers.Push(tmp[i]);

            BouncerCount = activeBouncers.Count;

            CapsuleBouncers = new CharacterBouncer[BouncerCount];
            for (int i = 0; i < CapsuleBouncers.Length; i++)
                CapsuleBouncers[i] = activeBouncers.Pop();

            tmp = null;
            activeBouncers.Clear();

            // do not emulate this trashy ass code

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