using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    internal class RunContext
    {
        public RunConfig config { get; }
        public Dictionary<string, Class> types { get; }

        public HybInstance _this { get; set; }

        private DateTime startsAt;

        public RunContext(RunConfig config)
        {
            this.config = config;

            this.types = new Dictionary<string, Class>();
        }
        public void Reset()
        {
            startsAt = DateTime.Now;
        }

        public bool IsExpird() => (DateTime.Now - startsAt).TotalMilliseconds >= config.timeout;
    }
}
