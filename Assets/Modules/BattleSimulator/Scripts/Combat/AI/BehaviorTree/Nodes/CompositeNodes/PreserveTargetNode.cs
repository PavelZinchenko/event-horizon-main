namespace Combat.Ai.BehaviorTree.Nodes
{
    public class PreserveTargetNode : INode
    {
        private readonly INode _node;

        public static INode Create(INode node)
        {
            if (node == EmptyNode.Success) return EmptyNode.Success;
            if (node == EmptyNode.Failure) return EmptyNode.Failure;
            if (node == EmptyNode.Running) return EmptyNode.Running;
            return new PreserveTargetNode(node);
        }

        public NodeState Evaluate(Context context)
        {
            var target = context.TargetShip;
            var result = _node.Evaluate(context);
            context.TargetShip = target;
            return result;
        }

        private PreserveTargetNode(INode node) => _node = node;
    }
}
