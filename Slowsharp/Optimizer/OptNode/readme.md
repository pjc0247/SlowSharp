about OptNode
====

`OptNode` stores cached runtime data that enables to skip some evaluations.


__Without OptNode (Evaluates everything, everytime)__
```cs
HybInstance RunLiteralNode(LiteralNode node) {
  if (node->literalType == LiteralType.Inte) 
    return HybInstance.Create(int.Parse(node.Raw));
  if (node->literalType == LiteralType.Float)
    return HybInstance.Create(float.Parse(node.Raw));
  /* ... and so on */
}
```

__With OptNode (Uses cached data if possible)__
```cs
HybInstance RunLiteralNode(LiteralNode node) {
  // If there is a pre-calculated data, just return it
  if (IsOptNodeAvaliable(node))
    return GetOptNode(node).value;
    
  /* rest of code goes here */
}
```
