using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class ClueInteractable : Interactable
{
    public MeshRenderer[] materials;
    Color _color;

    public override void Focus()
    {
        print($"You can now interact with {InteractableObject.name}");
        
        foreach(MeshRenderer renderer in materials) renderer.material.color = Color.white;
    }

    public override void InteractEnd() {/*por ahora no se usa*/}

    public override void InteractStart()
    {
        print($"You interacted with {InteractableObject.name}");
        var color = materials[0].material.color;
        foreach (MeshRenderer renderer in materials) renderer.material.color = Color.green;

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
    }

    void Start()
    {
        materials = GetComponentsInChildren<MeshRenderer>();
        _color = materials[0].material.color;
    }

    void FixedUpdate()
    {
        transform.Rotate(0, 1, 0);
    }
}
