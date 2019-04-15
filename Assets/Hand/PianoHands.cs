using UnityEngine;
using System.Collections;
using System.Linq;

public class PianoHands : MonoBehaviour
{
    private const int NoteLeftRightThreshold = 35;
    private float BlackKeyOffsetX = 0.07f;

    public PlayPiano player;
    public HandMovement handLeft;
    public HandMovement handRight;

    public bool useRightHandOnly = false;

    private float initHandX;

    private Fingering fingering;
    private int[] upcomingNoteBuffer = new int[5];

    private static int handSwitches = 0;

    // Use this for initialization
    void Start()
    {
        player.OnNotePress += OnNotePress;
        player.OnNoteRelease += OnNoteRelease;

        initHandX = handLeft.transform.position.x;

        if (useRightHandOnly)
            handLeft.TargetPositon += new Vector3(0, 0, 2.0f);

        fingering = new Fingering(player);
    }

    private void OnNotePress(int note)
    {
        HandMovement hand = note >= NoteLeftRightThreshold ? handRight : handLeft;
        OnNotePressWithHand(note, hand);
    }

    private void OnNotePressWithHand(int note, HandMovement hand)
    {
        if (useRightHandOnly)
            hand = handRight;

        HandMovement otherHand = hand == handLeft ? handRight : handLeft;

        int mult = hand == handRight ? 1 : -1;
        int handBase = hand == handRight ? 0 : 4; // Which finger to start on 

        // Get White key positions
        int whiteNote = player.GetKeyPosByNoteIndex(note);
        int whiteBaseNote = player.GetKeyPosByNoteIndex(hand.CurrentBaseNote);

        // Is note in current reach of the hand?
        if (whiteNote >= whiteBaseNote && whiteNote <= whiteBaseNote + 7)
        {
            // If yes, determine the finger
            int finger;
            int lowerNote = hand.LastPlayedNote;
            int upperNote = note;
            if (note < hand.LastPlayedNote)
            {
                lowerNote = note;
                upperNote = hand.LastPlayedNote;
            }
            if (lowerNote != upperNote && upperNote - lowerNote <= 12)
            {
                finger = fingering.GetOptimalFinger(hand.CurrentFinger + 1, lowerNote, upperNote, note < hand.LastPlayedNote) - 1;
            }
            else
            {
                // Repeated note -> stay on same finger
                finger = hand.CurrentFinger;
            }

            /*int finger = whiteNote - whiteBaseNote;

            // Check if a cross-over should be performed
            player.GetUpcomingNoteBuffer(upcomingNoteBuffer);
            if (finger == 3 && upcomingNoteBuffer[0] > note && upcomingNoteBuffer[1] > upcomingNoteBuffer[0])
            {
                finger = 0;
                hand.CurrentBaseNote = note;
            }*/

            // Check if it requires a greater hand span
            if (finger > 4)
            {
                // Use full octave span, but use different finger depending on actual note
                if (finger == 5) finger = 3;
                else if (finger == 6) finger = 4;
                else if (finger == 7) finger = 4;

                // Set hand to span an entire octave and move it accordingly
                hand.SetOctaveSpan(1.0f);
                int octaveMovement = hand == handLeft ? 3 : 4;
                float zPos = player.GetNoteWorldPosition(hand.CurrentBaseNote, octaveMovement).z;
                hand.TargetPositon = new Vector3(hand.transform.position.x, hand.transform.position.y, zPos);
                //hand.CurrentBaseNote = note; // Leave at old base note ??
            }
            else
            {
                // Reset hand span
                float zPos = player.GetNoteWorldPosition(hand.CurrentBaseNote, 2).z;
                hand.TargetPositon = new Vector3(hand.transform.position.x, hand.transform.position.y, zPos);
                hand.SetOctaveSpan(0.0f);
            }

            int actualFinger = Mathf.Abs(handBase - finger);
            hand.FingerDown(actualFinger);
            hand.LastPlayedNote = note;
            hand.CurrentFingerNotes[actualFinger] = note;
            
        }
        else
        {
            // If not, move hand to the according position

            // Check first, if hand can be moved -> otherwise try to use the other hand
            if (!useRightHandOnly && handSwitches < 2 && hand.IsOccupied() && !otherHand.IsOccupied())
            {
                handSwitches++;
                OnNotePressWithHand(note, otherHand);
                return;
            }
            else
            {
                handSwitches = 0;
            }

            float zPos = player.GetNoteWorldPosition(note, 2).z;
            hand.TargetPositon = new Vector3(hand.transform.position.x, hand.transform.position.y, zPos);
            hand.CurrentBaseNote = note;

            // Start with thumb or pinky - depending on the upcoming note(s)
            int nextNote = player.GetUpcomingNote();
            int finger = handBase; 
            
            if (nextNote < note)
            {
                // Start with pinky
                finger = 4 - handBase;
                zPos = player.GetNoteWorldPosition(note, -2).z;
                hand.TargetPositon = new Vector3(hand.transform.position.x, hand.transform.position.y, zPos);
                hand.CurrentBaseNote = note - 7;
            }

            hand.FingerDown(finger);
            hand.LastPlayedNote = note;
            hand.CurrentFingerNotes[finger] = note;

            // Reset hand span
            hand.SetOctaveSpan(0.0f);
        }

        // Handle Black Keys
        if (player.IsBlackKey(note))
        {
            var pos = hand.TargetPositon;
            pos.x = initHandX + BlackKeyOffsetX;
            hand.TargetPositon = pos;
        }
        else
        {
            var pos = hand.TargetPositon;
            pos.x = initHandX;
            hand.TargetPositon = pos;
        }
    }

    private void OnNoteRelease(int note)
    {
        HandMovement hand = note >= NoteLeftRightThreshold ? handRight : handLeft;

        if (useRightHandOnly)
            hand = handRight;

        //int finger = (note - hand.CurrentBaseNote) + 2;
        int finger = hand.CurrentFingerNotes.IndexOf(note);
        if (finger == -1)
            return;

        hand.CurrentFingerNotes[finger] = -1;
        hand.FingerUp(finger);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
