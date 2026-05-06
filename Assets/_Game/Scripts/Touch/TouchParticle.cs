using UnityEngine;

public class TouchParticle : MonoBehaviour
{
    private ITouch _owner;
    
    [SerializeField] private GameObject _particle;
    [SerializeField] private ParticleSystem _particleSystem;

    private void Awake()
    {
        _owner = GetComponentInParent<ITouch>();
        if (_owner is MonoBehaviour debug)
        {
            Debug.Log(debug.gameObject.name);
        }
    }

    private void Start()
    {
        if(!_particle) return;
        //_particle.SetActive(false);
        _owner.OnFocus += ActiveGameObject;
        _owner.OnUnfocus += DeactiveGameObject;
    }


    private void ActiveGameObject()
    {
        //_particle.SetActive(true);
        if(_particleSystem) _particleSystem?.Play();

    }

    private void DeactiveGameObject()
    {
        //_particle.SetActive(false);
        if(_particleSystem) _particleSystem?.Stop();
    }
}