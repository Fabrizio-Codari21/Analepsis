using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class FlashbackManager : PersistentSingleton<MonoBehaviour>
{
    [SerializeField] CCInputReader inputReader;
    [SerializeField] InspectionInputReader inspectionInputReader;
    [SerializeField] Material mainFlashbackShader;
    [SerializeField] Material highlightShader;
    [SerializeField] Image transitionPanel;
    [SerializeField] float transitionSpeed;
    bool _isFlashbackOn = false;
    Item _currentItemInspected;

    void Start()
    {
        transitionPanel.gameObject.SetActive(false);
        transitionPanel.color -= new Color(0, 0, 0, transitionPanel.color.a);
        inspectionInputReader.SeeFlashback += SeeFlashback;
    }

    public void SetCurrentItem(Item item) => _currentItemInspected = item;
    public void SeeFlashback()
    {
        if (!_isFlashbackOn) StartFlashback(_currentItemInspected); else EndFlashback(_currentItemInspected);
    }

    public async UniTask StartFlashback(Item inspected = default)
    {
        bool transitionPanelActive = false;

        transitionPanel.gameObject.SetActive(true);
        while (mainFlashbackShader.GetFloat("_Control") < 1f)
        {
            mainFlashbackShader.SetFloat("_Control", mainFlashbackShader.GetFloat("_Control") + 0.01f * transitionSpeed/5);
            //highlightShader.SetFloat("_Control", highlightShader.GetFloat("_Control") + 0.01f * transitionSpeed);

            if (!transitionPanelActive)
            {
                transitionPanel.color += new Color(0,0,0,a: 0.04f * transitionSpeed/5);
                if(transitionPanel.color.a >= 1f) transitionPanelActive = true;
            }
            else
            {
                transitionPanel.color += new Color(0, 0, 0, a: -0.02f * transitionSpeed/5);
            }

            await UniTask.Delay(20);
        }

        mainFlashbackShader.SetFloat("_Control", 1f);
        //highlightShader.SetFloat("_Control", 1f);
        transitionPanel.color -= new Color(0,0,0,transitionPanel.color.a);
        transitionPanel.gameObject.SetActive(false);
        _isFlashbackOn = true;
    }

    public async UniTask EndFlashback(Item inspected = default)
    {
        bool transitionPanelActive = false;

        transitionPanel.gameObject.SetActive(true);
        while (mainFlashbackShader.GetFloat("_Control") > 0f)
        {
            mainFlashbackShader.SetFloat("_Control", mainFlashbackShader.GetFloat("_Control") - 0.01f * transitionSpeed/5);
            //highlightShader.SetFloat("_Control", highlightShader.GetFloat("_Control") - 0.01f * transitionSpeed);

            if (!transitionPanelActive)
            {
                transitionPanel.color += new Color(0, 0, 0, a: 0.04f * transitionSpeed/5);
                if (transitionPanel.color.a >= 1f) transitionPanelActive = true;
            }
            else
            {
                transitionPanel.color += new Color(0, 0, 0, a: -0.02f * transitionSpeed/5);
            }

            await UniTask.Delay(20);
        }

        mainFlashbackShader.SetFloat("_Control", 0f);
        //highlightShader.SetFloat("_Control", 0f);
        transitionPanel.color -= new Color(0, 0, 0, transitionPanel.color.a);
        transitionPanel.gameObject.SetActive(false);
        _isFlashbackOn = false;
    }
}
