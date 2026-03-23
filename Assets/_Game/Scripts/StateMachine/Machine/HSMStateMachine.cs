using System.Collections.Generic;


/// <summary>
/// 
/// </summary>
public class HSMStateMachine {
   
    private StateNode _currentNode;
    private readonly Dictionary<IState, StateNode> _nodes = new Dictionary<IState, StateNode>();
    private readonly StateNode[] _exitStack = new StateNode[24];
    private readonly StateNode[] _enterStack = new StateNode[24];


    private readonly bool _debugMode;
    private HSMStateMachine(bool debugMode = false)
    {
        _debugMode  =  debugMode;
    }
    private void Start(IState initialState) 
    {
        if (!_nodes.TryGetValue(initialState, out var node)) return;
        // 下钻
        var targetNode = node;
        while (targetNode.DefaultSubState != null && _nodes.TryGetValue(targetNode.DefaultSubState, out var sub)) targetNode = sub;
        // 溯源
        int count = 0;
        for (var s = targetNode; s != null; s = s.Parent) _enterStack[count++] = s;
        for (int i = count - 1; i >= 0; i--) _enterStack[i].State.OnEnter();
        _currentNode = targetNode;
    }
    private void ChangeState(IState to) 
    {
        if (!_nodes.TryGetValue(to, out var target) || _currentNode == target) return;

        // 如果进入的是父状态，递归寻找其定义的默认子状态
        while (target.DefaultSubState != null && _nodes.TryGetValue(target.DefaultSubState, out var sub)) target = sub;
        

        var nodeA = _currentNode;
        var nodeB = target;
        int exitCount = 0;
        int enterCount = 0;

        // 1. LCA 寻路算法 (基于 Level 深度对齐)
        while (nodeA.Level > nodeB.Level) 
        {
            _exitStack[exitCount++] = nodeA;
            nodeA = nodeA.Parent;
        }
        while (nodeB.Level > nodeA.Level) 
        {
            _enterStack[enterCount++] = nodeB;
            nodeB = nodeB.Parent;
        }
        while (nodeA != nodeB) 
        {
            _exitStack[exitCount++] = nodeA;
            nodeA = nodeA.Parent;
            _enterStack[enterCount++] = nodeB;
            nodeB = nodeB.Parent;
        }

        if (_debugMode)
        {
#if UNITY_EDITOR
            // 构造整条流转链：[退出状态] -> [共同父节点(保留)] -> [进入状态]
            var sb = new System.Text.StringBuilder();
            sb.Append($"<b>[Transition Chain]</b>: ");

            // A. 退出链 (从深到浅)
            for (int i = 0; i < exitCount; i++)
            {
                sb.Append($"<color=#ff4d4d> Exit [{_exitStack[i].State.GetType().Name}] </color>→");
            }

            // B. 共同祖先 (连接点)
            sb.Append($" <color=#808080>[Lca node : {nodeA.State.GetType().Name}]</color>");

            // C. 进入链 (从浅到深)
            for (int i = enterCount - 1; i >= 0; i--)
            {
                sb.Append($"→<color=#4db8ff>[{_enterStack[i].State.GetType().Name}] Enter</color>");
            }

            UnityEngine.Debug.Log(sb.ToString());
#endif
        }


        // 2. 状态切换回调
        for (int i = 0; i < exitCount; i++) _exitStack[i].State.OnExit();
        _currentNode = target;
        for (int i = enterCount - 1; i >= 0; i--) _enterStack[i].State.OnEnter();
    }

    public void Update() 
    {
        if (_currentNode == null) return;
        
        var transition = GetTransition(_currentNode);
        if (transition != null)
        {
            ChangeState(transition.To);
            return;
        }
        UpdateRecursive(_currentNode);
    }
    

    public void FixedUpdate() 
    {
        if (_currentNode == null) return;
        FixedUpdateRecursive(_currentNode);
    }

    public void RequestChange(IState to)
    {
        ChangeState(to);
    }
    private void UpdateRecursive(StateNode node) 
    {
        if(node == null) return;
        if (node.Parent != null) UpdateRecursive(node.Parent);
        node.State.Update();
    }

    private void FixedUpdateRecursive(StateNode node) 
    {
        if (node == null) return;
        if (node.Parent != null) FixedUpdateRecursive(node.Parent);
        node.State.FixedUpdate();
    }

    private Transition GetTransition(StateNode node) 
    {
        // 父节点递归检查转换，实现抢占
        if (node.Parent != null) {
            var pt = GetTransition(node.Parent);
            if (pt != null) return pt;
        }
        // 当前节点检查转换
        foreach (var t in node.Transitions) if (t.Evaluate()) return t;
        return null;
    }
    private void AddNode(IState state, IState parent = null, bool isDefaultAtParent = false) 
    {
        if (_nodes.ContainsKey(state)) return;
        StateNode parentNode = (parent != null && _nodes.TryGetValue(parent, out var p)) ? p : null;
        var newNode = new StateNode(state, parentNode);
        _nodes.Add(state, newNode);
        if (isDefaultAtParent && parentNode != null) {
            parentNode.DefaultSubState = state;
        }
    }

    private void AddTransition<T>(IState from, IState to, T condition) 
    {
        if (_nodes.TryGetValue(from, out var node)) {
            node.Transitions.Add(new Transition<T>(to, condition));
        }
    }
    
    
    public class Builder 
    {
        private readonly HSMStateMachine _sm;
        public Builder(IState root, bool debugMode = false) {
            _sm = new HSMStateMachine(debugMode);
            _sm.AddNode(root); 
        }

        public Builder State(IState state, IState parent = null, bool isDefault = false) 
        {
            _sm.AddNode(state, parent, isDefault);
            return this;
        }

        public Builder At<TValue>(IState from, IState to, TValue condition) {
            _sm.AddTransition(from, to, condition);
            return this;
        }
        
        public HSMStateMachine Build(IState entryState) {
            _sm.Start(entryState);
            return _sm;
        }
    }
    
    private class StateNode
    {
        public IState State { get; }
        public readonly StateNode Parent;
    
        public IState DefaultSubState;

        public readonly int Level;
        public readonly List<Transition> Transitions = new List<Transition>();
        /// <summary>
        /// parent can be null, but only in root State
        /// </summary>
        /// <param name="state"></param>
        /// <param name="parent"></param>
        public StateNode(IState state , StateNode parent = null) 
        {
            State = state;
            Parent = parent;
            Level = parent != null ? parent.Level + 1 : 0;
        }

    
    }

}

public class Alive : IState
{
    
}

public class Death : IState
{
    
}





