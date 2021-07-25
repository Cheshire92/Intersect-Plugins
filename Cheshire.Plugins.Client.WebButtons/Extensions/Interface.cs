using Intersect.Client.Framework.Gwen.Control;
using Intersect.Client.Interface;
using System.Collections.Generic;

namespace Cheshire.Plugins.Client.WebButtons.Extensions
{
    public static class Interface
    {
        /// <summary>
        /// Finds a control of the given name on the supplied Base.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Base FindByName (this List<Base> controls, string name)
        {
            var found = controls.Find(x => x.Name == name);
            if (found != null)
            {
                return found;
            }
            else
            {
                foreach (var child in controls)
                {
                    found = child.Children.FindByName(name);
                    if (found != null)
                    {
                        return found;
                    }
                }

                return null;
            }
        }
    }
}
