using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class ModulePicker : MonoBehaviour
{
    [Required, SerializeField] private GameObject moduleSelector;
    [Required, SerializeField] private ModulePickerSlot modulePickerSlot; 
    [SerializeField] private List<GameObject> modules;
    
    [Required, SerializeField] private Transform cableSpawn;
    [Required, SerializeField] private GameObject cablePrefab;
    
    [SerializeField] private RectTransform content;

    private void OnEnable()
    {
        PopulateUi();
    }

    [Button("PopulateUI")]
    private void PopulateUi()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(content.GetChild(i).gameObject);
        }

        foreach (var module in modules)
        {
            var newInstance = Instantiate(moduleSelector, content);
            var button = newInstance.GetComponent<Button>();
            newInstance.GetComponentInChildren<TMP_Text>().text = module.name;
            button.onClick.AddListener(() => modulePickerSlot.SetModule(module));
        }
    }

    public void AddCable()
    {
        Instantiate(cablePrefab, cableSpawn.position, cableSpawn.rotation);
    }
}