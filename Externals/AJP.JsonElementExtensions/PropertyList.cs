using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AJP
{
    public class PropertyList : IEnumerable<Property>
    {
        private readonly List<Property> _properties = new List<Property>();

        public Property GetByName(string name)
        {
            return _properties.FirstOrDefault(p => p.Name == name);
        }

        public void Remove(string name)
        {
            var propertyToRemove = _properties.First(p => p.Name == name);
            _properties.Remove(propertyToRemove);
        }

        public void Insert(string name, object value, int insertAt)
        {
            _properties.Insert(insertAt, new Property { Name = name, Value = value });
        }

        public void Add(string name, object value)
        {
            _properties.Add(new Property { Name = name, Value = value });
        }

        IEnumerator<Property> IEnumerable<Property>.GetEnumerator()
        {
            return _properties.GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return _properties.GetEnumerator();
        }
    }
}