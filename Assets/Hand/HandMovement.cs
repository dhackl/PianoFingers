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

    public Vector3 TargetPosition
    {
        get { return targetPosition; }
        set
        {
            targetPosition = value;
            float diff = Mathf.Abs(targetPosition.z - transform.position.z);
            handSpeed = diff * 20.0f;
            lerp = 0;
            ypos = 0;
            doArcMovement = diff > 0.5f;
        }
    }
    private Vector3 targetPosition;

    private float baseYPos;
    private float ypos;
    private bool doArcMovement;

    private float lerp;

    // Use this for initialization
    void Start()
    {
        animator = GetComponent<Animator>();

        CurrentFingerNotes = new List<int>();
        for (int i = 0; i < 5; i++)
            CurrentFingerNotes.Add(-1);

        baseYPos = transform.position.y;
        TargetPosition = transform.position;
    }

    public void FingerDown(int finger, bool crossover = false, int crossDown = -1)
    {
        string name = fingerNames[finger];
        int layer = finger + 1;
        
        if (crossover && crossDown != -1)
        {
            animator.SetLayerWeight(crossDown + 1, 1.0f);
            animator.Play("AnimCross" + fingerNames[crossDown] + "_Down", crossDown + 1);
        }
        else
        {
            animator.SetLayerWeight(layer, 1.0f);
            animator.Play("Anim" + (crossover ? "Cross" : "") + name, layer);
        }

        CurrentFinger = finger;

        Debug.Log(finger);
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

    public int GetOccupiedFingerCount()
    {
        int count = 0;
        for (int i = 0; i < 5; i++)
        {
            if (IsFingerOccupied(i))
                count++;
        }
        return count;
    }

    // Update is called once per frame
    void Update()
    {
        /*Vector3 diff = TargetPositon - transform.position;
        if (diff.magnitude > 0.1f)
            transform.position += diff.normalized * handSpeed * Time.deltaTime;*/

        lerp += Time.deltaTime * handSpeed;
        Vector3 newPos = transform.position;

        // Arc movement
        if (doArcMovement)
        {
            if (lerp < 0.5f)
                ypos += Time.deltaTime;
            else
                ypos -= Time.deltaTime;
        }
        newPos.y = baseYPos + ypos * 10.0f;

        // Horizontal movement
        newPos.z = Mathf.Lerp(transform.position.z, TargetPosition.z, lerp);

        transform.position = newPos;
    }
}
