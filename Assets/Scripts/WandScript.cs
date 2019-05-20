using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WandScript : MonoBehaviour
{
    [SerializeField]
    private Sprite VanillaWand,
                   TracingWand,
                   LoadingWand,
                   ShootingWand;

    private SpriteRenderer RendererReference;
    private Transform TransformReference;
    private int RotationTimer;
    private CounterScript RotationCounter;
    // Start is called before the first frame update
    void Start()
    {
        RendererReference = GetComponent<SpriteRenderer>();
        TransformReference = GetComponent<Transform>();
        RotationTimer = TimerScript.MakeTimer(20);
        RotationCounter = new CounterScript(0, 9, 1, 1);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetMouseButtonDown(0)) {
            RendererReference.sprite = TracingWand;
        } else if (Input.GetMouseButtonUp(0)) {
            RendererReference.sprite = VanillaWand;
        }
        
        if (TimerScript.HasPassed(RotationTimer)) {
            TimerScript.Remove(RotationTimer);
            RotationTimer = TimerScript.MakeTimer(20);
            TransformReference.RotateAround(TransformReference.position - 
                                            new Vector3(RendererReference.bounds.size.x / 2f, RendererReference.bounds.size.y / 2f, 0),
                                            new Vector3(1, 1, 1), 5 * ((RotationCounter.Get() > 4) ? -1 : 1));
        }
    }
}
