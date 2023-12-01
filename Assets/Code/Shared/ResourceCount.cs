using System;

namespace Shared
{
    [Serializable]
    public struct ResourceCount
    {
        public Resource resource;
        public int count;

        public ResourceCount(Resource resource, int count)
        {
            this.resource = resource;
            this.count = count;
        }
    }
}