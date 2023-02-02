﻿using System.Collections.Generic;
using System;
using SlugBase.Features;
using System.IO;
using Menu;
using System.Linq;
using UnityEngine;

namespace SlugBase
{
    /// <summary>
    /// A character added by SlugBase.
    /// </summary>
    public class SlugBaseCharacter
    {
        /// <summary>
        /// Stores all registered <see cref="SlugBaseCharacter"/>s.
        /// </summary>
        public static JsonRegistry<SlugcatStats.Name, SlugBaseCharacter> Registry { get; } = new((key, json) => new(key, json));

        /// <summary>
        /// Gets a <see cref="SlugBaseCharacter"/> by <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The <see cref="SlugcatStats.Name"/> to search for.</param>
        /// <param name="character">The <see cref="SlugBaseCharacter"/> with the given <paramref name="name"/>, or <c>null</c> if it was not found.</param>
        /// <returns><c>true</c> if the <see cref="SlugBaseCharacter"/> was found, <c>false</c> otherwise.</returns>
        public static bool TryGet(SlugcatStats.Name name, out SlugBaseCharacter character) => Registry.TryGet(name, out character);

        /// <summary>
        /// Gets a <see cref="SlugBaseCharacter"/> by <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The <see cref="SlugcatStats.Name"/> to search for.</param>
        /// <returns>The <see cref="SlugBaseCharacter"/>, or <c>null</c> if it was not found.</returns>
        public static SlugBaseCharacter Get(SlugcatStats.Name name) => Registry.GetOrDefault(name);

        /// <summary>
        /// This character's unique name.
        /// </summary>
        public SlugcatStats.Name Name { get; }

        /// <summary>
        /// The displayed name of this character, such as "The Survivor", "The Monk", or "The Hunter".
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// A description of this character that appears on the select menu.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Settings, abilities, or other <see cref="Feature"/>s of this character.
        /// </summary>
        public FeatureList Features { get; } = new();

        private SlugBaseCharacter(SlugcatStats.Name name, JsonObject json)
        {
            Name = name;

            DisplayName = json.GetString("name");
            Description = json.GetString("description");

            Features.Clear();
            if (json.TryGet("features")?.AsObject() is JsonObject obj)
                Features.AddMany(obj);
        }

        /// <summary>
        /// Stores the <see cref="Feature"/>s of a <see cref="SlugBaseCharacter"/>.
        /// </summary>
        public class FeatureList
        {
            private readonly Dictionary<Feature, object> _features = new();

            /// <summary>
            /// Get the value of a <see cref="Feature{T}"/>.
            /// </summary>
            /// <typeparam name="T">The <see cref="Feature{T}"/>'s data type.</typeparam>
            /// <param name="feature">The feature to get data from.</param>
            /// <param name="value">The feature's data, or <typeparamref name="T"/>'s default value if it was not found.</param>
            /// <returns><c>true</c> if the feature was found, <c>false</c> otherwise.</returns>
            public bool TryGet<T>(Feature<T> feature, out T value)
            {
                if (_features.TryGetValue(feature, out object outObj))
                {
                    value = (T)outObj;
                    return true;
                }
                else
                {
                    value = default;
                    return false;
                }
            }

            /// <summary>
            /// Check this list for a <see cref="Feature"/>.
            /// </summary>
            /// <param name="feature">The <see cref="Feature"/> to check for.</param>
            /// <returns><c>true</c> if <paramref name="feature"/> was found, <c>false</c> otherwise.</returns>
            public bool Contains(Feature feature) => _features.ContainsKey(feature);

            internal void AddMany(JsonObject json)
            {
                foreach (var pair in json)
                {
                    if (FeatureManager.TryGetFeature(pair.Key, out Feature feature))
                        _features.Add(feature, feature.Create(pair.Value));
                    else
                        throw new JsonException($"Couldn't find feature: {pair.Key}!", json);
                }
            }

            internal void Clear()
            {
                _features.Clear();
            }
        }
    }
}
