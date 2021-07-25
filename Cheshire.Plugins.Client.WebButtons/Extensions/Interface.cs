using Intersect.Client.Framework.Gwen.Control;
using Intersect.Client.Interface;

namespace Cheshire.Plugins.Client.WebButtons.Extensions
{
    public static class Interface
    {
        /// <summary>
        /// Finds a control of the given name on the supplied interface.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="activeInterface"></param>
        /// <returns></returns>
        public static Base FindControlOnInterface(this IMutableInterface activeInterface, string name)
        {
            var found = activeInterface.Children.Find(x => x.Name == name);
            if (found != null)
            {
                return found;
            }
            else
            {
                foreach (var child in activeInterface.Children)
                {
                    found = FindControlOnBase(child, name);
                    if (found != null)
                    {
                        return found;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Finds a control of the given name on the supplied Base.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Base FindControlOnBase(this Base control, string name)
        {
            var found = control.Children.Find(x => x.Name == name);
            if (found != null)
            {
                return found;
            }
            else
            {
                foreach (var child in control.Children)
                {
                    found = FindControlOnBase(child, name);
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
