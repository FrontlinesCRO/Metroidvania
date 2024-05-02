using System;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public readonly struct TagData
    {
        public readonly string Name;
        public readonly int Hash;
        public readonly TagType Type;
        public readonly float Time;
        public readonly bool IsActive;

        public TagData(string name, int hash, TagType type, float time, bool isActive)
        {
            Name = name;
            Hash = hash;
            Type = type;
            Time = time;
            IsActive = isActive;
        }
    }

    public enum TagType
    {
        Moving,
        Grounded,
        Falling,
        Jumping,
        Sprinting,
        Aiming,
        Dashing,
        Gliding,
        Driving,
        Fighting,
        Flying,
        Charging,
        Laser,
        SuperSaiyan,
        Ultimate,
        Invulnerable,
        Poisoned,
        Burning,
        Bleeding,
        Drunk,
    }

    public class GameplayTags
    {
        private const int INITIAL_CONTAINER_SIZE = 30;

        private class Tag
        {
            private bool _isActive = false;
            private Stopwatch _stopwatch = new Stopwatch();

            public string Name { get; private set; }
            public int Hash { get; private set; }
            public TagType Type { get; private set; }
            public float Time => (float)_stopwatch.Elapsed.TotalSeconds;
            public bool IsActive
            {
                get => _isActive;
                private set
                {
                    if (value == _isActive) return;

                    _isActive = value;

                    TagData data;
                    if (value)
                    {
                        data = new TagData(Name, Hash, Type, Time, value);
                        Began?.Invoke(data);
                    }
                    else
                    {
                        data = new TagData(Name, Hash, Type, Time, value);
                        Ended?.Invoke(data);
                    }

                    data = new TagData(Name, Hash, Type, Time, value);
                    Changed?.Invoke(data);

                    _stopwatch.Restart();
                }
            }

            public event Action<TagData> Began;
            public event Action<TagData> Ended;
            public event Action<TagData> Changed;

            public Tag(TagType statusType)
            {
                Name = statusType.ToString();
                Hash = Animator.StringToHash(Name);
                Type = statusType;
                Began = null;
                Ended = null;
                Changed = null;
            }

            public void Reset()
            {
                if (IsActive == false)
                {
                    _stopwatch.Restart();
                    return;
                }

                IsActive = false;
            }

            public void Dispose()
            {
                IsActive = false;

                Began = null;
                Ended = null;
                Changed = null;

                _stopwatch.Stop();
            }

            public void SetTag(bool active)
            {
                IsActive = active;
            }

            public override string ToString()
            {
                return string.IsNullOrEmpty(Name) ? "Status Name Not Set" : Name;
            }

            public static int GetHash(string statusName)
            {
                return Animator.StringToHash(statusName);
            }

            public static implicit operator bool(Tag container) => container.IsActive;
        }

        private Dictionary<TagType, Tag> _tags;

        public event Action<TagData> TagChanged;

        public GameplayTags()
        {
            _tags = new Dictionary<TagType, Tag>(INITIAL_CONTAINER_SIZE);
        }

        private Tag TryCreateTagContainer(TagType tagType)
        {
            if (!_tags.ContainsKey(tagType))
            {
                var container = new Tag(tagType);

                container.Changed += InvokeTagChanged;

                _tags.Add(tagType, container);

                return container;
            }

            return _tags[tagType];
        }

        private void InvokeTagChanged(TagData data)
        {
            TagChanged?.Invoke(data);
        }

        public void Reset()
        {
            foreach (var tag in _tags.Values)
            {
                tag.Reset();
            }
        }

        public void Dispose()
        {
            foreach (var tag in _tags.Values)
            {
                tag.Dispose();
            }

            _tags.Clear();
        }

        public void AddTagEnabledCallback(TagType tagType, Action<TagData> handler)
        {
            var status = TryCreateTagContainer(tagType);

            status.Began += handler;
        }

        public void RemoveTagEnabledCallback(TagType tagType, Action<TagData> handler)
        {
            if (!_tags.ContainsKey(tagType)) return;

            _tags[tagType].Began -= handler;
        }

        public void AddTagDisabledCallback(TagType tagType, Action<TagData> handler)
        {
            var status = TryCreateTagContainer(tagType);

            status.Ended += handler;
        }

        public void RemoveTagDisabledCallback(TagType tagType, Action<TagData> handler)
        {
            if (!_tags.ContainsKey(tagType)) return;

            _tags[tagType].Ended -= handler;
        }

        public void AddTagChangedCallback(TagType tagType, Action<TagData> handler)
        {
            var status = TryCreateTagContainer(tagType);

            status.Changed += handler;
        }

        public void RemoveTagChangedCallback(TagType tagType, Action<TagData> handler)
        {
            if (!_tags.ContainsKey(tagType)) return;

            _tags[tagType].Changed -= handler;
        }

        public TagData GetTag(TagType tagType)
        {
            if (!_tags.ContainsKey(tagType))
                return default;

            var tag = _tags[tagType];
            var data = new TagData(tag.Name, tag.Hash, tag.Type, tag.Time, tag.IsActive);

            return data;
        }

        public void SetTag(TagType tagType, bool active)
        {
            var tag = TryCreateTagContainer(tagType);

            tag.SetTag(active);
        }

        public bool IsTagActive(TagType tagType)
        {
            if (_tags.ContainsKey(tagType))
            {
                var tag = _tags[tagType];
                return tag != null && tag.IsActive;
            }
            else
                return false;
        }

        public int GetTagAsFlag()
        {
            int flag = 0;
            foreach(var tag in _tags)
            {
                if (tag.Value.IsActive)
                    flag ^= (1 << (int)tag.Key); 
            }

            return flag;
        }
    }
}
