using System;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// Расчет координат развертки меша лица относительно текстуры от видеоисточника
/// </summary>
public class UVRecalculate : MonoBehaviour
{
    [SerializeField] private Camera _arCamera;
    [SerializeField] private Transform _matrix;
    [SerializeField] private ARFaceManager _faceManager;
    
    private MeshFilter _meshFilter;
    private Mesh _mesh;
    private Vector3[] _verts;
    private Vector2[] _uvs;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        
        _mesh = _meshFilter.mesh;
        _verts = _mesh.vertices;
        _uvs = new Vector2[_mesh.vertexCount];
        _mesh.triangles = _mesh.triangles.Reverse().ToArray();
    }

    private void OnEnable() => _faceManager.facesChanged += OnFaceChanged;
    private void OnDisable() => _faceManager.facesChanged -= OnFaceChanged;

    private void OnFaceChanged(ARFacesChangedEventArgs eventArgs)
    {
        foreach (var face in eventArgs.updated) 
            UpdateMeshUV(face.transform);
    }

    private void UpdateMeshUV(Transform arFaceTransform) => _mesh.SetUVs(0, TranslateVertices(arFaceTransform));

    private Vector2[] TranslateVertices(Transform arFaceTransform)
    {
        Matrix4x4 worldMatrix = arFaceTransform.localToWorldMatrix;
        worldMatrix *= Matrix4x4.TRS(_matrix.position, _matrix.rotation, _matrix.localScale);

        for (int i = 0; i < _verts.Length; i++)
        {
            var worldPoint = worldMatrix.MultiplyPoint3x4(_verts[i]);
            var uv = _arCamera.WorldToViewportPoint(worldPoint);
            _uvs[i] = new Vector2(uv.x, uv.y);
        }
        return _uvs;
    }
}
