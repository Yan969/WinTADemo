using UnityEngine;
using UnityEngine.Rendering.TADemo;

public class PositionTrackObject : MonoBehaviour
{
    private Vector3 m_currentPos;
    void OnEnable()
    {
        GrassRendererFeature.trackActor = this;
    }
    
    void Update()
    {
        m_currentPos = transform.position;
    }

    void OnDisable()
    {
        GrassRendererFeature.trackActor = null;
    }

    public Vector3 GetTrackActorPosition()
    {
        return m_currentPos;
    }
}
