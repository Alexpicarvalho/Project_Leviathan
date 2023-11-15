using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MeshTrailFX : MonoBehaviour
{
    [Header("Values and Refs")]
    [SerializeField] private float _activeTime = 1f;
    [SerializeField] private float _meshRendererRefreshRate = 0.1f;
    [SerializeField] private Material _trailMaterial;
    [SerializeField] private float _trailParticleLifetime = 1f;
    [SerializeField] private float _trailStartZOffset = 0.5f;   
    private Dasher _dasher;

    [Header("Debug")]
    [SerializeField] private bool _fixedTime = false;

    private bool _isActive = false;
    private float _timer = 0f;

    private SkinnedMeshRenderer[] _meshRenderers;

    private void Awake()
    {
        _dasher = GetComponentInParent<Dasher>();
        _meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        SetActiveTime(_dasher.DashDuration);
        if (_meshRenderers.Length == 0 || !_dasher) Destroy(this);
    }
    public void SetActiveTime(float newActiveTime)
    {
        if (_fixedTime) return;
        _activeTime = newActiveTime;
    }

    public void TurnOn()
    {
        _isActive = true;
        StartCoroutine(ActivateTrail(_activeTime));
    }

    IEnumerator ActivateTrail(float activeTime)
    {
        while (activeTime > 0)
        {
            activeTime -= _meshRendererRefreshRate;

            for (int i = 0; i < _meshRenderers.Length; i++)
            {
                GameObject trailPiece = new GameObject("DashFX");

                trailPiece.transform.position = transform.position 
                    + transform.forward * _trailStartZOffset;
                trailPiece.transform.rotation = transform.rotation;

                MeshRenderer mr = trailPiece.AddComponent<MeshRenderer>();
                MeshFilter mf = trailPiece.AddComponent<MeshFilter>();

                Mesh mesh = new Mesh();
                _meshRenderers[i].BakeMesh(mesh);
                mf.mesh = mesh;
                mr.material = _trailMaterial;

                mr.materials[0].DOFloat(0, "_Alpha", _trailParticleLifetime);

                Destroy(trailPiece, _trailParticleLifetime);
                DOVirtual.DelayedCall(_trailParticleLifetime, () => Destroy(mr));
                DOVirtual.DelayedCall(_trailParticleLifetime, () => Destroy(mf));
                foreach (var mat in mr.materials)
                {
                    DOVirtual.DelayedCall(_trailParticleLifetime, () => Destroy(mat));
                }
            }
            yield return new WaitForSeconds(_meshRendererRefreshRate);
        }

        _isActive = false;
    }

    private void OnEnable()
    {
        _dasher.OnDash += TurnOn;
    }

    private void OnDisable()
    {
        _dasher.OnDash -= TurnOn;
    }
}
