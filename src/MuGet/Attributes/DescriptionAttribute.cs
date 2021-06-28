using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Resources;

namespace MuGet.Attributes
{
    public sealed class DisplayAttribute : DescriptionAttribute
    {
        private readonly static Dictionary<Type, ResourceManager> Resources =
            new Dictionary<Type, ResourceManager>();

        private readonly Type _resourceType;
        private readonly string _resourceKey;

        public DisplayAttribute(string resourceKey, Type resourceType)
        {
            if (!Resources.ContainsKey(resourceType))
                Resources.Add(resourceType, new ResourceManager(resourceType));

            _resourceType = resourceType;
            _resourceKey = resourceKey;
        }

        public override string Description
        {
            get
            {
                var displayName = Resources[_resourceType].GetString(_resourceKey ?? string.Empty);
                return string.IsNullOrEmpty(displayName)
                    ? _resourceKey
                    : displayName;
            }
        }
    }
}