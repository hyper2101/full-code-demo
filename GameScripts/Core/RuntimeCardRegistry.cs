using System.Collections.Generic;
using UnityEngine;

public class RuntimeCardRegistry : MonoBehaviour
{
    public static RuntimeCardRegistry Instance { get; private set; }

    private Dictionary<string, CardRuntimeState> _states = new Dictionary<string, CardRuntimeState>();
    private Dictionary<string, GameCard> _views = new Dictionary<string, GameCard>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RegisterCard(CardRuntimeState state, GameCard view)
    {
        if (state == null || string.IsNullOrEmpty(state.UniqueId)) return;
        
        _states[state.UniqueId] = state;
        
        if (view != null)
        {
            _views[state.UniqueId] = view;
        }
    }

    public void UnregisterCard(string uniqueId)
    {
        if (string.IsNullOrEmpty(uniqueId)) return;
        
        if (_states.ContainsKey(uniqueId))
        {
            _states.Remove(uniqueId);
        }
        if (_views.ContainsKey(uniqueId))
        {
            _views.Remove(uniqueId);
        }
    }

    public CardRuntimeState GetState(string uniqueId)
    {
        if (string.IsNullOrEmpty(uniqueId)) return null;
        _states.TryGetValue(uniqueId, out CardRuntimeState state);
        return state;
    }

    public GameCard GetView(string uniqueId)
    {
        if (string.IsNullOrEmpty(uniqueId)) return null;
        _views.TryGetValue(uniqueId, out GameCard view);
        return view;
    }

    public IReadOnlyCollection<CardRuntimeState> GetAllStates()
    {
        return _states.Values;
    }

    public void ClearAll()
    {
        _states.Clear();
        _views.Clear();
    }
}
