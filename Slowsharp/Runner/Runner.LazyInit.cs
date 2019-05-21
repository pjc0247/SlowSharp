using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    public partial class Runner
    {
        private List<Action> initializers = new List<Action>();
        private bool initialized = false;

        private HashSet<Class> initializedTypes = new HashSet<Class>();

        public void AddLazyInitializer(Action callback)
        {
            initializers.Add(callback);
        }
        public void RunLazyInitializers()
        {
            if (initialized) return;
            initialized = true;

            foreach (var cb in initializers)
                cb();

            InitializeTypes();
        }

        /// <summary>
        /// Runs all static constructors.
        /// </summary>
        private void InitializeTypes()
        {
            var oldTypeResolver = Resolver;
            Resolver = new TypeResolverForStaticInitialization(this, Resolver);

            foreach (var init in staticInitializers)
            {
                RunStaticInitializer(init.Key);
            }

            Resolver = oldTypeResolver;
        }
        internal void RunStaticInitializer(Class klass)
        {
            if (initializedTypes.Add(klass) == false)
                return;

            SSMethodInfo methodInfo = null;
            if (staticInitializers.TryGetValue(klass, out methodInfo))
            {
                RunMethod(methodInfo, new HybInstance[] { });
            }
        }
    }
}
