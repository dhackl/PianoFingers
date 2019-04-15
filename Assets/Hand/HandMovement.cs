using UnityEngine;
using System.Collections.Generic;

public class HandMovement : MonoBehaviour
{
    private float handSpeed = 10.0f;

    private const int SpanLayer = 6;
    private string[] fingerNames = { "Thumb", "Index", "Middle", "Ring", "Pinky" };

    public int CurrentBaseNote { get; set; } // On which note is the hand (the middle finger) currently aligned
    public int CurrentFinger { get; set; } // Which finger was the last one to use (for scales, fingering algorithm etc.)
    public int LastPlayedNote { get; set; }

    private Animator animator;

    public List<int> CurrentFingerNotes { get; set; } // Which finger currently holds which note

    public Vector3 TargetPositon
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    // Use this for initialization
    void Start()
    {
        animator = GetComponent<Animator>();

        CurrentFingerNotes = new List<int>();
        for (int i = 0; i < 5; i++)
            CurrentFingerNotes.Add(-1);

        TargetPositon = transform.position;
    }

    public void FingerDown(int finger)
    {
        string name = fingerNames[finger];
        int layer = finger + 1;

        animator.SetLayerWeight(layer, 1.0f);
        animator.Play("Anim" + name, layer);

        CurrentFinger = finger;
    }

    public void FingerUp(int finger)
    {
        string name = fingerNames[finger];
        int layer = finger + 1;

        animator.Play("Anim" + name + "_Up", layer);
    }

    public void SetOctaveSpan(float weight)
    {
        animator.SetLayerWeight(SpanLayer, weight);
        animator.Play("AnimOctaveSpan", SpanLayer);
    }

    public bool IsFingerOccupied(int finger)
    {
        return CurrentFingerNotes[finger] != -1;
    }

    public bool IsOccupied()
    {
        foreach (int note in CurrentFingerNotes)
        {
            if (note != -1)
                return true;
        }
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        /*Vector3 diff = TargetPositon - transform.position;
        if (diff.magnitude > 0.1f)
            transform.position += diff.normalized * handSpeed * Time.deltaTime;*/
    }
}
