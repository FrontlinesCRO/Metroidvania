using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Player.Actions
{
    [Serializable]
    public class ActionContainer
    {
        [SerializeReference]
        private List<IAction> _actions;

        public IReadOnlyList<IAction> Actions => _actions;

        public void AddAction(IAction action)
        {
            if (action == null)
            {
                Debug.LogWarning("The give action is null - nothing to add.");
                return;
            }

            if (_actions == null)
                _actions = new List<IAction>();

            _actions.Add(action);
        }

        public void RemoveAction(IAction action)
        {
            if (action == null)
            {
                Debug.LogWarning("The give action is null - nothing to remove.");
                return;
            }

            if (_actions == null || _actions.Count <= 0)
                return;

            var index = -1;
            for (var i = 0; i < _actions.Count; i++)
            {
                if (_actions[i] == action)
                { 
                    index = i;
                    break;
                }    
            }

            if (index < 0)
                return;

            _actions.RemoveAt(index);
        }

        public bool TryGetActionOfType<T>(Type type, out T action) where T : IAction
        {
            action = default;

            if (_actions == null || _actions.Count <= 0)
                return false;

            for (var i = 0; i < _actions.Count; i++)
            {
                var temp = _actions[i];

                if (temp.GetType() == type)
                {
                    action = (T)temp;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetActionOfType<T>(out T action) where T : IAction
        {
            return TryGetActionOfType(typeof(T), out action);
        }
    }
}
