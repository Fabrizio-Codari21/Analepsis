using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemSelectionPage : NotebookPage
{
    [Header("Selection Root")]
    [SerializeField] private Transform m_contentRoot;
    [SerializeField] private ButtonSetting m_itemButton;
    [SerializeField] private ItemEventChannel m_addItem;
    [SerializeField] private ItemEventChannel m_requiredItemInfo;

    [SerializeField] private Check m_checkMarked;
    [SerializeField] private EvidenceEvent  m_sentNoteToTheoryBoardEvent;
    private void Awake()
    {
        m_addItem.OnEventRaised += AddItem;
    }

    private readonly HashSet<Item> _items = new();
    
    private void AddItem(Item item)
    {
        
        if(!_items.Add(item)) return;
        Debug.Log("Add Item");
        SpawnButton(item);
    }

    private void SpawnButton(Item item)
    {

        ItemEvidence itemEvidence = EvidenceDataBase.Instance.GetOrCreate(item.guid, () => new ItemEvidence(item.Name,item.guid,item.doesItProveAnything,item));
        ButtonWithSubButton button = FlyweightFactory.Instance.Spawn<ButtonWithSubButton>(m_itemButton,Vector3.zero, Quaternion.identity, m_contentRoot);
        button.RemoveAllListeners();

        button.SetText(item.Name);
        
        button.AddListener(() =>
        {
            m_requiredItemInfo?.Raise(item);
        });
        
        var subButton = button.AddSubButton();
        subButton.RemoveAllListeners();
        
        
        bool isAlreadyMarked = m_checkMarked.Request(item.guid);
        subButton.PlayAnimation(isAlreadyMarked);
        
        subButton.AddListener(() =>
        {
            bool currentMarkedState = m_checkMarked.Request(item.guid);

            if (currentMarkedState)
            {
                m_sentNoteToTheoryBoardEvent?.Raise(itemEvidence);
                subButton.PlayAnimation(false); 
            }
            else
            {
                m_sentNoteToTheoryBoardEvent?.Raise(itemEvidence);
                subButton.PlayAnimation(true);
            }
        });
    }

    private void OnDestroy()
    {
        m_addItem.OnEventRaised -= AddItem;
    }
}