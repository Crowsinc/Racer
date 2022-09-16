using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SuspensionResizer : MonoBehaviour
{
    /// <summary>
    /// The slider joint that defines the suspension travel.
    /// </summary>
    public SliderJoint2D SliderJoint;

    /// <summary>
    /// The object whose y-scale will be resized to match the suspension travel. 
    /// </summary>
    public GameObject SuspensionObject;

    private Vector2 _origScale = Vector2.zero;
    private float _scaleRatio = 1.0f;

    void Awake()
    {
        if (SliderJoint == null || SuspensionObject == null)
        {
            Debug.LogError("Suspension object is misconfigured");
            return;
        }

        _origScale = SuspensionObject.transform.localScale;
        _scaleRatio = (SliderJoint.limits.max - SliderJoint.limits.min) / _origScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        if(SliderJoint.enabled)
        {
            var p1 = SliderJoint.attachedRigidbody.transform.TransformPoint(SliderJoint.anchor);
            var p2 = SliderJoint.connectedBody.transform.TransformPoint(SliderJoint.connectedAnchor);
            var centre = 0.5f * (p1 + p2);
            
            SuspensionObject.transform.position = centre;
            SuspensionObject.transform.localScale = new Vector2(
                SuspensionObject.transform.localScale.x,
                SliderJoint.jointTranslation * _scaleRatio
            );
        }
        else SuspensionObject.transform.localScale = _origScale;
    }
}
