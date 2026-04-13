using UnityEngine;

public class ShaderPlaceHolderController : MonoBehaviour
{
    public Material shader;

    // otro gran codigo
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            if(shader.GetFloat("_Control") <= 0f) shader.SetFloat("_Control", 1f);
            else shader.SetFloat("_Control", 0f);
        }
    }

    public void OnApplicationQuit()
    {
        shader.SetFloat("_Control", 0f);
    }
}
