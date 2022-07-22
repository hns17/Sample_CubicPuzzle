using CubicSystem.CubicPuzzle;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TextMeshControl : MonoBehaviour
{
    [SerializeField] private int sortOrder;
    [SerializeField] private BlockPresenter blockPresenter;


    private TextMesh textMesh;
    private MeshRenderer meshRenderer;

    private StageInputActions input;


    private void Awake()
    {
        input = new StageInputActions();
    }

    void Start()
    {
        textMesh = this.GetComponent<TextMesh>();
        meshRenderer = this.GetComponent<MeshRenderer>();
        meshRenderer.sortingOrder = sortOrder;

        input.Editor.PressDown.started += PressDown_performed;
    }

    private void OnEnable()
    {
        input.Editor.Enable();
    }

    private void OnDisable()
    {
        input.Editor.Disable();
    }

    private void PressDown_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        var block = blockPresenter.Block;
        
        textMesh.text = block.Idx.ToString();
    }

    private void LateUpdate()
    {
        var block = blockPresenter.Block;

        textMesh.text = block.Idx.ToString();
        //textMesh.text = blockPresenter.Block.Position.ToString();
    }

}
