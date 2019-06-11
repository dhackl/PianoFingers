using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveFingers : MonoBehaviour
{
    /*public Animator animator;

    private const int SpanLayer = 6;
    private string[] fingerNames = { "Thumb", "Index", "Middle", "Ring", "Pinky" };

    // Use this for initialization
    void Start()
    {
        StartCoroutine(MoveFinger());
    }

    IEnumerator MoveFinger()
    {
        for (int i = 0; i < 5; i++)
        {
            string name = fingerNames[i];
            int layer = i + 1;

            animator.SetLayerWeight(layer, 1.0f);
            animator.Play("Anim" + name, layer);

            yield return new WaitForSeconds(0.5f);

            animator.Play("Anim" + name + "_Up", layer);

            if (i == 2)
                SetOctaveSpan(1.0f);
            if (i == 4)
                SetOctaveSpan(0.0f);
        }
    }

    private void SetOctaveSpan(float weight)
    {
        animator.SetLayerWeight(SpanLayer, weight);
        animator.Play("AnimOctaveSpan", SpanLayer);
    }

    // Update is called once per frame
    void Update()
    {

    }*/
}
