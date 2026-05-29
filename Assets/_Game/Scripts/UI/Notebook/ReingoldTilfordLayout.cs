using UnityEngine;


public static class ReingoldTilfordLayout
{
   
    private const float NodeHorizontalDistance = 1f;

    public static void CalculatePositions(TreeNode rootNode)
    {
        if (rootNode == null)
        {
            return;
        }

        ResetTreeNodes(rootNode);
        ExecuteFirstWalk(rootNode);

        float minimumX = 0f;
        ExecuteSecondWalk(rootNode, 0f, 0, ref minimumX);

        
        if (minimumX < 0f) ExecuteThirdWalk(rootNode, -minimumX);
        
    }

    private static void ResetTreeNodes(TreeNode currentNode)
    {
        currentNode.X = 0f;
        currentNode.Y = 0f;
        
        currentNode.Mod = 0;
        currentNode.Shift = 0f;
        currentNode.Change = 0f;
        
        currentNode.Thread = null;
        currentNode.Ancestor = currentNode;

        foreach (TreeNode childNode in currentNode.Children)
        {
            ResetTreeNodes(childNode);
        }
    }

    private static void ExecuteFirstWalk(TreeNode currentNode)
    {
        if (currentNode.IsLeaf)
        {
            TreeNode leftSibling = currentNode.GetLeftSibling();

            if (leftSibling != null) currentNode.X = leftSibling.X + NodeHorizontalDistance;
            
            else currentNode.X = 0.0f;
            
        }
        else
        {
            TreeNode defaultAncestor = currentNode.Children[0];

            foreach (TreeNode childNode in currentNode.Children)
            {
                ExecuteFirstWalk(childNode);
                defaultAncestor = ExecuteApportion(childNode, defaultAncestor);
            }

            ExecuteShifts(currentNode);

         
            float midpoint = (currentNode.Children[0].X + currentNode.Children[^1].X) * 0.5f;
            TreeNode leftSibling = currentNode.GetLeftSibling();

            if (leftSibling != null)
            {
                currentNode.X = leftSibling.X + NodeHorizontalDistance;
                currentNode.Mod = currentNode.X - midpoint;
            }
            else
            {
                currentNode.X = midpoint;
            }
        }
    }

    private static TreeNode ExecuteApportion(TreeNode currentNode, TreeNode defaultAncestor)
    {
        TreeNode leftSibling = currentNode.GetLeftSibling();

        if (leftSibling == null) return defaultAncestor;

        // v = currentNode
        // w = leftSibling
        
        
        TreeNode insideRightNode = currentNode;
        TreeNode outsideRightNode = currentNode;
        TreeNode insideLeftNode = leftSibling;
        TreeNode outsideLeftNode = currentNode.GetLeftMostSibling();

        float insideRightModifierSum = insideRightNode.Mod;
        float outsideRightModifierSum = outsideRightNode.Mod;
        float insideLeftModifierSum = insideLeftNode.Mod;
        float outsideLeftModifierSum = outsideLeftNode.Mod;

        while (insideLeftNode.GetNextRight() != null && insideRightNode.GetNextLeft() != null)
        {
            insideLeftNode = insideLeftNode.GetNextRight();
            insideRightNode = insideRightNode.GetNextLeft();

            outsideLeftNode = outsideLeftNode?.GetNextLeft();

            outsideRightNode = outsideRightNode?.GetNextRight();

            if (outsideRightNode != null) outsideRightNode.Ancestor = currentNode;
            

            float currentSubtreeShift = (insideLeftNode.X + insideLeftModifierSum) 
                                      - (insideRightNode.X + insideRightModifierSum) 
                                      + NodeHorizontalDistance;

            if (currentSubtreeShift > 0f)
            {
                TreeNode correctAncestor = GetResolvedAncestor(insideLeftNode, currentNode, defaultAncestor);
                MoveSubtree(correctAncestor, currentNode, currentSubtreeShift);
                
                insideRightModifierSum += currentSubtreeShift;
                outsideRightModifierSum += currentSubtreeShift;
            }

            insideLeftModifierSum += insideLeftNode.Mod;
            insideRightModifierSum += insideRightNode.Mod;
            
            if (outsideLeftNode != null) outsideLeftModifierSum += outsideLeftNode.Mod;
            
            
            if (outsideRightNode != null) outsideRightModifierSum += outsideRightNode.Mod;
            
        }

       
        if (insideLeftNode.GetNextRight() != null && outsideRightNode != null && outsideRightNode.GetNextRight() == null)
        {
            outsideRightNode.Thread = insideLeftNode.GetNextRight();
            outsideRightNode.Mod += insideLeftModifierSum - outsideRightModifierSum;
        }

        if (insideRightNode.GetNextLeft() == null || outsideLeftNode == null || outsideLeftNode.GetNextLeft() != null)
            return defaultAncestor;
        outsideLeftNode.Thread = insideRightNode.GetNextLeft();
        outsideLeftNode.Mod += insideRightModifierSum - outsideLeftModifierSum;
        defaultAncestor = currentNode;

        return defaultAncestor;
    }

    private static void MoveSubtree(TreeNode leftNode, TreeNode rightNode, float subtreeShift)
    {
        int totalSubtreesBetween = rightNode.Number - leftNode.Number;

        if (totalSubtreesBetween <= 0)
        {
            return;
        }

        float shiftRatio = subtreeShift / totalSubtreesBetween;

        rightNode.Change -= shiftRatio;
        rightNode.Shift += subtreeShift;
        leftNode.Change += shiftRatio;
        rightNode.X += subtreeShift;
        rightNode.Mod += subtreeShift;
    }

    private static void ExecuteShifts(TreeNode parentNode)
    {
        float currentShiftAccumulator = 0f;
        float currentChangeAccumulator = 0f;

        for (int index = parentNode.Children.Count - 1; index >= 0; index--)
        {
            TreeNode childNode = parentNode.Children[index];
            childNode.X += currentShiftAccumulator;
            childNode.Mod += currentShiftAccumulator;
            
            currentChangeAccumulator += childNode.Change;
            currentShiftAccumulator += childNode.Shift + currentChangeAccumulator;
        }
    }

    private static TreeNode GetResolvedAncestor(TreeNode insideLeftNode, TreeNode currentNode, TreeNode defaultAncestor)
    {
        if (insideLeftNode.Ancestor != null && insideLeftNode.Ancestor.Parent == currentNode.Parent) return insideLeftNode.Ancestor;
        
        return defaultAncestor;
    }

    private static void ExecuteSecondWalk(TreeNode currentNode, float modifierSum, int currentDepth, ref float minimumX)
    {
        currentNode.X += modifierSum;
        currentNode.Y = currentDepth;
        
        minimumX = Mathf.Min(minimumX, currentNode.X);

        foreach (TreeNode childNode in currentNode.Children) ExecuteSecondWalk(childNode, modifierSum + currentNode.Mod, currentDepth + 1, ref minimumX);
        
    }

    private static void ExecuteThirdWalk(TreeNode currentNode, float globalShiftAmount)
    {
        currentNode.X += globalShiftAmount;
        
        foreach (TreeNode childNode in currentNode.Children) ExecuteThirdWalk(childNode, globalShiftAmount);
        
    }
}