using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class ClueInteractable : Interactable
{
    [HideInInspector] public MeshRenderer[] materials;
    public string clueID;
    InspectableObject _inspectable;
    Color _color;

    public override void Focus()
    {
        print($"You can now interact with {InteractableObject.name}");
        
        foreach(MeshRenderer renderer in materials) renderer.material.color = Color.white;

        string plural = actionCost > 1 ? "s" : "";
        interactText.text = $"Inspect {InteractableObject.name}? \n (Costs {actionCost} action{plural})";
        interactText.gameObject.SetActive(true);
    }

    public override void InteractEnd() {/*por ahora no se usa*/}

    public override void InteractStart()
    {
        print($"You interacted with {InteractableObject.name}");
        var color = materials[0].material.color;
        foreach (MeshRenderer renderer in materials) renderer.material.color = Color.green;
        interactText.gameObject.SetActive(false);

        ActionTimer.instance.ConsumeActions(actionCost);
        _inspectable.Inspect();

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

    void Start()
    {
        materials = GetComponentsInChildren<MeshRenderer>();
        _inspectable = GetComponent<InspectableObject>();
        _color = materials[0].material.color;
    }

    void FixedUpdate()
    {
        transform.Rotate(0, 1, 0);
    }
}
