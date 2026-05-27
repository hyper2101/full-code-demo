using System;
using System.Collections.Generic;
using UnityEngine;

public class SimulationWorld : MonoBehaviour
{
    public static SimulationWorld Instance { get; private set; }

    public class TickManager
    {
        private float _slowTickTimer = 0f;
        private const float SlowTickInterval = 1f;

        public event Action<float> OnTick;
        public event Action OnSlowTick;

        public void Update(float deltaTime)
        {
            OnTick?.Invoke(deltaTime);

            _slowTickTimer += deltaTime;
            if (_slowTickTimer >= SlowTickInterval)
            {
                _slowTickTimer -= SlowTickInterval;
                OnSlowTick?.Invoke();
            }
        }
    }

    public class EntityRegistry
    {
        private readonly List<IPrimaryRunEntity> _entities = new List<IPrimaryRunEntity>();

        public IReadOnlyList<IPrimaryRunEntity> RegisteredEntities => _entities;

        public void Register(IPrimaryRunEntity entity)
        {
            if (!_entities.Contains(entity))
            {
                _entities.Add(entity);
            }
        }

        public void Unregister(IPrimaryRunEntity entity)
        {
            _entities.Remove(entity);
        }
    }

    public TickManager Ticks { get; private set; }
    public EntityRegistry Entities { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Ticks = new TickManager();
        Entities = new EntityRegistry();
    }

    private void Update()
    {
        if (WorldManager.instance != null && !WorldManager.instance.GamePaused)
        {
            float scaledDeltaTime = Time.deltaTime * WorldManager.instance.TimeScale;
            Ticks.Update(scaledDeltaTime);
        }
    }
}
