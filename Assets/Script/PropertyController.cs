using UnityEngine;


[ExecuteInEditMode]
public class PropertyController : MonoBehaviour
{
    public Texture2D BaseMap;
    public Material SharedMaterial;

    private MeshRenderer _renderer;
    private MaterialPropertyBlock _propBlock;

    void Start()
    {
        _renderer = GetComponent<MeshRenderer>();
        if (_renderer != null && SharedMaterial != null)
        {
            _renderer.sharedMaterial = SharedMaterial;
        }
        _propBlock = new MaterialPropertyBlock();
    }

    void Update()
    {
        if (_renderer == null)
        {
            _renderer = GetComponent<MeshRenderer>();
            if (_renderer == null)
            {
                return;
            }

            if (SharedMaterial != null)
            {
                _renderer.sharedMaterial = SharedMaterial;
            }
        }

        if (_propBlock == null)
        {
            _propBlock = new MaterialPropertyBlock();
        }

        _renderer.GetPropertyBlock(_propBlock);

        // Set the BaseMap texture
        if (BaseMap != null)
        {
            _propBlock.SetTexture("_BaseMap", BaseMap);
        }
        // Removed other property settings

        _renderer.SetPropertyBlock(_propBlock);
    }
}