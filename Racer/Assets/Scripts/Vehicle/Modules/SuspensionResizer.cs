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
    private Vector2 _origPosition = Vector2.zero;

    void Awake()
    {
        if (SliderJoint == null || SuspensionObject == null)
        {
            Debug.LogError("Suspension object is misconfigured");
            return;
        }

        _origScale = SuspensionObject.transform.localScale;
        _origPosition = SuspensionObject.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (SliderJoint.enabled)
        {
            var p1 = SliderJoint.attachedRigidbody.transform.TransformPoint(SliderJoint.anchor);
            var p2 = SliderJoint.connectedBody.transform.TransformPoint(SliderJoint.connectedAnchor);
            var centre = 0.5f * (p1 + p2);

            SuspensionObject.transform.position = centre;
            SuspensionObject.transform.localScale = new Vector2(
                SuspensionObject.transform.localScale.x,
                SliderJoint.jointTranslation
            );
        }
        else
        {
            SuspensionObject.transform.localPosition = _origPosition;
            SuspensionObject.transform.localScale = _origScale;
        }
    }
}
