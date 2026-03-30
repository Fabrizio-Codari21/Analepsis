using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class ClueInteractable : Interactable
{
    [HideInInspector] public MeshRenderer[] materials;
    public string clueID;
    Color _color;

    public override void Focus()
    {
        print($"You can now interact with {InteractableObject.name}");
        
        foreach(MeshRenderer renderer in materials) renderer.material.color = Color.white;

        string plural = actionCost > 1 ? "s" : "";
        interactText.text = $"Inspect {InteractableObject.name}? \n (Costs {actionCost} action{plural})";
        interactText.gameObject.SetActive(true);
    }
    
    private void Start()
    {
        materials = GetComponentsInChildren<MeshRenderer>();
        _color = materials[0].material.color;
    }
    
    public override void InteractStart()
    {
        base.InteractStart();
        print($"You interacted with {InteractableObject.name}");
        var color = materials[0].material.color;
        foreach (MeshRenderer renderer in materials) renderer.material.color = Color.green;
        interactText.gameObject.SetActive(false);

        ActionTimer.instance.ConsumeActions(actionCost);

        this.WaitAndThen(timeToWait: 0.2f, () =>
        {
            foreach (MeshRenderer renderer in materials) renderer.material.color = color;
        },
        cancelCondition: () => false);
    }

    public override void Unfocus()
    {
        print($"You got too far away from {InteractableObject.name} and can no longer interact with it.");
        foreach (MeshRenderer renderer in materials) renderer.material.color = _color;
        interactText.gameObject.SetActive(false);
    }

 

    void FixedUpdate()
    {
        transform.Rotate(0, 1, 0);
    }
}
