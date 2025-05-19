using UnityEngine;


[RequireComponent(typeof(SpriteRenderer))]
[ExecuteInEditMode]
public class PropertyController : MonoBehaviour
{
    public Texture2D BumpMap;
    [Range(0,1)] public float BumpScale = 1;
    public Texture2D SpecularMask1;
    [Range(0, 1)] public float SpecularMaskStrength1 = 1.0f;

    // 引用相关组件
    private SpriteRenderer _renderer;
    private MaterialPropertyBlock _propBlock;

    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _propBlock = new MaterialPropertyBlock();
    }

    void Update()
    {
        if (_renderer == null)
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        if (_propBlock == null)
        {
            _propBlock = new MaterialPropertyBlock();
        }

        _renderer.GetPropertyBlock(_propBlock);

        _propBlock.SetTexture("_BumpMap", BumpMap);
        _propBlock.SetFloat("_BumpScale", BumpScale);
        _propBlock.SetTexture("_SpecularMask1", SpecularMask1);
        _propBlock.SetFloat("_SpecularMaskStrength1", SpecularMaskStrength1);

        _renderer.SetPropertyBlock(_propBlock);
    }
}