using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SFDC.Models.MetaDataComponents
{
    public class ElementFactory
    {
        Dictionary<string, Type> _elements;

        public ElementFactory()
        {
            LoadTypesICanReturn();
        }

        public IElement CreateInstance(string name)
        {
            Type t = GetTypeToCreate(name);

            if (t == null)
                return null;

            return Activator.CreateInstance(t) as IElement;
        }

        Type GetTypeToCreate(string name)
        {
            foreach (var item in _elements)
            {
                if (item.Key == name + "Element")
                {
                    return _elements[item.Key];
                }
            }

            return null;
        }

        void LoadTypesICanReturn()
        {
            _elements = new Dictionary<string, Type>();

            Type[] typesInThisAssembly = Assembly.GetExecutingAssembly().GetTypes();

            foreach (Type type in typesInThisAssembly)
            {
                if (type.GetInterface(typeof(IElement).ToString()) != null)
                {
                    _elements.Add(type.Name, type);
                }
            }
        }
    }
}
