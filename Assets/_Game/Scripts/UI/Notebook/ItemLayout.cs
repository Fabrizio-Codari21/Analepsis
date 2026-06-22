using UnityEngine;

public class ItemLayout : NotebookLayout
{
    [SerializeField, TextArea] private string leftEmptyReason = "No Item Founded";
    [SerializeField, TextArea] private string rightEmptyReason = "No Item Founded";

    [SerializeField] private ItemSelectionPage m_leftPage;
    [SerializeField] private ItemInfoPage m_rightPage;
    public override void Initialize(Transform leftRoot, Transform rightRoot)
    {
        m_leftPage = Instantiate(m_leftPage,leftRoot);
        m_leftPage.Hide();
        
        m_rightPage = Instantiate(m_rightPage,rightRoot);
        m_rightPage.Hide();
    }


    public override void Show()
    {
        base.Show();
        m_leftPage.Show();
        m_rightPage.Show();
    }

    public override void Hide()
    {
        base.Hide();
        m_leftPage.Hide();
        m_rightPage.Hide();
    }
}