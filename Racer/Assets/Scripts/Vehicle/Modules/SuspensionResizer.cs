using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    void Awake()
    {
        _origScale = SuspensionObject.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if(SliderJoint == null || SuspensionObject == null)
        {
            Debug.LogError("Suspension object is misconfigured");
            return;
        }


        if(SliderJoint.enabled)
        {
            SuspensionObject.transform.localScale = new Vector2(
                SuspensionObject.transform.localScale.x,
                SliderJoint.jointTranslation
            );
        }
        else SuspensionObject.transform.localScale = _origScale;
    }
}
