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
        private Dictionary<SyntaxNode, OptNodeBase> Cache = new Dictionary<SyntaxNode, OptNodeBase>();

        public bool IsOptimized(SyntaxNode node)
            => Cache.ContainsKey(node);

        public TOptNode GetOrCreate<TNode, TOptNode>(TNode node, Func<TOptNode> creator)
            where TNode : SyntaxNode
            where TOptNode : OptNodeBase
        {
            OptNodeBase optNode = null;
            if (Cache.TryGetValue(node, out optNode))
                return (TOptNode)optNode;

            optNode = creator();
            Cache[node] = optNode;
            return (TOptNode)optNode;
        }
    }
}
