using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

namespace Slowsharp
{
    /// <summary>
    /// Basic optimization based on imuutable syntax properties
    /// </summary>
    internal class OptNodeCache
    {
        private Dictionary<SyntaxNode, OptNodeBase> cache = new Dictionary<SyntaxNode, OptNodeBase>();

        public bool IsOptimized(SyntaxNode node)
            => cache.ContainsKey(node);

        public TOptNode GetOrCreate<TNode, TOptNode>(TNode node, Func<TOptNode> creator)
            where TNode : SyntaxNode
            where TOptNode : OptNodeBase
        {
            if (IsOptimized(node))
                return (TOptNode)cache[node];

            var optNode = creator();
            cache[node] = optNode;
            return optNode;
        }
    }
}
